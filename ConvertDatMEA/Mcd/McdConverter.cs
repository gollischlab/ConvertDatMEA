using System;
using System.Linq;
using System.IO;


namespace ConvertDatMEA
{
    class McdConverter : DataConverter
    {
        public McdConverter(OutputFunction function, ProgressUpdate updater, BinaryWriter datWriter, string outPath, string[] channelOrder = null)
            : base(function, updater, datWriter, outPath, channelOrder) { }

        public override long ExtractData(string filepath, BinaryWriter auxWriter)
        {
            // Frame time file for this stimulus
            _auxWriter = auxWriter;

            // Number of total samples written (filtered data only)
            long total_samples = 0;

            // Give some indicator that it's loading
            _progressUpdate(0, "Loading");

            using (McdReader fileReader = new McdReader(filepath))
            {
                // Iterate over ordered streams (sorted to start with analog)
                foreach (Stream stream in fileReader.Streams.OrderBy(v => v.Key).Select(v => v.Value))
                {
                    // Signal properties
                    double unitsPerAd = stream.Format.UnitsPerAd;
                    if (!stream.isAnalog)
                        unitsPerAd *= 1e3; // Filtered stream is in microvolts

                    // Channels of this stream
                    int nChannels = (int)stream.Header.ChannelCount;

                    // Sort channels
                    int[] sortIds = McdProcessor.ChannelOrder(stream, channelOrderList);
                    int[] chanOrder = Enumerable.Range(0, nChannels).ToArray();
                    Array.Sort(sortIds, chanOrder); // Get the inverse indices

                    // Data as byte and short
                    short[] buffer = new short[nChannels]; // Reading buffer
                    short[] bufferChunk = new short[nChannels * 10000]; // Conversion buffer
                    byte[] bytebuffer = new byte[bufferChunk.Length * sizeof(short)]; // Writing buffer

                    long currBufferSize = 0; // How much of bufferChunk is filled
                    int nChunks = stream.Chunks.Count;
                    long chunkLen = stream.Chunks.Max(v => (long)v.ChunkLen - 16) / sizeof(short); // Largest size among all chunks (should all be identical)

                    // Iterate over all chunks
                    for (int chunkNum = 0; chunkNum < stream.Chunks.Count; chunkNum++)
                    {
                        Mcdfile.StreamChunk chunk = stream.Chunks[chunkNum];

                        // Read one sample for all channels at a time (for easier channel ordering)
                        int sampleNum = 0;
                        while (chunk.ReadSamples(buffer, 0, nChannels * sampleNum, nChannels) > 0)
                        {
                            // Order channels (chanOrder is read-only)
                            Array.Sort(Array.AsReadOnly(chanOrder).ToArray(), buffer);

                            // Instead of immediately converting and writing, copy into larger array
                            buffer.CopyTo(bufferChunk, currBufferSize);

                            currBufferSize += nChannels;
                            sampleNum += 1;
                        }

                        // Do not convert and write too often. Conversion is slightly faster with more data at once
                        if (currBufferSize + chunkLen > bufferChunk.Length || chunkNum >= nChunks - 1)
                        {
                            ConvertRange(bufferChunk, bytebuffer, (short)stream.Format.AdZero, unitsPerAd);
                            WriteBin(stream.isAnalog, bytebuffer, (int)currBufferSize * sizeof(short));

                            // Reset buffer
                            currBufferSize = 0;
                        }

                        // Count the number of samples written
                        if (!stream.isAnalog)
                            total_samples += sampleNum;

                        _progressUpdate((float)chunkNum / nChunks, stream.isAnalog ? "Analog" : "");
                    }
                }
            }

            return total_samples;
        }
    }
}

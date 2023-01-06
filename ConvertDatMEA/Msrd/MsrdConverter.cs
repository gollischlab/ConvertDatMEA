using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using Mcs.RawDataFileIO;
using Mcs.DataStream;


// Fernando Rozenblit, 2018

namespace ConvertDatMEA
{
    class MsrdConverter : DataConverter
    {
        public MsrdConverter(OutputFunction function, ProgressUpdate updater, BinaryWriter datWriter, string outPath, string[] channelOrder = null)
            : base(function, updater, datWriter, outPath, channelOrder) { }

        public override long ExtractData(string filepath, BinaryWriter auxWriter)
        {
            // Frame time file for this stimulus
            _auxWriter = auxWriter;

            // Number of total samples written (filtered data only)
            long total_samples = 0;

            // Give some indicator that it's loading
            _progressUpdate(0, "Loading");

            Reader fileReader = new Reader();
            fileReader.FileOpen(filepath);

            // Expect only one recording as guaranteed by FileProcessor::VerifyFiles
            var pair = fileReader.RecordingHdr.FirstOrDefault();
            var recordId = pair.Key;
            var header = pair.Value;

            // Notify if corrupt
            double corruptAt = double.PositiveInfinity;

            // Sort analog data and filtered data (analog data first)
            var streams = header.AnalogStreams.Where(v => (v.Value.DataSubType == enAnalogSubType.Auxiliary && v.Value.Label.Contains("Analog")));
            streams = streams.Concat(header.AnalogStreams.Where(v => (v.Value.DataSubType == enAnalogSubType.Electrode && v.Value.Label.Contains("Filter"))));

            // Get sampling rate from a random channel. Common sampling rate guaranteed by FileProcessor::VerifyFiles
            long oneSecond = 1000000;
            long sampleLen = streams.First().Value.Entities.FirstOrDefault().Tick; // Sample length in microseconds
            long samplesPerSec = oneSecond / sampleLen; // Samples per second

            // Iterate over analog and filtered data
            foreach (var analogStream in streams)
            {
                var analogInfo = analogStream.Value;
                var analogGuid = analogStream.Key;
                bool isAnalog = analogInfo.DataSubType == enAnalogSubType.Auxiliary;

                // Signal properties
                InfoChannel electrode = analogInfo.Entities.FirstOrDefault();
                int adzero = electrode.ADZero;
                double unitsPerAd = electrode.ConversionFactor * Math.Pow(10, electrode.Unit.Exponent);
                if (!isAnalog)
                    unitsPerAd *= 1e3; // Filtered stream is in microvolts

                // Channels of this stream
                var entitiesIDs = analogInfo.Entities.GetIDs();
                int nEntities = entitiesIDs.Count;

                // Sort channels by their label
                int[] sortIds = MsrdProcessor.ChannelOrder(analogInfo, channelOrderList);
                Dictionary<int, int> chanOrder = Enumerable.Range(0, nEntities).ToDictionary(x => sortIds[x], x => x);

                // Mystery solved: header.Duration is in microseconds, not samples*100
                long tF = header.Duration;
                long t0 = 0;
                long missingSamplesAll = 0;

                // Nchannel × Nsamples
                int[] buffer = new int[analogInfo.Entities.Count * samplesPerSec];
                byte[] bytebuffer = new byte[analogInfo.Entities.Count * samplesPerSec * sizeof(short)];

                // Iterate over one-second chunks
                while (t0 < tF && t0 < corruptAt)
                {
                    long tChunkSize = Math.Min(tF - t0, oneSecond); // Chunk size in microseconds
                    long fChunkSize = tChunkSize / sampleLen; // Chunk size in samples

                    // Hey, let's query data samples in time units but return sample units! Wow, nice idea!
                    var dataChunk = fileReader.GetChannelData<int>(recordId, analogGuid, sortIds, t0, t0 + tChunkSize);

                    // Abort safely and keep the data so far, if the data is corrupted
                    if (dataChunk.Any(v => v.Value.Count != 1)) {
                        corruptAt = t0;
                        break;
                    }

                    // Iterate over channels
                    foreach (var channelChunk in dataChunk)
                    {
                        // Some samples are missing in the beginning sometimes, why?
                        var chnk = channelChunk.Value.First();
                        long missingSamples = (chnk.Key - t0) / sampleLen;
                        int[] rawdata;

                        // Fill missing samples
                        if (missingSamples > 0)
                        {
                            // Pad with zeros for channels and pad with first value for analog
                            if (isAnalog)
                                rawdata = Enumerable.Repeat(chnk.Value[0], (int) (missingSamples+chnk.Value.Length)).ToArray();
                            else
                                rawdata = new int[missingSamples + chnk.Value.Length];
                            Array.Copy(chnk.Value, 0, rawdata, missingSamples, chnk.Value.Length); // Skip missing sample entries
                            missingSamplesAll += missingSamples;
                        }
                        else
                            rawdata = chnk.Value;

                        // Order the electrodes by channel map
                        int keyId = chanOrder[channelChunk.Key];

                        // Store the chunk in place at the Nchannels vs Nsamples matrix
                        for (int t = 0; t < rawdata.Length; t++)
                            buffer[nEntities * t + keyId] = rawdata[t];
                    }

                    ConvertRange(buffer, bytebuffer, adzero, unitsPerAd);

                    // Only write the portion that was read
                    int length = (int)(nEntities * fChunkSize * sizeof(short));
                    WriteBin(isAnalog, bytebuffer, length);

                    // Count the number of samples written
                    if (!isAnalog)
                        total_samples += fChunkSize;

                    _progressUpdate((float)t0 / tF, isAnalog ? "Analog" : "");

                    // Increment reading position
                    t0 += tChunkSize;
                }

                // Notify about missing samples
                if (missingSamplesAll > 0)
                {
                    string errorMsg = string.Format("Filled {0} (avg) missing {1} samples in {2} with {3}.",
                        missingSamplesAll / nEntities, isAnalog ? "analog" : "filtered",
                        Path.GetFileName(filepath), isAnalog ? "next found value" : "zeros");
                    FileProcessor.OutputError(errorMsg);
                }
            }

            // Incomplete conversion due to corrupt chunk
            if (corruptAt < double.PositiveInfinity)
            {
                string errorMsg = string.Format("Encountered corrupt data in {0}. Keeping the first {1} samples; missing {2} samples.",
                    Path.GetFileName(filepath), total_samples, (header.Duration - corruptAt) / sampleLen);
                FileProcessor.OutputError(errorMsg);

                // Write to file to not get lost
                File.WriteAllText(Path.Combine(outPath, Path.GetFileNameWithoutExtension(filepath) + "_incomplete.txt"), errorMsg);
            }

            fileReader.FileClose();
            return total_samples;
        }
    }
}

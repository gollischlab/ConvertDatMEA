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

            // Iterate over analog data and filtered data (analog data first)
            var streams = header.AnalogStreams.Where(v => (v.Value.DataSubType == enAnalogSubType.Auxiliary && v.Value.Label.Contains("Analog")));
            streams = streams.Concat(header.AnalogStreams.Where(v => (v.Value.DataSubType == enAnalogSubType.Electrode && v.Value.Label.Contains("Filter"))));
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

                long tF = header.Duration;
                long t0 = 0;
                long stride = 10000;

                // Nchannel × Nsamples
                int[] buffer = new int[analogInfo.Entities.Count * stride];
                byte[] bytebuffer = new byte[analogInfo.Entities.Count * stride * sizeof(short)];

                while (t0 * 100 < tF && t0 < corruptAt)
                {
                    // Someone should check this factor of *100. There's something odd about it...
                    long chunkSize = Math.Min(tF - t0 * 100, stride * 100);

                    var dataChunk = fileReader.GetChannelData<int>(recordId, analogGuid, entitiesIDs, t0 * 100, t0 * 100 + chunkSize);

                    // Abort safely and keep the data so far, if the data is corrupted
                    if (dataChunk.Any(v => v.Value.Count != 1)) {
                        corruptAt = t0;
                        break;
                    }

                    foreach (var channelChunk in dataChunk)
                    {
                        // ChannelChunk is weird. It has only one element.
                        var rawdata = channelChunk.Value.First().Value; // The key should always be t0 * 100, but I encountered a file where it wasn't

                        // Order the electrodes by channel map
                        int keyId = chanOrder[channelChunk.Key];

                        // Store the chunk in place at the Nchannels vs Nsamples matrix
                        for (int t = 0; t < Math.Min(stride, rawdata.Length); t++)
                            buffer[nEntities * t + keyId] = rawdata[t];
                    }

                    ConvertRange(buffer, bytebuffer, adzero, unitsPerAd);

                    // Only write the portion that was read
                    int length = (int)(nEntities * chunkSize / 100 * sizeof(short));
                    WriteBin(isAnalog, bytebuffer, length);

                    // Count the number of samples written
                    if (!isAnalog)
                        total_samples += chunkSize / 100;

                    _progressUpdate((float)t0 * 100 / tF, isAnalog ? "Analog" : "");

                    t0 += stride;
                }
            };

            // Incomplete conversion due to corrupt chunk
            if (corruptAt < double.PositiveInfinity)
            {
                string errorMsg = string.Format("Encountered corrupt data in {0}. Keeping the first {1} samples.", Path.GetFileName(filepath), total_samples);
                FileProcessor.OutputError(errorMsg);

                // Write to file to not get lost
                File.WriteAllText(Path.Combine(outPath, Path.GetFileNameWithoutExtension(filepath) + "_incomplete.txt"), errorMsg);
            }

            fileReader.FileClose();
            return total_samples;
        }
    }
}

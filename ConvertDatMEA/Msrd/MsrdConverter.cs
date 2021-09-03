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
        public MsrdConverter(OutputFunction function, ProgressUpdate updater, BinaryWriter datWriter, string[] channelOrder = null) : base(function, updater, datWriter, channelOrder) { }

        public override long ExtractData(string filepath, BinaryWriter auxWriter)
        {
            // Frame time file for this stimulus
            _auxWriter = auxWriter;

            // Number of total samples written (filtered data only)
            long total_samples = 0;

            Reader fileReader = new Reader();
            fileReader.FileOpen(filepath);

            // Expect only one recording as guaranteed by FileProcessor::VerifyFiles
            var pair = fileReader.RecordingHdr.FirstOrDefault();
            var recordId = pair.Key;
            var header = pair.Value;

            // Iterate over analog data and filtered data
            foreach (var analogStream in header.AnalogStreams.Where(v =>
                   (v.Value.DataSubType == enAnalogSubType.Auxiliary && v.Value.Label.Contains("Analog"))
                || (v.Value.DataSubType == enAnalogSubType.Electrode && v.Value.Label.Contains("Filter"))
            ))
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

                while (t0 * 100 < tF)
                {
                    // Someone should check this factor of *100. There's something odd about it...
                    long chunkSize = Math.Min(tF - t0 * 100, stride * 100);

                    var dataChunk = fileReader.GetChannelData<int>(recordId, analogGuid, entitiesIDs, t0 * 100, t0 * 100 + chunkSize);

                    foreach (var channelChunk in dataChunk)
                    {
                        // ChannelChunk is weird. It has only one element.
                        var rawdata = channelChunk.Value[t0 * 100];

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

            fileReader.FileClose();
            return total_samples;
        }
    }
}

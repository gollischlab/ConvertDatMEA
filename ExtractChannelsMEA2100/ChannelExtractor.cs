using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using System.IO;
using Mcs.RawDataFileIO;
using Mcs.DataStream;


// Fernando Rozenblit, 2018

namespace ExtractChannels2
{
    class ChannelExtractor
    {
        const int amplifierMinVolt = -4096; //mv
        const int amplifierMaxVolt = 4096; //mv

        const int auxiliaryGain = 1;
        const int electrodeGain = 1100; // MEA-256 gain

        public delegate void OutputFunction(string line);
        public delegate void ProgressUpdate(double percent, int stimulusId);

        OutputFunction _outputFunction = null;
        ProgressUpdate _progressUpdate = null;

        private void OutputLine(string line)
        {
            _outputFunction(line);
        }

        public ChannelExtractor(OutputFunction function, ProgressUpdate updater)
        {
            _outputFunction = function;
            _progressUpdate = updater;
        }

        private byte[] ConvertRange(int[] data, InfoChannel electrode, int gain = 1)
        {
            byte[] newData = new byte[sizeof(ushort) * data.Count()];
            ConvertRange(data, electrode, gain, newData);

            return newData;
        }

        private static void ConvertRange(int[] data, InfoChannel electrode, int gain, byte[] newData)
        {
            if (2 * data.Count() != newData.Count())
                throw new ArgumentException("Data and newData must be the same size");

            double voltToSample = (1 << 16) / (amplifierMaxVolt - amplifierMinVolt); // bin files have a 16-bit range
            double electrodeConvFactor = electrode.ConversionFactor * 1e-12 * 1e3 * gain;
            double adzero = electrode.ADZero;

            int dataCount = data.Count();
            Parallel.For(0, dataCount, i =>
           {
               double valueInMilliVolts = (data[i] - adzero) * electrodeConvFactor; // the exponent -12 is magic? 
               valueInMilliVolts = (valueInMilliVolts - amplifierMinVolt) * voltToSample; // Convert to 16-bit sample centered around 0

               byte[] inBytes = BitConverter.GetBytes((ushort)valueInMilliVolts);
               newData[2 * i] = inBytes[0];
               newData[2 * i + 1] = inBytes[1];
           });
        }


        private void WriteBin(string filePath, byte[] data)
        {
            using (BinaryWriter writer = new BinaryWriter(File.Open(filePath, FileMode.CreateNew)))
            {
                writer.Write((UInt32)(data.Count() / 2));
                writer.Write((UInt32)2);
                writer.Write((UInt32)3);
                writer.Write((UInt32)4);

                writer.Write(data);
            }
        }

        /*
        private void WriteBin(string filePath, ushort[] data)
        {
            using (MemoryStream memstream = new MemoryStream(4 * 4 + 2 * data.Count()))
            {                // Header: [number of samples | 2 | 3 | 4] (4 bytes each)
                using (BinaryWriter writer = new BinaryWriter(memstream))
                {
                    writer.Write((UInt32)data.Count());
                    writer.Write((UInt32)2);
                    writer.Write((UInt32)3);
                    writer.Write((UInt32)4);
                    
                    // Data points are stored side-by-side, 2 bytes each
                    foreach (ushort datapoint in data)
                    {
                        writer.Write(datapoint);
                    }

                    WriteBin(filePath, memstream.ToArray());
                }
            }
        }
        */
        private int GetBinID(InfoStreamAnalog streaminfo, InfoChannel electrode)
        {
            switch (streaminfo.DataSubType)
            {
                case enAnalogSubType.Electrode:
                    return electrode.ID - 1;

                case enAnalogSubType.Auxiliary:
                    if (Int32.TryParse(electrode.Label, out int auxnumber))
                    {
                        return 252 + auxnumber;
                    }
                    else
                    {
                        return electrode.ID;
                    }

                default:
                    return 0xBEEF;
            }
        }

        private string GetBinPath(int stimulusId, int binId, string root)
        {
            return String.Format("{0}{1}_{2}.bin", root, stimulusId, binId);
        }

        public void ExtractBins(string filepath, int stimulusId)
        {
                Reader fileReader = new Reader();
                fileReader.FileOpen(filepath);
                string rootPath = Path.GetDirectoryName(filepath);
                rootPath = Path.Combine(rootPath, "RawChannels/");
                Directory.CreateDirectory(rootPath);


                foreach (int recordId in fileReader.Recordings)
                {
                    OutputLine(String.Format("Recording {0}\r\n", recordId));
                    var header = fileReader.RecordingHdr[recordId];


                    // Count the total number of channels
                    int totalChannels = 0;
                    foreach (var analogStream in header.AnalogStreams)
                        totalChannels += analogStream.Value.Entities.Count();

                    int processedChannels = 0;
                    foreach (var analogStream in header.AnalogStreams)
                    {
                        var analogInfo = analogStream.Value;
                        var analogGuid = analogStream.Key;
                        int signalGain = GetGain(analogInfo, false);

                        OutputLine(String.Format("{0}\r\n", analogInfo.Label));
                        OutputLine(String.Format("{0}\r\n", analogGuid));
                        OutputLine(String.Format("{0}\r\n", analogInfo.DataSubType));
                        OutputLine("------------------------\r\n");


                        //byte[] outputData = null; // placeholder for converted data

                        foreach (var electrode in analogInfo.Entities)
                        {
                            var data = fileReader.GetChannelData<int>(recordId, analogGuid, electrode.ID)[0];

                            int binId = GetBinID(analogInfo, electrode);
                            string binPath = GetBinPath(stimulusId, binId, rootPath);
                            byte[] outputData = ConvertRange(data, electrode, signalGain);

                            WriteBin(binPath, ConvertRange(data, electrode, signalGain));
                            DisplayProgress(electrode, stimulusId);
                            processedChannels += 1;
                            _progressUpdate((float)processedChannels / totalChannels, stimulusId);
                        }
                    }
                }
                fileReader.FileClose();
        }

        // Gets the amplifier gain (inverts electrode gain if needed)
        private static int GetGain(InfoStreamAnalog analogInfo, bool invertSignal = false)
        {
            switch (analogInfo.DataSubType)
            {
                case enAnalogSubType.Electrode:
                    return invertSignal ? -electrodeGain : electrodeGain;
                case enAnalogSubType.Auxiliary:
                    return auxiliaryGain;
                default:
                    return 1;
            }
        }

        private void DisplayProgress(InfoChannel electrode, int stimulusId)
        {
            OutputLine(String.Format("{2} - {0} ({1})\r\n", electrode.ID, electrode.Label, stimulusId));
        }
    }
}

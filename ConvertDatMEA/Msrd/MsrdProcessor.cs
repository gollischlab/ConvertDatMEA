using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using Mcs.DataStream;
using Mcs.RawDataFileIO;

namespace ConvertDatMEA
{
    public class MsrdProcessor : FileProcessor
    {
        protected override string fileExt {  get { return ".MSRD"; } }
        private readonly Reader fileReader = new Reader();
        private static readonly Regex rgx = new Regex("([a-zA-Z]?)(\\d+)");

        public MsrdProcessor(List<string> filelist) : base(filelist) { }

        protected override void CheckFileFormat(string file)
        {
            // This will throw an exception on its own
            fileReader.FileOpen(file);

            try
            {
                // Check for exactly one recording
                if (fileReader.Recordings.Count != 1)
                    throw new InvalidFileFormatException("File contains no or more than one recording");
                var header = fileReader.RecordingHdr.FirstOrDefault().Value;

                // Check for exactly one analog stream
                var streams = header.AnalogStreams.Where(v => v.Value.DataSubType == enAnalogSubType.Auxiliary && v.Value.Label.Contains("Analog"));
                if (streams.Count() != 1)
                    throw new InvalidFileFormatException("File contains no or more than one analog stream");

                // Retrieve sampling rate from a random analog channel
                long tick = 0;
                var electrode = streams.First().Value.Entities.FirstOrDefault();
                if (electrode != null)
                    tick = electrode.Tick;

                // Retrieve voltage conversion factor (mV per unit) from a random analog channel
                Dictionary<string, double> factor = new Dictionary<string, double>();
                factor["analog"] = (1 << electrode.ADCBits) * electrode.ConversionFactor * Math.Pow(10, electrode.Unit.Exponent) / (1 << 16) * 1e3;

                // Check for exactly one filtered stream
                streams = header.AnalogStreams.Where(v => v.Value.DataSubType == enAnalogSubType.Electrode && v.Value.Label.Contains("Filter"));
                if (streams.Count() != 1)
                    throw new InvalidFileFormatException("File contains no or more than one filtered stream");

                // Check for common sampling rate
                if (streams.First().Value.Entities.Any(v => v.Tick != tick))
                    throw new DataMisalignedException("Sampling rate does not match across channels");

                // Retrieve voltage conversion factor (mV per unit) from a random electrode channel
                electrode = streams.First().Value.Entities.FirstOrDefault();
                factor["filter"] = (1 << electrode.ADCBits) * electrode.ConversionFactor * Math.Pow(10, electrode.Unit.Exponent) / (1 << 16) * 1e3;

                // Check for matching sampling rate
                long fs = 1000000 / tick; // From microseconds to Hz
                if (samplingRate == -1)
                    samplingRate = fs;
                else if (samplingRate != fs)
                    throw new DataMisalignedException("Sampling rate does not match across files");

                // Check for matching channel number
                int nCh = streams.First().Value.Entities.Count;
                if (numChannels == -1)
                    numChannels = nCh;
                else if (numChannels != nCh)
                    throw new DataMisalignedException("Number of channels does not match across files");

                // Check for matching conversion factors
                foreach (string typeName in factor.Keys)
                    if (double.IsNaN(conversionFactor[typeName]))
                        conversionFactor[typeName] = factor[typeName];
                    else if (conversionFactor[typeName] != factor[typeName])
                        throw new DataMisalignedException(string.Format("Conversion factor ({0}) does not match across files", typeName));
            }
            catch (Exception)
            {
                // This construct ensures the the file will be closed properly (see below)
                throw;
            }
            finally
            {
                fileReader.FileClose();
            }
        }

        public static int[] ChannelOrder(InfoStreamAnalog stream)
        {
            var channelIds = stream.Entities.GetIDs();
            int nChannels = channelIds.Count();
            string[] sortNames = new string[nChannels];
            int[] sortIds = new int[nChannels];
            for (int i = 0; i < nChannels; i++)
            {
                int id = channelIds[i];
                Match match = rgx.Match(stream.Entities[id].Label);
                string letter = match.Groups[1].Value;
                int.TryParse(match.Groups[2].Value, out int num);
                sortNames[i] = string.Format("{0}{1:00}", letter, num);
                sortIds[i] = id;
            }
            Array.Sort(sortNames, sortIds);
            return sortIds;
        }

        protected override void PrintMetadataFile(string file)
        {
            fileReader.FileOpen(file);
            var header = fileReader.RecordingHdr.FirstOrDefault().Value;

            // Voltage conversion
            string[] factor = new string[3] {
                conversionFactor["analog"].ToString("g4").PadRight(4+1+4),
                conversionFactor["filter"].ToString("g4").PadRight(4+1+4),
                (1 / DataConverter.voltToSample).ToString("g4").PadRight(4+1+4)
            };

            // Count the total number of channels in recording (analog + electrodes, for example)
            List<int> numChannels = new List<int>();
            foreach (var analogStream in header.AnalogStreams)
                numChannels.Add(analogStream.Value.Entities.Count);

            Console.WriteLine("Channels:       {0} ({1})", numChannels.Sum(), string.Join(" + ", numChannels));
            Console.WriteLine("Duration:       {0:0.###} sec", header.Duration / 1000 / 1000);
            Console.WriteLine("Sampling rate:  {0} Hz", samplingRate);
            Console.WriteLine("Voltage range:  {0,9:±0.###} mV, factor {1} ({2})", conversionFactor["analog"] * (1 << 15), factor[0], "analog");
            Console.WriteLine("                {0,9:±0.###} mV, factor {1} ({2})", conversionFactor["filter"] * (1 << 15), factor[1], "electrode no gain");
            Console.WriteLine("DAT range (16): {0,9:±0.###} mV, factor {1} ({2})", 1 / DataConverter.voltToSample * (1 << 15), factor[2], "analog");
            Console.WriteLine("                {0,9:±0.###} μV, factor {1} ({2})", 1 / DataConverter.voltToSample * (1 << 15), factor[2], "electrode no gain");
            Console.WriteLine("Electrode gain: {0:~0.####}", conversionFactor["analog"] / conversionFactor["filter"]);

            short i = 0;
            foreach (var analogStream in header.AnalogStreams)
                Console.WriteLine("Stream {0}:       {1} ({2})", i++, analogStream.Value.Label, analogStream.Value.DataSubType);

            Console.Write("Channel order:  ");
            InfoStreamAnalog stream = header.AnalogStreams.Where(v => v.Value.Label.Contains("Filter")).FirstOrDefault().Value;
            int[] channels = ChannelOrder(stream);
            foreach (int channel in channels)
            {
                Console.Write("{0,3} ", stream.Entities[channel].Label);
                if (Console.CursorLeft + 4 > Console.WindowWidth)
                {
                    Console.WriteLine();
                    Console.Write(new string(' ', 16));
                }
            }
            Console.WriteLine();
        }
    }
}

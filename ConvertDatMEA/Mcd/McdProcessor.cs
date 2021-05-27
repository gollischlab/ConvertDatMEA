﻿using System;
using System.Collections.Generic;
using System.Linq;

namespace ConvertDatMEA
{
    class McdProcessor : FileProcessor
    {
        protected override string fileExt { get { return ".MCD"; } }

        public McdProcessor(List<string> filelist) : base(filelist) { }

        protected override void CheckFileFormat(string file)
        {
            using (McdReader fileReader = new McdReader(file))
            {
                // Check for exactly two streams
                if (fileReader.StreamCount != 2)
                    throw new InvalidFileFormatException("File does not contain two streams");

                // Check for one filtered and one analog stream
                if (!fileReader.Streams.ContainsKey("filt0001") || !fileReader.Streams.ContainsKey("anlg0001"))
                    throw new InvalidFileFormatException("File does not contain both filtered and analog streams");

                // Retrieve sampling rate
                long fs = fileReader.SamplingRate;

                // Check for common sampling rate
                if (fileReader.Streams.Any(v => v.Value.Header.Millisamplespersecond / 1000 != fs))
                    throw new DataMisalignedException("Sampling rate does not match across streams");

                // Check for matching sampling rate
                if (samplingRate == -1)
                    samplingRate = fs;
                else if (samplingRate != fs)
                    throw new DataMisalignedException("Sampling rate does not match across files");

                // Check for matching channel number
                int nCh = fileReader.ElectrodeChannels;
                if (numChannels == -1)
                    numChannels = nCh;
                else if (numChannels != nCh)
                    throw new DataMisalignedException("Number of channels does not match across files");

                // Retrieve voltage conversion factor (mV per unit)
                Dictionary<string, double> factor = new Dictionary<string, double>();
                foreach (Stream stream in fileReader.Streams.Values)
                    factor[stream.Header.TypeName] = (1 << stream.Format.AdBits) * stream.Format.UnitsPerAd / (1 << 16) * 1e3;

                // Check for matching conversion factors
                foreach (string typeName in factor.Keys)
                    if (double.IsNaN(conversionFactor[typeName]))
                        conversionFactor[typeName] = factor[typeName];
                    else if (conversionFactor[typeName] != factor[typeName])
                        throw new DataMisalignedException(string.Format("Conversion factor ({0}) does not match across files", typeName));
            }
        }

        public static Mcdfile.ChannelInfo[] ChannelOrder(Stream stream)
        {
            int nChannels = (int)stream.Header.ChannelCount;
            List<Mcdfile.ChannelInfo> localCopy = stream.Channels.GetRange(0, nChannels);
            localCopy.Sort(delegate (Mcdfile.ChannelInfo a, Mcdfile.ChannelInfo b)
            {
                if (a.PosY == b.PosY)
                    return ((ushort)a.PosX).CompareTo((ushort)b.PosX);
                else
                    return ((ushort)a.PosY).CompareTo((ushort)b.PosY);
            });
            return localCopy.ToArray();
        }

        protected override void PrintMetadataFile(string file)
        {
            using (McdReader fileReader = new McdReader(file))
            {
                string[] factor = new string[3] {
                    conversionFactor["analog"].ToString("g4").PadRight(4+1+4),
                    conversionFactor["filter"].ToString("g4").PadRight(4+1+4),
                    (1 / DataConverter.voltToSample).ToString("g4").PadRight(4+1+4)
                };

                Console.WriteLine("Channels:       {0} ({1} + {2})", fileReader.TotalChannels, fileReader.ElectrodeChannels, fileReader.AnalogChannels);
                Console.WriteLine("Duration:       {0:0.###} sec", fileReader.Duration);
                Console.WriteLine("Sampling rate:  {0} Hz", fileReader.SamplingRate);
                Console.WriteLine("Voltage range:  {0,9:±0.###} mV, factor {1} ({2})", conversionFactor["analog"] * (1 << 15), factor[0], "analog");
                Console.WriteLine("                {0,9:±0.###} mV, factor {1} ({2})", conversionFactor["filter"] * (1 << 15), factor[1], "electrode no gain");
                Console.WriteLine("DAT range (16): {0,9:±0.###} mV, factor {1} ({2})", 1 / DataConverter.voltToSample * (1 << 15), factor[2], "analog");
                Console.WriteLine("                {0,9:±0.###} μV, factor {1} ({2})", 1 / DataConverter.voltToSample * (1 << 15), factor[2], "electrode no gain");
                Console.WriteLine("Electrode gain: {0}", fileReader.Streams["filt0001"].Channels.FirstOrDefault().Gain / 1000);

                short i = 0;
                foreach (Stream stream in fileReader.Streams.Values)
                    Console.WriteLine("Stream {0}:       {1} '{2}' ({3})", i++, stream.Header.StreamName, stream.Header.BufferId, stream.Header.TypeName);

                Console.Write("Channel order:  ");
                Mcdfile.ChannelInfo[] channels = ChannelOrder(fileReader.Streams["filt0001"]);
                foreach(Mcdfile.ChannelInfo channel in channels)
                {
                    Console.Write("{0,2} ", channel.DecoratedName);
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
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using MCStream;

namespace ConvertDatMEA
{
    class McdProcessor : FileProcessor
    {
        protected override string fileExt { get { return ".MCD"; } }
        private readonly MCSSTRM fileReader = new MCSSTRM();
        private static readonly byte[] headerbytes = new byte[] {
            77, 67, 83, 83, 84, 82, 77, 32, 255, 255, 255, 255, 255, 255, 255, 255, 76, 73, 83, 84, 104, 100, 114, 32
        };

        public McdProcessor(List<string> filelist) : base(filelist) { }

        protected override void CheckFileFormat(string file)
        {
            // MCStream does not have any error handling. Check file's integrity manually first
            using (BinaryReader preReader = new BinaryReader(File.Open(file, FileMode.Open, FileAccess.Read, FileShare.Read)))
            {
                if (!headerbytes.SequenceEqual(preReader.ReadBytes(24)))
                {
                    throw new InvalidFileFormatException("File is not a valid MCD file");
                }
            }

            fileReader.OpenFile(file);

            try
            {
                // Check for exactly two streams
                if (fileReader.StreamCount != 2)
                    throw new InvalidFileFormatException("File does not contain two streams");

                // Check for one filtered and one analog stream
                string[] streams = new string[] { fileReader.GetStream(0).DataType, fileReader.GetStream(1).DataType };
                if (!streams.Contains("filter") || !streams.Contains("analog"))
                    throw new InvalidFileFormatException("File does not contain both filtered and analog streams");

                // Retrieve sampling rate
                long fs = fileReader.MillisamplesPerSecond / 1000;

                // Check for common sampling rate
                if (fileReader.GetStream(0).GetSampleRate() != fs || fileReader.GetStream(1).GetSampleRate() != fs)
                    throw new DataMisalignedException("Sampling rate does not match across streams");

                // Check for matching sampling rate
                if (samplingRate == -1)
                    samplingRate = fs;
                else if (samplingRate != fs)
                    throw new DataMisalignedException("Sampling rate does not match across files");

                // Check for matching channel number
                int nCh = fileReader.GetElectrodeChannels();
                if (numChannels == -1)
                    numChannels = nCh;
                else if (numChannels != nCh)
                    throw new DataMisalignedException("Number of channels does not match across files");
            }
            catch (Exception)
            {
                // This construct ensures the the file will be closed properly (see below)
                throw;
            }
            finally
            {
                fileReader.CloseFile();
            }
        }

        protected override void PrintMetadataFile(string file)
        {
            fileReader.OpenFile(file);

            int numElec = fileReader.GetElectrodeChannels();
            int numAnalog = fileReader.GetAnalogChannels();
            int numTotal = fileReader.GetTotalChannels();
            float durationMs = fileReader.GetStopTime().GetSecondFromStart() * 1000 + fileReader.GetStopTime().GetMillisecondFromStart();
            float samplingRate = fileReader.MillisamplesPerSecond / 1000;
            float amplifierGain = fileReader.GetLayout().GetChannelLayout(fileReader.GetLayout().LayoutType).Gain;
            float voltRange = fileReader.GetVoltageRange();

            Console.WriteLine("Channels:       {0} ({1} + {2})", numTotal, numElec, numAnalog);
            Console.WriteLine("Duration:       {0} sec", durationMs / 1000);
            Console.WriteLine("Sampling rate:  {0} Hz", samplingRate);
            Console.WriteLine("Amplifier gain: {0}", amplifierGain);
            Console.WriteLine("Voltage range:  {0}", voltRange);

            for (int i = 0; i < fileReader.StreamCount; i++)
            {
                MCSSTREAM stream = fileReader.GetStream(i);
                Console.WriteLine("Stream {0}:       {1} '{2}' ({3})", i, stream.Name, stream.GetBufferID(), stream.DataType);
            }

            fileReader.CloseFile();
        }
    }
}

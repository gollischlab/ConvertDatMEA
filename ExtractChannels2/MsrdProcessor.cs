using System;
using System.Collections.Generic;
using System.Linq;
using Mcs.DataStream;
using Mcs.RawDataFileIO;

namespace ExtractChannels2
{
    public class MsrdProcessor : FileProcessor
    {
        protected override string fileExt {  get { return ".MSRD"; } }
        private readonly Reader fileReader = new Reader();

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

                // Check for exactly one filtered stream
                streams = header.AnalogStreams.Where(v => v.Value.DataSubType == enAnalogSubType.Electrode && v.Value.Label.Contains("Filter"));
                if (streams.Count() != 1)
                    throw new InvalidFileFormatException("File contains no or more than one filtered stream");

                // Check for common sampling rate
                if (streams.First().Value.Entities.Any(v => v.Tick != tick))
                    throw new DataMisalignedException("Sampling rate does not match across channels");

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

        protected override void PrintMetadataFile(string file)
        {
            fileReader.FileOpen(file);

            int numRec = fileReader.Recordings.Count;
            if (numRec > 1)
            {
                Console.WriteLine("Number of recordings: {0}\r\n", numRec);
            }

            foreach (int recordId in fileReader.Recordings)
            {
                if (numRec > 1)
                    Console.WriteLine("Recording {0}", recordId);

                var header = fileReader.RecordingHdr[recordId];

                // Count the total number of channels in recording (analog + electrodes, for example)
                List<int> numChannels = new List<int>();
                foreach (var analogStream in header.AnalogStreams)
                    numChannels.Add(analogStream.Value.Entities.Count);

                Console.WriteLine("Channels:       {0} ({1})", numChannels.Sum(), string.Join(" + ", numChannels));
                Console.WriteLine("Duration:       {0} sec", header.Duration / 1000 / 1000);
                Console.WriteLine("Sampling rate:  {0} Hz", samplingRate);

                // Loop over each recorded stream (analog data, filtered data, raw data, etc...)
                short i = 0;
                foreach (var analogStream in header.AnalogStreams)
                {
                    Console.WriteLine("Stream {0}:       {1} ({2})", i, analogStream.Value.Label, analogStream.Value.DataSubType);
                    i += 1;
                }

                Console.WriteLine();
            }
        }
    }
}

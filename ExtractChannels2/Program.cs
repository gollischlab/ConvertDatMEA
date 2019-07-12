using System;

using MC_StreamNetLib;
using Mcs.RawDataFileIO;


namespace ExtractChannels2
{

    class Program
    {
        static void Main(string[] args)
        {
            ExtractArguments.DebugPrintArgs(args);

            ExtractArguments arguments = new ExtractArguments(args);

            ChannelExtractor extractor = new ChannelExtractor(OutputFunction, ProgressUpdate);

            Console.WriteLine("----");
            Console.WriteLine("MetadataOnly: {0}", arguments.onlyMetadata);

            for (int stimIdx = 0; stimIdx < arguments.files.Count; stimIdx++)
            {
                string file = arguments.files[stimIdx];

                //TODO: check if file exists

                Console.WriteLine("- {0}", file);
                try
                {
                    extractor.ExtractBins(file, stimIdx);
                }
                catch (ExcFileIO)
                {
                    Console.WriteLine("Wrong file format");
                }
            }


            Console.ReadLine();
        }

        private static void ProgressUpdate(double percent, int stimulusId)
        {
            int originalTop = Console.CursorTop;
            int originalLeft = Console.CursorLeft;

            Console.SetCursorPosition(0, Math.Max(Console.CursorTop, Console.WindowHeight));
            Console.WriteLine("{0}: {1:##.##%}", stimulusId, percent);
            Console.SetCursorPosition(originalLeft, originalTop);
        }

        private static void OutputFunction(string line)
        {
            Console.WriteLine(line);
        }
    }
}

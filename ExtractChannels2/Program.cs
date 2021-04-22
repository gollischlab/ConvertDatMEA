using System;

namespace ExtractChannels2
{

    class Program
    {
        static void Main(string[] args)
        {
            ExtractArguments arguments = new ExtractArguments(args);

#if DEBUG
            ExtractArguments.DebugPrintArgs(args);
            Console.WriteLine("----");
            Console.WriteLine("MetadataOnly: {0}", arguments.onlyMetadata);
            Console.WriteLine();
#endif

            FileProcessor files = new FileProcessor(arguments.files);

            if (arguments.onlyMetadata)
                files.PrintMetadata();
            else
                files.Convert();

            // Remove this to close the programm when done
            Console.ReadLine();
        }
    }
}

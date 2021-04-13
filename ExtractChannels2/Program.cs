using System;

namespace ExtractChannels2
{

    class Program
    {
        static void Main(string[] args)
        {
            ExtractArguments.DebugPrintArgs(args);

            ExtractArguments arguments = new ExtractArguments(args);

            Console.WriteLine("----");
            Console.WriteLine("MetadataOnly: {0}", arguments.onlyMetadata);
            Console.WriteLine();

            FileProcessor proc = new FileProcessor(arguments.files, arguments.onlyMetadata);

            Console.ReadLine();
        }
    }
}

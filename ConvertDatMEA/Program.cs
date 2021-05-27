using System;

namespace ConvertDatMEA
{

    class Program
    {
        static void Main(string[] args)
        {
            Console.SetWindowSize(Math.Min(Console.LargestWindowWidth, 85), Math.Min(Console.LargestWindowHeight, 40));
            Console.Title = "ConvertDatMEA";

            ExtractArguments arguments = new ExtractArguments(args);

#if DEBUG
            ExtractArguments.DebugPrintArgs(args);
            Console.WriteLine("----");
            Console.WriteLine("MetadataOnly: {0}", arguments.onlyMetadata);
            Console.WriteLine("NoWait: {0}", arguments.noWait);
            Console.WriteLine();
#endif

            if (arguments.files.Count == 0)
                return;

            FileProcessor files = FileProcessor.Create(arguments.files);

            if (files != null)
                if (arguments.onlyMetadata)
                    files.PrintMetadata();
                else
                    files.Convert();

            if (!arguments.noWait)
                Console.ReadLine();
        }
    }
}

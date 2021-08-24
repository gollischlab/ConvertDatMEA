using System;

namespace ConvertDatMEA
{

    class Program
    {
        static void Main(string[] args)
        {
            ExtractArguments arguments = new ExtractArguments(args);

            Console.SetWindowSize(Math.Min(Console.LargestWindowWidth, arguments.windowWidth), Math.Min(Console.LargestWindowHeight, arguments.windowHeight));
            Console.SetBufferSize(Console.WindowWidth, Console.BufferHeight);
            Console.Title = "ConvertDatMEA";

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

            if (!arguments.noWait || !files.success)
                Console.ReadLine();

            // Proper exit code
            Environment.Exit(files.success ? 0 : 1);
        }
    }
}

using System;
using System.IO;

namespace ConvertDatMEA
{
    class Program
    {
        public static ConsoleCopy logger;

        static void Main(string[] args)
        {
            ExtractArguments arguments = new ExtractArguments(args);

            try
            {
                Console.SetWindowSize(Math.Min(Console.LargestWindowWidth, arguments.windowWidth), Math.Min(Console.LargestWindowHeight, arguments.windowHeight));
                Console.SetBufferSize(Console.WindowWidth, Console.BufferHeight);
            }
            catch (NotImplementedException)
            {
                Console.ForegroundColor = ConsoleColor.DarkGray;
                Console.WriteLine("WARNING: Console.SetWindowSize not supported");
                Console.ResetColor();
            }
            string windowTitle = Console.Title;
            bool cursor = Console.CursorVisible;
            Console.Title = "ConvertDatMEA";

#if DEBUG
            ExtractArguments.DebugPrintArgs(args);
            Console.WriteLine("----");
            Console.WriteLine("MetadataOnly: {0}", arguments.onlyMetadata);
            Console.WriteLine("NoWait: {0}", arguments.noWait);
            if (arguments.channelOrder)
                Console.WriteLine("ChannelOrder: {0}", arguments.channelFile);
            else
                Console.WriteLine("ChannelOrder: {0}", arguments.channelOrder);
            Console.WriteLine();
#endif

            bool success = true;
            if (arguments.initialized && arguments.files.Count > 0)
            {
                FileProcessor files = FileProcessor.Create(arguments.files);

                if (files != null && files.verified)
                {
                    using (logger = new ConsoleCopy(Path.Combine(files.OutputPath, "conversion_output_logger.txt"), disable: arguments.onlyMetadata))
                    {
                        if (arguments.channelOrder)
                            files.SetChannelOrder(arguments.channelFile);

                        if (arguments.onlyMetadata)
                            files.PrintMetadata();
                        else
                            files.Convert();
                    }

                    success = files.success;
                }
                else
                {
                    success = false;
                }
            }

            // If noWait and stderr is redirected, exit even when there was an error
            if (!arguments.noWait || (!success && !Console.IsErrorRedirected))
                Console.ReadLine();

            // Reset console
            Console.Title = windowTitle;
            Console.CursorVisible = cursor;

            // Proper exit code
            Environment.Exit(success ? 0 : 1);
        }
    }
}

using System;
using System.Collections.Generic;

namespace ConvertDatMEA
{
    class ExtractArguments
    {
        public readonly List<string> files;
        public readonly bool initialized = false;
        public readonly bool onlyMetadata = false;
        public readonly bool noWait = false;
        public readonly int windowWidth = 150;
        public readonly int windowHeight = 40;

        public static void DebugPrintArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine("{0}: {1}", i, args[i]);
            }
        }

        public ExtractArguments(string[] args)
        {
            if (args.Length == 0)
                return;

            files = new List<string>();

            foreach (var arg in args)
            {
                if (arg[0] == '-')
                {
                    if (arg == "-metadata")
                        onlyMetadata = true;
                    else if (arg == "-nowait")
                        noWait = true;
                    else if (arg.StartsWith("-w"))
                        Int32.TryParse(arg.Substring(2), out windowWidth);
                    else if (arg.StartsWith("-h"))
                        Int32.TryParse(arg.Substring(2), out windowHeight);
                    continue;
                }

                files.Add(arg);               
            }
            
            initialized = true;
        }
    }
}

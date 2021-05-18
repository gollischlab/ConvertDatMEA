using System;
using System.Collections.Generic;

namespace ConvertDatMEA
{
    class ExtractArguments
    {
        public readonly List<string> files;
        public readonly bool initialized = false;
        public readonly bool onlyMetadata = false;

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
                    if (arg == "-metadata") { onlyMetadata = true; }
                    continue;
                }

                files.Add(arg);               
            }
            
            initialized = true;
        }
    }
}

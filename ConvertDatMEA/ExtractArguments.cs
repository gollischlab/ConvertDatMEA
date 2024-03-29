﻿using System;
using System.Collections.Generic;
using System.IO;

namespace ConvertDatMEA
{
    class ExtractArguments
    {
        public readonly List<string> files;
        public readonly bool initialized = false;
        public readonly bool onlyMetadata = false;
        public readonly bool noWait = false;
        public readonly bool channelOrder = false;
        public readonly string channelFile = null;
        public readonly int windowWidth = 150;
        public readonly int windowHeight = 40;

        public static void DebugPrintArgs(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                Console.WriteLine("{0}: {1}", i, args[i]);
            }
        }

        private static void Usage()
        {
            Console.WriteLine("");
            Console.WriteLine("{0} [options] \"filepath1\" [\"filepath2\" [...]]", Path.GetFileName(System.Reflection.Assembly.GetEntryAssembly().Location));
            Console.WriteLine("");
            Console.WriteLine("  Options");
            Console.WriteLine("  -------");
            Console.WriteLine("    -help                      Show this information");
            Console.WriteLine("    -metadata                  Show meta data of all files without processing");
            Console.WriteLine("    -nowait                    Close application when done");
            Console.WriteLine("    -wN                        Window width. N is the number of columns");
            Console.WriteLine("    -hN                        Window height. N is the number of rows");
            Console.WriteLine("    -channelorder \"file.txt\"   Specify a custom channel order in a textfile");
            Console.WriteLine("");
        }

        public ExtractArguments(string[] args)
        {
            if (args.Length == 0)
            {
                Usage();
                return;
            }

            files = new List<string>();

            foreach (var arg in args)
            {
                if (arg[0] == '-')
                {
                    string argL = arg.ToLower();
                    if (argL == "-help")
                    {
                        Usage();
                        return;
                    }
                    else if (argL == "-metadata")
                        onlyMetadata = true;
                    else if (argL == "-nowait")
                        noWait = true;
                    else if (argL.StartsWith("-w"))
                        Int32.TryParse(arg.Substring(2), out windowWidth);
                    else if (argL.StartsWith("-h"))
                        Int32.TryParse(arg.Substring(2), out windowHeight);
                    else if (argL == "-channelorder")
                        channelOrder = true;
                    continue;
                }

                if (channelOrder && string.IsNullOrEmpty(channelFile))
                    channelFile = arg;
                else
                    files.Add(arg);
            }

            if (channelOrder && string.IsNullOrEmpty(channelFile))
                channelOrder = false;

            if (files.Count == 0)
            {
                Usage();
                return;
            }

            initialized = true;
        }
    }
}

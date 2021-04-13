using System;
using System.Collections.Generic;
using System.IO;
using Mcs.RawDataFileIO;

namespace ExtractChannels2
{
    public class FileProcessor
    {
        static readonly string fileExt = ".MSRD";
        static readonly string outDir = "ks_sorted";
        static readonly string outFile = "alldata.dat";

        private readonly List<string> files;
        protected string rootPath = null;

        private bool VerifyFiles()
        {
            if (files.Count < 1)
            {
                Console.WriteLine("No files passed");
                return false;
            }

            Reader fileReader = new Reader();
            bool valid = true;
            bool sameDir = true;
            foreach (string file in files)
            {
                if (!File.Exists(file))
                {
                    Console.WriteLine("File not found: {0}", file);
                    valid = false;
                    continue;
                }

                // Verify file
                if (Path.GetExtension(file).ToUpper() != fileExt)
                {
                    Console.WriteLine("Wrong file extension: {0}", file);
                    valid = false;
                }
                else
                {
                    // Check file format
                    try
                    {
                        fileReader.FileOpen(file);
                    }
                    catch (Exception ex) when (ex is ExcFileIO || ex is ArgumentException) // Any other exceptions necessary?
                    {
                        Console.WriteLine("Invalid or corrupt file: {0}", file);
                        valid = false;
                    }
                }

                // Verify root directory
                if (rootPath == null)
                {
                    rootPath = Path.GetDirectoryName(file);
                }
                else if (rootPath != Path.GetDirectoryName(file))
                {
                    valid = sameDir = false;
                }
            }

            // Add general fails at the end
            if (!sameDir)
            {
                Console.WriteLine("All files must reside in the same directory");
            }

            return valid;
        }

        private int FileTimeCompare(string file1, string file2)
        {
            // Assumes the files exist
            return File.GetLastWriteTime(file1).CompareTo(File.GetLastWriteTime(file2));
        }

        public FileProcessor(List<string> filelist, bool metaonly)
        {
            files = filelist;

            // Need to check all files before starting the conversion
            if (!VerifyFiles())
                return;

            // Sort the files by file time to get the correct stimulus sequence in the output file
            files.Sort(FileTimeCompare);

            // Create the output directory
            rootPath = Path.Combine(rootPath, outDir) + Path.DirectorySeparatorChar;
            try
            {
                Directory.CreateDirectory(rootPath);
            }
            catch (Exception ex) when (ex is IOException
                                    || ex is UnauthorizedAccessException
                                    || ex is ArgumentException
                                    || ex is PathTooLongException)
            {
                Console.WriteLine("Directory could not be created: {0}", outDir);
                return;
            }

            // Make sure not to accidentally overwrite an existing extraction
            string outPath = Path.Combine(rootPath, outFile);
            if (File.Exists(outPath))
            {
                Console.WriteLine("Output file already exists: {0}", outPath);
                return;
            }

            // All set, let's go
            using (BinaryWriter writer = new BinaryWriter(File.Open(outPath, FileMode.CreateNew)))
            {
                ChannelExtractor extractor = new ChannelExtractor(OutputFunction, ProgressUpdate, writer);

                // Read the stimulus files
                for (int stimIdx = 0; stimIdx < files.Count; stimIdx++)
                {
                    string file = files[stimIdx];

                    Console.WriteLine("Processing {0}", file);
                    try
                    {
                        extractor.ExtractBins(file, stimIdx);
                    }
                    catch (ExcFileIO)
                    {
                        Console.WriteLine("Reading error. Corrupt file?");
                    }
                }
            }

            Console.WriteLine("Done");
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

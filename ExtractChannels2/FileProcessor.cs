using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using Mcs.RawDataFileIO;

namespace ExtractChannels2
{
    public class FileProcessor
    {
        static readonly string fileExt = ".MSRD";
        static readonly string outDir = "ks_sorted";
        static readonly string outFile = "alldata.dat";
        static readonly Regex rgx = new Regex("\\d+");

        private readonly List<string> files;
        protected string rootPath = null;

        private bool VerifyFiles()
        {
            if (files.Count < 1)
            {
                OutputError("No files passed");
                return false;
            }

            Reader fileReader = new Reader();
            bool valid = true;
            bool sameDir = true;
            foreach (string file in files)
            {
                string filename = Path.GetFileName(file);

                if (!File.Exists(file))
                {
                    OutputError(String.Format("File not found: {0}", file));
                    valid = false;
                    continue;
                }

                // Verify file
                if (Path.GetExtension(file).ToUpper() != fileExt)
                {
                    OutputError(String.Format("Wrong file extension: {0}", filename));
                    valid = false;
                    continue;
                }

                // Verify the file name contains a stimulus ID, i.e. "01_stimulusname.msrd"
                if (rgx.Match(Path.GetFileNameWithoutExtension(file)).Value == "")
                {
                    OutputError(String.Format("File name does not contain a stimulus number: {0}", filename));
                    valid = false;
                    continue;
                };

                // Roughly check file format
                try
                {
                    fileReader.FileOpen(file);
                    fileReader.FileClose();
                }
                catch (Exception ex) when (ex is ExcFileIO || ex is ArgumentException) // Any other exceptions necessary?
                {
                    OutputError(String.Format("Invalid or corrupt file {0}", filename), ex);
                    valid = false;
                    continue;
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
                OutputError("All files must reside in the same directory");
            }

            return valid;
        }

        private int FileStimulusIDCompare(string file1, string file2)
        {
            Int32.TryParse(rgx.Match(file1).Value, out int id1);
            Int32.TryParse(rgx.Match(file2).Value, out int id2);
            return id1 - id2;
        }

        public FileProcessor(List<string> filelist, bool metaonly)
        {
            files = filelist;

            // Need to check all files before starting the conversion
            if (!VerifyFiles())
                return;

            // Sort the files by stimulus ID
            files.Sort(FileStimulusIDCompare);

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
                OutputError(String.Format("Directory {0} could not be created", outDir), ex);
                return;
            }

            // Make sure not to accidentally overwrite an existing extraction
            string outPath = Path.Combine(rootPath, outFile);
            if (File.Exists(outPath))
            {
#if DEBUG
                File.Delete(outPath);
#else
                OutputError(String.Format("Output file already exists: {0}", outPath));
                return;
#endif
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
                        extractor.ExtractBins(file, stimIdx, metaonly);
                    }
                    catch (ExcFileIO ex)
                    {
                        OutputError("Reading error", ex);
                        break; // Abort
                    }
                }
            }

            Console.WriteLine("Done");
        }

        private static void OutputError(string text, Exception ex = null)
        {
            if (String.IsNullOrWhiteSpace(text))
                text = "Error";
            Console.ForegroundColor = ConsoleColor.DarkRed;
            if (ex == null || String.IsNullOrWhiteSpace(ex.Message))
                Console.Error.WriteLine(text);
            else
                Console.Error.WriteLine("{0}: {1}", text, ex.Message);
            Console.ResetColor();
        }

        private static void ProgressUpdate(double percent, int stimulusId)
        {
            int originalTop = Console.CursorTop;
            int originalLeft = Console.CursorLeft;

            Console.SetCursorPosition(0, Math.Max(Console.CursorTop, Console.WindowHeight));
            Console.WriteLine("{0}: {1:##.##%}", stimulusId, percent);
            Console.SetCursorPosition(originalLeft, originalTop);
        }

        private static void OutputFunction(string text)
        {
            Console.Write(text);
        }
    }
}

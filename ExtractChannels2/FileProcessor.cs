using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using Mcs.DataStream;
using Mcs.RawDataFileIO;

namespace ExtractChannels2
{
    public class FileProcessor
    {
        const string fileExt = ".MSRD";
        const string outDir = "ks_sorted";
        const string outFile = "alldata.dat";
        static readonly Regex rgx = new Regex("\\d+");

        private readonly int stimulusId = 0;
        private readonly string stimulusFileName = "";
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

                    // Check for exactly one recording
                    if (fileReader.Recordings.Count != 1)
                        throw new ExcFileIO("File contains no or more than one recording");
                    var header = fileReader.RecordingHdr.FirstOrDefault().Value;

                    // Check for exactly one analog stream
                    var streams = header.AnalogStreams.Where(v => v.Value.DataSubType == enAnalogSubType.Auxiliary && v.Value.Label.Contains("Analog"));
                    if (streams.Count() != 1)
                        throw new ExcFileIO("File contains no or more than one analog stream");

                    // Check for exaclty one filtered stream
                    streams = header.AnalogStreams.Where(v => v.Value.DataSubType == enAnalogSubType.Electrode && v.Value.Label.Contains("Filter"));
                    if (streams.Count() != 1)
                        throw new ExcFileIO("File contains no or more than one filtered stream");
                    var filtered = streams.First();

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

        private int GetStimulusID(string file)
        {
            Int32.TryParse(rgx.Match(file).Value, out int id);
            return id;
        }

        private int FileStimulusIDCompare(string file1, string file2)
        {
            int id1 = GetStimulusID(file1);
            int id2 = GetStimulusID(file2);
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

            // Same for the analog files
            string[] stimPath = new string[files.Count];
            for (int fileIdx = 0; fileIdx < files.Count; fileIdx++)
            {
                string file = files[fileIdx];
                stimPath[fileIdx] = Path.Combine(rootPath, string.Format("{0}_aux.dat", Path.GetFileNameWithoutExtension(file)));
                if (File.Exists(stimPath[fileIdx]))
                {
#if DEBUG
                    File.Delete(stimPath[fileIdx]);
#else
                    OutputError(String.Format("Output file already exists: {0}", stimPath[i]));
                    return;
#endif
                }
            }

            // All set, let's go
            Console.CursorVisible = false;
            using (BinaryWriter writer = new BinaryWriter(File.Open(outPath, FileMode.CreateNew)))
            {
                ChannelExtractor extractor = new ChannelExtractor(OutputFunction, ProgressUpdate, writer);

                // Read the stimulus files
                for (int fileIdx = 0; fileIdx < files.Count; fileIdx++)
                {
                    string file = files[fileIdx];
                    stimulusFileName = Path.GetFileNameWithoutExtension(file);
                    stimulusId = GetStimulusID(stimulusFileName);

                    Console.WriteLine("------------------------\r\n");
                    Console.WriteLine("Processing {0}\r\n", file);

                    using (BinaryWriter auxWriter = new BinaryWriter(File.Open(stimPath[fileIdx], FileMode.CreateNew)))
                    {
                        try
                        {
                            extractor.ExtractBins(file, metaonly, auxWriter);
                        }
                        catch (ExcFileIO ex)
                        {
                            OutputError("Reading error", ex);
                            break; // Abort
                        }
                    }

                    ClearConsoleLine(GetLastConsoleLine());
                }
            }
            Console.CursorVisible = true;

            Console.WriteLine("------------------------");
            Console.WriteLine("\r\nDone");
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

        private static int GetLastConsoleLine()
        {
            return Math.Max(Console.CursorTop, Console.WindowHeight);
        }

        private static void ClearConsoleLine(int line)
        {
            int originalTop = Console.CursorTop;
            int originalLeft = Console.CursorLeft;

            Console.SetCursorPosition(0, line);
            Console.Write(new string(' ', Console.WindowWidth));
            Console.SetCursorPosition(originalLeft, originalTop);
        }

        private void ProgressUpdate(double percent, string type = "")
        {
            if (!String.IsNullOrWhiteSpace(type))
                type = "(" + type + ")";
            string progress = string.Format("{0,6:##0.00%}", percent);

            int originalTop = Console.CursorTop;
            int originalLeft = Console.CursorLeft;

            // Get last line
            int line = GetLastConsoleLine();

            // Clear that line
            ClearConsoleLine(line);

            // Write stimulus file info (left)
            Console.SetCursorPosition(0, line);
            Console.Write("{0,2}: {1} {2}", stimulusId, stimulusFileName, type);

            // Write progress (right)
            Console.SetCursorPosition(Math.Max(Console.CursorLeft, Console.WindowWidth) - 7, line);
            Console.Write(progress);

            // Restore cursor position
            Console.SetCursorPosition(originalLeft, originalTop);

            // Update title bar
            Console.Title = string.Format("{0}: {1}", stimulusId, progress);
        }

        private static void OutputFunction(string text)
        {
            Console.Write(text);
        }
    }
}

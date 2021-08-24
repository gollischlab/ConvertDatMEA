using System;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;

namespace ConvertDatMEA
{
    public class InvalidFileFormatException : Exception
    {
        public InvalidFileFormatException() : base() { }
        public InvalidFileFormatException(string message) : base(message) { }
        public InvalidFileFormatException(string message, Exception inner) : base(message, inner) { }
    }

    public abstract class FileProcessor
    {
        protected abstract string fileExt { get; } // .MSRD or .MCD
        private const string outDir = "ks_sorted";
        private const string auxSubDir = "analog"; // You cannot create a directory named "aux" in Windows. wow
        private const string outFile = "alldata.dat";
        private const string auxSuffix = "_aux.dat";
        private const string metaFile = "bininfo.txt";
        static readonly Regex rgxId = new Regex("^\\d+");
        static readonly Regex rgxPart = new Regex("\\d{4}$");

        private int stimulusId = 0;
        private string stimulusFileName = "";
        private readonly List<string> files;
        protected string rootPath = null;
        protected string auxPath = null;
        protected string extPath = null;
        protected long samplingRate = -1;
        protected long numChannels = -1;
        protected Dictionary<string, double> conversionFactor = new Dictionary<string, double>() { { "analog", double.NaN }, { "filter", double.NaN } };
        private readonly bool verified = false;
        private bool sameDir = true;
        private int bufferline = 0;
        private bool started = false;
        public bool success = false;

        protected abstract void CheckFileFormat(string file);
        protected abstract void PrintMetadataFile(string file);

        public static dynamic Create(List<string> filelist)
        {
            List<string> filelistOld = new List<string>(filelist);
            foreach (string file in filelistOld)
            {
                if (Directory.Exists(file))
                {
                    List<string> filelistSub = new List<string>(Directory.GetFiles(file, "*.mcd"));
                    filelistSub.AddRange(Directory.GetFiles(file, "*.msrd"));
                    int idx = filelist.IndexOf(file);
                    filelist.Remove(file);
                    filelist.InsertRange(idx, filelistSub);
#if DEBUG
                    Console.WriteLine("Expanding directory {0}", file);
                    foreach (string fnew in filelistSub)
                        Console.WriteLine(" {0}", fnew);
                    Console.WriteLine();
#endif
                }
            }

            switch (Path.GetExtension(filelist[0]).ToUpper())
            {
                case ".MSRD":
                    return new MsrdProcessor(filelist);

                case ".MCD":
                    return new McdProcessor(filelist);

                default:
                    OutputError("Only MCD and MSRD files are supported");
                    return null;
            }
        }

        private bool CheckFile(string file)
        {
            string filename = Path.GetFileName(file);

            if (!File.Exists(file))
            {
                OutputError(String.Format("File not found: {0}", file));
                return false;
            }

            // Verify file extension
            if (Path.GetExtension(file).ToUpper() != fileExt)
            {
                OutputError(String.Format("Wrong file extension: {0}", filename));
                return false;
            }

            // Verify the file name contains a stimulus ID, i.e. "01_stimulusname.ext"
            if (rgxId.Match(Path.GetFileNameWithoutExtension(file)).Value == "")
            {
                OutputError(String.Format("File name does not contain a stimulus number: {0}", filename));
                return false;
            };

            // Roughly check file format
            try
            {
                CheckFileFormat(file);
            }
            catch (Exception ex) // Let's just catch all exceptions
            {
                OutputError(String.Format("Invalid or corrupt file {0}", filename), ex);
                return false;
            }

            // Verify root directory
            if (rootPath == null)
            {
                rootPath = Path.GetDirectoryName(file);
            }
            else if (rootPath != Path.GetDirectoryName(file))
            {
                sameDir = false;
                return false;
            }

            return true;
        }

        private bool VerifyFiles()
        {
            if (verified)
                return true;

            if (files.Count < 1)
            {
                OutputError("No files passed");
                return false;
            }

            bool valid = true;
            foreach (string file in files)
            {
                valid &= CheckFile(file);
            }

            // Add general fails at the end
            if (!sameDir)
            {
                OutputError("All files must reside in the same directory");
            }

            return valid;
        }

        private int GetStimulusId(string file)
        {
            Int32.TryParse(rgxId.Match(Path.GetFileNameWithoutExtension(file)).Value, out int id);
            return id;
        }

        private int GetPartNum(string file)
        {
            Int32.TryParse(rgxPart.Match(Path.GetFileNameWithoutExtension(file)).Value, out int num);
            return num;
        }

        private int FileStimulusIdCompare(string file1, string file2)
        {
            int id1 = GetStimulusId(file1);
            int id2 = GetStimulusId(file2);
            if (id1 == id2)
            {
                id1 = GetPartNum(file1);
                id2 = GetPartNum(file2);
            }
            return id1 - id2;
        }

        protected FileProcessor(List<string> filelist)
        {
            files = filelist;

            // Need to check all files before starting the conversion
            verified = VerifyFiles();

            // Sort the files by stimulus ID and part number
            files.Sort(FileStimulusIdCompare);

            // Set up paths
            if (!string.IsNullOrWhiteSpace(rootPath))
            {
                rootPath = Path.Combine(rootPath, outDir) + Path.DirectorySeparatorChar;
                auxPath = Path.Combine(rootPath, auxSubDir) + Path.DirectorySeparatorChar;
            };
        }

        public void PrintMetadata()
        {
            if (!verified)
                return;

            foreach (string file in files)
            {
                Console.WriteLine(file);
                PrintMetadataFile(file);
                Console.WriteLine();
            }
        }

        public void Convert()
        {
            if (!verified)
                return;

            // Create the output directory
            try
            {
                Directory.CreateDirectory(rootPath);
            }
            catch (Exception ex) // Let's just catch all exceptions
            {
                OutputError(String.Format("Directory {0} could not be created", outDir), ex);
                return;
            }

            // Create the aux output directory
            try
            {
                Directory.CreateDirectory(auxPath);
            }
            catch (Exception ex) // Let's just catch all exceptions
            {
                OutputError(String.Format("Directory {0} could not be created", auxSubDir), ex);
                return;
            }

            // Make sure not to accidentally overwrite an existing extraction
            bool fileExists = false;
            string outPath = Path.Combine(rootPath, outFile);
            if (File.Exists(outPath))
            {
#if DEBUG
                File.Delete(outPath);
#else
                OutputError(String.Format("Output file already exists: {0}", outPath));
                fileExists = true;
#endif
            }

            // Same for the analog files
            int stimCount = 0;
            string[] stimPath = new string[files.Count];
            for (int fileIdx = 0; fileIdx < files.Count; fileIdx++)
            {
                string file = files[fileIdx];
                string filename = Path.GetFileNameWithoutExtension(file);
                if (GetPartNum(file) > 0)
                    filename = filename.Substring(0, filename.Length - 4);
                else
                    stimCount += 1;

                stimPath[fileIdx] = Path.Combine(auxPath, string.Format("{0}{1}", filename, auxSuffix));
                if (File.Exists(stimPath[fileIdx]))
                {
#if DEBUG
                    File.Delete(stimPath[fileIdx]);
#else
                    OutputError(String.Format("Output file already exists: {0}", stimPath[fileIdx]));
                    fileExists = true;
#endif
                }
            }

            if (fileExists)
                return;

            // Avoid confusion on fail: Remove the info file beforehand
            string metaPath = Path.Combine(rootPath, metaFile);
            if (File.Exists(metaPath))
                File.Delete(metaPath);

            // Set up info text
            const int headLines = 3;
            double[] metaLines = new double[headLines + stimCount];
            metaLines[0] = numChannels;                      // Number of channels
            metaLines[1] = samplingRate;                     // Sampling rate
            metaLines[2] = (1 / DataConverter.voltToSample); // Conversion from int16 to mV

            // All set, let's go
            Console.CursorVisible = false;
            success = true;
            using (BinaryWriter writer = new BinaryWriter(File.Open(outPath, FileMode.CreateNew)))
            {
                DataConverter extractor = DataConverter.FromFormat(fileExt, OutputFunction, ProgressUpdate, writer);

                // Read the stimulus files
                int stimIdx = -1;
                for (int fileIdx = 0; fileIdx < files.Count; fileIdx++)
                {
                    string file = files[fileIdx];
                    stimulusFileName = Path.GetFileNameWithoutExtension(file);
                    stimulusId = GetStimulusId(stimulusFileName);
                    if (GetPartNum(stimulusFileName) == 0)
                        stimIdx += 1;

                    Console.WriteLine(file);

                    using (BinaryWriter auxWriter = new BinaryWriter(File.Open(stimPath[fileIdx], FileMode.Append)))
                    {
                        PrintMetadataFile(file);
                        Console.WriteLine();
                        try
                        {
                            long num_samples = extractor.ExtractData(file, auxWriter);
                            metaLines[headLines + stimIdx] += num_samples;
                        }
                        catch (Exception ex)  // Let's just catch all exceptions
                        {
                            OutputError("Reading error", ex);
                            success = false;
                            break; // Abort
                        }
                    }

                    ClearConsoleLine(GetLastConsoleLine());
                }
            }
            Console.CursorVisible = true;

            if (success)
            {
                File.WriteAllLines(metaPath, Array.ConvertAll(metaLines, i => i.ToString()));
                Console.WriteLine();
                Console.WriteLine("Done");
                Console.Title = "100% complete";
            }
            else
            {
                Console.WriteLine();
                Console.Title = "Conversion failed";
            }
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
            return Math.Max(Console.CursorTop, Console.WindowHeight-2);
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
            if (!string.IsNullOrWhiteSpace(type))
                type = "(" + type + ")";
            string info = string.Format("{0,2}: {1} {2}", stimulusId, stimulusFileName, type);
            string progress = string.Format("{0,6:##0.00%}", percent);

            int originalTop = Console.CursorTop;
            int originalLeft = Console.CursorLeft;

            // Clear previously used line and set to last line (semi-robust to scrolling and resizing)
            if (started)
                ClearConsoleLine(bufferline);
            bufferline = GetLastConsoleLine();
            ClearConsoleLine(bufferline);
            started = true;

            // Write stimulus file info (left)
            Console.SetCursorPosition(0, bufferline);
            Console.Write(info);

            // Show progress bar (if there is room for it)
            int percentWidth = Console.BufferWidth - (info.Length + progress.Length + 6);
            if (5 < percentWidth && percentWidth < Console.BufferWidth)
            {
                Console.SetCursorPosition(info.Length + 1, bufferline);
                Console.Write("[" + new string('#', (int)(percentWidth * percent)));
                Console.SetCursorPosition(info.Length + 1 + percentWidth, bufferline);
                Console.Write("]");
            }

            // Write percentage (right)
            Console.SetCursorPosition(Math.Max(Console.CursorLeft, Console.WindowWidth) - 7, bufferline);
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

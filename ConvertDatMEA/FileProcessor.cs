using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
        protected abstract string FileExt { get; } // .MSRD or .MCD
        private const string outDir = "ks_sorted";
        private const string auxSubDir = "analog"; // You cannot create a directory named "aux" in Windows. wow
        private const string outFile = "alldata.dat";
        private const string auxSuffix = "_aux.dat";
        private const string metaFile = "bininfo.txt";
        static readonly Regex rgxId = new Regex("^\\d+");
        static readonly Regex rgxPart = new Regex("\\d{4}$");
        private static readonly Regex rgxChnLbl = new Regex("([a-zA-Z]?)(\\d+)");

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
        protected string[] channelListOrder;

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
                OutputError(string.Format("File not found: {0}", file));
                return false;
            }

            // Verify file extension
            if (Path.GetExtension(file).ToUpper() != FileExt)
            {
                OutputError(string.Format("Wrong file extension: {0}", filename));
                return false;
            }

            // Verify the file name contains a stimulus ID, i.e. "01_stimulusname.ext"
            if (rgxId.Match(Path.GetFileNameWithoutExtension(file)).Value == "")
            {
                OutputError(string.Format("File name does not contain a stimulus number: {0}", filename));
                return false;
            };

            // Roughly check file format
            try
            {
                CheckFileFormat(file);
            }
            catch (Exception ex) // Let's just catch all exceptions
            {
                OutputError(string.Format("Invalid or corrupt file {0}", filename), ex);
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
            int id;
            Int32.TryParse(rgxId.Match(Path.GetFileNameWithoutExtension(file)).Value, out id);
            return id;
        }

        private int GetPartNum(string file)
        {
            // Path.GetFileNameWithoutExtension fails if the filename contains a period but no file extension
            string basename = Path.GetFileName(file);
            if (basename.ToUpper().EndsWith(FileExt))
            {
                basename = basename.Substring(0, basename.Length - FileExt.Length);
            }

            // Find part number from file name
            int num;
            Int32.TryParse(rgxPart.Match(basename).Value, out num);

            // Check if there are actually previous parts to make sure the indices are correct
            int count = 0;
            for (int i = 0; i < num; i++)
            {
                string previousPart = basename.Substring(0, basename.Length - 4);
                if (i > 0)
                    previousPart += i.ToString("D4");
                count += files.Any(v => Path.GetFileNameWithoutExtension(v) == previousPart) ? 1 : 0;
            }

            return count;
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

        public void SetChannelOrder(string filepath)
        {
            if (!File.Exists(filepath) || Path.GetExtension(filepath).ToUpper() != ".TXT")
            {
                OutputError(string.Format("Channel order file {0} is not a valid txt file. Falling back to auto-ordering", filepath));
                return;
            }

            string[] chanOrder = File.ReadAllLines(filepath);

            // Trim strings
            for (short i = 0; i < chanOrder.Length; i++)
                chanOrder[i] = chanOrder[i].Trim();

            // Remove empty lines
            chanOrder = chanOrder.Where(s => !string.IsNullOrWhiteSpace(s)).ToArray();

            if (chanOrder.Length != numChannels)
            {
                OutputError(string.Format("Number of channels in channel order list file ({0}) does not match with recording files ({1}). Falling back to auto-ordering", chanOrder.Length, numChannels));
                return;
            }

            channelListOrder = chanOrder;
        }

        protected static int[] ChannelOrder(Dictionary<int, string> channels, string[] channelOrder = null, bool verbose = false)
        {
            int nChannels = channels.Count;
            int[] channelIds = channels.Keys.ToArray();

            int[] sortIds = new int[nChannels];

            if (channelOrder != null)
            {
                bool valid = true;

                // Get indices from specified channel order
                for (int i = 0; i < nChannels; i++)
                {
                    string label = channelOrder[i];

                    if (!channels.ContainsValue(label))
                    {
                        if (verbose)
                            OutputError(string.Format("Channel name {0} from channel order file not found. Falling back to auto-ordering", label));
                        valid = false;
                        break;
                    }

                    sortIds[i] = channelIds[channels.Values.ToList().IndexOf(label)];
                }

                if (valid)
                {
                    if (verbose)
                        OutputFunction("From file\r\n");
                    return sortIds;
                }
            }

            // Sort by label
            string[] sortNames = new string[nChannels];
            for (int i = 0; i < nChannels; i++)
            {
                int id = channelIds[i];
                Match match = rgxChnLbl.Match(channels[id]);
                string letter = match.Groups[1].Value;
                int num;
                int.TryParse(match.Groups[2].Value, out num);
                sortNames[i] = string.Format("{0}{1:000}", letter, num);
                sortIds[i] = id;
            }
            Array.Sort(sortNames, sortIds);

            if (verbose)
                OutputFunction("Automatic ordering\r\n");
            return sortIds;
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
                OutputError(string.Format("Directory {0} could not be created", outDir), ex);
                return;
            }

            // Create the aux output directory
            try
            {
                Directory.CreateDirectory(auxPath);
            }
            catch (Exception ex) // Let's just catch all exceptions
            {
                OutputError(string.Format("Directory {0} could not be created", auxSubDir), ex);
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
                OutputError(string.Format("Output file already exists: {0}", outPath));
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
                    OutputError(string.Format("Output file already exists: {0}", stimPath[fileIdx]));
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
                DataConverter extractor = DataConverter.FromFormat(FileExt, OutputFunction, ProgressUpdate, writer, rootPath, channelListOrder);

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
                            OutputError(string.Format("Error while converting {0}", file), ex);
                            success = false;
                            break; // Abort
                        }
                    }

                    // Reset progress bar
                    if (started)
                    {
                        ClearConsoleLine(bufferline);
                        started = false;
                    }

                    Console.WriteLine();
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

        public static void OutputError(string text, Exception ex = null)
        {
            if (String.IsNullOrWhiteSpace(text))
                text = "Error";
            int originalLeft = Console.CursorLeft;
            Console.ForegroundColor = ConsoleColor.Red;
            if (ex != null && !String.IsNullOrWhiteSpace(ex.Message))
            {
                text += ": " + ex.Message;
            }
            Console.Error.WriteLine(text);

            // If redirected also show in console
            if (Console.IsErrorRedirected && !Console.IsOutputRedirected)
            {
                Console.WriteLine(text);
            }

            Console.ResetColor();
            Console.SetCursorPosition(originalLeft, Console.CursorTop); // Maintain possible indent
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
            Console.Write(new string(' ', Console.BufferWidth));
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
            if (bufferline == originalTop)
                bufferline += 1;
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
            Console.SetCursorPosition(Math.Max(Console.CursorLeft, Console.BufferWidth) - 7, bufferline);
            Console.Write(progress);

            // Restore cursor position
            Console.SetCursorPosition(originalLeft, originalTop);

            // Update title bar
            Console.Title = string.Format("{0}: {1}", stimulusId, progress);
        }

        private static void OutputFunction(string text)
        {
            int originalTop = Console.CursorTop;
            int originalLeft = Console.CursorLeft;

            Console.Write(text);

            if (originalTop != Console.CursorTop)
                Console.SetCursorPosition(originalLeft, Console.CursorTop); // Maintain possible indent
        }
    }
}

/*
 * Modified from https://stackoverflow.com/questions/420429/mirroring-console-output-to-a-file#6927051
 */

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConvertDatMEA
{
    class ConsoleCopy : IDisposable
    {

        FileStream fileStream;
        StreamWriter fileWriter;
        TextWriter doubleWriterOut;
        TextWriter doubleWriterErr;
        TextWriter oldOut;
        TextWriter oldErr;
        bool writeDate = false;

        class DoubleWriter : TextWriter
        {

            TextWriter one;
            TextWriter two;
            public bool outOnly = false;

            public DoubleWriter(TextWriter one, TextWriter two)
            {
                this.one = one;
                this.two = two;
            }

            public override Encoding Encoding
            {
                get { return one.Encoding; }
            }

            public override void Flush()
            {
                if (!outOnly)
                    one.Flush();
                two.Flush();
            }

            public override void Write(char value)
            {
                if (!outOnly)
                    one.Write(value);
                two.Write(value);
            }

        }

        public ConsoleCopy(string path, bool writeDate=true, bool disable=false)
        {
            oldOut = Console.Out;
            oldErr = Console.Error;

            if (disable)
                return;

            try
            {
                string directory = Path.GetDirectoryName(path);
                if (!Directory.Exists(directory))
                    Directory.CreateDirectory(directory);

                fileStream = File.Create(path);

                fileWriter = new StreamWriter(fileStream);
                fileWriter.AutoFlush = true;

                doubleWriterOut = new DoubleWriter(fileWriter, oldOut);
                doubleWriterErr = new DoubleWriter(fileWriter, oldErr);

                this.writeDate = writeDate;

                if (writeDate)
                    fileWriter.WriteLine("Started at {0}{1}", DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"), Environment.NewLine);
            }
            catch (Exception e)
            {
                FileProcessor.OutputError("Failed to create output logger file", e);
                return;
            }

            Console.SetOut(doubleWriterOut);

            bool errorToOut = Console.IsErrorRedirected && !Console.IsOutputRedirected;
            if (!errorToOut)
                Console.SetError(doubleWriterErr);
        }

        public void Dispose()
        {
            Console.SetOut(oldOut);
            Console.SetError(oldErr);
            if (fileWriter != null)
            {
                if (writeDate)
                    fileWriter.WriteLine("{0}Finished at {1}", Environment.NewLine, DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"));

                fileWriter.Flush();
                fileWriter.Close();
                fileWriter = null;
            }
            if (fileStream != null)
            {
                fileStream.Close();
                fileStream = null;
            }
        }

        public bool GetOutOnly()
        {
            return ((DoubleWriter) doubleWriterOut).outOnly;
        }

        public void SetOutOnly(bool on)
        {
            ((DoubleWriter) doubleWriterOut).outOnly = on;
            ((DoubleWriter) doubleWriterErr).outOnly = on;
        }
    }
}

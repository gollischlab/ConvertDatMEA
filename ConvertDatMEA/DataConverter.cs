using System;
using System.Threading.Tasks;
using System.IO;
using System.Collections.Concurrent;


// Fernando Rozenblit, 2018

namespace ConvertDatMEA
{
    abstract class DataConverter
    {
        private const int minVolt = -4096; // mV
        private const int maxVolt = 4096; // mV
        public const double voltToSample = (1 << 16) / (maxVolt - minVolt); // 16-bit range

        public delegate void OutputFunction(string line);
        public delegate void ProgressUpdate(double percent, string type = "");

        protected OutputFunction _outputFunction = null;
        protected ProgressUpdate _progressUpdate = null;
        protected BinaryWriter _channelWriter = null;
        protected BinaryWriter _auxWriter = null;
        protected string[] channelOrderList = null;

        protected void OutputText(string text)
        {
            _outputFunction(text);
        }

        protected void OutputLine(string line)
        {
            OutputText(line + "\r\n");
        }

        public static DataConverter FromFormat(string fileExtension, OutputFunction function, ProgressUpdate updater, BinaryWriter datWriter, string[] channelOrder = null)
        {
            switch (fileExtension)
            {
                case ".MCD":
                    return new McdConverter(function, updater, datWriter, channelOrder);

                case ".MSRD":
                    return new MsrdConverter(function, updater, datWriter, channelOrder);

                default:
                    throw new InvalidFileFormatException(string.Format("File format not supported: {0}", fileExtension));
            }
        }

        protected DataConverter(OutputFunction function, ProgressUpdate updater, BinaryWriter datWriter, string[] channelOrder=null)
        {
            _outputFunction = function;
            _progressUpdate = updater;
            _channelWriter = datWriter; // Dat file
            channelOrderList = channelOrder;
        }

        // Whether 16-bit or 24-bit, the ADzero has to be subtracted with correct sign bit
        private static double Subtract<T>(T a, T b)
        {
            if (typeof(T) == typeof(int))
                return (int)(object)a - (int)(object)b;
            else if (typeof(T) == typeof(short))
                return (short)((short)(object)a - (short)(object)b); // Result has to be cast to 16-bit first
            else
                throw new NotSupportedException();
        }

        protected static void ConvertRange<T>(T[] data, byte[] newData, T adzero, double unitsPerAd)
        {
            if (2 * data.Length != newData.Length)
                throw new ArgumentException("Data and newData must be the same size");

            void loop(int from, int to)
            {
                for (int i = from; i < to; i++)
                {
                    double milliVolts = Subtract(data[i], adzero) * unitsPerAd * 1e3;
                    double samples = milliVolts * voltToSample; // Convert to 16-bit range
                    short samples16 = (short)Math.Round(samples, MidpointRounding.AwayFromZero); // Rounding is important to avoid noise

                    byte[] inBytes = BitConverter.GetBytes(samples16);
                    newData[2 * i] = inBytes[0];
                    newData[2 * i + 1] = inBytes[1];
                }
            }

            // Only parallelize for large arrays
            if (data.Length > 5000)
            {
                var rangePartitioner = Partitioner.Create(0, data.Length);
                Parallel.ForEach(rangePartitioner, range => loop(range.Item1, range.Item2));
            }
            else
            {
                loop(0, data.Length);
            }
        }

        protected void WriteBin(bool isAnalog, byte[] data, int length = 0)
        {
            if (length == 0)
                length = data.Length;

            if (isAnalog)
                _auxWriter.Write(data, 0, length);
            else
                _channelWriter.Write(data, 0, length);
        }

        public abstract long ExtractData(string filepath, BinaryWriter auxWriter);
    }
}

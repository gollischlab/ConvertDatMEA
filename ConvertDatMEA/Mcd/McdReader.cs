using System;
using System.Collections.Generic;
using System.Linq;

// This class serves as interface to the Kaitai generated class Mcdfile for better access

namespace ConvertDatMEA
{
    class McdReader : IDisposable
    {
        private readonly Mcdfile file;

        // MCS Header
        public readonly uint SoftwareVersionMajor;
        public readonly uint SoftwareVersionMinor;
        public readonly string FilePath;
        public readonly ulong TimeStampStart;
        public readonly ulong TimeStampEnd;
        public readonly uint StreamCount;
        public readonly uint MillisamplesPerSecond;
        public readonly ushort ElectrodeChannels;
        public readonly ushort ElectrodeChannelOffset;
        public readonly ushort AnalogChannels;
        public readonly ushort AnalogChannelOffset;
        public readonly ushort DigitalChannels;
        public readonly ushort DigitalChannelOffset;
        public readonly ushort TotalChannels;
        public readonly uint SegmentTime;
        public readonly uint DriverVersionMajor;
        public readonly uint DriverVersionMinor;
        public readonly string ImageFilePathName;
        public readonly uint VoltageRange;
        public readonly string DataSoureName;
        public readonly uint BusType;
        public readonly uint VendorId;
        public readonly uint ProductId;

        // Conventient
        public readonly long SamplingRate;
        public readonly double Duration; // Seconds
        public readonly Dictionary<string, Stream> Streams = new Dictionary<string, Stream>();

        public McdReader(string fileName)
        {
            // Load file structure
            try
            {
                file = Mcdfile.FromFile(fileName);
            }
            catch (Exception ex) // Let's just catch all exceptions, clearly reading the file failed
            {
                throw new InvalidFileFormatException(string.Format("File is not a valid MCD file: {0}", ex.Message));
            }

            // Find all headers to combine or extract information
            Mcdfile.McsHeader mcsheader = null;
            int i = 0;
            while (i < file.HeaderIndex.HeaderList.Headers.Count)
            {
                Mcdfile.Header hdr = file.HeaderIndex.HeaderList.Headers[i];
                if (hdr.Content is Mcdfile.McsHeader mHdr)
                    mcsheader = mHdr;
                else if (hdr.Content is Mcdfile.StreamHeader sHdr)
                    Streams.Add(sHdr.BufferId, new Stream(sHdr, (Mcdfile.StreamFormat)file.HeaderIndex.HeaderList.Headers[++i].Content, file.Data));
                i++;
            }

            if (mcsheader == null)
                throw new InvalidFileFormatException("MCS header not found");

            // File all basic top-level properties
            SoftwareVersionMajor = mcsheader.SoftwareVersionMajor;
            SoftwareVersionMinor = mcsheader.SoftwareVersionMinor;
            FilePath = mcsheader.FilePath;
            TimeStampStart = mcsheader.TimestampStart;
            TimeStampEnd = mcsheader.TimestampEnd;
            StreamCount = mcsheader.StreamCount;
            MillisamplesPerSecond = mcsheader.MillisamplesPerSecond;
            ElectrodeChannels = mcsheader.ElectrodeChannels;
            ElectrodeChannelOffset = mcsheader.ElectrodeChannelOffset;
            AnalogChannels = mcsheader.AnalogChannels;
            AnalogChannelOffset = mcsheader.AnalogChannelOffset;
            DigitalChannels = mcsheader.DigitalChannels;
            DigitalChannelOffset = mcsheader.DigitalChannelOffset;
            TotalChannels = mcsheader.TotalChannels;
            SegmentTime = mcsheader.SegmentTime;
            DriverVersionMajor = mcsheader.DriverVersionMajor;
            DriverVersionMinor = mcsheader.DriverVersionMinor;
            ImageFilePathName = mcsheader.ImageFilePathName;
            VoltageRange = mcsheader.VoltageRange;
            DataSoureName = mcsheader.DataSoureName;
            BusType = mcsheader.BusType;
            VendorId = mcsheader.VendorId;
            ProductId = mcsheader.ProductId;

            // Convenience
            SamplingRate = MillisamplesPerSecond / 1000;
            Duration = (TimeStampEnd - TimeStampStart) * 1e-9;
        }

        public void Dispose()
        {
            // Do the references have to be removed?
            foreach (Stream stream in Streams.Values)
                stream.Free();

            Streams.Clear();

            file.M_Io.Dispose();
        }
    }

    public partial class Mcdfile
    {
        public partial class StreamChunk
        {
            public int ReadSamples<T>(T[] data, int idx, long fromSample, long numSamples)
            {
                long totalSamples = (long)(ChunkLen - 16) / sizeof(short);
                if (fromSample > totalSamples)
                    return 0;
                if (fromSample + numSamples > totalSamples)
                    numSamples = totalSamples - fromSample;

                m_io.Seek(_dataAddr + fromSample * sizeof(short));
                for (long p = idx; p < idx + numSamples; p++)
                    data[p] = (T)(object)m_io.ReadS2le();

                return (int)numSamples;
            }
        }
    }

    class Stream
    {
        public Mcdfile.StreamHeader Header;
        public Mcdfile.StreamFormat Format;
        public List<Mcdfile.ChannelInfo> Channels;
        private List<Mcdfile.StreamChunk> _chunks;
        private Mcdfile.McdData _data;
        private string _bufferId;
        public readonly bool isAnalog = false;

        public Stream(Mcdfile.StreamHeader header, Mcdfile.StreamFormat format, Mcdfile.McdData data)
        {
            Header = header;
            Format = format;
            Channels = header.Channels;
            _chunks = null;
            _data = data;
            _bufferId = header.BufferId;
            isAnalog = header.TypeName == "analog";
        }

        public List<Mcdfile.StreamChunk> Chunks
        {
            get
            {
                if (_chunks == null)
                {
                    _chunks = _data.Chunks.Where(v => v.Name == _bufferId).ToList();
                }
                return _chunks;
            }
        }

        public void Free()
        {
            // Is this necessary in C#?
            Header = null;
            Format = null;
            Channels = null;
            _chunks = null;
        }
    }
}

// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

// Slightly modified to skip reading the whole file into memory!
// Lines 34-35, 308-309, 433-435 and 442

using System.Collections.Generic;
using Kaitai;

namespace ConvertDatMEA
{
    public partial class Mcdfile : KaitaiStruct
    {
        public static Mcdfile FromFile(string fileName)
        {
            return new Mcdfile(new KaitaiStream(fileName));
        }

        public Mcdfile(KaitaiStream p__io, KaitaiStruct p__parent = null, Mcdfile p__root = null) : base(p__io)
        {
            m_parent = p__parent;
            m_root = p__root ?? this;
            _read();
        }
        private void _read()
        {
            _magicbytes = m_io.ReadBytes(16);
            if (!((KaitaiStream.ByteArrayCompare(Magicbytes, new byte[] { 77, 67, 83, 83, 84, 82, 77, 32, 255, 255, 255, 255, 255, 255, 255, 255 }) == 0)))
            {
                throw new ValidationNotEqualError(new byte[] { 77, 67, 83, 83, 84, 82, 77, 32, 255, 255, 255, 255, 255, 255, 255, 255 }, Magicbytes, M_Io, "/seq/0");
            }
            _headerIndex = new McdHeader(m_io, this, m_root);
            _idxheader = new IdxHeader(m_io, this, m_root);
            _data = new McdData(m_io, this, m_root);
            // _rest = m_io.ReadBytesFull(); // Do not read this
            m_io.Seek(m_io.BaseStream.Length);
        }
        public partial class StreamFormat : KaitaiStruct
        {
            public static StreamFormat FromFile(string fileName)
            {
                return new StreamFormat(new KaitaiStream(fileName));
            }

            public StreamFormat(KaitaiStream p__io, Mcdfile.Header p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _formatVersion = m_io.ReadU2le();
                _unitSign = System.Text.Encoding.GetEncoding("ascii").GetString(m_io.ReadBytes(2));
                _adBits = m_io.ReadU2le();
                _adZero = m_io.ReadU2le();
                _unitsPerAd = m_io.ReadF8le();
                _bytesPerChannel = m_io.ReadU2le();
                _defaultSamplesPerSegment = m_io.ReadU2le();
                _defaultSegmentCount = m_io.ReadU2le();
                __unnamed8 = m_io.ReadBytes(2);
                _unknown = m_io.ReadBytes(8);
                _param = m_io.ReadBytesFull();
            }
            private ushort _formatVersion;
            private string _unitSign;
            private ushort _adBits;
            private ushort _adZero;
            private double _unitsPerAd;
            private ushort _bytesPerChannel;
            private ushort _defaultSamplesPerSegment;
            private ushort _defaultSegmentCount;
            private byte[] __unnamed8;
            private byte[] _unknown;
            private byte[] _param;
            private Mcdfile m_root;
            private Mcdfile.Header m_parent;
            public ushort FormatVersion { get { return _formatVersion; } }
            public string UnitSign { get { return _unitSign; } }
            public ushort AdBits { get { return _adBits; } }
            public ushort AdZero { get { return _adZero; } }
            public double UnitsPerAd { get { return _unitsPerAd; } }
            public ushort BytesPerChannel { get { return _bytesPerChannel; } }
            public ushort DefaultSamplesPerSegment { get { return _defaultSamplesPerSegment; } }
            public ushort DefaultSegmentCount { get { return _defaultSegmentCount; } }
            public byte[] Unnamed_8 { get { return __unnamed8; } }
            public byte[] Unknown { get { return _unknown; } }
            public byte[] Param { get { return _param; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.Header M_Parent { get { return m_parent; } }
        }
        public partial class StreamHeader : KaitaiStruct
        {
            public static StreamHeader FromFile(string fileName)
            {
                return new StreamHeader(new KaitaiStream(fileName));
            }

            public StreamHeader(KaitaiStream p__io, Mcdfile.Header p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _version = m_io.ReadU2le();
                _typeName = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(9), 0, false));
                _streamName = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(257), 0, false));
                _comment = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(513), 0, false));
                __unnamed4 = m_io.ReadBytes(1);
                _streamId = m_io.ReadU2le();
                _millisamplespersecond = m_io.ReadU4le();
                _channelCount = m_io.ReadU4le();
                _bufferId = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(9), 0, false));
                __unnamed9 = m_io.ReadBytes(3);
                _channels = new List<ChannelInfo>((int) (ChannelCount));
                for (var i = 0; i < ChannelCount; i++)
                {
                    _channels.Add(new ChannelInfo(m_io, this, m_root));
                }
            }
            private ushort _version;
            private string _typeName;
            private string _streamName;
            private string _comment;
            private byte[] __unnamed4;
            private ushort _streamId;
            private uint _millisamplespersecond;
            private uint _channelCount;
            private string _bufferId;
            private byte[] __unnamed9;
            private List<ChannelInfo> _channels;
            private Mcdfile m_root;
            private Mcdfile.Header m_parent;
            public ushort Version { get { return _version; } }
            public string TypeName { get { return _typeName; } }
            public string StreamName { get { return _streamName; } }
            public string Comment { get { return _comment; } }
            public byte[] Unnamed_4 { get { return __unnamed4; } }
            public ushort StreamId { get { return _streamId; } }
            public uint Millisamplespersecond { get { return _millisamplespersecond; } }
            public uint ChannelCount { get { return _channelCount; } }
            public string BufferId { get { return _bufferId; } }
            public byte[] Unnamed_9 { get { return __unnamed9; } }
            public List<ChannelInfo> Channels { get { return _channels; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.Header M_Parent { get { return m_parent; } }
        }
        public partial class McsHeader : KaitaiStruct
        {
            public static McsHeader FromFile(string fileName)
            {
                return new McsHeader(new KaitaiStream(fileName));
            }

            public McsHeader(KaitaiStream p__io, Mcdfile.Header p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                __unnamed0 = m_io.ReadBytes(4);
                _softwareVersionMajor = m_io.ReadU4le();
                _softwareVersionMinor = m_io.ReadU4le();
                _filePath = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(780), 0, false));
                _timestamp = m_io.ReadU8le();
                _streamCount = m_io.ReadU4le();
                _millisamplesPerSecond = m_io.ReadU4le();
                __unnamed7 = m_io.ReadBytes(20);
                _electrodeChannels = m_io.ReadU2le();
                _electrodeChannelOffset = m_io.ReadU2le();
                _analogChannels = m_io.ReadU2le();
                _analogChannelOffset = m_io.ReadU2le();
                _digitalChannels = m_io.ReadU2le();
                _digitalChannelOffset = m_io.ReadU2le();
                _totalChannels = m_io.ReadU2le();
                __unnamed15 = m_io.ReadBytes(6);
                _timestampEnd = m_io.ReadU8le();
                __unnamed17 = m_io.ReadBytes(12);
                _segmentTime = m_io.ReadU4le();
                _timestampStart = m_io.ReadU8le();
                _driverVersionMajor = m_io.ReadU4le();
                _driverVersionMinor = m_io.ReadU4le();
                _imageFilePathName = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(1306), 0, false));
                __unnamed23 = m_io.ReadBytes(6);
                _voltageRange = m_io.ReadU4le();
                _dataSoureName = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(100), 0, false));
                _busType = m_io.ReadU4le();
                _vendorId = m_io.ReadU4le();
                _productId = m_io.ReadU4le();
                __unnamed29 = m_io.ReadBytes(8);
            }
            private byte[] __unnamed0;
            private uint _softwareVersionMajor;
            private uint _softwareVersionMinor;
            private string _filePath;
            private ulong _timestamp;
            private uint _streamCount;
            private uint _millisamplesPerSecond;
            private byte[] __unnamed7;
            private ushort _electrodeChannels;
            private ushort _electrodeChannelOffset;
            private ushort _analogChannels;
            private ushort _analogChannelOffset;
            private ushort _digitalChannels;
            private ushort _digitalChannelOffset;
            private ushort _totalChannels;
            private byte[] __unnamed15;
            private ulong _timestampEnd;
            private byte[] __unnamed17;
            private uint _segmentTime;
            private ulong _timestampStart;
            private uint _driverVersionMajor;
            private uint _driverVersionMinor;
            private string _imageFilePathName;
            private byte[] __unnamed23;
            private uint _voltageRange;
            private string _dataSoureName;
            private uint _busType;
            private uint _vendorId;
            private uint _productId;
            private byte[] __unnamed29;
            private Mcdfile m_root;
            private Mcdfile.Header m_parent;
            public byte[] Unnamed_0 { get { return __unnamed0; } }
            public uint SoftwareVersionMajor { get { return _softwareVersionMajor; } }
            public uint SoftwareVersionMinor { get { return _softwareVersionMinor; } }
            public string FilePath { get { return _filePath; } }
            public ulong Timestamp { get { return _timestamp; } }
            public uint StreamCount { get { return _streamCount; } }
            public uint MillisamplesPerSecond { get { return _millisamplesPerSecond; } }
            public byte[] Unnamed_7 { get { return __unnamed7; } }
            public ushort ElectrodeChannels { get { return _electrodeChannels; } }
            public ushort ElectrodeChannelOffset { get { return _electrodeChannelOffset; } }
            public ushort AnalogChannels { get { return _analogChannels; } }
            public ushort AnalogChannelOffset { get { return _analogChannelOffset; } }
            public ushort DigitalChannels { get { return _digitalChannels; } }
            public ushort DigitalChannelOffset { get { return _digitalChannelOffset; } }
            public ushort TotalChannels { get { return _totalChannels; } }
            public byte[] Unnamed_15 { get { return __unnamed15; } }
            public ulong TimestampEnd { get { return _timestampEnd; } }
            public byte[] Unnamed_17 { get { return __unnamed17; } }
            public uint SegmentTime { get { return _segmentTime; } }
            public ulong TimestampStart { get { return _timestampStart; } }
            public uint DriverVersionMajor { get { return _driverVersionMajor; } }
            public uint DriverVersionMinor { get { return _driverVersionMinor; } }
            public string ImageFilePathName { get { return _imageFilePathName; } }
            public byte[] Unnamed_23 { get { return __unnamed23; } }
            public uint VoltageRange { get { return _voltageRange; } }
            public string DataSoureName { get { return _dataSoureName; } }
            public uint BusType { get { return _busType; } }
            public uint VendorId { get { return _vendorId; } }
            public uint ProductId { get { return _productId; } }
            public byte[] Unnamed_29 { get { return __unnamed29; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.Header M_Parent { get { return m_parent; } }
        }
        public partial class McdData : KaitaiStruct
        {
            public static McdData FromFile(string fileName)
            {
                return new McdData(new KaitaiStream(fileName));
            }

            public McdData(KaitaiStream p__io, Mcdfile p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _chunks = new List<StreamChunk>();
                {
                    var i = 0;
                    StreamChunk M_;
                    do {
                        M_ = new StreamChunk(m_io, this, m_root);
                        _chunks.Add(M_);
                        i++;
                    } while (!((long)(M_Parent.Idxheader.FinalIndexPointer - 1) < M_Root.M_Io.Pos));
                }
            }
            private List<StreamChunk> _chunks;
            private Mcdfile m_root;
            private Mcdfile m_parent;
            public List<StreamChunk> Chunks { get { return _chunks; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile M_Parent { get { return m_parent; } }
        }
        public partial class HeaderUnknown : KaitaiStruct
        {
            public static HeaderUnknown FromFile(string fileName)
            {
                return new HeaderUnknown(new KaitaiStream(fileName));
            }

            public HeaderUnknown(KaitaiStream p__io, Mcdfile.Header p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                // _content = m_io.ReadBytesFull(); // Do not read this
                m_io.Seek(m_io.BaseStream.Length);
            }
            private byte[] _content;
            private Mcdfile m_root;
            private Mcdfile.Header m_parent;
            public byte[] Content { get { return _content; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.Header M_Parent { get { return m_parent; } }
        }
        public partial class ChannelInfo : KaitaiStruct
        {
            public static ChannelInfo FromFile(string fileName)
            {
                return new ChannelInfo(new KaitaiStream(fileName));
            }

            public ChannelInfo(KaitaiStream p__io, Mcdfile.StreamHeader p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _version = m_io.ReadU2le();
                __unnamed1 = m_io.ReadBytes(2);
                _id = m_io.ReadU4le();
                _hwid = m_io.ReadU4le();
                _name = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(257), 0, false));
                _comment = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(513), 0, false));
                __unnamed6 = m_io.ReadBytes(2);
                _refCount = m_io.ReadU4le();
                _groupId = m_io.ReadU2le();
                if (Version >= 1) {
                    _decoratedName = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(257), 0, false));
                }
                __unnamed10 = m_io.ReadBytes((Version >= 1 ? 1 : 2));
                _gain = m_io.ReadU4le();
                if (Version >= 2) {
                    _rest1 = new List<uint>((int) (7));
                    for (var i = 0; i < 7; i++)
                    {
                        _rest1.Add(m_io.ReadU4le());
                    }
                }
                if (Version >= 2) {
                    _posY = m_io.ReadU4le();
                }
                if (Version >= 2) {
                    _posX = m_io.ReadU4le();
                }
                if (Version >= 2) {
                    _rest2 = new List<uint>((int) (4));
                    for (var i = 0; i < 4; i++)
                    {
                        _rest2.Add(m_io.ReadU4le());
                    }
                }
                if (RefCount != 0) {
                    _refCh = new List<RefchrHeader>((int) (RefCount));
                    for (var i = 0; i < RefCount; i++)
                    {
                        _refCh.Add(new RefchrHeader(m_io, this, m_root));
                    }
                }
            }
            private ushort _version;
            private byte[] __unnamed1;
            private uint _id;
            private uint _hwid;
            private string _name;
            private string _comment;
            private byte[] __unnamed6;
            private uint _refCount;
            private ushort _groupId;
            private string _decoratedName;
            private byte[] __unnamed10;
            private uint _gain;
            private List<uint> _rest1;
            private uint? _posY;
            private uint? _posX;
            private List<uint> _rest2;
            private List<RefchrHeader> _refCh;
            private Mcdfile m_root;
            private Mcdfile.StreamHeader m_parent;
            public ushort Version { get { return _version; } }
            public byte[] Unnamed_1 { get { return __unnamed1; } }
            public uint Id { get { return _id; } }
            public uint Hwid { get { return _hwid; } }
            public string Name { get { return _name; } }
            public string Comment { get { return _comment; } }
            public byte[] Unnamed_6 { get { return __unnamed6; } }
            public uint RefCount { get { return _refCount; } }
            public ushort GroupId { get { return _groupId; } }
            public string DecoratedName { get { return _decoratedName; } }
            public byte[] Unnamed_10 { get { return __unnamed10; } }
            public uint Gain { get { return _gain; } }
            public List<uint> Rest1 { get { return _rest1; } }
            public uint? PosY { get { return _posY; } }
            public uint? PosX { get { return _posX; } }
            public List<uint> Rest2 { get { return _rest2; } }
            public List<RefchrHeader> RefCh { get { return _refCh; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.StreamHeader M_Parent { get { return m_parent; } }
        }
        public partial class StreamChunk : KaitaiStruct
        {
            public static StreamChunk FromFile(string fileName)
            {
                return new StreamChunk(new KaitaiStream(fileName));
            }

            public StreamChunk(KaitaiStream p__io, Mcdfile.McdData p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _name = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(8), 0, false));
                _chunkLen = m_io.ReadU8le();
                _timestampStart = m_io.ReadU8le();
                _timestampEnd = m_io.ReadU8le();
                // _chunkSample = m_io.ReadBytes((ChunkLen - 16)); // Do not load the entire file into memory on opening
                _dataAddr = m_io.Pos;
                m_io.Seek(m_io.Pos + (long)(ChunkLen - 16)); // Skip chunk for now
            }
            private string _name;
            private ulong _chunkLen;
            private ulong _timestampStart;
            private ulong _timestampEnd;
            private byte[] _chunkSample;
            private long _dataAddr;
            private Mcdfile m_root;
            private Mcdfile.McdData m_parent;
            public string Name { get { return _name; } }
            public ulong ChunkLen { get { return _chunkLen; } }
            public ulong TimestampStart { get { return _timestampStart; } }
            public ulong TimestampEnd { get { return _timestampEnd; } }
            public byte[] ChunkSample { get { return _chunkSample; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.McdData M_Parent { get { return m_parent; } }
        }
        public partial class IdxHeader : KaitaiStruct
        {
            public static IdxHeader FromFile(string fileName)
            {
                return new IdxHeader(new KaitaiStream(fileName));
            }

            public IdxHeader(KaitaiStream p__io, Mcdfile p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magicbytes = m_io.ReadBytes(8);
                if (!((KaitaiStream.ByteArrayCompare(Magicbytes, new byte[] { 73, 68, 88, 49, 80, 79, 83, 32 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 73, 68, 88, 49, 80, 79, 83, 32 }, Magicbytes, M_Io, "/types/idx_header/seq/0");
                }
                _size4Pointer = m_io.ReadU8le();
                _finalIndexPointer = m_io.ReadU8le();
            }
            private byte[] _magicbytes;
            private ulong _size4Pointer;
            private ulong _finalIndexPointer;
            private Mcdfile m_root;
            private Mcdfile m_parent;
            public byte[] Magicbytes { get { return _magicbytes; } }
            public ulong Size4Pointer { get { return _size4Pointer; } }
            public ulong FinalIndexPointer { get { return _finalIndexPointer; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile M_Parent { get { return m_parent; } }
        }
        public partial class Header : KaitaiStruct
        {
            public static Header FromFile(string fileName)
            {
                return new Header(new KaitaiStream(fileName));
            }

            public Header(KaitaiStream p__io, Mcdfile.McdHeaderlist p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _name = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(8), 0, false));
                _headersize = m_io.ReadU8le();
                switch (Name) {
                case "MCSHDR  ": {
                    __raw_content = m_io.ReadBytes(Headersize);
                    var io___raw_content = new KaitaiStream(__raw_content);
                    _content = new McsHeader(io___raw_content, this, m_root);
                    break;
                }
                case "STRMHDR ": {
                    __raw_content = m_io.ReadBytes(Headersize);
                    var io___raw_content = new KaitaiStream(__raw_content);
                    _content = new StreamHeader(io___raw_content, this, m_root);
                    break;
                }
                case "STRMFMT ": {
                    __raw_content = m_io.ReadBytes(Headersize);
                    var io___raw_content = new KaitaiStream(__raw_content);
                    _content = new StreamFormat(io___raw_content, this, m_root);
                    break;
                }
                default: {
                    __raw_content = m_io.ReadBytes(Headersize);
                    var io___raw_content = new KaitaiStream(__raw_content);
                    _content = new HeaderUnknown(io___raw_content, this, m_root);
                    break;
                }
                }
            }
            private string _name;
            private ulong _headersize;
            private KaitaiStruct _content;
            private Mcdfile m_root;
            private Mcdfile.McdHeaderlist m_parent;
            private byte[] __raw_content;
            public string Name { get { return _name; } }
            public ulong Headersize { get { return _headersize; } }
            public KaitaiStruct Content { get { return _content; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.McdHeaderlist M_Parent { get { return m_parent; } }
            public byte[] M_RawContent { get { return __raw_content; } }
        }
        public partial class RefchrHeader : KaitaiStruct
        {
            public static RefchrHeader FromFile(string fileName)
            {
                return new RefchrHeader(new KaitaiStream(fileName));
            }

            public RefchrHeader(KaitaiStream p__io, Mcdfile.ChannelInfo p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _bufferid = m_io.ReadU2le();
                _unknown = m_io.ReadBytes(2);
                _channelid = m_io.ReadU4le();
            }
            private ushort _bufferid;
            private byte[] _unknown;
            private uint _channelid;
            private Mcdfile m_root;
            private Mcdfile.ChannelInfo m_parent;
            public ushort Bufferid { get { return _bufferid; } }
            public byte[] Unknown { get { return _unknown; } }
            public uint Channelid { get { return _channelid; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.ChannelInfo M_Parent { get { return m_parent; } }
        }
        public partial class McdHeaderlist : KaitaiStruct
        {
            public static McdHeaderlist FromFile(string fileName)
            {
                return new McdHeaderlist(new KaitaiStream(fileName));
            }

            public McdHeaderlist(KaitaiStream p__io, Mcdfile.McdHeader p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _headers = new List<Header>();
                {
                    var i = 0;
                    while (!m_io.IsEof) {
                        _headers.Add(new Header(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<Header> _headers;
            private Mcdfile m_root;
            private Mcdfile.McdHeader m_parent;
            public List<Header> Headers { get { return _headers; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.McdHeader M_Parent { get { return m_parent; } }
        }
        public partial class McdHeader : KaitaiStruct
        {
            public static McdHeader FromFile(string fileName)
            {
                return new McdHeader(new KaitaiStream(fileName));
            }

            public McdHeader(KaitaiStream p__io, Mcdfile p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _magic = m_io.ReadBytes(8);
                if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 76, 73, 83, 84, 104, 100, 114, 32 }) == 0)))
                {
                    throw new ValidationNotEqualError(new byte[] { 76, 73, 83, 84, 104, 100, 114, 32 }, Magic, M_Io, "/types/mcd_header/seq/0");
                }
                _headerLen = m_io.ReadU8le();
                __raw_headerList = m_io.ReadBytes(HeaderLen);
                var io___raw_headerList = new KaitaiStream(__raw_headerList);
                _headerList = new McdHeaderlist(io___raw_headerList, this, m_root);
            }
            private byte[] _magic;
            private ulong _headerLen;
            private McdHeaderlist _headerList;
            private Mcdfile m_root;
            private Mcdfile m_parent;
            private byte[] __raw_headerList;
            public byte[] Magic { get { return _magic; } }
            public ulong HeaderLen { get { return _headerLen; } }
            public McdHeaderlist HeaderList { get { return _headerList; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile M_Parent { get { return m_parent; } }
            public byte[] M_RawHeaderList { get { return __raw_headerList; } }
        }
        private byte[] _magicbytes;
        private McdHeader _headerIndex;
        private IdxHeader _idxheader;
        private McdData _data;
        private byte[] _rest;
        private Mcdfile m_root;
        private KaitaiStruct m_parent;
        public byte[] Magicbytes { get { return _magicbytes; } }
        public McdHeader HeaderIndex { get { return _headerIndex; } }
        public IdxHeader Idxheader { get { return _idxheader; } }
        public McdData Data { get { return _data; } }
        public byte[] Rest { get { return _rest; } }
        public Mcdfile M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}

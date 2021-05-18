// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using System.Collections.Generic;

namespace Kaitai
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
            _header = new McdHeader(m_io, this, m_root);
            _idxheader = new IdxHeader(m_io, this, m_root);
            _data = new McdData(m_io, this, m_root);
            _rest = m_io.ReadBytesFull();
        }
        public partial class StreamHeader : KaitaiStruct
        {
            public static StreamHeader FromFile(string fileName)
            {
                return new StreamHeader(new KaitaiStream(fileName));
            }

            public StreamHeader(KaitaiStream p__io, Mcdfile.Stream p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _version = m_io.ReadU2le();
                _typename = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(9), 0, false));
                _streamname = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(257), 0, false));
                _comment = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(513), 0, false));
                __unnamed4 = m_io.ReadBytes(1);
                _streamid = m_io.ReadU2le();
                _millisamplespersecond = m_io.ReadU4le();
                _channelcount = m_io.ReadU4le();
                _bufferid = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(9), 0, false));
                __unnamed9 = m_io.ReadBytes(3);
                _channels = new List<StreamChannelinfo>((int)(Channelcount));
                for (var i = 0; i < Channelcount; i++)
                {
                    _channels.Add(new StreamChannelinfo(m_io, this, m_root));
                }
            }
            private ushort _version;
            private string _typename;
            private string _streamname;
            private string _comment;
            private byte[] __unnamed4;
            private ushort _streamid;
            private uint _millisamplespersecond;
            private uint _channelcount;
            private string _bufferid;
            private byte[] __unnamed9;
            private List<StreamChannelinfo> _channels;
            private Mcdfile m_root;
            private Mcdfile.Stream m_parent;
            public ushort Version { get { return _version; } }
            public string Typename { get { return _typename; } }
            public string Streamname { get { return _streamname; } }
            public string Comment { get { return _comment; } }
            public byte[] Unnamed_4 { get { return __unnamed4; } }
            public ushort Streamid { get { return _streamid; } }
            public uint Millisamplespersecond { get { return _millisamplespersecond; } }
            public uint Channelcount { get { return _channelcount; } }
            public string Bufferid { get { return _bufferid; } }
            public byte[] Unnamed_9 { get { return __unnamed9; } }
            public List<StreamChannelinfo> Channels { get { return _channels; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.Stream M_Parent { get { return m_parent; } }
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
                    do
                    {
                        M_ = new StreamChunk(m_io, this, m_root);
                        _chunks.Add(M_);
                        i++;
                    } while (!(((long) M_Parent.Idxheader.Finalindexpointer - 1) < M_Root.M_Io.Pos));
                }
            }
            private List<StreamChunk> _chunks;
            private Mcdfile m_root;
            private Mcdfile m_parent;
            public List<StreamChunk> Chunks { get { return _chunks; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile M_Parent { get { return m_parent; } }
        }
        public partial class Stream : KaitaiStruct
        {
            public static Stream FromFile(string fileName)
            {
                return new Stream(new KaitaiStream(fileName));
            }

            public Stream(KaitaiStream p__io, Mcdfile.McdStreamlist p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _name = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(8), 0, false));
                _streamsize = m_io.ReadU8le();
                switch (Name)
                {
                    case "STRMHDR ":
                        {
                            __raw_content = m_io.ReadBytes(Streamsize);
                            var io___raw_content = new KaitaiStream(__raw_content);
                            _content = new StreamHeader(io___raw_content, this, m_root);
                            break;
                        }
                    default:
                        {
                            __raw_content = m_io.ReadBytes(Streamsize);
                            var io___raw_content = new KaitaiStream(__raw_content);
                            _content = new StreamUnknown(io___raw_content, this, m_root);
                            break;
                        }
                }
            }
            private string _name;
            private ulong _streamsize;
            private KaitaiStruct _content;
            private Mcdfile m_root;
            private Mcdfile.McdStreamlist m_parent;
            private byte[] __raw_content;
            public string Name { get { return _name; } }
            public ulong Streamsize { get { return _streamsize; } }
            public KaitaiStruct Content { get { return _content; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.McdStreamlist M_Parent { get { return m_parent; } }
            public byte[] M_RawContent { get { return __raw_content; } }
        }
        public partial class StreamUnknown : KaitaiStruct
        {
            public static StreamUnknown FromFile(string fileName)
            {
                return new StreamUnknown(new KaitaiStream(fileName));
            }

            public StreamUnknown(KaitaiStream p__io, Mcdfile.Stream p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _content = m_io.ReadBytesFull();
            }
            private byte[] _content;
            private Mcdfile m_root;
            private Mcdfile.Stream m_parent;
            public byte[] Content { get { return _content; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.Stream M_Parent { get { return m_parent; } }
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
                _chunklen = m_io.ReadU8le();
                _timestamp = m_io.ReadBytes(16);
                _chunksample = m_io.ReadBytes((Chunklen - 16));
            }
            private string _name;
            private ulong _chunklen;
            private byte[] _timestamp;
            private byte[] _chunksample;
            private Mcdfile m_root;
            private Mcdfile.McdData m_parent;
            public string Name { get { return _name; } }
            public ulong Chunklen { get { return _chunklen; } }
            public byte[] Timestamp { get { return _timestamp; } }
            public byte[] Chunksample { get { return _chunksample; } }
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
                _size4pointer = m_io.ReadU8le();
                _finalindexpointer = m_io.ReadU8le();
            }
            private byte[] _magicbytes;
            private ulong _size4pointer;
            private ulong _finalindexpointer;
            private Mcdfile m_root;
            private Mcdfile m_parent;
            public byte[] Magicbytes { get { return _magicbytes; } }
            public ulong Size4pointer { get { return _size4pointer; } }
            public ulong Finalindexpointer { get { return _finalindexpointer; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile M_Parent { get { return m_parent; } }
        }
        public partial class StreamChannelinfo : KaitaiStruct
        {
            public static StreamChannelinfo FromFile(string fileName)
            {
                return new StreamChannelinfo(new KaitaiStream(fileName));
            }

            public StreamChannelinfo(KaitaiStream p__io, Mcdfile.StreamHeader p__parent = null, Mcdfile p__root = null) : base(p__io)
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
                _refcount = m_io.ReadU4le();
                _groupid = m_io.ReadU2le();
                if (Version >= 1)
                {
                    _decoratedname = System.Text.Encoding.GetEncoding("ascii").GetString(KaitaiStream.BytesTerminate(m_io.ReadBytes(257), 0, false));
                }
                __unnamed10 = m_io.ReadBytes((Version >= 1 ? 1 : 2));
                _gain = m_io.ReadU4le();
                if (Version >= 2)
                {
                    _rest = new List<uint>((int)(13));
                    for (var i = 0; i < 13; i++)
                    {
                        _rest.Add(m_io.ReadU4le());
                    }
                }
                if (Refcount != 0)
                {
                    _refch = new List<RefchrHeader>((int)(Refcount));
                    for (var i = 0; i < Refcount; i++)
                    {
                        _refch.Add(new RefchrHeader(m_io, this, m_root));
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
            private uint _refcount;
            private ushort _groupid;
            private string _decoratedname;
            private byte[] __unnamed10;
            private uint _gain;
            private List<uint> _rest;
            private List<RefchrHeader> _refch;
            private Mcdfile m_root;
            private Mcdfile.StreamHeader m_parent;
            public ushort Version { get { return _version; } }
            public byte[] Unnamed_1 { get { return __unnamed1; } }
            public uint Id { get { return _id; } }
            public uint Hwid { get { return _hwid; } }
            public string Name { get { return _name; } }
            public string Comment { get { return _comment; } }
            public byte[] Unnamed_6 { get { return __unnamed6; } }
            public uint Refcount { get { return _refcount; } }
            public ushort Groupid { get { return _groupid; } }
            public string Decoratedname { get { return _decoratedname; } }
            public byte[] Unnamed_10 { get { return __unnamed10; } }
            public uint Gain { get { return _gain; } }
            public List<uint> Rest { get { return _rest; } }
            public List<RefchrHeader> Refch { get { return _refch; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.StreamHeader M_Parent { get { return m_parent; } }
        }
        public partial class RefchrHeader : KaitaiStruct
        {
            public static RefchrHeader FromFile(string fileName)
            {
                return new RefchrHeader(new KaitaiStream(fileName));
            }

            public RefchrHeader(KaitaiStream p__io, Mcdfile.StreamChannelinfo p__parent = null, Mcdfile p__root = null) : base(p__io)
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
            private Mcdfile.StreamChannelinfo m_parent;
            public ushort Bufferid { get { return _bufferid; } }
            public byte[] Unknown { get { return _unknown; } }
            public uint Channelid { get { return _channelid; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile.StreamChannelinfo M_Parent { get { return m_parent; } }
        }
        public partial class McdStreamlist : KaitaiStruct
        {
            public static McdStreamlist FromFile(string fileName)
            {
                return new McdStreamlist(new KaitaiStream(fileName));
            }

            public McdStreamlist(KaitaiStream p__io, Mcdfile.McdHeader p__parent = null, Mcdfile p__root = null) : base(p__io)
            {
                m_parent = p__parent;
                m_root = p__root;
                _read();
            }
            private void _read()
            {
                _streams = new List<Stream>();
                {
                    var i = 0;
                    while (!m_io.IsEof)
                    {
                        _streams.Add(new Stream(m_io, this, m_root));
                        i++;
                    }
                }
            }
            private List<Stream> _streams;
            private Mcdfile m_root;
            private Mcdfile.McdHeader m_parent;
            public List<Stream> Streams { get { return _streams; } }
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
                _headerlen = m_io.ReadU8le();
                __raw_streamlist = m_io.ReadBytes(Headerlen);
                var io___raw_streamlist = new KaitaiStream(__raw_streamlist);
                _streamlist = new McdStreamlist(io___raw_streamlist, this, m_root);
            }
            private byte[] _magic;
            private ulong _headerlen;
            private McdStreamlist _streamlist;
            private Mcdfile m_root;
            private Mcdfile m_parent;
            private byte[] __raw_streamlist;
            public byte[] Magic { get { return _magic; } }
            public ulong Headerlen { get { return _headerlen; } }
            public McdStreamlist Streamlist { get { return _streamlist; } }
            public Mcdfile M_Root { get { return m_root; } }
            public Mcdfile M_Parent { get { return m_parent; } }
            public byte[] M_RawStreamlist { get { return __raw_streamlist; } }
        }
        private byte[] _magicbytes;
        private McdHeader _header;
        private IdxHeader _idxheader;
        private McdData _data;
        private byte[] _rest;
        private Mcdfile m_root;
        private KaitaiStruct m_parent;
        public byte[] Magicbytes { get { return _magicbytes; } }
        public McdHeader Header { get { return _header; } }
        public IdxHeader Idxheader { get { return _idxheader; } }
        public McdData Data { get { return _data; } }
        public byte[] Rest { get { return _rest; } }
        public Mcdfile M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}

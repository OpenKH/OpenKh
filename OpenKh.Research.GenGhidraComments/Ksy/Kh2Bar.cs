// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace OpenKh.Research.GenGhidraComments.Ksy
{
    public partial class Kh2Bar : KaitaiStruct
    {
        public Tracer M_Tracer;

        public static Kh2Bar FromFile(string fileName)
        {
            return new Kh2Bar(new KaitaiStream(fileName));
        }

        public Kh2Bar(KaitaiStream p__io, KaitaiStruct p__parent = null, Kh2Bar p__root = null, Tracer tracer = null) : base(p__io)
        {
            M_Tracer = tracer;
            var entityName = nameof(Kh2Bar);
            m_parent = p__parent;
            m_root = p__root ?? this;
            M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
            _read();
            M_Tracer.EndRead();
        }
        private void _read()
        {
            M_Tracer.BeginMember(nameof(Magic));
            _magic = m_io.ReadBytes(4);
            M_Tracer.EndMember();
            if (!((KaitaiStream.ByteArrayCompare(Magic, new byte[] { 66, 65, 82, 1 }) == 0)))
            {
                throw new ValidationNotEqualError(new byte[] { 66, 65, 82, 1 }, Magic, M_Io, "/seq/0");
            }
            M_Tracer.BeginMember(nameof(NumFiles));
            _numFiles = m_io.ReadS4le();
            M_Tracer.EndMember();
            M_Tracer.BeginMember(nameof(Padding));
            _padding = m_io.ReadBytes(8);
            M_Tracer.EndMember();
            _files = new List<FileEntry>((int) (NumFiles));
            for (var i = 0; i < NumFiles; i++)
            {
                M_Tracer.BeginArrayMember(nameof(Files));
                _files.Add(new FileEntry(m_io, this, m_root, tracer: M_Tracer));
                M_Tracer.EndArrayMember();
            }
        }
        public partial class FileEntry : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static FileEntry FromFile(string fileName)
            {
                return new FileEntry(new KaitaiStream(fileName));
            }

            public FileEntry(KaitaiStream p__io, Kh2Bar p__parent = null, Kh2Bar p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(FileEntry);
                m_parent = p__parent;
                m_root = p__root;
                f_file = false;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Type));
                _type = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Duplicate));
                _duplicate = m_io.ReadU2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Name));
                _name = System.Text.Encoding.GetEncoding("UTF-8").GetString(m_io.ReadBytes(4));
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Offset));
                _offset = m_io.ReadS4le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Size));
                _size = m_io.ReadS4le();
                M_Tracer.EndMember();
            }
            private bool f_file;
            private object _file;
            public object File
            {
                get
                {
                    if (f_file)
                        return _file;
                    if (Size != 0) {
                        KaitaiStream io = M_Root.M_Io;
                        long _pos = io.Pos;
                        io.Seek(Offset);
                        M_Tracer.Seek(Offset);
                        switch (Type) {
                        case 17: {
                            __raw_file = io.ReadBytes(Size);
                            var io___raw_file = new KaitaiStream(__raw_file);
                            M_Tracer.DeclareNewIo();
                            M_Tracer.BeginMember(nameof(File));
                            _file = new Kh2Bar(io___raw_file, tracer: M_Tracer);
                            M_Tracer.EndMember();
                            break;
                        }
                        case 4: {
                            __raw_file = io.ReadBytes(Size);
                            var io___raw_file = new KaitaiStream(__raw_file);
                            M_Tracer.DeclareNewIo();
                            M_Tracer.BeginMember(nameof(File));
                            _file = new Kh2Model(io___raw_file, tracer: M_Tracer);
                            M_Tracer.EndMember();
                            break;
                        }
                        case 13: {
                            __raw_file = io.ReadBytes(Size);
                            var io___raw_file = new KaitaiStream(__raw_file);
                            M_Tracer.DeclareNewIo();
                            M_Tracer.BeginMember(nameof(File));
                            _file = new Kh2SpawnScript(io___raw_file, tracer: M_Tracer);
                            M_Tracer.EndMember();
                            break;
                        }
                        case 12: {
                            __raw_file = io.ReadBytes(Size);
                            var io___raw_file = new KaitaiStream(__raw_file);
                            M_Tracer.DeclareNewIo();
                            M_Tracer.BeginMember(nameof(File));
                            _file = new Kh2SpawnPoint(io___raw_file, tracer: M_Tracer);
                            M_Tracer.EndMember();
                            break;
                        }
                        case 9: {
                            __raw_file = io.ReadBytes(Size);
                            var io___raw_file = new KaitaiStream(__raw_file);
                            M_Tracer.DeclareNewIo();
                            M_Tracer.BeginMember(nameof(File));
                            _file = new Kh2Motion(io___raw_file, tracer: M_Tracer);
                            M_Tracer.EndMember();
                            break;
                        }
                        case 18: {
                            __raw_file = io.ReadBytes(Size);
                            var io___raw_file = new KaitaiStream(__raw_file);
                            M_Tracer.DeclareNewIo();
                            M_Tracer.BeginMember(nameof(File));
                            _file = new Kh2Pax(io___raw_file, tracer: M_Tracer);
                            M_Tracer.EndMember();
                            break;
                        }
                        default: {
                            M_Tracer.BeginMember(nameof(File));
                            _file = io.ReadBytes(Size);
                            M_Tracer.EndMember();
                            break;
                        }
                        }
                        io.Seek(_pos);
                        f_file = true;
                    }
                    return _file;
                }
            }
            private ushort _type;
            private ushort _duplicate;
            private string _name;
            private int _offset;
            private int _size;
            private Kh2Bar m_root;
            private Kh2Bar m_parent;
            private byte[] __raw_file;
            public ushort Type { get { return _type; } }
            public ushort Duplicate { get { return _duplicate; } }
            public string Name { get { return _name; } }
            public int Offset { get { return _offset; } }
            public int Size { get { return _size; } }
            public Kh2Bar M_Root { get { return m_root; } }
            public Kh2Bar M_Parent { get { return m_parent; } }
            public byte[] M_RawFile { get { return __raw_file; } }
        }
        private byte[] _magic;
        private int _numFiles;
        private byte[] _padding;
        private List<FileEntry> _files;
        private Kh2Bar m_root;
        private KaitaiStruct m_parent;
        public byte[] Magic { get { return _magic; } }
        public int NumFiles { get { return _numFiles; } }
        public byte[] Padding { get { return _padding; } }
        public List<FileEntry> Files { get { return _files; } }
        public Kh2Bar M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}

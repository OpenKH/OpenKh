// This is a generated file! Please edit source .ksy file and use kaitai-struct-compiler to rebuild

using Kaitai;
using System.Collections.Generic;

namespace OpenKh.Research.GenGhidraComments.Ksy
{
    public partial class Kh2SpawnScript : KaitaiStruct
    {
        public Tracer M_Tracer;

        public static Kh2SpawnScript FromFile(string fileName)
        {
            return new Kh2SpawnScript(new KaitaiStream(fileName));
        }

        public Kh2SpawnScript(KaitaiStream p__io, KaitaiStruct p__parent = null, Kh2SpawnScript p__root = null, Tracer tracer = null) : base(p__io)
        {
            M_Tracer = tracer;
            var entityName = nameof(Kh2SpawnScript);
            m_parent = p__parent;
            m_root = p__root ?? this;
            M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
            _read();
            M_Tracer.EndRead();
        }
        private void _read()
        {
            _program = new List<SpawnProgram>();
            {
                var i = 0;
                SpawnProgram M_;
                do {
                    M_Tracer.BeginArrayMember(nameof(Program));
                    M_ = new SpawnProgram(m_io, this, m_root, tracer: M_Tracer);
                    _program.Add(M_);
                    M_Tracer.EndArrayMember();
                    i++;
                } while (!(M_.Id == -1));
            }
        }
        public partial class SpawnProgram : KaitaiStruct
        {
            public Tracer M_Tracer;

            public static SpawnProgram FromFile(string fileName)
            {
                return new SpawnProgram(new KaitaiStream(fileName));
            }

            public SpawnProgram(KaitaiStream p__io, Kh2SpawnScript p__parent = null, Kh2SpawnScript p__root = null, Tracer tracer = null) : base(p__io)
            {
                M_Tracer = tracer;
                var entityName = nameof(SpawnProgram);
                m_parent = p__parent;
                m_root = p__root;
                M_Tracer.BeginRead(entityName, this, p__io, p__parent, p__root);
                _read();
                M_Tracer.EndRead();
            }
            private void _read()
            {
                M_Tracer.BeginMember(nameof(Id));
                _id = m_io.ReadS2le();
                M_Tracer.EndMember();
                M_Tracer.BeginMember(nameof(Length));
                _length = m_io.ReadS2le();
                M_Tracer.EndMember();
                if (Id != -1) {
                    _byteCode = new List<byte>((int) ((Length - 4)));
                    for (var i = 0; i < (Length - 4); i++)
                    {
                        M_Tracer.BeginArrayMember(nameof(ByteCode));
                        _byteCode.Add(m_io.ReadU1());
                        M_Tracer.EndArrayMember();
                    }
                }
            }
            private short _id;
            private short _length;
            private List<byte> _byteCode;
            private Kh2SpawnScript m_root;
            private Kh2SpawnScript m_parent;
            public short Id { get { return _id; } }
            public short Length { get { return _length; } }
            public List<byte> ByteCode { get { return _byteCode; } }
            public Kh2SpawnScript M_Root { get { return m_root; } }
            public Kh2SpawnScript M_Parent { get { return m_parent; } }
        }
        private List<SpawnProgram> _program;
        private Kh2SpawnScript m_root;
        private KaitaiStruct m_parent;
        public List<SpawnProgram> Program { get { return _program; } }
        public Kh2SpawnScript M_Root { get { return m_root; } }
        public KaitaiStruct M_Parent { get { return m_parent; } }
    }
}

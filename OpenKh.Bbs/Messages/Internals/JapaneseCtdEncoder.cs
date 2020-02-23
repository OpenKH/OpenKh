using System.Collections.Generic;

namespace OpenKh.Bbs.Messages.Internals
{
    internal class JapaneseCtdEncoder : ICtdMessageEncoder
    {
        internal JapaneseCtdEncoder()
        {
            throw new System.NotImplementedException();
        }

        public string ToText(byte[] data)
        {
            throw new System.NotImplementedException();
        }

        public byte[] FromText(string text)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<byte> FromUcs(IEnumerable<ushort> ucs)
        {
            throw new System.NotImplementedException();
        }

        public IEnumerable<ushort> ToUcs(IEnumerable<byte> data)
        {
            throw new System.NotImplementedException();
        }
    }
}

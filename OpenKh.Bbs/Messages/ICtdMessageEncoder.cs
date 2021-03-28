using System.Collections.Generic;

namespace OpenKh.Bbs.Messages
{
    public interface ICtdMessageEncoder : ICtdMessageDecode, ICtdMessageEncode
    {
        IEnumerable<ushort> ToUcs(IEnumerable<byte> data);

        IEnumerable<byte> FromUcs(IEnumerable<ushort> ucs);
    }
}

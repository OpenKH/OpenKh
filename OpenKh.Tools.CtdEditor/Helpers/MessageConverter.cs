using OpenKh.Bbs.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.CtdEditor.Helpers
{
    public record MessageConverter(
        Func<string, byte[]> FromText,
        Func<byte[], string> ToText
    )
    {
        public static MessageConverter Default { get; } = new MessageConverter(
            FromText: text => CtdEncoders.Unified.FromText(text),
            ToText: bytes => CtdEncoders.Unified.ToText(bytes)
        );

        public static MessageConverter ShiftJIS { get; } = new MessageConverter(
            FromText: text => CtdEncoders.Unified.FromText(text, new CtdFromTextOptions { EncodeAsShiftJIS = true, }),
            ToText: bytes => CtdEncoders.Unified.ToText(bytes, new CtdToTextOptions { DecodeAsShiftJIS = true, })
        );
    }
}

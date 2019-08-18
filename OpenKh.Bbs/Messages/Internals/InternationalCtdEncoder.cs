using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKh.Bbs.Messages.Internals
{
    internal class InternationalCtdEncoder : ICtdMessageEncoder
    {
        private static readonly string _mapping0 =
            " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[¥]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        private static readonly string _mapping99 =
            "ÀÁÂÄÆÇÈÉÊËÌÍÎÏÑÒÓÔÕÖŒÙÚÛÜßàáâäæçèéêëìíîïñòóôõöùúûüœ¿¡";

        private static readonly Dictionary<char, int> _inverseMapping = _mapping0
            .Select((x, i) => new { Ch = x, Data = i + 0x20 })
            .ToDictionary(x => x.Ch, x => x.Data);

        public string Decode(byte[] data)
        {
            var builder = new StringBuilder(data.Length * 3 / 2);
            for (var i = 0; i < data.Length;)
            {
                builder.Append(GetCharacter(data, ref i));
            }

            return builder.ToString();
        }

        public byte[] Encode(string text)
        {
            var encoded = new List<byte>(text.Length * 3 / 2);
            foreach (var ch in text)
            {
                var data = _inverseMapping[ch];
                
                if (data < 0x100)
                    encoded.Add((byte)data);
            }

            return encoded.ToArray();
        }

        private static char GetCharacter(byte[] data, ref int index)
        {
            var ch = data[index++];

            if (ch >= 0x00 && ch < 0x20)
                return (char)ch;

            if (ch >= 0x20 && ch < 0x80)
                return _mapping0[ch - 0x20];

            var ch2 = data[index++];
            switch (ch)
            {
                case 0x99:
                    return _mapping99[ch2 - 0x80];
            }

            throw new Exception($"Data {ch:X02} cannot be decoded.");
        }
    }
}

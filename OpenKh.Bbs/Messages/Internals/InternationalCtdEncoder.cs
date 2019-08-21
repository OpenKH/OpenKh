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
        private static readonly string _mapping81 =
            "$??%#&*@";
        private static readonly string _mapping99 =
            "ÀÁÂÄÆÇÈÉÊËÌÍÎÏÑÒÓÔÕÖŒÙÚÛÜßàáâäæçèéêëìíîïñòóôõöùúûüœ¿¡‚„—°«»≤≥❤¹²³⁴⁵£€§·¢¨‘’©®™‾ₐ";
        private static readonly Dictionary<byte, string> _mappingF1 = new Dictionary<byte, string>
        {
            [0xae] = "button-triangle",
            [0xaf] = "button-circle",
            [0xb0] = "button-square",
            [0xb1] = "button-cross",
            [0xb2] = "button-analog",
            [0xb3] = "button-r",
            [0xb4] = "button-l",
            [0xc3] = "button-dpad",
            [0xc4] = "button-dpad-h",
            [0xc5] = "button-dpad-v",
        };
        private static readonly Dictionary<byte, string> _mappingF9 = new Dictionary<byte, string>
        {
            [0x41] = "default",
            [0x58] = "white",
            [0x59] = "yellow",
        };

        private static readonly Dictionary<char, int> _inverseMapping = _mapping0
            .Select((x, i) => new { Ch = x, Data = i + 0x20 })
            .Concat(_mapping81.Select((x, i) => new { Ch = x, Data = 0x8190 + i }))
            .Concat(_mapping99.Select((x, i) => new { Ch = x, Data = 0x9980 + i }))
            .GroupBy(x => x.Ch)
            .Select(x => x.First())
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
                else
                {
                    encoded.Add((byte)(data >> 8));
                    encoded.Add((byte)(data & 0xFF));
                }
            }

            return encoded.ToArray();
        }

        private static string GetCharacter(byte[] data, ref int index)
        {
            var ch = data[index++];

            if (ch >= 0x00 && ch < 0x20)
                return $"{(char)ch}";

            if (ch >= 0x20 && ch < 0x80)
                return $"{_mapping0[ch - 0x20]}";

            var param = data[index++];
            switch (ch)
            {
                case 0x81:
                    return $"{_mapping81[param - 0x90]}";
                case 0x99:
                    return $"{_mapping99[param - 0x80]}";
                default:
                    return GetCommand(ch, param);
            }
        }

        private static string GetCommand(byte command, byte param)
        {
            string name;
            string value;

            switch (command)
            {
                case 0xf1:
                    name = "icon";
                    value = _mappingF1[param];
                    break;
                case 0xf9:
                    name = "color";
                    value = _mappingF9[param];
                    break;
                default:
                    throw new Exception($"Data \"{command:X02} {param:X02}\" cannot be decoded.");
            }

            return $"{{:{name} {value}}}";
        }
    }
}

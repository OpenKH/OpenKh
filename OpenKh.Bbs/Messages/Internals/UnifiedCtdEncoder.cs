using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKh.Bbs.Messages.Internals
{
    internal class UnifiedCtdEncoder : ICtdMessageEncoder
    {
        private static readonly ushort[] _jisToUcs = new ushort[0x60]
        {
            // ■！”＃＄％＆’
            0x81a1, 0x8149, 0x8168, 0x8194, 0x8190, 0x8193, 0x8195, 0x8166,
            // （）＊＋，－．／
            0x8169, 0x816a, 0x8196, 0x817b, 0x8143, 0x817c, 0x8144, 0x815e,
            // ０１２３４５６７
            0x824f, 0x8250, 0x8251, 0x8252, 0x8253, 0x8254, 0x8255, 0x8256,
            // ８９：；＜＝＞？
            0x8257, 0x8258, 0x8146, 0x8147, 0x8183, 0x8181, 0x8184, 0x8148,
            // ＠ＡＢＣＤＥＦＧ
            0x8197, 0x8260, 0x8261, 0x8262, 0x8263, 0x8264, 0x8265, 0x8266,
            // ＨＩＪＫＬＭＮＯ
            0x8267, 0x8268, 0x8269, 0x826a, 0x826b, 0x826c, 0x826d, 0x826e,
            // ＰＱＲＳＴＵＶＷ
            0x826f, 0x8270, 0x8271, 0x8272, 0x8273, 0x8274, 0x8275, 0x8276,
            // ＸＹＺ［￥］＾＿
            0x8277, 0x8278, 0x8279, 0x816d, 0x818f, 0x816e, 0x814f, 0x8151,
            // 卷ａｂｃｄｅｆｇ
            0x99c9, 0x8281, 0x8282, 0x8283, 0x8284, 0x8285, 0x8286, 0x8287,
            // ｈｉｊｋｌｍｎｏ
            0x8288, 0x8289, 0x828a, 0x828b, 0x828c, 0x828d, 0x828e, 0x828f,
            // ｐｑｒｓｔｕｖｗ
            0x8290, 0x8291, 0x8292, 0x8293, 0x8294, 0x8295, 0x8296, 0x8297,
            // ｘｙｚ｛｜｝～
            0x8298, 0x8299, 0x829a, 0x816f, 0x8162, 0x8170, 0x8160, 0x007f,
        };

        private static readonly string _mapping0 =
            " !\"#$%&'()*+,-./0123456789:;<=>?@ABCDEFGHIJKLMNOPQRSTUVWXYZ[¥]^_`abcdefghijklmnopqrstuvwxyz{|}~";
        private static readonly string _mapping81 =
            " ､｡,.·:;?!_____^¯__________–-_/\\~-|…‥‘’“”()__[]{}⟨⟩⟪⟫「」『』__+-±×·÷=≠<>≤≥∞∴♂♀°′″_¥$¢£%#&*@§☆★○●◎◇◆□■△▲▽▼※〒→←↑↓________________________________________________________________________♪†‡¶________";
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
        private static readonly Dictionary<byte, string> _mappingF2 = new Dictionary<byte, string>
        {
            [0xF6] = "unk"
        };
        private static readonly Dictionary<byte, string> _mappingF5 = new Dictionary<byte, string>
        {
            [0x67] = "unk",
            [0x76] = "button-dpad-and-analog"
        };
        private static readonly Dictionary<byte, string> _mappingF9 = new Dictionary<byte, string>
        {
            [0x41] = "default",
            [0x58] = "white",
            [0x59] = "yellow",
        };

        private static readonly Dictionary<byte, (string CommandName, Dictionary<byte, string> Mapping)> _firstByteToMapping
            = new Dictionary<byte, (string, Dictionary<byte, string>)>
            {
                [0xf1] = ("icon", _mappingF1),
                [0xf2] = ("icon", _mappingF2),
                [0xf5] = ("icon", _mappingF5),
                [0xf9] = ("color", _mappingF9),
            };

        private static readonly Dictionary<char, int> _inverseMapping = _mapping0
            .Select((x, i) => new { Ch = x, Data = i + 0x20 })
            .Concat(_mapping81.Select((x, i) => new { Ch = x, Data = 0x8190 + i }))
            .Concat(_mapping99.Select((x, i) => new { Ch = x, Data = 0x9980 + i }))
            .GroupBy(x => x.Ch)
            .Select(x => x.First())
            .ToDictionary(x => x.Ch, x => x.Data);

        public string ToText(byte[] data, CtdToTextOptions opts = null)
        {
            opts ??= new CtdToTextOptions();

            var builder = new StringBuilder(data.Length * 3 / 2);
            for (var i = 0; i < data.Length;)
            {
                builder.Append(GetCharacter(data, ref i, opts.AllowUnkUsage, opts.DecodeAsShiftJIS));
            }

            return builder.ToString();
        }

        public byte[] FromText(string text, CtdFromTextOptions opts = null)
        {
            opts ??= new CtdFromTextOptions();

            var encoded = new List<byte>(text.Length * 3 / 2);

            for (int i = 0; i < text.Length;)
            {
                var ch = text[i++];
                if (ch == '{' && i + 3 < text.Length && text[i] == ':')
                {
                    // `{:unk ff}`, `{:icon unk}`, `{:color white}`, or such
                    int end = text.IndexOf('}', i);
                    if (end < 0)
                        throw new Exception("Invalid command format (need closing bracket)");

                    // inside => `unk ff`
                    var inside = text.Substring(i + 1, end - (i + 1));

                    var spaceAt = inside.IndexOf(' ');
                    if (spaceAt < 0)
                        throw new Exception("Invalid command format (need space between command name and argument)");

                    var commandName = inside.Substring(0, spaceAt).Trim();
                    var commandArg = inside.Substring(spaceAt + 1).Trim();

                    if (commandName == "unk")
                    {
                        var hexByte = Convert.ToByte(commandArg, 16);
                        encoded.Add(hexByte);
                    }
                    else
                    {
                        var firstBytePair = _firstByteToMapping
                            .FirstOrDefault(it => it.Value.CommandName == commandName);

                        if (firstBytePair.Value.Mapping is var mapping && mapping != null)
                        {
                            var data = mapping.FirstOrDefault(it => it.Value == commandArg);
                            if (data.Value == null)
                                throw new Exception($"Unknown {commandName} name: {commandArg}");
                            encoded.Add(firstBytePair.Key);
                            encoded.Add(data.Key);
                        }
                        else
                        {
                            throw new Exception($"Unknown command name: {commandName}");
                        }
                    }

                    i = end + 1;
                    continue;
                }

                if (ch >= 0x20)
                {
                    if (true
                        && opts.EncodeAsShiftJIS
                        && ShiftJISTable.TryToEncodeToShiftJISCharacter(ch) is ushort sjis
                    )
                    {
                        encoded.Add((byte)(sjis >> 8));
                        encoded.Add((byte)(sjis & 0xFF));
                    }
                    else
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
                }
                else
                    encoded.Add((byte)ch);
            }

            return encoded.ToArray();
        }

        private static string GetCharacter(
            byte[] data,
            ref int index,
            bool allowUnkUsage,
            bool decodeAsShiftJIS
        )
        {
            var ch = data[index++];

            if (ch >= 0x00 && ch < 0x20)
                return $"{(char)ch}";

            if (ch >= 0x20 && ch < 0x80)
                return $"{_mapping0[ch - 0x20]}";

            if (decodeAsShiftJIS)
            {
                if (ShiftJISTable.IsLeadingByteOfShiftJIS(ch, out int lineIndex))
                {
                    var param = data[index++];
                    var sjisChar = ShiftJISTable.GetShiftJISCharFromTrailingByte(param, lineIndex);
                    if (sjisChar != null)
                    {
                        return sjisChar.Value.ToString();
                    }
                    else
                    {
                        throw new Exception($"Data \"{ch:X02} {param:X02}\" cannot be decoded as Shift_JIS.");
                    }
                }
            }

            if (ch == 0x81)
            {
                var param = data[index++];
                return $"{_mapping81[param - 0x40]}";
            }

            if (ch == 0x99)
            {
                var param = data[index++];
                return $"{_mapping99[param - 0x80]}";
            }

            if (_firstByteToMapping.TryGetValue(ch, out var tuple))
            {
                var param = data[index++];
                if (tuple.Mapping.TryGetValue(param, out string commandArg))
                {
                    return ComposeCommand(tuple.CommandName, commandArg);
                }
            }

            if (allowUnkUsage)
            {
                return ComposeCommand("unk", ch.ToString("x02"));
            }

            {
                var param = data[index++];
                throw new Exception($"Data \"{ch:X02} {param:X02}\" cannot be decoded.");
            }
        }

        private static string ComposeCommand(string name, string value)
            => $"{{:{name} {value}}}";

        public IEnumerable<ushort> ToUcs(IEnumerable<byte> data)
        {
            var enumerator = data.GetEnumerator();
            while (enumerator.MoveNext())
            {
                var ch = enumerator.Current;
                if (ch < 0x21)
                    yield return ch;
                else if (ch < 0x80)
                    yield return _jisToUcs[ch - 0x20];
                else if (ch < 0xa0)
                {
                    if (!enumerator.MoveNext())
                        yield break;
                    else
                        yield return (ushort)((ch << 8) | enumerator.Current);
                }
                else if (ch < 0xe0)
                    yield return ch; // TODO convert 0xa0-0xdf characters to UCS
                else if (ch < 0xf0)
                {
                    if (!enumerator.MoveNext())
                        yield break;
                    else
                        yield return (ushort)((ch << 8) | enumerator.Current);
                }
                else
                    yield return ch;
            }
        }

        public IEnumerable<byte> FromUcs(IEnumerable<ushort> ucs)
        {
            foreach (var ch in ucs)
            {
                var index = 0x20;
                foreach (var digit in _jisToUcs)
                {
                    if (ch == digit)
                        yield return (byte)index;
                    index++;
                }

                yield return (byte)(ch >> 8);
                yield return (byte)(ch & 0xff);
            }
        }
    }
}

using OpenKh.Common.Exceptions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;

namespace OpenKh.Kh2.Messages
{
    public partial class MsgSerializer
    {
        private class SerializerModel
        {
            public string Name { get; set; }
            public MessageCommand Command { get; set; }
            public Func<MessageCommandModel, string> Serializer { get; set; }
            public Func<string, byte[]> Deserializer { get; set; }
        }

        private static List<SerializerModel> _serializeModel = new List<SerializerModel>
        {
            new SerializerModel
            {
                Name = "end",
                Command = MessageCommand.End
            },
            new SerializerModel
            {
                Name = "text",
                Command = MessageCommand.PrintText,
                Serializer = x => x.Text,
                Deserializer = null
            },
            new SerializerModel
            {
                Name = "complex",
                Command = MessageCommand.PrintComplex,
                Serializer = x => x.Text,
                Deserializer = null
            },
            new SerializerModel
            {
                Name = "tabulation",
                Command = MessageCommand.Tabulation,
            },
            new SerializerModel
            {
                Name = "reset",
                Command = MessageCommand.Reset,
            },
            new SerializerModel
            {
                Name = "theme",
                Command = MessageCommand.Theme,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "unk05",
                Command = MessageCommand.Unknown05,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "unk06",
                Command = MessageCommand.Unknown06,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "color",
                Command = MessageCommand.Color,
                Serializer = x => $"#{x.Data[0]:X02}{x.Data[1]:X02}{x.Data[2]:X02}{x.Data[3]:X02}",
                Deserializer = x => DeserializeColor(x)
            },
            new SerializerModel
            {
                Name = "unk08",
                Command = MessageCommand.Unknown08,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "icon",
                Command = MessageCommand.PrintIcon,
                Serializer = x =>  _icons[x.Data[0]],
                Deserializer = x => DeserializeIcon(x)
            },
            new SerializerModel
            {
                Name = "scale",
                Command = MessageCommand.TextScale,
                Serializer = x => x.Data[0].ToString(),
                Deserializer = x => DeserializeScale(x)
            },
            new SerializerModel
            {
                Name = "width",
                Command = MessageCommand.TextWidth,
                Serializer = x => x.Data[0].ToString(),
                Deserializer = x => DeserializeWidth(x)
            },
            new SerializerModel
            {
                Name = "linespacing",
                Command = MessageCommand.LineSpacing,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "unk0d",
                Command = MessageCommand.Unknown0d,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => new byte[0]
            },
            new SerializerModel
            {
                Name = "unk0e",
                Command = MessageCommand.Unknown0e,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "unk0f",
                Command = MessageCommand.Unknown0f,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "clear",
                Command = MessageCommand.Clear,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => new byte[0]
            },
            new SerializerModel
            {
                Name = "position",
                Command = MessageCommand.Position,
                Serializer = x => ToPosition(x),
                Deserializer = x => FromPosition(x)
            },
            new SerializerModel
            {
                Name = "unk12",
                Command = MessageCommand.Unknown12,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "unk13",
                Command = MessageCommand.Unknown13,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "delay",
                Command = MessageCommand.Delay,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "chardelay",
                Command = MessageCommand.CharDelay,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "unk16",
                Command = MessageCommand.Unknown16,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "delayandfade",
                Command = MessageCommand.DelayAndFade,
                Serializer = x => ToDelayAndFade(x.Data),
                Deserializer = x => FromDelayAndFade(x)
            },
            new SerializerModel
            {
                Name = "unk18",
                Command = MessageCommand.Unknown18,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "t2",
                Command = MessageCommand.Table2,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "t3",
                Command = MessageCommand.Table3,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "t4",
                Command = MessageCommand.Table4,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "t5",
                Command = MessageCommand.Table5,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "t6",
                Command = MessageCommand.Table6,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "t7",
                Command = MessageCommand.Table7,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "t8",
                Command = MessageCommand.Table8,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
            new SerializerModel
            {
                Name = "unk",
                Command = MessageCommand.Unsupported,
                Serializer = x => ToStringRawData(x.Data),
                Deserializer = x => FromStringToByte(x)
            },
        };

        private static Dictionary<MessageCommand, SerializerModel> _serializer =
            _serializeModel.ToDictionary(x => x.Command, x => x);

        private static Dictionary<string, SerializerModel> _deserializer =
            _serializeModel.ToDictionary(x => x.Name, x => x);

        private static Dictionary<byte, string> _icons =
            new Dictionary<byte, string>()
            {
                [0] = "item-consumable",
                [1] = "item-tent",
                [2] = "item-key",
                [3] = "ability-unequip",
                [4] = "weapon-keyblade",
                [5] = "weapon-staff",
                [6] = "weapon-shield",
                [7] = "armor",
                [8] = "magic",
                [9] = "material",
                [10] = "exclamation-mark",
                [11] = "question-mark",
                [12] = "auto-equip",
                [13] = "ability-equip",
                [14] = "weapon-keyblade-equip",
                [15] = "weapon-staff-equip",
                [16] = "weapon-shield-equip",
                [17] = "accessory",
                [18] = "magic-nocharge",
                [19] = "party",
                [20] = "button-select",
                [21] = "button-start",
                [22] = "button-dpad",
                [23] = "tranquil",
                [24] = "remembrance",
                [25] = "form",
                [26] = "ai-mode-frequent",
                [27] = "ai-mode-moderate",
                [28] = "ai-mode-rare",
                [29] = "ai-settings",
                [30] = "button-r1",
                [31] = "button-r2",
                [32] = "button-l1",
                [33] = "button-l2",
                [34] = "button-triangle",
                [35] = "button-cross",
                [36] = "button-square",
                [37] = "button-circle",
                [38] = "gem-dark",
                [39] = "gem-blaze",
                [40] = "gem-frost",
                [41] = "gem-lightning",
                [42] = "gem-power",
                [43] = "gem-lucid",
                [44] = "gem-dense",
                [45] = "gem-twilight",
                [46] = "gem-mythril",
                [47] = "gem-bright",
                [48] = "gem-energy",
                [49] = "gem-serenity",
                [50] = "gem-orichalcum",
                [51] = "rank-s",
                [52] = "rank-a",
                [53] = "rank-b",
                [54] = "rank-c",
                [55] = "gumi-brush",
                [56] = "gumi-blueprint",
                [57] = "gumi-ship",
                [58] = "gumi-block",
                [59] = "gumi-gear",
                //HD ports only
                [60] = "xb-analog",
                [61] = "xb-dpad-updown",
                [62] = "xb-dpad-leftright",
                [63] = "xb-dpad-up",
                [64] = "xb-dpad-down",
                [65] = "xb-dpad-left",
                [66] = "xb-dpad-right",
                [67] = "ps-triangle",
                [68] = "ps-cross",
                [69] = "ps-square",
                [70] = "ps-circle",
                [71] = "ps-r1",
                [72] = "ps-r2",
                [73] = "ps-l1",
                [74] = "ps-l2",
                [75] = "ps-r3",
                [76] = "ps-l3",
                [77] = "ps-analog-right",
                [78] = "ps-analog-left",
                [79] = "ps-touchpad",
                [80] = "ps-options",
                [81] = "ps-dpad",
                [82] = "ps-analog",
                [83] = "ps-dpad-updown",
                [84] = "ps-dpad-leftright",
                [85] = "ps-dpad-up",
                [86] = "ps-dpad-down",
                [87] = "ps-dpad-left",
                [88] = "ps-dpad-right",
                [89] = "gen-1",
                [90] = "gen-2",
                [91] = "gen-3",
                [92] = "gen-4",
                [93] = "gen-r1",
                [94] = "gen-r2",
                [95] = "gen-l1",
                [96] = "gen-l2",
                [97] = "gen-select",
                [98] = "gen-start",
                [99] = "xb-left-analog-up",
                [100] = "xb-left-analog-down",
                [101] = "xb-left-analog-left",
                [102] = "xb-left-analog-right",
                [103] = "xb-right-analog-up",
                [104] = "xb-right-analog-down",
                [105] = "xb-right-analog-left",
                [106] = "xb-right-analog-right",
                [107] = "ps-left-analog-up",
                [108] = "ps-left-analog-down",
                [109] = "ps-left-analog-left",
                [110] = "ps-left-analog-right",
                [111] = "kb-a",
                [112] = "kb-b",
                [113] = "kb-c",
                [114] = "kb-d",
                [115] = "kb-e",
                [116] = "kb-f",
                [117] = "kb-g",
                [118] = "kb-h",
                [119] = "kb-i",
                [120] = "kb-j",
                [121] = "kb-k",
                [122] = "kb-l",
                [123] = "kb-m",
                [124] = "kb-n",
                [125] = "kb-o",
                [126] = "kb-p",
                [127] = "kb-q",
                [128] = "kb-r",
                [129] = "kb-s",
                [130] = "kb-t",
                [131] = "kb-u",
                [132] = "kb-v",
                [133] = "kb-w",
                [134] = "kb-x",
                [135] = "kb-y",
                [136] = "kb-z",
                [137] = "kb-left-shift",
                [138] = "kb-right-shift",
                [139] = "kb-left-ctrl",
                [140] = "kb-right-crtl",
                [141] = "kb-left-alt",
                [142] = "kb-right-alt",
                [143] = "kb-enter",
                [144] = "kb-backspace",
                [145] = "kb-space",
                [146] = "kb-esc",
                [147] = "kb-insert",
                [148] = "kb-delete",
                [149] = "kb-1",
                [150] = "kb-2",
                [151] = "kb-3",
                [152] = "kb-4",
                [153] = "kb-5",
                [154] = "kb-6",
                [155] = "kb-7",
                [156] = "kb-8",
                [157] = "kb-9",
                [158] = "kb-0",
                [159] = "kb-numpad-1",
                [160] = "kb-numpad-2",
                [161] = "kb-numpad-3",
                [162] = "kb-numpad-4",
                [163] = "kb-numpad-5",
                [164] = "kb-numpad-6",
                [165] = "kb-numpad-7",
                [166] = "kb-numpad-8",
                [167] = "kb-numpad-9",
                [168] = "kb-numpad-0",
                [169] = "kb-numpad-divide",
                [170] = "kb-numpad-multiply",
                [171] = "kb-numpad-minus",
                [172] = "kb-numpad-plus",
                [173] = "kb-numpad-period",
                [174] = "kb-up",
                [175] = "kb-down",
                [176] = "kb-left",
                [177] = "kb-right",
                [178] = "kb-f1",
                [179] = "kb-f2",
                [180] = "kb-f3",
                [181] = "kb-f4",
                [182] = "kb-f5",
                [183] = "kb-f6",
                [184] = "kb-f7",
                [185] = "kb-f8",
                [186] = "kb-f9",
                [187] = "kb-f10",
                [188] = "kb-f11",
                [189] = "kb-f12",
                [190] = "mouse-left-click",
                [191] = "mouse-right-click",
                [192] = "mouse-middle-click",
                [193] = "mouse-extra-click1",
                [194] = "mouse-extra-click2",
                [195] = "mouse-up",
                [196] = "mouse-down",
                [197] = "mouse-left",
                [198] = "mouse-right",
                [199] = "mouse-scroll-up",
                [200] = "mouse-scroll-down",
                [201] = "mouse-neutral",
                [202] = "ps-right-analog-up",
                [203] = "ps-right-analog-down",
                [204] = "ps-right-analog-left",
                [205] = "ps-right-analog-right",
                [206] = "hash",
                //IDs 207-217 are blank
                //These are special IDs that dynamically change what they look like in-game depending on active controller.
                //For example "dynamic-cross" would dynamically switch the Playstation "X" icon to
                //the Xbox "A" icon if you changed from a PS controller to an XBox one mid gameplay.
                [218] = "dynamic-cross",
                [219] = "dynamic-circle",
                [220] = "dynamic-circle-jp",    //When using Circle confirm
                [221] = "dynamic-cross-jp",     //When using Circle confirm
                [222] = "dynamic-square",
                [223] = "dynamic-triangle",
                [224] = "dynamic-l1",
                [225] = "dynamic-r1",
                [226] = "dynamic-l2",
                [227] = "dynamic-r2",
                [228] = "dynamic-l3",
                [229] = "dynamic-r3",
                [230] = "dynamic-select",
                [231] = "dynamic-start",
                [232] = "dynamic-left-analog",
                [233] = "dynamic-right-analog",
                [234] = "dynamic-analog",
                [235] = "dynamic-dpad",
                [236] = "dynamic-dpad-up-down",
                [237] = "dynamic-dpad-left-right",
                [238] = "dynamic-dpad-up",
                [239] = "dynamic-dpad-down",
                [240] = "dynamic-dpad-left",
                [241] = "dynamic-dpad-right",
                [242] = "dynamic-left-analog-up",
                [243] = "dynamic-left-analog-down",
                [244] = "dynamic-left-analog-left",
                [245] = "dynamic-left-analog-right",
                [246] = "dynamic-right-analog-up",
                [247] = "dynamic-right-analog-down",
                [248] = "dynamic-right-analog-left",
                [249] = "dynamic-right-analog-right",
            };

        private static Dictionary<string, byte> _iconsDeserialize =
            _icons.ToDictionary(x => x.Value, x => x.Key);

        private static byte[] DeserializeScale(string parameter) => new byte[] { byte.Parse(parameter) };
        private static byte[] DeserializeWidth(string parameter) => new byte[] { byte.Parse(parameter) };

        private static byte[] FromStringToByte(string parameter) =>
            parameter.Split(' ').Select(x => byte.Parse(x, NumberStyles.HexNumber)).ToArray();

        private static byte[] DeserializeColor(string value)
        {
            if (value[0] == '#')
                value = value.Substring(1);

            // horrible piece of crap, but works
            return new byte[]
            {
                byte.Parse(value.Substring(0, 2), NumberStyles.HexNumber),
                byte.Parse(value.Substring(2, 2), NumberStyles.HexNumber),
                byte.Parse(value.Substring(4, 2), NumberStyles.HexNumber),
                byte.Parse(value.Substring(6, 2), NumberStyles.HexNumber),
            };
        }

        private static byte[] DeserializeIcon(string value)
        {
            if (!_iconsDeserialize.TryGetValue(value, out var data))
                throw new ParseException(value, 0, "Icon not supported");

            return new byte[] { data };
        }

        private static string ToPosition(MessageCommandModel command) =>
            $"{command.PositionX},{command.PositionY}";

        private static byte[] FromPosition(string text)
        {
            var parameters = text.Split(',')
                .Select(x => short.TryParse(x.Trim(), out var result) ? result : 0)
                .ToArray();

            var xCoord = parameters.Length > 0 ? parameters[0] : 0;
            var yCoord = parameters.Length > 1 ? parameters[1] : 0;

            return new byte[4]
            {
                (byte)((ushort)xCoord & 0xFF),
                (byte)(((ushort)xCoord >> 8) & 0xFF),
                (byte)((ushort)yCoord & 0xFF),
                (byte)(((ushort)yCoord >> 8) & 0xFF),
            };
        }

        private static string ToDelayAndFade(byte[] data) => ToStringRawData(data);

        private static byte[] FromDelayAndFade(string text) => FromStringToByte(text);

        private static string ToStringRawData(byte[] data) =>
            string.Join(" ", data.Select(x => $"{x:X02}"));
    }
}

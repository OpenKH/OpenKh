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
                Name = "newline",
                Command = MessageCommand.NewLine,
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
                Name = "delay&fade",
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

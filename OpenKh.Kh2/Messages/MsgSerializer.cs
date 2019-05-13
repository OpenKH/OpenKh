using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace OpenKh.Kh2.Messages
{
    public partial class MsgSerializer
    {
        private class SerializeModel
        {
            public string Name { get; set; }
            public Func<MessageCommandModel, string> ValueGetter { get; set; }
        }

        private static Dictionary<MessageCommand, SerializeModel> _serializer =
            new Dictionary<MessageCommand, SerializeModel>()
            {
                [MessageCommand.End] = null,
                [MessageCommand.PrintText] = new SerializeModel { Name = "text", ValueGetter = x => x.Text },
                [MessageCommand.PrintComplex] = new SerializeModel { Name = "complex", ValueGetter = x => x.Text },
                [MessageCommand.Tabulation] = new SerializeModel { Name = "tabulation" },
                [MessageCommand.NewLine] = new SerializeModel { Name = "newline" },
                [MessageCommand.Reset] = new SerializeModel { Name = "reset" },
                [MessageCommand.Theme] = new SerializeModel { Name = "theme", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown05] = new SerializeModel { Name = "unk05", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown06] = new SerializeModel { Name = "unk06", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Color] = new SerializeModel { Name = "color", ValueGetter = x => $"#{x.Data[0]:X02}{x.Data[1]:X02}{x.Data[2]:X02}{x.Data[3]:X02}" },
                [MessageCommand.Unknown08] = new SerializeModel { Name = "unk08", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.PrintIcon] = new SerializeModel { Name = "icon", ValueGetter = x => _icons[x.Data[0]] },
                [MessageCommand.TextScale] = new SerializeModel { Name = "scale", ValueGetter = x => x.Data[0].ToString() },
                [MessageCommand.TextWidth] = new SerializeModel { Name = "width", ValueGetter = x => x.Data[0].ToString() },
                [MessageCommand.LineSpacing] = new SerializeModel { Name = "linespacing", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown0d] = new SerializeModel { Name = "unk0d" },
                [MessageCommand.Unknown0e] = new SerializeModel { Name = "unk0e", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown0f] = new SerializeModel { Name = "unk0f", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Clear] = new SerializeModel { Name = "clear", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown11] = new SerializeModel { Name = "unk11", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown12] = new SerializeModel { Name = "unk12", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown13] = new SerializeModel { Name = "unk13", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Delay] = new SerializeModel { Name = "delay", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.CharDelay] = new SerializeModel { Name = "chardelay", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown16] = new SerializeModel { Name = "unk16", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown18] = new SerializeModel { Name = "unk18", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown19] = new SerializeModel { Name = "unk19", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown1a] = new SerializeModel { Name = "unk1a", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown1b] = new SerializeModel { Name = "unk1b", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown1c] = new SerializeModel { Name = "unk1c", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown1d] = new SerializeModel { Name = "unk1d", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown1e] = new SerializeModel { Name = "unk1e", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unknown1f] = new SerializeModel { Name = "unk1f", ValueGetter = x => ToStringRawData(x.Data) },
                [MessageCommand.Unsupported] = new SerializeModel { Name = "unk", ValueGetter = x => ToStringRawData(x.Data) }
            };

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

        private static string ToStringRawData(byte[] data) =>
            string.Join(" ", data.Select(x => $"{x:X02}"));
    }
}

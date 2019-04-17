using kh.kh2.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace kh.kh2.Messages
{
    public class MsgSerializer
    {
        private static Dictionary<MessageCommand, Func<MessageCommandModel, XNode>> _serializer =
            new Dictionary<MessageCommand, Func<MessageCommandModel, XNode>>()
            {
                [MessageCommand.End] = x => null,
                [MessageCommand.PrintText] = x => new XElement("text", x.Text),
                [MessageCommand.NewLine] = x => new XElement("newline"),
                [MessageCommand.Reset] = x => new XElement("reset"),
                [MessageCommand.Unknown04] = x => new XElement("unk04", ToStringRawData(x.Data)),
                [MessageCommand.Unknown05] = x => new XElement("unk05", ToStringRawData(x.Data)),
                [MessageCommand.Unknown06] = x => new XElement("unk06", ToStringRawData(x.Data)),
                [MessageCommand.Color] = x =>
                {
                    var rgba = $"#{x.Data[0]:X02}{x.Data[1]:X02}{x.Data[2]:X02}{x.Data[3]:X02}";
                    return new XElement("color", rgba);
                },
                [MessageCommand.Unknown08] = x => new XElement("unk08", ToStringRawData(x.Data)),
                [MessageCommand.PrintIcon] = x => SerializePrintIcon(x),
                [MessageCommand.TextScale] = x => new XElement("scale", x.Data[0].ToString()),
                [MessageCommand.TextWidth] = x => new XElement("width", x.Data[0].ToString()),
                [MessageCommand.Unknown0c] = x => new XElement("unk0c", ToStringRawData(x.Data)),
                [MessageCommand.Unknown0d] = x => new XElement("unk0d"),
                [MessageCommand.Unknown0e] = x => new XElement("unk0e", ToStringRawData(x.Data)),
                [MessageCommand.Unknown0f] = x => new XElement("unk0f", ToStringRawData(x.Data)),
                [MessageCommand.Unknown12] = x => new XElement("unk12", ToStringRawData(x.Data)),
                [MessageCommand.Unknown13] = x => new XElement("unk13", ToStringRawData(x.Data)),
                [MessageCommand.Unknown14] = x => new XElement("unk14", ToStringRawData(x.Data)),
                [MessageCommand.Unknown15] = x => new XElement("unk15", ToStringRawData(x.Data)),
                [MessageCommand.Unknown16] = x => new XElement("unk16", ToStringRawData(x.Data)),
                [MessageCommand.Unknown18] = x => new XElement("unk18", ToStringRawData(x.Data)),
                [MessageCommand.Unknown19] = x => new XElement("unk19", ToStringRawData(x.Data)),
                [MessageCommand.Unknown1a] = x => new XElement("unk1a", ToStringRawData(x.Data)),
                [MessageCommand.Unknown1b] = x => new XElement("unk1b", ToStringRawData(x.Data)),
                [MessageCommand.Unknown1c] = x => new XElement("unk1c", ToStringRawData(x.Data)),
                [MessageCommand.Unknown1d] = x => new XElement("unk1d", ToStringRawData(x.Data)),
                [MessageCommand.Unknown1e] = x => new XElement("unk1e", ToStringRawData(x.Data)),
                [MessageCommand.Unknown1f] = x => new XElement("unk1f", ToStringRawData(x.Data)),
                [MessageCommand.Number] = x => new XElement("number", x.Data[0].ToString()),
            };

        private static Dictionary<byte, string> _icons =
            new Dictionary<byte, string>()
            {
                [0] = "consumable",
                [1] = "tent",
                [2] = "key-item",
                [3] = "ability",
                [4] = "weapon-keyblade",
                [5] = "weapon-staff",
                [6] = "weapon-shield",
                [7] = "armor",
                [8] = "magic",
                [9] = "material",
                [10] = "exclamation-mark",
                [11] = "question-mark",
                [12] = "consumable-equipped",
                [13] = "ability-equipped",
                [14] = "weapon-keyblade-equipped",
                [15] = "weapon-staff-equipped",
                [16] = "weapon-shield-equipped",
                [17] = "accessory",
                [30] = "button-r1",
                [31] = "button-r2",
                [32] = "button-l1",
                [33] = "button-l2",
                [34] = "button-triangle",
                [35] = "button-cross",
                [36] = "button-square",
                [37] = "button-circle",
            };

        public static XElement SerializeXEntries(IEnumerable<Msg.Entry> entries, bool ignoreExceptions = false)
        {
            return new XElement("messages", entries.Select(x =>
            {
                List<MessageCommandModel> parsedEntries;

                try
                {
                    parsedEntries = x.Map();
                }
                catch (NotImplementedException ex)
                {
                    if (ignoreExceptions)
                        return new XElement("msgerror",
                            new XAttribute("id", x.Id),
                            ex.Message);
                    else
                        throw ex;
                }

                return SerializeXEntries(x.Id, parsedEntries, ignoreExceptions); ;
            }));
        }

        public static XElement SerializeXEntries(int id, IEnumerable<MessageCommandModel> entries, bool ignoreExceptions = false)
        {
            var root = new XElement("message", new XAttribute("id", id));
            foreach (var entry in entries)
            {
                XNode node;

                try
                {
                    if (!_serializer.TryGetValue(entry.Command, out var funcSerializer))
                        throw new NotImplementedException($"The command {entry.Command} serialization is not implemented yet.");

                    node = funcSerializer?.Invoke(entry);
                }
                catch (NotImplementedException ex)
                {
                    if (ignoreExceptions)
                        node = new XElement("error", ex.Message);
                    else
                        throw ex;
                }

                root.Add(node);
            }
            return root;
        }

        private static XElement SerializePrintIcon(MessageCommandModel entry)
        {
            byte value = entry.Data[0];
            if (!_icons.TryGetValue(value, out var content))
                throw new NotImplementedException($"The icon {value} is not implemented yet.");

            return new XElement("icon", new XAttribute("class", content));
        }

        private static string ToStringRawData(byte[] data) =>
            string.Join(" ", data.Select(x => $"{x:X02}"));
    }
}

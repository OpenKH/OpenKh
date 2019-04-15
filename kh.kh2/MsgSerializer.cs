using System;
using System.Collections.Generic;
using System.Linq;
using System.Xml.Linq;

namespace kh.kh2
{
    public class MsgSerializer
    {
        private static Dictionary<MsgParser.Command, Func<MsgParser.Entry, XElement>> _serializer =
            new Dictionary<MsgParser.Command, Func<MsgParser.Entry, XElement>>()
            {
                [MsgParser.Command.End] = x => null,
                [MsgParser.Command.PrintText] = x => new XElement("text", x.Text),
                [MsgParser.Command.NewLine] = x => new XElement("newline"),
                [MsgParser.Command.Reset] = x => new XElement("reset"),
                [MsgParser.Command.Unknown04] = x => new XElement("unk04", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown05] = x => new XElement("unk05", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown06] = x => new XElement("unk06", ToStringRawData(x.Data)),
                [MsgParser.Command.Color] = x =>
                {
                    var rgba = $"#{x.Data[0]:X02}{x.Data[1]:X02}{x.Data[2]:X02}{x.Data[3]:X02}";
                    return new XElement("color", rgba);
                },
                [MsgParser.Command.Unknown08] = x => new XElement("unk08", ToStringRawData(x.Data)),
                [MsgParser.Command.PrintIcon] = x => SerializePrintIcon(x),
                [MsgParser.Command.TextScale] = x => new XElement("scale", x.Data[0].ToString()),
                [MsgParser.Command.TextWidth] = x => new XElement("width", x.Data[0].ToString()),
                [MsgParser.Command.Unknown0c] = x => new XElement("unk0c", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown0d] = x => new XElement("unk0d"),
                [MsgParser.Command.Unknown0e] = x => new XElement("unk0e", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown0f] = x => new XElement("unk0f", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown12] = x => new XElement("unk12", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown13] = x => new XElement("unk13", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown14] = x => new XElement("unk14", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown15] = x => new XElement("unk15", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown16] = x => new XElement("unk16", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown18] = x => new XElement("unk18", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown19] = x => new XElement("unk19", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown1a] = x => new XElement("unk1a", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown1b] = x => new XElement("unk1b", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown1c] = x => new XElement("unk1c", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown1d] = x => new XElement("unk1d", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown1e] = x => new XElement("unk1e", ToStringRawData(x.Data)),
                [MsgParser.Command.Unknown1f] = x => new XElement("unk1f", ToStringRawData(x.Data)),
                [MsgParser.Command.Number] = x => new XElement("number", x.Data[0].ToString()),
            };

        private static Dictionary<byte, string> _icons =
            new Dictionary<byte, string>()
            {
                [0] = "consumable",
                [1] = "tent",
                [2] = "material",
                [3] = "ability-off",
                [4] = "weapon-keyblade",
                [5] = "weapon-staff",
                [6] = "weapon-shield"
            };

        public static XElement SerializeXEntries(IEnumerable<Msg.Entry> entries, bool ignoreExceptions = false)
        {
            return new XElement("messages", entries.Select(x =>
            {
                List<MsgParser.Entry> parsedEntries;

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

        public static XElement SerializeXEntries(int id, IEnumerable<MsgParser.Entry> entries, bool ignoreExceptions = false)
        {
            var root = new XElement("message", new XAttribute("id", id));
            foreach (var entry in entries)
            {
                XElement element;

                try
                {
                    element = SerializeXEntry(entry);
                }
                catch (NotImplementedException ex)
                {
                    if (ignoreExceptions)
                        element = new XElement("error", ex.Message);
                    else
                        throw ex;
                }

                root.Add(element);
            }
            return root;
        }

        public static XElement SerializeXEntry(MsgParser.Entry entry)
        {
            if (!_serializer.TryGetValue(entry.Command, out var funcSerializer))
                throw new NotImplementedException($"The command {entry.Command} serialization is not implemented yet.");

            return funcSerializer?.Invoke(entry);
        }

        private static XElement SerializePrintIcon(MsgParser.Entry entry)
        {
            byte value = entry.Data[0];
            if (!_icons.TryGetValue(value, out var content))
                throw new NotImplementedException($"The icon {value} is not implemented yet.");

            return new XElement("icon", content);
        }

        private static string ToStringRawData(byte[] data) =>
            string.Join(" ", data.Select(x => $"{x:X02}"));
    }
}

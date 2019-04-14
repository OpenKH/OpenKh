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
                [MsgParser.Command.End] = x => new XElement("end"),
                [MsgParser.Command.PrintIcon] = x => SerializePrintIcon(x),
                [MsgParser.Command.PrintText] = x => new XElement("text", x.Text),
                [MsgParser.Command.Parameter] = x => new XElement("param", x.Data[0].ToString()),
                [MsgParser.Command.LineFeed] = x => new XElement("linefeed"),
                [MsgParser.Command.TextSize] = x => new XElement("size", x.Data[0].ToString()),
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

            return funcSerializer(entry);
        }

        private static XElement SerializePrintIcon(MsgParser.Entry entry)
        {
            byte value = entry.Data[0];
            if (!_icons.TryGetValue(value, out var content))
                throw new NotImplementedException($"The icon {value} is not implemented yet.");

            return new XElement("icon", content);
        }
    }
}

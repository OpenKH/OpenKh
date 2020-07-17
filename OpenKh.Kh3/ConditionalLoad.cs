using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh3
{
    public class ConditionalLoad
    {
        private class Header
        {
            [Data] public int NameCount { get; set; }
            [Data] public int EventCount { get; set; }
            [Data] public int EntryCount { get; set; }
            [Data] public int Unknown0c { get; set; }
            [Data] public int NameOffset { get; set; }
            [Data] public int NameLength { get; set; }
            [Data] public int EventOffset { get; set; }
            [Data] public int EventLength { get; set; }
            [Data] public int ConditionOffset { get; set; }
            [Data] public int ConditionLength { get; set; }
            [Data] public int EntryOffset { get; set; }
            [Data] public int EntryLength { get; set; }
        }

        private class Condition : IEqualityComparer<Condition>
        {
            public static Condition Comparer = new Condition();

            public int Operator { get; set; }
            public int Gameflow { get; set; }
            public int Progress { get; set; }

            public bool Equals(Condition x, Condition y) =>
                x.Operator == y.Operator &&
                x.Gameflow == y.Gameflow &&
                x.Progress == y.Progress;

            public int GetHashCode(Condition obj) =>
                obj.Operator |
                obj.Gameflow << 8 |
                obj.Progress << 16;
        }

        public enum ConditionalOperator
        {
            Break = 0,
            Greater = 4,
            Less = 5,
            Condition = 9,
            Any = 10,
        }

        public string World { get; set; }
        public string Pawn { get; set; }
        public string Blueprint { get; set; }
        public ConditionalOperator Operator { get; set; }
        public string GameFlow { get; set; }
        public string Progress { get; set; }

        public override string ToString()
        {
            var str = $"For {World}, load {Blueprint} with {Pawn}";
            if (Operator == ConditionalOperator.Greater)
                str += $" if greater than {GameFlow}{Progress}";
            if (Operator == ConditionalOperator.Less)
                str += $" if less than {GameFlow}{Progress}";

            return str;
        }

        public static List<ConditionalLoad> Read(Stream stream)
        {
            var reader = new BinaryReader(stream);
            var header = BinaryMapping.ReadObject<Header>(stream);

            stream.Position = header.EventOffset;
            var eventTable = Enumerable.Range(0, header.EventCount)
                .Select(_ => reader.ReadInt32())
                .ToList()
                .Select(offset => ReadCString(stream.SetPosition(header.NameOffset + offset)))
                .ToList();

            stream.Position = header.EntryOffset;
            return Enumerable.Range(0, header.EntryCount)
                .Select(_ => new
                {
                    worldNameOffset = reader.ReadInt32(),
                    unknownOffset = reader.ReadInt32(),
                    pawnNameOffset = reader.ReadInt32(),
                    blueprintNameOffset = reader.ReadInt32()
                })
                .ToList()
                .Select(x =>
                {
                    ConditionalOperator op = ConditionalOperator.Break;
                    string gameflow = null;
                    string progress = null;

                    bool isRunning = true;
                    if (x.unknownOffset >= 0)
                    {
                        stream.Position = header.ConditionOffset + x.unknownOffset;
                        do
                        {
                            switch ((ConditionalOperator)reader.ReadInt32())
                            {
                                case ConditionalOperator.Break:
                                    isRunning = false;
                                    break;
                                case ConditionalOperator.Greater:
                                    op = ConditionalOperator.Greater;
                                    break;
                                case ConditionalOperator.Less:
                                    op = ConditionalOperator.Less;
                                    break;
                                case ConditionalOperator.Condition:
                                    if (gameflow == null)
                                        gameflow = eventTable[reader.ReadInt32()];
                                    else
                                        progress = eventTable[reader.ReadInt32()];
                                    break;
                                case ConditionalOperator.Any:
                                    op = ConditionalOperator.Any;
                                    reader.ReadInt32();
                                    break;
                            }
                        } while (isRunning);
                    }

                    return new ConditionalLoad
                    {
                        World = ReadCString(stream.SetPosition(header.NameOffset + x.worldNameOffset)),
                        Pawn = ReadCString(stream.SetPosition(header.NameOffset + x.pawnNameOffset)),
                        Blueprint = ReadCString(stream.SetPosition(header.NameOffset + x.blueprintNameOffset)),
                        GameFlow = gameflow,
                        Progress = progress,
                        Operator = op
                    };
                })
                .ToList();
        }

        public static void Write(Stream stream, List<ConditionalLoad> conditions)
        {
            var eventList = new List<string>();
            var conditionIndices = new List<int>();
            var conditionList = new List<Condition>();
            foreach (var x in conditions)
            {
                var condition = new Condition
                {
                    Operator = (int)x.Operator,
                    Gameflow = x.GameFlow != null ? AddIfNotExists(eventList, x.GameFlow) : -1,
                    Progress = x.Progress != null ? AddIfNotExists(eventList, x.Progress) : -1,
                };

                int index;
                for (index = 0; index < conditionList.Count; index++)
                {
                    if (Condition.Comparer.Equals(conditionList[index], condition))
                        break;
                }

                if (index == conditionList.Count)
                    conditionList.Add(condition);
                conditionIndices.Add(index);
            }

            var names = new List<string>();
            foreach (var item in conditions)
            {
                AddIfNotExists(names, item.World);
                AddIfNotExists(names, item.Pawn);
                AddIfNotExists(names, item.Blueprint);
            }
            foreach (var item in eventList)
                AddIfNotExists(names, item);

            var header = new Header
            {
                NameCount = names.Count,
                EventCount = eventList.Count,
                EntryCount = conditions.Count,
                Unknown0c = 0x10,
            };

            stream.Position = 0x30;
            header.NameOffset = (int)stream.Position;
            var nameDictionary = new Dictionary<string, int>();
            foreach (var item in names)
            {
                nameDictionary.Add(item, (int)(stream.Position) - 0x30);
                WriteCString(stream, item);
            }
            stream.WriteByte(0);
            header.NameLength = (int)(stream.Position - header.NameOffset);

            stream.AlignPosition(4);
            header.EventOffset = (int)stream.Position;
            foreach (var item in eventList)
                stream.Write(nameDictionary[item]);
            header.EventLength = (int)(stream.Position - header.EventOffset);

            header.ConditionOffset = (int)stream.Position;
            for (var i = 0; i < conditionList.Count; i++)
            {
                for (var j = 0; j < conditionIndices.Count; j++)
                {
                    var conditionIndex = conditionIndices[j];
                    if (conditionIndex == i)
                        conditionIndices[j] = (int)(stream.Position - header.ConditionOffset);
                }

                var conditionItem = conditionList[i];
                switch (conditionItem.Operator)
                {
                    case 0:
                    case 10:
                        stream.Write(10);
                        stream.Write(-1);
                        stream.Write(0);
                        break;
                    default:
                        stream.Write(9);
                        stream.Write(conditionItem.Gameflow);
                        stream.Write(9);
                        stream.Write(conditionItem.Progress);
                        stream.Write(conditionItem.Operator);
                        stream.Write(0);
                        break;
                }
            }
            header.ConditionLength = (int)(stream.Position - header.ConditionOffset);

            header.EntryOffset = (int)stream.Position;
            for (var i = 0; i < conditions.Count; i++)
            {
                var item = conditions[i];
                stream.Write(nameDictionary[item.World]);
                stream.Write(item.Operator == ConditionalOperator.Break ? -1 : conditionIndices[i]);
                stream.Write(nameDictionary[item.Pawn]);
                stream.Write(nameDictionary[item.Blueprint]);
            }
            header.EntryLength = (int)(stream.Position - header.EntryOffset);

            stream.Position = 0;
            BinaryMapping.WriteObject(stream, header);
            stream.Position = stream.Length;
        }

        private static int AddIfNotExists(List<string> list, string value)
        {
            var index = list.IndexOf(value);
            if (index < 0)
            {
                index = list.Count;
                list.Add(value);
            }

            return index;
        }

        private static string ReadCString(Stream stream)
        {
            var sb = new StringBuilder();
            while (true)
            {
                var ch = stream.ReadByte();
                if (ch == 0)
                    return sb.ToString();

                sb.Append((char)ch);
            }
        }

        private static void WriteCString(Stream stream, string value)
        {
            foreach (var ch in value)
                stream.Write((byte)ch);
            stream.Write((byte)0);
        }
    }
}

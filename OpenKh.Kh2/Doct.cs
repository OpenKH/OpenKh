using OpenKh.Common;
using OpenKh.Kh2.Extensions;
using OpenKh.Kh2.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Doct
    {
        private const uint MagicCode = 0x54434F44U;
        private const int HeaderSize = 44;
        private const int Entry1Size = 48;
        private const int Entry2Size = 28;

        public List<Entry1> Entry1List { get; set; } = new List<Entry1>();
        public List<Entry2> Entry2List { get; set; } = new List<Entry2>();
        public HeaderDef Header { get; set; } = new HeaderDef();

        public Doct()
        {

        }

        public Doct(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            if (stream.Length < HeaderSize)
                throw new InvalidDataException("Invalid header length");

            Header = BinaryMapping.ReadObject<HeaderDef>(stream);
            if (Header.MagicCode != MagicCode)
                throw new InvalidDataException("Invalid header");

            Entry1List = stream.ReadList<Entry1>(Header.Entry1FirstOffset, Header.Entry1TotalLength / Entry1Size);
            Entry2List = stream.ReadList<Entry2>(Header.Entry2FirstOffset, Header.Entry2TotalLength / Entry2Size);
        }

        public class HeaderDef
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public int Version { get; set; } = 2; // always 2?
            [Data] public int Unk2 { get; set; } // 1, 2, or 4?
            [Data] public int HeaderOffset { get; set; }

            [Data] public int HeaderLength { get; set; }
            [Data] public int Entry1FirstOffset { get; set; }
            [Data] public int Entry1TotalLength { get; set; }
            [Data] public int Entry2FirstOffset { get; set; }

            [Data] public int Entry2TotalLength { get; set; }
            [Data] public int Entry3FirstOffset { get; set; }
            [Data] public int Entry3TotalLength { get; set; }
        }

        public Entry2 Add(Entry2 it)
        {
            Entry2List.Add(it);
            return it;
        }

        public class Entry1
        {
            [Data] public short Child1 { get; set; } = -1;
            [Data] public short Child2 { get; set; } = -1;
            [Data] public short Child3 { get; set; } = -1;
            [Data] public short Child4 { get; set; } = -1;
            [Data] public short Child5 { get; set; } = -1;
            [Data] public short Child6 { get; set; } = -1;
            [Data] public short Child7 { get; set; } = -1;
            [Data] public short Child8 { get; set; } = -1;
            [Data] public BoundingBox BoundingBox { get; set; }
            [Data] public ushort Entry2Index { get; set; }
            [Data] public ushort Entry2LastIndex { get; set; }
            [Data] public uint Unk { get; set; }

            public override string ToString() => string.Concat(
                $"({Child1,4},{Child2,4},{Child3,4},{Child4,4},{Child5,4},{Child6,4},{Child7,4},{Child8,4})",
                $" ({BoundingBox.Minimum.X,8},{BoundingBox.Minimum.Y,8},{BoundingBox.Minimum.Z,8}) ({BoundingBox.Maximum.X,8},{BoundingBox.Maximum.Y,8},{BoundingBox.Maximum.Z,8})",
                $" {Entry2Index,4} {Entry2LastIndex,4} {Unk:X8}"
            );
        }

        public void Write(Stream stream) =>
            Doct.Write(stream, this);

        public class Entry2
        {
            [Data] public uint Flags { get; set; }
            [Data] public BoundingBox BoundingBox { get; set; }

            public override string ToString() => string.Concat(
                $"{Flags:X8}",
                $" ({BoundingBox.Minimum.X,8},{BoundingBox.Minimum.Y,8},{BoundingBox.Minimum.Z,8}) ({BoundingBox.Maximum.X,8},{BoundingBox.Maximum.Y,8},{BoundingBox.Maximum.Z,8})"
            );
        }

        public static void Write(Stream stream, Doct doct)
        {
            if (!stream.CanWrite || !stream.CanSeek)
                throw new InvalidDataException($"Write or seek must be supported.");

            var entry1Data = new MemoryStream();
            {
                entry1Data.WriteList(doct.Entry1List);
            }

            var entry2Data = new MemoryStream();
            {
                entry2Data.WriteList(doct.Entry2List);
            }

            {
                var newHeader = new HeaderDef
                {
                    MagicCode = MagicCode,
                    Version = doct.Header.Version,
                    Unk2 = doct.Header.Unk2,
                    HeaderOffset = 0,
                    HeaderLength = 0x2C,
                    Entry1FirstOffset = 0x2C,
                    Entry1TotalLength = Convert.ToInt32(entry1Data.Length),
                    Entry2FirstOffset = 0x2C + Convert.ToInt32(entry1Data.Length),
                    Entry2TotalLength = Convert.ToInt32(entry2Data.Length),
                    Entry3FirstOffset = 0x2C + Convert.ToInt32(entry1Data.Length) + Convert.ToInt32(entry2Data.Length),
                    Entry3TotalLength = 0,
                };

                BinaryMapping.WriteObject(stream, newHeader);

                entry1Data.Position = 0;
                entry1Data.CopyTo(stream);

                entry2Data.Position = 0;
                entry2Data.CopyTo(stream);
            }
        }

        public static Doct Read(Stream stream) => new Doct(stream);

        public Entry1 CompleteAndAdd(Entry1 it)
        {
            Entry1List.Add(it);

            it.BoundingBox = Enumerable.Range(
                it.Entry2Index,
                it.Entry2LastIndex - it.Entry2Index
            )
                .Select(index => Entry2List[index].BoundingBox)
                .Concat(SelectBoundingBoxList(it))
                .MergeAll();

            return it;
        }

        private IEnumerable<BoundingBox> SelectBoundingBoxList(Entry1 ent1)
        {
            if (ent1.Child1 != -1)
            {
                yield return Entry1List[ent1.Child1].BoundingBox;

                if (ent1.Child2 != -1)
                {
                    yield return Entry1List[ent1.Child2].BoundingBox;

                    if (ent1.Child3 != -1)
                    {
                        yield return Entry1List[ent1.Child3].BoundingBox;

                        if (ent1.Child4 != -1)
                        {
                            yield return Entry1List[ent1.Child4].BoundingBox;

                            if (ent1.Child5 != -1)
                            {
                                yield return Entry1List[ent1.Child5].BoundingBox;

                                if (ent1.Child6 != -1)
                                {
                                    yield return Entry1List[ent1.Child6].BoundingBox;

                                    if (ent1.Child7 != -1)
                                    {
                                        yield return Entry1List[ent1.Child7].BoundingBox;

                                        if (ent1.Child8 != -1)
                                        {
                                            yield return Entry1List[ent1.Child8].BoundingBox;
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }

        public void ReverseEntry1()
        {
            var maxIndex = Entry1List.Count - 1;

            Entry1List.Reverse();

            short ReverseChildIndex(short child)
            {
                if (child != -1)
                {
                    child = Convert.ToInt16(maxIndex - child);
                }
                return child;
            }

            foreach (var one in Entry1List)
            {
                one.Child1 = ReverseChildIndex(one.Child1);
                one.Child2 = ReverseChildIndex(one.Child2);
                one.Child3 = ReverseChildIndex(one.Child3);
                one.Child4 = ReverseChildIndex(one.Child4);
                one.Child5 = ReverseChildIndex(one.Child5);
                one.Child6 = ReverseChildIndex(one.Child6);
                one.Child7 = ReverseChildIndex(one.Child7);
                one.Child8 = ReverseChildIndex(one.Child8);
            }
        }
    }
}

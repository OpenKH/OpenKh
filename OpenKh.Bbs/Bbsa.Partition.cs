using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public partial class Bbsa
    {
        protected interface ILba
        {
            uint Hash { get; }
        }

        protected class Partition<TLba> where TLba : ILba
        {
            [Data] public int Name { get; set; }
            [Data] public short Count { get; set; }
            [Data] public short Offset { get; set; }
            public TLba[] Lba { get; set; }

            public override string ToString() =>
                $"{Name:X08} {Count:X04} {Offset:X04}";
        }

        protected class Lba : ILba
        {
            [Data] public uint Hash { get; set; }
            [Data] public uint Info { get; set; }
            public int Offset => (int)(Info >> 12);
            public int Size => (int)(Info & 0xFFF);

            public override string ToString() =>
                $"{Hash:X08} {Offset:X06} {Size:X03}";

            internal static Lba Read(Stream stream) =>
                BinaryMapping.ReadObject<Lba>(stream);
        }

        protected class Lba2 : ILba
        {
            [Data] public uint Hash { get; set; }
            [Data] public short Offset { get; set; }
            [Data] public byte Count { get; set; }
            [Data] public byte Unknown { get; set; }
            public UnknownStruct[] UnknownItems { get; set; }

            public override string ToString() =>
                $"{Hash:X08} {Offset:X04} {Count:X02} {Unknown:X02}";

            internal static Lba2 Read(Stream stream) =>
                BinaryMapping.ReadObject<Lba2>(stream);
        }

        protected class UnknownStruct
        {
            [Data] public short Unknown00 { get; set; }
            [Data] public int Unknown04 { get; set; }

            public override string ToString() =>
                $"{Unknown04:X08} {Unknown00:X04}";

            internal static UnknownStruct Read(Stream stream) =>
                BinaryMapping.ReadObject<UnknownStruct>(stream);
        }

        private static Partition<TLba>[] ReadPartitions<TLba>(Stream stream, int offset, int count)
            where TLba : ILba
        {
            stream.Position = offset;
            return Enumerable.Range(0, count)
                .Select(x => ReadPartition<TLba>(stream))
                .ToArray();
        }

        private static Partition<TLba> ReadPartition<TLba>(Stream stream)
            where TLba : ILba =>
                BinaryMapping.ReadObject<Partition<TLba>>(stream);

        private static void ReadPartitionLba(IEnumerable<Partition<Lba>> partitions, Stream stream, int baseOffset)
        {
            stream.Position = baseOffset;
            foreach (var partition in partitions)
            {
                stream.Position = baseOffset + partition.Offset * LbaLength;
                partition.Lba = Enumerable.Range(0, partition.Count)
                    .Select(x => Lba.Read(stream)).ToArray();
            }
        }

        private static void ReadPartitionLba(IEnumerable<Partition<Lba2>> partitions, Stream stream, int baseOffset)
        {
            stream.Position = baseOffset;
            foreach (var partition in partitions)
            {
                stream.Position = baseOffset + partition.Offset * LbaLength;
                partition.Lba = Enumerable.Range(0, partition.Count)
                    .Select(x => Lba2.Read(stream)).ToArray();
            }
        }

        private static void ReadUnknownStruct(IEnumerable<Partition<Lba2>> partitions, Stream stream, int baseOffset)
        {
            foreach (var partition in partitions)
            {
                foreach (var lba in partition.Lba)
                {
                    stream.Position = baseOffset + lba.Offset * 6;
                    lba.UnknownItems = Enumerable.Range(0, lba.Count)
                        .Select(x => UnknownStruct.Read(stream)).ToArray();
                }
            }
        }
    }
}

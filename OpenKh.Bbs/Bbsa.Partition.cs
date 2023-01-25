using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public partial class Bbsa
    {
        public interface ILba
        {
            uint Hash { get; }
        }

        public class Partition<TLba> where TLba : ILba
        {
            [Data] public uint Name { get; set; }
            [Data] public short Count { get; set; }
            [Data] public short Offset { get; set; }
            public TLba[] Lba { get; set; }

            public override string ToString() =>
                $"{Name:X08} {Count:X04} {Offset:X04}";
        }

        public class PartitionFileEntry : ILba
        {
            [Data] public uint Hash { get; set; }
            [Data] public uint Info { get; set; }
            public int Offset => (int)(Info >> 12);
            public int Size => (int)(Info & 0xFFF);

            public long LocationOffset;
            public override string ToString() =>
                $"{Hash:X08} {Offset:X06} {Size:X03}";

            internal static PartitionFileEntry Read(Stream stream)
            {
                var pastPos = stream.Position;
                PartitionFileEntry part = BinaryMapping.ReadObject<PartitionFileEntry>(stream);
                part.LocationOffset = pastPos;
                return part;
            }
            internal void Write(Stream stream)
            {
                Info = (uint)((Offset << 12) + Size);
                BinaryMapping.WriteObject<PartitionFileEntry>(stream, this, (int)LocationOffset);
            }
        }

        public class ArchivePartitionEntry : ILba
        {
            [Data] public uint Hash { get; set; }
            [Data] public short Count { get; set; }
            [Data] public short Offset { get; set; }

            public long LocationOffset;
            public ArcEntry[] UnknownItems { get; set; }

            public override string ToString() =>
                $"{Hash:X08} {Offset:X04} {Count:X02}";

            internal static ArchivePartitionEntry Read(Stream stream)
            {
                var pastPos = stream.Position;
                ArchivePartitionEntry archivepart = BinaryMapping.ReadObject<ArchivePartitionEntry>(stream);
                archivepart.LocationOffset = pastPos;
                return archivepart;
            }
        }

        public class ArcEntry
        {
            [Data] public short LinkIndex { get; set; }//+6 to partition entry index
            [Data] public uint Hash { get; set; }

            public override string ToString() =>
                $"{Hash:X08} {LinkIndex:X04}";

            internal static ArcEntry Read(Stream stream) =>
                BinaryMapping.ReadObject<ArcEntry>(stream);
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

        private static void ReadPartitionLba(IEnumerable<Partition<PartitionFileEntry>> partitions, Stream stream, int baseOffset)
        {
            stream.Position = baseOffset;
            foreach (var partition in partitions)
            {
                stream.Position = baseOffset + partition.Offset * LbaLength;
                partition.Lba = Enumerable.Range(0, partition.Count)
                    .Select(x => PartitionFileEntry.Read(stream)).ToArray();
            }
        }

        private static void ReadPartitionLba(IEnumerable<Partition<ArchivePartitionEntry>> partitions, Stream stream, int baseOffset)
        {
            stream.Position = baseOffset;
            foreach (var partition in partitions)
            {
                stream.Position = baseOffset + partition.Offset * LbaLength;
                partition.Lba = Enumerable.Range(0, partition.Count)
                    .Select(x => ArchivePartitionEntry.Read(stream)).ToArray();
            }
        }

        private static void ReadArcEntries(IEnumerable<Partition<ArchivePartitionEntry>> partitions, Stream stream, int baseOffset)
        {
            foreach (var partition in partitions)
            {
                foreach (var lba in partition.Lba)
                {
                    stream.Position = baseOffset + lba.Offset * 6;
                    lba.UnknownItems = Enumerable.Range(0, lba.Count)
                        .Select(x => ArcEntry.Read(stream)).ToArray();
                }
            }
        }
    }
}

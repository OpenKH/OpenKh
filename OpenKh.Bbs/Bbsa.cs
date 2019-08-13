using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public partial class Bbsa
    {
        protected static string[] KnownExtensions = new string[] {
            "Arc", "Bin", "Tm2", "Pmo",
            "Pam", "Pmp", "Pvd", "Bcd",
            "Fep", "Frr", "Ead", "Ese",
            "Lub", "Lad", "L2d", "Pst",
            "Epd", "Olo", "Bep", "Txa",
            "Aac", "Abc", "Scd", "Bsd",
            "Seb", "Ctd", "Ecm", "Ept",
            "Mss", "Nmd", "Ite", "Itb",
            "Itc", "Bdd", "Bdc", "Ngd",
            "Exb", "Gpd", "Exa", "Esd",
            "MTX", "INF", "COD", "CLU",
            "PMF", "ESE", "PTX", ""
        };

        protected static Dictionary<int, string> Paths = new Dictionary<int, string>
        {
            [0x0050414D] = "arc/map",
            [0x4E455645] = "arc/event",
            [0x00004350] = "arc/pc",
            [0x10004350] = "arc/pc_ven",
            [0x20004350] = "arc/pc_aqua",
            [0x30004350] = "arc/pc_terra",
            [0x4D454E45] = "arc/enemy",
            [0x53534F42] = "arc/boss",
            [0x0043504E] = "arc/npc",
            [0x4D4D4947] = "arc/gimmick",
            [0x50414557] = "arc/weapon",
            [0x4D455449] = "arc/item",
            [0x45464645] = "arc/effect",
            [0x554E454D] = "arc/menu",
            [0x00435445] = "arc/etc",
            [0x00535953] = "arc/system",
            [0x53455250] = "arc/preset",
            [0x41455250] = "arc/preset.alpha",
            [0x55424544] = "arc/debug",
        };

        protected static Dictionary<string, int> PathsReverse =
            Paths.ToDictionary(x => x.Value, x => x.Key);

        protected static string[] AllPaths =
            PathsReverse
            .Select(x => (x.Key + '/').ToUpper())
            .Concat(new string[] { string.Empty })
            .ToArray();

        protected static string[] KnownExtensionsWithDot =
            KnownExtensions.Select(x => ('.' + x).ToUpper()).ToArray();

        protected class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Version { get; set; }
            [Data] public short PartitionCount { get; set; }
            [Data] public short Unk0a { get; set; }
            [Data] public short Unk0c { get; set; }
            [Data] public short FileDirectoryCount { get; set; }
            [Data] public int PartitionOffset { get; set; }
            [Data] public int FileDirectoryOffset { get; set; }
            [Data] public short PartitionTable2SectorIndex { get; set; }
            [Data] public short Archive0SectorIndex { get; set; }
            [Data] public int TotalSectorCount { get; set; }
            [Data] public int Archive1SectorIndex { get; set; }
            [Data] public int Archive2SectorIndex { get; set; }
            [Data] public int Archive3SectorIndex { get; set; }
            [Data] public int Archive4SectorIndex { get; set; }
            public Partition<Lba>[] Partitions { get; set; }
        }

        protected class Header2
        {
            [Data] public byte Unknown00 { get; set; }
            [Data] public byte PartitionCount { get; set; }
            [Data] public short Unknown02 { get; set; }
            [Data] public short LbaStartOffset { get; set; }
            [Data] public short UnknownOffset { get; set; }
            public Partition<ArcLba>[] Partitions { get; set; }
        }

        protected class DirectoryEntry
        {
            public uint FileHash { get; set; }
            public uint Info { get; set; }
            public uint DirectoryHash { get; set; }
            public int Offset => (int)(Info >> 12);
            public int Size => (int)(Info & 0xFFF);

            public override string ToString() =>
                $"{DirectoryHash:X08}/{FileHash:X08} {Offset:X05} {Size:X03}";
        }

        private const int LbaLength = 8;
        protected readonly Header _header;
        protected readonly Header2 _header2;
        protected readonly DirectoryEntry[] _directoryEntries;

        protected Bbsa(Stream stream)
        {
            _header = BinaryMapping.ReadObject<Header>(stream, (int)stream.Position);
            _header.Partitions = ReadPartitions<Lba>(stream, 0x30, _header.PartitionCount);
            ReadPartitionLba(_header.Partitions, stream, _header.PartitionOffset);

            stream.Position = _header.FileDirectoryOffset;
            var reader = new BinaryReader(stream);
            _directoryEntries = Enumerable.Range(0, _header.FileDirectoryCount)
                    .Select(x => new DirectoryEntry
                    {
                        FileHash = reader.ReadUInt32(),
                        Info = reader.ReadUInt32(),
                        DirectoryHash = reader.ReadUInt32()
                    }).ToArray();

            int header2Offset = _header.PartitionTable2SectorIndex * 0x800;
            stream.Position = header2Offset;
            _header2 = BinaryMapping.ReadObject<Header2>(stream);
            _header2.Partitions = ReadPartitions<ArcLba>(stream, header2Offset + 8, _header2.PartitionCount);
            ReadPartitionLba(_header2.Partitions, stream, header2Offset + _header2.LbaStartOffset);
            ReadUnknownStruct(_header2.Partitions, stream, header2Offset + _header2.UnknownOffset);
        }

        public IEnumerable<Entry> Files
        {
            get
            {
                foreach (var partition in _header.Partitions)
                {
                    Paths.TryGetValue(partition.Name, out var folder);

                    foreach (var lba in partition.Lba)
                    {
                        NameDictionary.TryGetValue(lba.Hash, out var fileName);

                        yield return new Entry(
                            this,
                            lba.Offset,
                            lba.Size,
                            fileName,
                            folder,
                            lba.Hash,
                            0);
                    }
                }

                foreach (var file in _directoryEntries)
                {
                    NameDictionary.TryGetValue(file.FileHash, out var fileName);
                    NameDictionary.TryGetValue(file.DirectoryHash, out var folderName);

                    yield return new Entry(
                        this,
                        file.Offset,
                        file.Size,
                        fileName,
                        folderName,
                        file.FileHash,
                        file.DirectoryHash);
                }
            }
        }

        public int GetOffset(string fileName)
        {
            var directory = Path.GetDirectoryName(fileName).Replace('\\', '/');
            var file = Path.GetFileName(fileName);
            var name = Path.GetFileNameWithoutExtension(file);

            if (!PathsReverse.TryGetValue(directory, out var pathId))
                return -1;

            var pathInfo = _header.Partitions.FirstOrDefault(x => x.Name == pathId);
            if (pathInfo == null)
                return -1;

            var hash = GetHash(name.ToUpper());
            var lba = pathInfo.Lba.FirstOrDefault(x => x.Hash == hash);
            if (lba == null)
                return -1;

            return (lba.Offset + _header.Archive0SectorIndex) * 0x800;
        }

        public static Bbsa Read(Stream stream) => new Bbsa(stream);
    }
}

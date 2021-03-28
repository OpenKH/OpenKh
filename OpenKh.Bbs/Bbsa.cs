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

        protected static Dictionary<uint, string> Paths = new Dictionary<uint, string>
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

        protected static Dictionary<byte, string> PathCategories = new Dictionary<byte, string>
        {
            [0x00] = "arc_",
            [0x80] = "sound/bgm",
            [0xC0] = "lua",
            [0x90] = "sound/se/common",
            [0x91] = "sound/se/event/{1}",
            [0x92] = "sound/se/footstep/{1}",
            [0x93] = "sound/se/enemy",
            [0x94] = "sound/se/weapon",
            [0x95] = "sound/se/act",
            [0xA1] = "sound/voice/{0}/event/{1}",
            [0xAA] = "sound/voice/{0}/battle",
            [0xD0] = "message/{0}/system",
            [0xD1] = "message/{0}/map",
            [0xD2] = "message/{0}/menu",
            [0xD3] = "message/{0}/event",
            [0xD4] = "message/{0}/mission",
            [0xD5] = "message/{0}/npc_talk/{1}",
            [0xD6] = "message/{0}/network",
            [0xD7] = "message/{0}/battledice",
            [0xD8] = "message/{0}/minigame",
            [0xD9] = "message/{0}/shop",
            [0xDA] = "message/{0}/playerselect",
            [0xDB] = "message/{0}/report",
        };

        protected static Dictionary<string, uint> PathCategoriesReverse = PathCategories
            .SelectMany(x =>
                Constants.Language.Select((l, i) => new
                {
                    Key = (x.Key << 24) | (i << 21),
                    Value = x.Value.Replace("{0}", l)
                })
            )
            .SelectMany(x =>
                Constants.Worlds.Select((w, i) => new
                {
                    Key = x.Key | (i << 16),
                    Value = x.Value.Replace("{1}", w)
                })
            )
            .GroupBy(x => x.Value)
            .ToDictionary(x => x.Key, x => (uint)x.First().Key);

        protected static Dictionary<string, uint> PathsReverse =
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
            [Data] public short DirectoryCount { get; set; }
            [Data] public int PartitionOffset { get; set; }
            [Data] public int DirectoryOffset { get; set; }
            [Data] public short ArchivePartitionSector { get; set; }
            [Data] public short Archive0Sector { get; set; }
            [Data] public int TotalSectorCount { get; set; }
            [Data] public int Archive1Sector { get; set; }
            [Data] public int Archive2Sector { get; set; }
            [Data] public int Archive3Sector { get; set; }
            [Data] public int Archive4Sector { get; set; }
            public Partition<PartitionFileEntry>[] Partitions { get; set; }
        }

        protected class ArchivePartitionHeader
        {
            [Data] public byte Unknown00 { get; set; }
            [Data] public byte PartitionCount { get; set; }
            [Data] public short Unknown02 { get; set; }
            [Data] public short LbaStartOffset { get; set; }
            [Data] public short UnknownOffset { get; set; }
            public Partition<ArchivePartitionEntry>[] Partitions { get; set; }
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
        protected readonly ArchivePartitionHeader _header2;
        protected readonly DirectoryEntry[] _directoryEntries;

        protected Bbsa(Stream stream)
        {
            _header = BinaryMapping.ReadObject<Header>(stream, (int)stream.Position);
            _header.Partitions = ReadPartitions<PartitionFileEntry>(stream, 0x30, _header.PartitionCount);
            ReadPartitionLba(_header.Partitions, stream, _header.PartitionOffset);

            stream.Position = _header.DirectoryOffset;
            var reader = new BinaryReader(stream);
            _directoryEntries = Enumerable.Range(0, _header.DirectoryCount)
                    .Select(x => new DirectoryEntry
                    {
                        FileHash = reader.ReadUInt32(),
                        Info = reader.ReadUInt32(),
                        DirectoryHash = reader.ReadUInt32()
                    }).ToArray();

            int header2Offset = _header.ArchivePartitionSector * 0x800;
            stream.Position = header2Offset;
            _header2 = BinaryMapping.ReadObject<ArchivePartitionHeader>(stream);
            _header2.Partitions = ReadPartitions<ArchivePartitionEntry>(stream, header2Offset + 8, _header2.PartitionCount);
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

            return (lba.Offset + _header.Archive0Sector) * 0x800;
        }

        public static Bbsa Read(Stream stream) => new Bbsa(stream);

        public static string GetDirectoryName(uint hash) =>
            Paths.TryGetValue(hash, out var path) ? path : CalculateFolderName(hash);

        public static uint GetDirectoryHash(string directory)
        {
            if (PathsReverse.TryGetValue(directory.ToLower(), out var hash))
                return (uint)hash;

            if (PathCategoriesReverse.TryGetValue(directory.ToLower(), out hash))
                return (uint)hash;

            return uint.MaxValue;
        }
    }
}

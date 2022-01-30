using System.Collections.Generic;
using System.IO;
using System;
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
            //[0x0050414D] = "arc/map",
            //[0x4E455645] = "arc/event",
            //[0x00004350] = "arc/pc",
            //[0x10004350] = "arc/pc_ven",
            //[0x20004350] = "arc/pc_aqua",
            //[0x30004350] = "arc/pc_terra",
            //[0x4D454E45] = "arc/enemy",
            //[0x53534F42] = "arc/boss",
            //[0x0043504E] = "arc/npc",
            //[0x4D4D4947] = "arc/gimmick",
            //[0x50414557] = "arc/weapon",
            //[0x4D455449] = "arc/item",
            //[0x45464645] = "arc/effect",
            //[0x554E454D] = "arc/menu",
            //[0x00435445] = "arc/etc",
            //[0x00535953] = "arc/system",
            //[0x53455250] = "arc/preset",
            //[0x41455250] = "arc/preset.alpha",
            //[0x55424544] = "arc/debug",
            [0x53534f42] = "arc/boss",
            [0x5353cfc2] = "arc/boss/de",
            [0x53d34f42] = "arc/boss/es",
            [0x53534fc2] = "arc/boss/fr",
            [0x5353cf42] = "arc/boss/it",
            [0x55424544] = "arc/debug",
            [0x5542c5c4] = "arc/debug/de",
            [0x55c24544] = "arc/debug/es",
            [0x554245c4] = "arc/debug/fr",
            [0x5542c544] = "arc/debug/it",
            [0x45464645] = "arc/effect",
            [0x4546c6c5] = "arc/effect/de",
            [0x45c64645] = "arc/effect/es",
            [0x454646c5] = "arc/effect/fr",
            [0x4546c645] = "arc/effect/it",
            [0x4d454e45] = "arc/enemy",
            [0x4d45cec5] = "arc/enemy/de",
            [0x4dc54e45] = "arc/enemy/es",
            [0x4d454ec5] = "arc/enemy/fr",
            [0x4d45ce45] = "arc/enemy/it",
            [0x00435445] = "arc/etc",
            [0x0043d4c5] = "arc/etc/de",
            [0x00c35445] = "arc/etc/es",
            [0x004354c5] = "arc/etc/fr",
            [0x0043d445] = "arc/etc/it",
            [0x4e455645] = "arc/event",
            [0x4e45d6c5] = "arc/event/de",
            [0x4ec55645] = "arc/event/es",
            [0x4e4556c5] = "arc/event/fr",
            [0x4e45d645] = "arc/event/it",
            [0x4d4d4947] = "arc/gimmick",
            [0x4d4dc9c7] = "arc/gimmick/de",
            [0x4dcd4947] = "arc/gimmick/es",
            [0x4d4d49c7] = "arc/gimmick/fr",
            [0x4d4dc947] = "arc/gimmick/it",
            [0x4d455449] = "arc/item",
            [0x4d45d4c9] = "arc/item/de",
            [0x4dc55449] = "arc/item/es",
            [0x4d4554c9] = "arc/item/fr",
            [0x4d45d449] = "arc/item/it",
            [0x0050414d] = "arc/map",
            [0x0050c1cd] = "arc/map/de",
            [0x00d0414d] = "arc/map/es",
            [0x005041cd] = "arc/map/fr",
            [0x0050c14d] = "arc/map/it",
            [0x554e454d] = "arc/menu",
            [0x554ec5cd] = "arc/menu/de",
            [0x55ce454d] = "arc/menu/es",
            [0x554e45cd] = "arc/menu/fr",
            [0x554ec54d] = "arc/menu/it",
            [0x0043504e] = "arc/npc",
            [0x0043d0ce] = "arc/npc/de",
            [0x00c3504e] = "arc/npc/es",
            [0x004350ce] = "arc/npc/fr",
            [0x0043d04e] = "arc/npc/it",
            [0x00004350] = "arc/pc",
            [0x0000c3d0] = "arc/pc/de",
            [0x00804350] = "arc/pc/es",
            [0x000043d0] = "arc/pc/fr",
            [0x0000c350] = "arc/pc/it",
            [0x20004350] = "arc/pc_aqua",
            [0x2000c3d0] = "arc/pc_aqua/de",
            [0x20804350] = "arc/pc_aqua/es",
            [0x200043d0] = "arc/pc_aqua/fr",
            [0x2000c350] = "arc/pc_aqua/it",
            [0x30004350] = "arc/pc_terra",
            [0x3000c3d0] = "arc/pc_terra/de",
            [0x30804350] = "arc/pc_terra/es",
            [0x300043d0] = "arc/pc_terra/fr",
            [0x3000c350] = "arc/pc_terra/it",
            [0x10004350] = "arc/pc_ven",
            [0x1000c3d0] = "arc/pc_ven/de",
            [0x10804350] = "arc/pc_ven/es",
            [0x100043d0] = "arc/pc_ven/fr",
            [0x1000c350] = "arc/pc_ven/it",
            [0x53455250] = "arc/preset",
            [0x5345d2d0] = "arc/preset/de",
            [0x53c55250] = "arc/preset/es",
            [0x534552d0] = "arc/preset/fr",
            [0x5345d250] = "arc/preset/it",
            [0x41455250] = "arc/preset.alpha",
            [0x4145d2d0] = "arc/preset.alpha/de",
            [0x41c55250] = "arc/preset.alpha/es",
            [0x414552d0] = "arc/preset.alpha/fr",
            [0x4145d250] = "arc/preset.alpha/it",
            [0x00535953] = "arc/system",
            [0x0053d9d3] = "arc/system/de",
            [0x00d35953] = "arc/system/es",
            [0x005359d3] = "arc/system/fr",
            [0x0053d953] = "arc/system/it",
            [0x50414557] = "arc/weapon",
            [0x5041c5d7] = "arc/weapon/de",
            [0x50c14557] = "arc/weapon/es",
            [0x504145d7] = "arc/weapon/fr",
            [0x5041c557] = "arc/weapon/it",
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

        public class Header
        {
            [Data] public int MagicCode { get; set; }
            [Data] public int Version { get; set; }
            [Data] public short PartitionCount { get; set; }
            [Data] public short PartitionEntriesCount { get; set; }
            [Data] public short DirectoryCount { get; set; }
            [Data] public short DirectoryEntriesCount { get; set; }
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
            public Directory[] _directories;
        }

        protected class ArchivePartitionHeader
        {
            [Data] public byte Unknown00 { get; set; }
            [Data] public byte PartitionCount { get; set; }
            [Data] public short PartitionEntriesCount { get; set; }
            [Data] public short PartitionEntriesOffset { get; set; }
            [Data] public short ArcEntriesOffset { get; set; }
            public Partition<ArchivePartitionEntry>[] Partitions { get; set; }
        }

        public class Directory
        {
            public short Count { get; set; }
            public short Offset { get; set; }
            public DirectoryEntry[] Files { get; set; }

            internal static Directory Read(Stream stream, int baseOffset, int x, int DirEntriescount, int DirCount)
            {
                var dir = new Directory();

                int blocksize = (DirEntriescount / DirCount) * 12;

                stream.Position = baseOffset + x * blocksize;

                dir.Offset = (short)(baseOffset + x * blocksize);



                dir.Files = Enumerable.Range(0, blocksize / 12)
                    .Select(x => DirectoryEntry.Read(stream)).ToArray();

                return dir;
            }
        }
        public class DirectoryEntry
        {
            [Data] public uint FileHash { get; set; }
            [Data] public uint Info { get; set; }
            [Data] public uint DirectoryHash { get; set; }
            public int Offset => (int)(Info >> 12);
            public int Size => (int)(Info & 0xFFF);

            public long LocationOffset;
            internal static DirectoryEntry Read(Stream stream)
            {
                var pastPos = stream.Position;
                DirectoryEntry part = BinaryMapping.ReadObject<DirectoryEntry>(stream);
                part.LocationOffset = pastPos;
                return part;
            }
            internal void Write(Stream stream)
            {
                Info = (uint)((Offset << 12) + Size);
                BinaryMapping.WriteObject<DirectoryEntry>(stream, this, (int)LocationOffset);
            }
            public override string ToString() =>
                $"{DirectoryHash:X08}/{FileHash:X08} {Offset:X05} {Size:X03}";
        }

        private const int LbaLength = 8;
        protected readonly Header _header;
        protected readonly ArchivePartitionHeader _header2;

        public Header GetHeader() => _header;

        protected Bbsa(Stream stream)
        {
            _header = BinaryMapping.ReadObject<Header>(stream, (int)stream.Position);
            _header.Partitions = ReadPartitions<PartitionFileEntry>(stream, 0x30, _header.PartitionCount);
            ReadPartitionLba(_header.Partitions, stream, _header.PartitionOffset);

            _header._directories = Enumerable.Range(0, _header.DirectoryCount)
                    .Select(x => Directory.Read(stream, _header.DirectoryOffset, x, _header.DirectoryEntriesCount, _header.DirectoryCount)).ToArray();


            int header2Offset = _header.ArchivePartitionSector * 0x800;
            stream.Position = header2Offset;
            _header2 = BinaryMapping.ReadObject<ArchivePartitionHeader>(stream);
            _header2.Partitions = ReadPartitions<ArchivePartitionEntry>(stream, header2Offset + 8, _header2.PartitionCount);
            ReadPartitionLba(_header2.Partitions, stream, header2Offset + _header2.PartitionEntriesOffset);
            //ReadArcEntries(_header2.Partitions, stream, header2Offset + _header2.ArcEntriesOffset);
            //Thanks Thamstras for the structure information, ArcEntries extraction and repacking to be implemented 
        }

        #region Repack
        protected struct TempHeader
        {
            public uint Archive1Sector;
            public uint Archive2Sector;
            public uint Archive3Sector;
            public uint Archive4Sector;
        }
        public static void RepackfromFolder(string inputBBSAFolderPath, string inputFilesFolderPath,
            string outputFolderPath, string ArchivePrefix = "BBS")
        {
            var prefix = ArchivePrefix ?? "BBS";

            var bbsaFileNames = Enumerable.Range(0, 5)
                    .Select(x => Path.Combine(inputBBSAFolderPath, $"{prefix}{x}.DAT"));

            var OUTbbsaFileNames = Enumerable.Range(0, 5)
                    .Select(x => Path.Combine(outputFolderPath, $"{prefix}{x}.DAT"));


            var streams = bbsaFileNames
                    .Select(x => File.OpenRead(x))
                    .ToArray();

            var OUTstreams = OUTbbsaFileNames
                    .Select(x => new FileStream(x,FileMode.OpenOrCreate ,FileAccess.ReadWrite))
                    .ToArray();
            
            Bbsa bbsa = Bbs.Bbsa.Read(streams[0]);

            byte[] Header = new byte[bbsa._header.Archive0Sector * 0x800];
            streams[0].Position = 0;
            streams[0].Read(Header, 0, (int)(bbsa._header.Archive0Sector * 0x800));
            WriteBytes(Header, OUTstreams[0], 0);

            var tempHeader = GetHeader(bbsa, inputFilesFolderPath, prefix, streams, out uint bbs4Count);
            
            #region Header Sectors Counts
            WriteUint(tempHeader.Archive4Sector + bbs4Count - 3, OUTstreams[0], 0x1c);
            WriteUint(tempHeader.Archive1Sector, OUTstreams[0], 0x20);
            WriteUint(tempHeader.Archive2Sector, OUTstreams[0], 0x24);
            WriteUint(tempHeader.Archive3Sector, OUTstreams[0], 0x28);
            WriteUint(tempHeader.Archive4Sector, OUTstreams[0], 0x2c);
            #endregion

            for (int i = 0; i < 5; i++)//Foreach BBS{i} folder/container
            {
                //Write container name+index+extension
                if (i > 0)
                {
                    byte[] writeName = System.Text.Encoding.Default.GetBytes($"{prefix.ToLower()}{i}.dat");
                    WriteBytes(writeName, OUTstreams[i], 0);
                    WriteBytes(new byte[] { (byte)bbsa._header.Version}, OUTstreams[i], 0x10);//Version IMPORTANT!!
                }

                #region Get Container Sector/Index
                uint Sector = 0;
                switch (i)
                {
                    case 0:
                        Sector = (uint)bbsa._header.Archive0Sector;
                        break;
                    default:
                        Sector = 1;
                        break;
                }
                #endregion//Used on Write

                string containerPath = Path.Combine(inputFilesFolderPath, $"{prefix}{i}");
                
                foreach (var file in bbsa.Files.Where(x => x.ArchiveIndex == i).OrderBy(x=>x.offset))
                {
                    var name = file.CalculateNameWithExtension(x => streams[x]);
                    string filePath = Path.Combine(containerPath, name);

                    //Verify file existence
                    if (!File.Exists(filePath))
                    {
                        Console.WriteLine("File not found:");
                        Console.WriteLine(filePath);
                        return;
                    }

                    byte[] FileX = File.ReadAllBytes(filePath);

                    //Verify file Sector Padding
                    if(FileX.Length % 0x800 != 0)
                    {
                        var FileXPadd = new List<byte>();
                        FileXPadd.AddRange(FileX);
                        while (FileXPadd.Count % 0x800 != 0)
                            FileXPadd.Add(0);
                        FileX = FileXPadd.ToArray();
                    }

                    //Get Pointer
                    uint FileSize = GetFileSectorSize((uint)FileX.Length);
                    uint FileOffset = (uint)GetFileOffset(tempHeader, (int)Sector, i);
                    uint Info = (FileOffset << 12) + (FileSize >= 0xFFF ? 0xFFF : FileSize);

                    //Write Pointer
                    WriteUint(Info, OUTstreams[0], file.Location + 4);

                    //Write Files
                    OUTstreams[i].Position = Sector * 0x800;
                    foreach (var b in FileX)
                        OUTstreams[i].WriteByte(b);

                    //Info Out
                    Console.WriteLine($"Nome: {name}\n" +
                        $"Index: {prefix}{i}");

                    //Add Size written
                    Sector += FileSize;
                    FileX = null;
                }
            }

            //Close and flush data(write remaining and close)
            OUTstreams[0].Close();
            OUTstreams[1].Close();
            OUTstreams[2].Close();
            OUTstreams[3].Close();
            OUTstreams[4].Close();
        }

        protected static TempHeader GetHeader(Bbsa bbsa, string inputFilesFolderPath, string prefix, FileStream[] streams, out uint bbs4Count)
        {
            uint bbs4Sec = 0;
            TempHeader tempHeader = new TempHeader();//Used to count sectors in containers
            for (int i = 0; i < 5; i++)//Foreach BBS{i} folder/container
            {

                #region Get Container Sector/Index
                uint Sector = 0;
                switch (i)
                {
                    case 0:
                        Sector = 125;
                        break;
                    default:
                        Sector = 1;
                        break;
                }
                #endregion//Used on Write

                string containerPath = Path.Combine(inputFilesFolderPath, $"{prefix}{i}");

                foreach (var file in bbsa.Files.Where(x => x.ArchiveIndex == i))
                {
                    var name = file.CalculateNameWithExtension(x => streams[x]);
                    string filePath = Path.Combine(containerPath, name);

                    var fileinfo = new FileInfo(filePath);
                    Sector += (uint)fileinfo.Length / 0x800;
                }
                bbs4Sec = Sector;
                #region Get Header Offsets
                switch (i)
                {
                    case 0:
                        tempHeader.Archive1Sector = (uint)(Sector - 125);
                        //Console.WriteLine($"Arch1: {tempHeader.Archive1Sector}");
                        break;
                    case 1:
                        tempHeader.Archive2Sector = (uint)(Sector+tempHeader.Archive1Sector - 2);
                        //Console.WriteLine($"Arch2: {tempHeader.Archive2Sector}");
                        break;
                    case 2:
                        tempHeader.Archive3Sector = (uint)(Sector + tempHeader.Archive2Sector - 3);
                        //Console.WriteLine($"Arch3: {tempHeader.Archive3Sector}");
                        break;
                    case 3:
                        tempHeader.Archive4Sector = (uint)(Sector + tempHeader.Archive3Sector - 2);
                        //Console.WriteLine($"Arch4: {tempHeader.Archive4Sector}");
                        break;
                }
                #endregion
            }
            bbs4Count = bbs4Sec;
            return tempHeader;
        }
        private static void WriteBytes(byte[] bytes, Stream destination, uint destinationOffset)
        {
            destination.Position = destinationOffset;
            foreach (var b in bytes)
                destination.WriteByte(b);
        }
        private static void WriteUint(uint value, Stream destination, uint destinationOffset)
        {
            byte[] valueBytes = BitConverter.GetBytes((UInt32)value);
            WriteBytes(valueBytes, destination, destinationOffset);
        }
        private static uint GetFileSectorSize(uint length)
        {
            uint result = length;
            while (length % 0x800 != 0)
                result++;
            return (uint)(result / 0x800);
        }
        protected static int GetArchiveIndex(Header header, int offset)
        {
            int archiveIndex = 0;
            if (offset >= header.Archive4Sector)
            {
                archiveIndex = 4;
            }
            else if (offset >= header.Archive3Sector)
            {
                archiveIndex = 3;
            }
            else if (offset >= header.Archive2Sector)
            {
                archiveIndex = 2;
            }
            else if (offset >= header.Archive1Sector)
            {
                archiveIndex = 1;
            }
            else if (offset >= header.Archive0Sector)
            {
                archiveIndex = 0;
            }
            else if(offset <= header.Archive0Sector)
            {
                archiveIndex = 0;
            }
            return archiveIndex;
        }
        protected static int GetFileOffset(TempHeader header, int baseOffset, int partIndex)
        {
            int resultOffset = 0;
            var temp = header;
            if (partIndex == 4)
            {
                resultOffset = baseOffset + (int)temp.Archive4Sector - 1;
            }
            else if (partIndex == 3)
            {
                resultOffset = baseOffset + (int)temp.Archive3Sector - 1;
            }
            else if (partIndex == 2)
            {
                resultOffset = baseOffset + (int)temp.Archive2Sector - 1;
            }
            else if (partIndex == 1)
            {
                resultOffset = baseOffset + (int)temp.Archive1Sector - 1;
            }
            else if (partIndex == 0)
            {
                resultOffset = baseOffset - 125;
            }
            else
            {
                resultOffset = -1;
            }
            return resultOffset;
        }
        #endregion
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
                            "arc",
                            folder,
                            lba.Hash,
                            0,
                            (uint)lba.LocationOffset,
                            lba.Offset==0?0:GetArchiveIndex(_header, lba.Offset));
                    }
                }

                for (int d = 0; d < _header.DirectoryCount; d++)
                {
                    string ext = "";
                    if (d < KnownExtensions.Length)
                    {
                        ext = KnownExtensions[d];
                    }

                    foreach (var file in _header._directories[d].Files)
                    {
                        NameDictionary.TryGetValue(file.FileHash, out var fileName);
                        NameDictionary.TryGetValue(file.DirectoryHash, out var folderName);

                        yield return new Entry(
                            this,
                            file.Offset,
                            file.Size,
                            fileName,
                            ext,
                            folderName,
                            file.FileHash,
                            file.DirectoryHash,
                            (uint)file.LocationOffset,
                            file.Offset==0?0:GetArchiveIndex(_header, file.Offset));
                    }
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

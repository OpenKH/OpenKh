using OpenKh.Common;
using System;
using System.IO;
using Xe.IO;

namespace OpenKh.Bbs
{
    public partial class Bbsa
    {
        public class Entry
        {
            private const int SectorLength = 0x800;
            private readonly Header bbsaHeader;
            private readonly int offset;
            private readonly int length;
            private readonly string fileName;
            private readonly string folderName;

            internal Entry(
                Bbsa bbsa,
                int offset,
                int length,
                string fileName,
                string folderName,
                uint fileHash,
                uint folderHash)
            {
                bbsaHeader = bbsa._header;
                this.offset = offset;
                this.length = length;
                this.fileName = fileName;
                this.folderName = folderName;
                FileHash = fileHash;
                FolderHash = folderHash;
            }

            public uint FileHash { get; }
            public uint FolderHash { get; }
            public string Name => $"{FolderName}/{FileName}";
            public bool HasCompleteName => fileName != null && folderName != null;

            public string CalculateNameWithExtension(Func<int, Stream> bbsaLoader)
            {
                if (!CalculateArchiveOffset(bbsaHeader, offset, out var archiveIndex, out var physicalSector))
                    return Name;

                var stream = bbsaLoader(archiveIndex);
                var extension = CalculateExtension(stream, physicalSector * SectorLength);
                if (extension == null)
                    return Name;

                return $"{Name}.{extension}";
            }

            public SubStream OpenStream(Func<int, Stream> bbsaLoader)
            {
                if (!CalculateArchiveOffset(bbsaHeader, offset, out var archiveIndex, out var physicalSector))
                    return null;

                var stream = bbsaLoader(archiveIndex);
                var subStreamOffset = physicalSector * SectorLength;
                var subStreamLength = length * SectorLength;

                if (length == 0xFFF)
                {
                    if (IsPsmf(stream, subStreamOffset))
                        subStreamLength = GetPsmfLength(stream, subStreamOffset);
                }

                return new SubStream(stream, subStreamOffset, subStreamLength);
            }

            private string FileName => fileName ?? $"@{FileHash:X08}";
            private string FolderName =>
                folderName ??
                CalculateFolderName(FolderHash) ??
                $"@{FolderHash:X08}";
        }

        private static bool IsPsmf(Stream stream, int offset) =>
            new BinaryReader(stream.SetPosition(offset)).ReadInt32() == 0x464D5350;

        private static int GetPsmfLength(Stream stream, int offset)
        {
            stream.SetPosition(offset + 12);
            return (stream.ReadByte() << 24) |
                (stream.ReadByte() << 16) |
                (stream.ReadByte() << 8) |
                (stream.ReadByte() << 0);
        }

        private static bool CalculateArchiveOffset(
            Header header, int offset, out int archiveIndex, out int physicalSector)
        {
            if (offset >= header.Archive4Sector)
            {
                archiveIndex = 4;
                physicalSector = offset - header.Archive4Sector + 1;
            }
            else if (offset >= header.Archive3Sector)
            {
                archiveIndex = 3;
                physicalSector = offset - header.Archive3Sector + 1;
            }
            else if (offset >= header.Archive2Sector)
            {
                archiveIndex = 2;
                physicalSector = offset - header.Archive2Sector + 1;
            }
            else if (offset >= header.Archive1Sector)
            {
                archiveIndex = 1;
                physicalSector = offset - header.Archive1Sector + 1;
            }
            else if (offset >= header.Archive0Sector)
            {
                archiveIndex = 0;
                physicalSector = offset + header.Archive0Sector;
            }
            else
            {
                archiveIndex = -1;
                physicalSector = -1;
                return false;
            }

            return true;
        }

        private static string CalculateExtension(Stream stream, int offset)
        {
            stream.Position = offset;
            var magicCode = new BinaryReader(stream).ReadUInt32();
            switch (magicCode)
            {
                case 0x61754C1B: return "lub";
                case 0x41264129: return "ice";
                case 0x44544340: return "ctd";
                case 0x50444540: return "edp";
                case 0x00435241: return "arc";
                case 0x44424D40: return "mbd";
                case 0x00444145: return "ead";
                case 0x07504546: return "fep";
                case 0x00425449: return "itb";
                case 0x00435449: return "itc";
                case 0x00455449: return "ite";
                case 0x004D4150: return "pam";
                case 0x004F4D50: return "pmo";
                case 0x42444553: return "scd";
                case 0x324D4954: return "tm2";
                case 0x00415854: return "txa";
                case 0x00617865: return "exa";
                default: return null;
            }
        }

        private static string CalculateFolderName(uint hash)
        {
            var category = (byte)(hash >> 24);
            var world = (hash >> 16) & 0x1F;
            var language = (hash >> 21) & 7;
            var id = hash & 0xFFFF;

            var strWorld = world < Constants.Worlds.Length ?
                Constants.Worlds[world] : null;
            var strLanguage = language < Constants.Language.Length ?
                Constants.Language[language] : null;

            if (!PathCategories.TryGetValue(category, out var pathCategory))
                return null;

            return string.Format(pathCategory, strLanguage, strWorld);
        }
    }
}

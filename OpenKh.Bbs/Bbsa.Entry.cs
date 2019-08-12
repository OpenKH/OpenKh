using System;
using System.IO;
using OpenKh.Common;
using Xe.IO;

namespace OpenKh.Bbs
{
    public partial class Bbsa
    {
        public class Entry
        {
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

            public SubStream OpenStream(Func<int, Stream> bbsaLoader)
            {
                int archiveIndex;
                int realOffset;

                if (offset >= bbsaHeader.Archive4SectorIndex)
                {
                    archiveIndex = 4;
                    realOffset = offset - bbsaHeader.Archive4SectorIndex + 1;
                }
                else if (offset >= bbsaHeader.Archive3SectorIndex)
                {
                    archiveIndex = 3;
                    realOffset = offset - bbsaHeader.Archive3SectorIndex + 1;
                }
                else if (offset >= bbsaHeader.Archive2SectorIndex)
                {
                    archiveIndex = 2;
                    realOffset = offset - bbsaHeader.Archive2SectorIndex + 1;
                }
                else if (offset >= bbsaHeader.Archive1SectorIndex)
                {
                    archiveIndex = 1;
                    realOffset = offset - bbsaHeader.Archive1SectorIndex + 1;
                }
                else if (offset >= bbsaHeader.Archive0SectorIndex)
                {
                    archiveIndex = 0;
                    realOffset = offset + bbsaHeader.Archive0SectorIndex;
                }
                else
                    return null;

                var stream = bbsaLoader(archiveIndex);
                var subStreamOffset = realOffset * 0x800;
                var subStreamLength = length * 0x800;

                if (length == 0xFFF)
                {
                    if (IsPsmf(stream, subStreamOffset))
                        subStreamLength = GetPsmfLength(stream, subStreamOffset);
                }

                return new SubStream(stream, subStreamOffset, subStreamLength);
            }

            private string FileName => fileName ?? $"@{FileHash:X08}";
            private string FolderName => folderName ?? $"@{FolderHash:X08}";
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
    }
}

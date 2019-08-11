using System;
using System.IO;
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
            private readonly uint fileHash;
            private readonly uint folderHash;

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
                this.fileHash = fileHash;
                this.folderHash = folderHash;
            }

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

                return new SubStream(bbsaLoader(archiveIndex), realOffset * 0x800, length * 0x800);
            }

            private string FileName => fileName ?? $"@{fileHash:X08}";
            private string FolderName => folderName ?? $"@{folderHash:X08}";
        }
    }
}

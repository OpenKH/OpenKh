using System;
using System.Collections.Generic;
using System.IO;
using Xe.IO;

namespace OpenKh.Kh2
{
    public class Img
    {
        public const int IsoBlockAlign = 0x800;

        public static string[] InternalIdxs =
        {
            "000al.idx",
            "000bb.idx",
            "000ca.idx",
            "000dc.idx",
            "000di.idx",
            "000eh.idx",
            "000es.idx",
            "000gumi.idx",
            "000hb.idx",
            "000he.idx",
            "000lk.idx",
            "000lm.idx",
            "000mu.idx",
            "000nm.idx",
            "000po.idx",
            "000tr.idx",
            "000tt.idx",
            "000wi.idx",
            "000wm.idx",
        };

        private readonly Stream stream;

        public Img(Stream stream, IEnumerable<Idx.Entry> idxEntries, bool loadAllIdx)
        {
            this.stream = stream;
            Entries = new IdxDictionary
            {
                idxEntries
            };

            if (loadAllIdx)
            {
                foreach (var fileName in InternalIdxs)
                {
                    FileOpen(fileName, entryStream =>
                    {
                        Entries.Add(Idx.Read(entryStream));
                    });
                }
            }
        }

        public IdxDictionary Entries { get; }

        public bool FileExists(string fileName) => Entries.Exists(fileName);

        public bool TryFileOpen(string fileName, out Stream stream)
        {
            bool result;
            if (result = Entries.TryGetEntry(fileName, out var entry))
            {
                stream = FileOpen(entry);
                return true;
            }

            stream = null;
            return false;
        }

        public bool FileOpen(string fileName, Action<Stream> callback)
        {
            bool result;
            if (result = Entries.TryGetEntry(fileName, out var entry))
                callback(FileOpen(entry));

            return result;
        }

        public Stream FileOpen(string fileName)
        {
            if (Entries.TryGetEntry(fileName, out var entry))
                return FileOpen(entry);

            throw new FileNotFoundException("File not found", fileName);
        }

        public Stream FileOpen(Idx.Entry entry)
        {
            if (entry.IsCompressed)
            {
                var fileStream = new SubStream(stream, entry.Offset * IsoBlockAlign, entry.BlockLength * IsoBlockAlign + IsoBlockAlign);
                return new MemoryStream(Decompress(fileStream));
            }

            return new SubStream(stream, entry.Offset * IsoBlockAlign, entry.Length);
        }

        public static byte[] Decompress(Stream src)
        {
            return Decompress(new BinaryReader(src).ReadBytes((int)src.Length));
        }

        public static byte[] Decompress(byte[] srcData)
        {
            var srcIndex = srcData.Length - 1;

            byte key = 0;
            while (srcIndex > 0 && (key = srcData[srcIndex--]) == 0) ;
            if (srcIndex == 0)
                return new byte[0];

            int decSize = srcData[srcIndex--] |
                (srcData[srcIndex--] << 8) |
                (srcData[srcIndex--] << 16) |
                (srcData[srcIndex--] << 24);

            int dstIndex = decSize - 1;
            var dstData = new byte[decSize];

            while (dstIndex >= 0 && srcIndex >= 0)
            {
                var data = srcData[srcIndex--];
                if (data == key && srcIndex >= 0)
                {
                    var copyIndex = srcData[srcIndex--];
                    if (copyIndex > 0 && srcIndex >= 0)
                    {
                        var copyLength = srcData[srcIndex--];
                        for (int i = 0; i < copyLength + 3 && dstIndex >= 0; i++)
                        {
                            dstData[dstIndex--] = dstData[dstIndex + copyIndex + 1];
                        }
                    }
                    else
                    {
                        dstData[dstIndex--] = data;
                    }

                }
                else
                {
                    dstData[dstIndex--] = data;
                }
            }

            return dstData;
        }

        public static byte[] Compress(byte[] srcData)
        {
            const int MaxWindowSize = 0x102;
            const int MaxSlidingIndex = 0xff;

            var decompressedLength = srcData.Length;
            var key = GetLeastUsedByte(srcData);
            var buffer = new List<byte>(decompressedLength);

            buffer.Add(key);
            buffer.Add((byte)(decompressedLength >> 0));
            buffer.Add((byte)(decompressedLength >> 8));
            buffer.Add((byte)(decompressedLength >> 16));
            buffer.Add((byte)(decompressedLength >> 24));

            int sourceIndex = decompressedLength - 1;

            while (sourceIndex >= 0)
            {
                var ch = srcData[sourceIndex];
                if (ch == key)
                {
                    buffer.Add(key);
                    buffer.Add(0);
                    sourceIndex--;
                    continue;
                }

                var windowSizeCandidate = 0;
                var slidingIndexCandidate = -1;
                var maxWindowSize = Math.Min(sourceIndex, MaxWindowSize);
                for (var slidingIndex = MaxSlidingIndex; slidingIndex > 0; slidingIndex--)
                {
                    if (slidingIndex + sourceIndex >= decompressedLength)
                        continue;

                    int windowSize;
                    for (windowSize = 0; windowSize < maxWindowSize; windowSize++)
                    {
                        var startWindow = sourceIndex + slidingIndex - windowSize;
                        if (srcData[startWindow] != srcData[sourceIndex - windowSize])
                            break;
                    }

                    if (windowSize > windowSizeCandidate)
                    {
                        windowSizeCandidate = windowSize;
                        slidingIndexCandidate = slidingIndex;
                    }
                }

                if (windowSizeCandidate > 3)
                {
                    buffer.Add(key);
                    buffer.Add((byte)slidingIndexCandidate);
                    buffer.Add((byte)(windowSizeCandidate - 3));
                    sourceIndex -= windowSizeCandidate;
                }
                else
                {
                    buffer.Add(ch);
                    sourceIndex--;
                }
            }

            var compressedLength = buffer.Count;
            var cmp = new byte[compressedLength];
            for (var i = 0; i < compressedLength; i++)
            {
                cmp[i] = buffer[compressedLength - i - 1];
            }

            return cmp;
        }

        private static byte GetLeastUsedByte(byte[] data)
        {
            var dictionary = new int[0x100];
            foreach (var ch in data)
                dictionary[ch]++;

            var elementFound = int.MaxValue;
            var indexFound = 1; // the least used byte is never zero
            for (var i = 1; i < 0x100; i++)
            {
                if (dictionary[i] < elementFound)
                {
                    elementFound = dictionary[i];
                    indexFound = i;
                }
            }

            return (byte)indexFound;
        }
    }
}

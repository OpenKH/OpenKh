using System.Collections.Generic;
using System.IO;
using Xe.IO;

namespace OpenKh.Kh2
{
    public class Img
    {
        const int IsoBlockAlign = 0x800;

        private static string[] InternalIdxs =
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

        public Img(Stream stream, Idx idx, bool loadAllIdx)
        {
            this.stream = stream;
            Idx = idx;
            if (loadAllIdx)
                Idx = LoadAllIdx(Idx);
        }

        public Idx Idx { get; }

        public Stream FileOpen(string fileName)
        {
            if (Idx.TryGetEntry(fileName, out var entry))
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

            byte key;
            while ((key = srcData[srcIndex--]) == 0) ;

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

        private static byte ReadByteBackward(Stream stream)
        {
            stream.Position -= 1;
            var b = stream.ReadByte();
            stream.Position -= 1;
            return (byte)b;
        }

        private static void WriteByteBackward(Stream stream, byte value)
        {
            stream.Position -= 1;
            stream.WriteByte(value);
            stream.Position -= 1;
        }

        private Idx LoadAllIdx(Idx idx)
        {
            foreach (var fileName in InternalIdxs)
            {
                idx = idx.Merge(Idx.Read(FileOpen(fileName)));
            }

            return idx;
        }
    }
}

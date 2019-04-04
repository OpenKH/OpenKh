using System.IO;
using Xe.IO;

namespace kh.kh2
{
    public class Img
    {
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
            var entry = Idx.GetEntry(fileName);
            if (entry == null)
                throw new FileNotFoundException("File not found", fileName);

            return new SubStream(stream, entry.Offset * 0x800, entry.Length);
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

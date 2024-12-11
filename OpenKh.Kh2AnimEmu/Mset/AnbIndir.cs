using OpenKh.Kh2Anim.Mset.Interfaces;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh2Anim.Mset
{
    /// <summary>
    /// anb indirection
    /// </summary>
    public class AnbIndir
    {
        private readonly IEnumerable<Bar.Entry> entries;

        public AnbIndir(IEnumerable<Bar.Entry> entries)
        {
            this.entries = entries;
        }

        public bool HasAnimationData => entries
                .Any(it => it.Type == Bar.EntryType.Motion);

        public IAnimMatricesProvider GetAnimProvider(Stream mdlxStream)
        {
            var animEntry = entries
                .First(it => it.Type == Bar.EntryType.Motion); // anb bar should have single 0x09 (AnimationData)
            var animStream = animEntry.Stream;

            var animReader = new AnimReader(animStream);

            return new EmuBasedAnimMatricesProvider(animReader, mdlxStream, animStream);
        }
    }
}

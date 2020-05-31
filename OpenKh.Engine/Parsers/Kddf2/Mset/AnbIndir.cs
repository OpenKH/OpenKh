using OpenKh.Engine.Parsers.Kddf2.Mset.EmuRunner;
using OpenKh.Engine.Parsers.Kddf2.Mset.Interfaces;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace OpenKh.Engine.Parsers.Kddf2.Mset
{
    /// <summary>
    /// anb indirection
    /// </summary>
    public class AnbIndir
    {
        private readonly IList<Bar.Entry> entries;

        public AnbIndir(IList<Bar.Entry> entries)
        {
            this.entries = entries;
        }

        public IAnimMatricesProvider GetAnimProvider(int anbBarOffset, Stream mdlxStream, Stream msetStream)
        {
            var animEntry = entries
                .First(it => it.Type == Bar.EntryType.AnimationData); // anb bar should have single 0x09 (AnimationData)
            var animStream = animEntry.Stream;

            var anbAbsOff = (uint)(anbBarOffset + animEntry.Offset);

            var animReader = new AnimReader(animStream);

            return new EmuBasedAnimMatricesProvider(animReader, anbAbsOff, mdlxStream, msetStream);
        }
    }
}

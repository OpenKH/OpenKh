using OpenKh.Kh2;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Kh2Anim.Mset
{
    /// <summary>
    /// mset indirection
    /// </summary>
    public class MsetIndir
    {
        private readonly IList<Bar.Entry> entries;

        public MsetIndir(IList<Bar.Entry> entries)
        {
            this.entries = entries;
        }

        public AnbIndir GetAnb(int barEntryIndex)
        {
            var anbBar = entries
                .Skip(barEntryIndex)
                .First(it => it.Type == Bar.EntryType.Anb);

            return new AnbIndir(Bar.Read(anbBar.Stream));
        }
    }
}

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh2.Extensions
{
    public static class BarExtensions
    {
        public static IEnumerable<Bar.Entry> ForEntry(this IEnumerable<Bar.Entry> entries, Bar.EntryType type, string name, int index, Action<Bar.Entry> funcEntry)
        {
            var entry = entries.FirstOrDefault(x => x.Type == type && x.Name == name && x.Index == index);
            if (entry == null)
            {
                entry = new Bar.Entry
                {
                    Type = type,
                    Name = name,
                    Index = index,
                    Stream = new MemoryStream()
                };

                entries = entries.Concat(new Bar.Entry[] { entry });
            }

            funcEntry(entry);
            return entries;
        }
    }
}

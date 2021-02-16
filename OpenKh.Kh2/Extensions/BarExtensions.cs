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

        public static IEnumerable<Bar.Entry> AddOrReplace(
            this IEnumerable<Bar.Entry> entries, Bar.Entry entry)
        {
            var existingEntry = entries
                .FirstOrDefault(x => x.Type == entry.Type && x.Name == entry.Name && x.Index == 0);
            if (existingEntry == null)
            {
                entries = entries.Concat(new Bar.Entry[] {
                    new Bar.Entry
                    {
                        Type = entry.Type,
                        Name = entry.Name,
                        Index = 0,
                        Stream = entry.Stream
                    }
                });
            }
            else
                existingEntry.Stream = entry.Stream;

            return entries;
        }

        public static bool ForEntry(this IEnumerable<Bar.Entry> entries, string name, Bar.EntryType type, Action<Stream> action)
        {
            var entry = entries.FirstOrDefault(x => x.Type == type && x.Name == name);
            if (entry == null)
                return false;

            entry.Stream.Seek(0, SeekOrigin.Begin);
            action(entry.Stream);
            return true;
        }

        public static T ForEntry<T>(this IEnumerable<Bar.Entry> entries, string name, Bar.EntryType type, Func<Stream, T> action) =>
            entries.ForEntry(x => x.Type == type && x.Name == name, action);

        public static T ForEntry<T>(this IEnumerable<Bar.Entry> entries, Func<Bar.Entry, bool> predicate, Func<Stream, T> action)
        {
            var entry = entries.FirstOrDefault(predicate);
            if (entry == null)
                return default;

            entry.Stream.Seek(0, SeekOrigin.Begin);
            return action(entry.Stream);
        }

        public static int ForEntries<T>(this IEnumerable<Bar.Entry> entries, string name, Bar.EntryType type, Action<Stream> action)
        {
            var itemFound = 0;
            foreach (var entry in entries.Where(x => x.Type == type && x.Name == name))
            {
                entry.Stream.Seek(0, SeekOrigin.Begin);
                action(entry.Stream);
                itemFound++;
            }

            return itemFound;
        }

        public static IEnumerable<T> ForEntries<T>(this IEnumerable<Bar.Entry> entries, string name, Bar.EntryType type, Func<Stream, T> action) =>
            entries.Where(x => x.Type == type && x.Name == name).Select(x =>
            {
                x.Stream.Seek(0, SeekOrigin.Begin);
                return action(x.Stream);
            });
    }
}

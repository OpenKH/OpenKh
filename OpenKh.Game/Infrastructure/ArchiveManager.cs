using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Game.Infrastructure
{
    public class ArchiveManager
    {
        private class Entry
        {
            public Stream Stream { get; set; }
            public object Object { get; set; }
        }

        private readonly Dictionary<Bar.EntryType, Func<Entry, object>> getters = new Dictionary<Bar.EntryType, Func<Entry, object>>
        {
            [Bar.EntryType.Layout] = entry => Layout.Read(entry.Stream),
            [Bar.EntryType.Seqd] = entry => Sequence.Read(entry.Stream),
            [Bar.EntryType.Imgd] = entry => Imgd.Read(entry.Stream),
            [Bar.EntryType.Imgz] = entry => new Imgz(entry.Stream),
            [Bar.EntryType.Vif] = entry => Mdlx.Read(entry.Stream),
            [Bar.EntryType.Tim2] = entry => ModelTexture.Read(entry.Stream),
        };
        private readonly Dictionary<(string name, Bar.EntryType type), Entry> archives;
        private readonly IDataContent dataContent;

        public ArchiveManager(IDataContent dataContent)
        {
            archives = new Dictionary<(string name, Bar.EntryType type), Entry>();
            this.dataContent = dataContent;
        }

        public void LoadArchive(string fileName)
        {
            var stream = dataContent.FileOpen(fileName);
            var entries = Bar.Read(stream);

            foreach (var entry in entries)
                archives[(entry.Name, entry.Type)] = new Entry
                {
                    Stream = entry.Stream
                };
        }

        public T Get<T>(string resourceName)
            where T : class
        {
            if (typeof(T) == typeof(Layout)) return GetItem<T>(resourceName, Bar.EntryType.Layout);
            if (typeof(T) == typeof(Sequence)) return GetItem<T>(resourceName, Bar.EntryType.Seqd);
            if (typeof(T) == typeof(Imgd)) return GetItem<T>(resourceName, Bar.EntryType.Imgd);
            if (typeof(T) == typeof(Imgz)) return GetItem<T>(resourceName, Bar.EntryType.Imgz);
            if (typeof(T) == typeof(Mdlx)) return GetItem<T>(resourceName, Bar.EntryType.Vif);
            if (typeof(T) == typeof(ModelTexture)) return GetItem<T>(resourceName, Bar.EntryType.Tim2);
            return null;
        }

        private T GetItem<T>(string resourceName, Bar.EntryType type)
            where T : class
        {
            var entry = archives[(resourceName, type)];
            if (entry.Object is T item)
                return item;

            entry.Stream.Position = 0;
            entry.Object = getters[type](entry);
            return entry.Object as T;
        }
    }
}

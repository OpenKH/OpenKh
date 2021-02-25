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
            [Bar.EntryType.LayoutData] = entry => Layout.Read(entry.Stream),
            [Bar.EntryType.SequenceData] = entry => Sequence.Read(entry.Stream),
            [Bar.EntryType.ImageData] = entry => Imgd.Read(entry.Stream),
            [Bar.EntryType.ImageZip] = entry => new Imgz(entry.Stream),
            [Bar.EntryType.Model] = entry => Mdlx.Read(entry.Stream),
            [Bar.EntryType.ModelTexture] = entry => ModelTexture.Read(entry.Stream),
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
            using var stream = dataContent.FileOpen(fileName);
            LoadArchive(Bar.Read(stream));
        }

        public void LoadArchive(List<Bar.Entry> entries)
        {
            foreach (var entry in entries)
                archives[(entry.Name, entry.Type)] = new Entry
                {
                    Stream = entry.Stream
                };
        }

        public T Get<T>(string resourceName)
            where T : class
        {
            if (typeof(T) == typeof(Layout)) return GetItem<T>(resourceName, Bar.EntryType.LayoutData);
            if (typeof(T) == typeof(Sequence)) return GetItem<T>(resourceName, Bar.EntryType.SequenceData);
            if (typeof(T) == typeof(Imgd)) return GetItem<T>(resourceName, Bar.EntryType.ImageData);
            if (typeof(T) == typeof(Imgz)) return GetItem<T>(resourceName, Bar.EntryType.ImageZip);
            if (typeof(T) == typeof(Mdlx)) return GetItem<T>(resourceName, Bar.EntryType.Model);
            if (typeof(T) == typeof(ModelTexture)) return GetItem<T>(resourceName, Bar.EntryType.ModelTexture);
            return null;
        }

        private T GetItem<T>(string resourceName, Bar.EntryType type)
            where T : class
        {
            if (!archives.TryGetValue((resourceName, type), out var entry))
                return null;

            if (entry.Object is T item)
                return item;

            entry.Stream.Position = 0;
            entry.Object = getters[type](entry);
            return entry.Object as T;
        }
    }
}

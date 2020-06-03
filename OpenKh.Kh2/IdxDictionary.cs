using System.Collections;
using System.Collections.Generic;

namespace OpenKh.Kh2
{
    public class IdxDictionary : IEnumerable<Idx.Entry>
    {
        private readonly Dictionary<long, Idx.Entry> _entries = new Dictionary<long, Idx.Entry>();

        public IEnumerator<Idx.Entry> GetEnumerator() =>
            _entries.Values.GetEnumerator();

        IEnumerator IEnumerable.GetEnumerator() =>
            _entries.Values.GetEnumerator();

        public void Add(Idx.Entry entry) =>
            _entries[GetHash(entry)] = entry;

        public void Add(IEnumerable<Idx.Entry> entries)
        {
            foreach (var entry in entries)
                _entries[GetHash(entry)] = entry;
        }

        public bool Exists(string name) => _entries.ContainsKey(GetHash(name));

        public bool TryGetEntry(string name, out Idx.Entry entry) =>
            _entries.TryGetValue(GetHash(name), out entry);

        public bool TryGetEntry(uint hash32, ushort hash16, out Idx.Entry entry) =>
            _entries.TryGetValue(GetHash(hash32, hash16), out entry);

        internal static long GetHash(string name) =>
            Idx.GetHash32(name) | ((long)Idx.GetHash16(name) << 32);

        internal static long GetHash(Idx.Entry e) =>
            e.Hash32 | ((long)e.Hash16 << 32);

        internal static long GetHash(uint hash32, ushort hash16) =>
            hash32 | ((long)hash16 << 32);
    }
}

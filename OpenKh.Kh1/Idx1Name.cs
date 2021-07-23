using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh1
{
    public record Idx1Name
    {
        public static string[] Names = File.ReadAllLines(Path.Combine(AppContext.BaseDirectory, "resources/kh1idx.txt"));
        private static Dictionary<uint, string> _nameDictionary = Names.ToDictionary(name => Idx1.GetHash(name), name => name);

        public Idx1 Entry { get; set; }
        public string Name { get; set; }

        public static IEnumerable<Idx1Name> Lookup(IEnumerable<Idx1> entries) => entries
            .Select(entry => new Idx1Name
            {
                Entry = entry,
                Name = Lookup(entry)
            });

        public static string Lookup(Idx1 entry) =>
            _nameDictionary.TryGetValue(entry.Hash, out var name) ? name : null;
    }
}

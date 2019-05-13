using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh2
{
    public partial class Idx
    {
        public class NameEntry
        {
            public Entry Entry { get; set; }

            public string Name { get; set; }
        }

        private Dictionary<string, string> nameDictionary;

        public IEnumerable<NameEntry> GetNameEntries()
        {
            var nameDic = GetNameDictionary();

            return Items
                .Select(x =>
                {
                    nameDic.TryGetValue($"{x.Hash32}{x.Hash16}", out var name);
                    return new NameEntry
                    {
                        Entry = x,
                        Name = name
                    };
                });
        }

        private Dictionary<string, string> GetNameDictionary()
        {
            if (nameDictionary == null)
            {
                nameDictionary = ReadLines("resources/kh2idx.txt")
                    .ToDictionary(x => $"{GetHash32(x)}{GetHash16(x)}", x => x);
            }

            return nameDictionary;
        }

        private static IEnumerable<string> ReadLines(string fileName)
        {
            using (var stream = File.OpenText(fileName))
            {
                while (true)
                {
                    var line = stream.ReadLine();
                    if (line == null)
                        break;

                    yield return line;
                }
            }
        }
    }
}

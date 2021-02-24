using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Bbs
{
    public partial class Bbsa
    {
        protected static Dictionary<uint, string> NameDictionary =
            TryReadLines(Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "resources/bbsa.txt"))
            .ToDictionary(x => GetHash(x), x => x);

        private static IEnumerable<string> TryReadLines(string fileName) =>
            File.Exists(fileName) ? ReadLines(fileName) : new string[0];

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

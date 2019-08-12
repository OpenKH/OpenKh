using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;

namespace OpenKh.Bbs
{
    public partial class Bbsa
    {
        private static Dictionary<uint, string> NameDictionary =
            TryReadLines(Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location), "resources/bbsa.txt"))
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

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace BruteforceKh2Names
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var files = LoadFilesList("resources/kh2idx.txt").ToArray();
            var knownExtensions = files.Select(x => Path.GetExtension(x)).Distinct().ToArray();
            var knownLangs = new[] { "jp", "us", "uk", "it", "fr", "sp", "gr", "fm", "fj" };
            var knownWorlds = new[] { "al", "bb", "ca", "cm", "dc", "di", "dw", "eh", "es", "ex", "fa", "gumi", "hb", "he", "lk", "lm", "mu", "nm", "po", "to", "tr", "tt", "wi", "wm", "zz" };
            var knownModelsPrefixes = new[] { "b_", "f_", "g_", "h_", "m_", "n_", "p_", "w_" };
            var knownPatterns = files
                .Distinct()
                .Select(x => ReplaceWithAnim(x))
                .Select(x => ReplaceWithModel(x, knownModelsPrefixes, knownWorlds))
                .Select(x => SecureWord(x, "libretto-", xx => Replace(x, knownWorlds, "{world}")))
                .Select(x => SecureWord(x, "libretto-", xx => Replace(x, knownWorlds, "{world2}")))
                .Select(x => SecureWord(x, "limit/", xx => Replace(x, knownLangs, "{lang}")))
                .Distinct()
                .ToArray();

            //Console.WriteLine(string.Join(",", knownModels.Select(x => $"\"{x}\"")));
        }

        private static IEnumerable<string> LoadFilesList(string fileName)
        {
            using (var stream = File.OpenText(fileName))
            {
                while (true)
                {
                    var name = stream.ReadLine();
                    if (name == null)
                        yield break;

                    yield return name;
                }
            }
        }

        private static string SecureWord(string str, string word, Func<string, string> func)
        {
            var indexOf = str.IndexOf(word);
            if (indexOf > 0)
                str = str.Replace(word, "#");

            str = func(str);

            if (indexOf > 0)
                str = str.Replace("#", word);

            return str;
        }

        private static string Replace(string str, string[] search, string replacement)
        {
            if (str.CompareTo("anm/nm/h_cm080/ENM{animId}.anb") == 0)
                str = str;

            var indexOf = search.Select(lang => str.IndexOf(lang, StringComparison.InvariantCultureIgnoreCase)).FirstOrDefault(langIndex => langIndex > 0);
            if (indexOf > 0)
            {
                // Hack to avoid the replacement of anm/ to a{world}/ due to nm world.
                if (indexOf == 1 && str.IndexOf("anm") == 0)
                    return $"anm{Replace(str.Substring(3), search, replacement)}";

                var toReplace = str.Substring(indexOf, 2);
                return str
                    .Replace(toReplace, replacement)
                    .Replace(toReplace.ToUpper(), replacement.ToUpper());
            }

            return str;
        }

        public static string ReplaceWithModel(string str, string[] modelPrefixes, string[] worlds)
        {
            var indexOf = modelPrefixes.Select(prefix => str.IndexOf(prefix)).FirstOrDefault(index => index > 0);
            var sub1 = str.Substring(indexOf + 2);
            if (sub1.Length < 5)
                return str;

            var sub2 = sub1.Substring(0, 5);
            var world = sub1.Substring(0, 2);
            var id = sub1.Substring(2, 3);
            if (worlds.Any(x => x == world) && id.All(x => char.IsNumber(x)))
                return str.Replace(sub2, "{model}");

            return str;
        }

        public static string ReplaceWithAnim(string str)
        {
            var indexOf = str.IndexOf(".anb");
            if (indexOf > 6)
            {
                var sub = str.Substring(indexOf - 6, 6);
                if (sub.All(x => char.IsNumber(x) || (char.IsLetter(x) && char.IsUpper(x))))
                    return str.Replace(sub, "{animId}");
            }

            return str;
        }
    }
}

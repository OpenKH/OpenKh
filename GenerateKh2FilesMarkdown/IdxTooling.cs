using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace GenerateKh2FilesMarkdown
{
    public static class IdxTooling
    {
        public class IdxResearch
        {
            [Data] public string Name { get; set; }
            [Data] public int Length { get => Items.TryGetCount(); set => Items = Items.CreateOrResize(value); }
            [Data] public List<IdxEntryResearch> Items { get; set; }
        }

        public class IdxEntryResearch
        {
            [Data] public uint Hash32 { get; set; }
            [Data] public ushort Hash16 { get; set; }
            [Data(Count=46)] public string FileName { get; set; }
            [Data(Count=32)] public string Games { get; set; }
        }

        private static Dictionary<string, Idx> idxDictionary;
        private static Dictionary<uint, (ushort, string)> names;

        static IdxTooling()
        {
            BinaryMapping.SetMemberLengthMapping<IdxResearch>(nameof(IdxResearch.Items), (o, m) => o.Length);

            Console.WriteLine("Generate IDX dictionary...");
            idxDictionary = GenerateIdxDictionary();

            Console.WriteLine("Load file list...");
            names = LoadNames("resources/kh2idx.txt");
        }

        public static IEnumerable<IdxEntryResearch> GetEntries()
        {
            return idxDictionary
                .SelectMany(x => x.Value.Items, (x, item) => new
                {
                    Game = x.Key,
                    Name = Lookup(names, item.Hash32, item.Hash16),
                    item.Hash32,
                    item.Hash16
                })
                .GroupBy(x => $"{x.Hash32}{x.Hash16}")
                .Select(x => new IdxEntryResearch
                {
                    Hash32 = x.First().Hash32,
                    Hash16 = x.First().Hash16,
                    FileName = x.First().Name,
                    Games = string.Join(",", x.Select(y => y.Game))
                })
                .OrderBy(x => x.FileName);
        }

        private static Dictionary<string, Idx> GenerateIdxDictionary()
        {
            return new Dictionary<string, Idx>
            {
                ["jp"] = GetIdx(@"../../../../[2005-12-06][SLPM66233] Kingdom Hearts II (JP).IDX"),
                ["us"] = GetIdx(@"../../../../[2006-02-02][SLUS21005] Kingdom Hearts II (US).IDX"),
                ["fr"] = GetIdx(@"../../../../[2006-07-21][SLES54232] Kingdom Hearts II (EU-FR).IDX"),
                ["it"] = GetIdx(@"../../../../[2006-07-21][SLES54234] Kingdom Hearts II (EU-IT).IDX"),
                ["au"] = GetIdx(@"../../../../[2006-07-24][SLES54114] Kingdom Hearts II (EU-AU).IDX"),
                ["de"] = GetIdx(@"../../../../[2006-07-24][SLES54233] Kingdom Hearts II (EU-DE).IDX"),
                ["es"] = GetIdx(@"../../../../[2006-07-24][SLES54235] Kingdom Hearts II (EU-ES).IDX"),
                ["fm"] = GetIdx(@"../../../../[2007-02-16][SLPM66675] Kingdom Hearts II Final Mix (JP).IDX"),
            };
        }


        private static Idx GetIdx(string fileName)
        {
            Idx idx;
            using (var stream = File.OpenRead(fileName))
            {
                idx = Idx.Read(stream);
            }

            string imgFilePath = fileName.Replace(".idx", ".img", StringComparison.InvariantCultureIgnoreCase);
            using (var stream = File.OpenRead(imgFilePath))
            {
                idx = new Img(stream, idx, true).Idx;
            }

            return idx;
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

        private static Dictionary<uint, (ushort, string)> LoadNames(string fileName)
        {
            var dic = new Dictionary<uint, (ushort, string)>();
            foreach (var name in ReadLines(fileName))
            {
                AddName(dic, name);
            }

            return dic;
        }

        private static string Lookup(Dictionary<uint, (ushort, string)> names, uint hash32, ushort hash16)
        {
            if (names.TryGetValue(hash32, out var pair) && pair.Item1 == hash16)
                return pair.Item2;
            return null;
        }

        private static void AddName(Dictionary<uint, (ushort, string)> names, string name) => names[Idx.GetHash32(name)] = (Idx.GetHash16(name), name);
    }
}

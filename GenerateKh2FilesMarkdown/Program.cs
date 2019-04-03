using kh.kh2;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace GenerateKh2FilesMarkdown
{
    class Program
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
            [Data] public string FileName { get; set; }
        }

        static void Main(string[] args)
        {
            BinaryMapping.SetMemberLengthMapping<IdxResearch>(nameof(IdxResearch.Items), (o, m) => o.Length);
            var idxDictionary = new Dictionary<string, Idx>
            {
                ["jp"] = GetIdx(@"H:\Kingdom Hearts\Kingdom Hearts II\[2005-12-06][SLPM66233] Kingdom Hearts II (JP).IDX"),
                ["us"] = GetIdx(@"H:\Kingdom Hearts\Kingdom Hearts II\[2006-02-02][SLUS21005] Kingdom Hearts II (US).IDX"),
                ["fr"] = GetIdx(@"H:\Kingdom Hearts\Kingdom Hearts II\[2006-07-21][SLES54232] Kingdom Hearts II (EU-FR).IDX"),
                ["it"] = GetIdx(@"H:\Kingdom Hearts\Kingdom Hearts II\[2006-07-21][SLES54234] Kingdom Hearts II (EU-IT).IDX"),
                ["au"] = GetIdx(@"H:\Kingdom Hearts\Kingdom Hearts II\[2006-07-24][SLES54114] Kingdom Hearts II (EU-AU).IDX"),
                ["de"] = GetIdx(@"H:\Kingdom Hearts\Kingdom Hearts II\[2006-07-24][SLES54233] Kingdom Hearts II (EU-DE).IDX"),
                ["es"] = GetIdx(@"H:\Kingdom Hearts\Kingdom Hearts II\[2006-07-24][SLES54235] Kingdom Hearts II (EU-ES).IDX"),
                ["fm"] = GetIdx(@"H:\Kingdom Hearts\Kingdom Hearts II\[2007-02-16][SLPM66675] Kingdom Hearts II Final Mix (JP).IDX"),
            };

            var names = LoadNames("resources/files.txt");

            var normalizedData = idxDictionary
                .SelectMany(x => x.Value.Items, (x, item) => new
                {
                    Game = x.Key,
                    Name = Lookup(names, item.Hash32, item.Hash16),
                    item.Hash32,
                    item.Hash16
                })
                .GroupBy(x => $"{x.Hash32}{x.Hash16}")
                .Select(x => new
                {
                    x.First().Name,
                    x.First().Hash32,
                    x.First().Hash16,
                    Games = string.Join(",", x.Select(y => y.Game))
                })
                .OrderBy(x => x.Name)
                .ToList();

            using (var stream = File.CreateText("files.md"))
            {
                stream.WriteLine("| Name | Hash32 | Hash16 | Games |");
                stream.WriteLine("|------|--------|--------|-------|");
                foreach (var item in normalizedData)
                {
                    stream.WriteLine($"|{item.Name}|{item.Hash32:X08}|{item.Hash16:X04}|{item.Games}");
                }
            }

            System.Console.WriteLine($"Discovered {normalizedData.Count(x => x.Name != null)} of {normalizedData.Count}");
        }

        static Dictionary<uint, (ushort, string)> LoadNames(string fileName)
        {
            var dic = new Dictionary<uint, (ushort, string)>();

            using (var stream = File.OpenRead(fileName))
            {
                var reader = new StreamReader(stream);
                string name;
                while ((name = reader.ReadLine()) != null)
                {
                    AddName(dic, name);
                }
            }

            return dic;
        }

        static void AddName(Dictionary<uint, (ushort, string)> names, string name) => names[Idx.GetHash32(name)] = (Idx.GetHash16(name), name);

        static string Lookup(Dictionary<uint, (ushort, string)> names, uint hash32, ushort hash16)
        {
            if (names.TryGetValue(hash32, out var pair) && pair.Item1 == hash16)
                return pair.Item2;
            return null;
        }

        static Idx GetIdx(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
                return Idx.Read(stream);
        }
    }
}

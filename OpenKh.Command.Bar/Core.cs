using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Command.Bar
{
    public static class Core
    {
        internal class BarRoot
        {
            [JsonProperty] public string OriginalFileName { get; set; }
            [JsonProperty] public  Kh2.Bar.MotionsetType Motionset { get; set; }
            [JsonProperty] public List<BarDesc> Entries { get; set; }
        }

        internal class BarDesc
        {
            [JsonProperty] public string FileName { get; set; }
            [JsonProperty] public string InternalName { get; set; }
            [JsonProperty] public int TypeId { get; set; }
            [JsonProperty] public int LinkIndex { get; set; }
            [JsonIgnore] public Stream Stream { get; set; }

            public override string ToString() => FileName;
        }

        private const string InvalidBarText = "The specified file is not a BAR file.";
        internal static readonly Exception InvalidBarFileException = new InvalidDataException(InvalidBarText);

        public static Kh2.Bar ReadEntries(string fileName)
        {
            using var stream = File.OpenRead(fileName);
            if (!Kh2.Bar.IsValid(stream))
                throw InvalidBarFileException;

            stream.Position = 0;
            return Kh2.Bar.Read(stream);
        }

        public static void ExportProject(string inputFileName, string outputFolder, bool suppress = false)
        {
            var binarc = ReadEntries(inputFileName);
            var project = new BarRoot
            {
                OriginalFileName = Path.GetFileName(inputFileName),
                Motionset = binarc.Motionset,
                Entries = binarc
                    .Select(x => new BarDesc
                    {
                        FileName = $"{x.Name}.{Helpers.GetSuggestedExtension(x.Type)}",
                        InternalName = x.Name,
                        TypeId = (int)x.Type,
                        LinkIndex = x.Index,
                        Stream = x.Stream,
                    })
                    .ToList()
            };

            foreach (var entryGroup in project.Entries
                .Where(x => x.LinkIndex == 0)
                .GroupBy(x => $"{x.InternalName}_{x.TypeId}"))
            {
                var items = entryGroup.ToArray();
                if (items.Length > 1)
                {
                    for (var i = 0; i < items.Length; i++)
                    {
                        var ext = Helpers.GetSuggestedExtension((Kh2.Bar.EntryType)items[0].TypeId);
                        items[i].FileName = $"{items[0].InternalName}_{i}.{ext}";
                    }
                }
            }

            if (!suppress)
            {
                var fileNameWithExt = Path.GetFileName(inputFileName);
                var projectFileName = Path.Combine(outputFolder, $"{fileNameWithExt}.json");
                if (!Directory.Exists(outputFolder))
                    Directory.CreateDirectory(outputFolder);

                File.WriteAllText(projectFileName, JsonConvert.SerializeObject(project, Formatting.Indented));
            }

            foreach (var entry in project.Entries.Where(x => x.LinkIndex == 0))
            {
                var outputFileName = Path.Combine(outputFolder, entry.FileName);

                using var stream = File.Create(outputFileName);
                entry.Stream.Position = 0;
                entry.Stream.CopyTo(stream);
            }
        }

        public static Kh2.Bar ImportProject(string inputProjectName, out string originalFileName)
        {
            var baseDirectory = Path.GetDirectoryName(inputProjectName);
            var project = JsonConvert.DeserializeObject<BarRoot>(File.ReadAllText(inputProjectName));
            originalFileName = project.OriginalFileName;

            var streams = project.Entries
                .Where(x => x.LinkIndex == 0)
                .ToDictionary(x => x.FileName,
                    x => File.OpenRead(Path.Combine(baseDirectory, x.FileName)));

            var binarc = new Kh2.Bar
            {
                Motionset = project.Motionset
            };
            binarc.AddRange(project.Entries
                .Select(x => new Kh2.Bar.Entry
                {
                    Name = x.InternalName,
                    Type = (Kh2.Bar.EntryType)x.TypeId,
                    Index = x.LinkIndex,
                    Stream = x.LinkIndex == 0 ? streams[x.FileName] : null
                }));

            return binarc;
        }
    }
}

using McMaster.Extensions.CommandLineUtils;
using OpenKh.Command.TexFooter.Models;
using OpenKh.Command.TexFooter.Utils;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.TextureFooter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace OpenKh.Command.TexFooter.Subcommands
{
    [HelpOption]
    [Command(Description = "map file: import map or mdlx texture footer. yml -> map")]
    public class ImportCommand
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "Map file (in and out)")]
        public string MapFile { get; set; }

        [Argument(1, Description = "YML file (`P_EX100.footer.yml`)")]
        public string YmlFile { get; set; }

        protected int OnExecute(CommandLineApplication app) => Execute();

        public int Execute()
        {
            var outDir = Path.GetDirectoryName(MapFile);
            var baseName = Path.GetFileNameWithoutExtension(MapFile);

            var perTexture = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithAttributeOverride<TextureFrame>(it => it.Data, new YamlIgnoreAttribute())
                .Build()
                .Deserialize<PerTexture>(File.ReadAllText(Path.Combine(outDir, YmlFile ?? $"{baseName}.footer.yml")));

            var anyChanges = false;

            var barEntries = File.OpenRead(MapFile).Using(Bar.Read);
            foreach (var entry in barEntries
                .Where(entry => entry.Type == Bar.EntryType.ModelTexture
                    && ModelTexture.IsValid(entry.Stream)
                    && perTexture.Textures.ContainsKey(entry.Name)
                )
            )
            {
                var model = perTexture.Textures[entry.Name];

                entry.Stream.SetPosition(0);

                var modelTexture = ModelTexture.Read(entry.Stream);

                model.ConvertBackTo(
                    pngFile =>
                    {
                        return new SpriteBitmap(Path.Combine(outDir, pngFile));
                    },
                    modelTexture.TextureFooterData
                );

                {
                    var buffer = new MemoryStream();
                    modelTexture.Write(buffer);
                    buffer.Position = 0;

                    entry.Stream = buffer;
                }

                anyChanges |= true;
            }

            if (anyChanges)
            {
                var buffer = new MemoryStream();
                Bar.Write(buffer, barEntries);

                File.WriteAllBytes(MapFile, buffer.ToArray());
            }

            return 0;
        }
    }
}

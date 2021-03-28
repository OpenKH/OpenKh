using McMaster.Extensions.CommandLineUtils;
using OpenKh.Command.TexFooter.Models;
using OpenKh.Command.TexFooter.TypeConverters;
using OpenKh.Command.TexFooter.Utils;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.TextureFooter;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using YamlDotNet.Serialization;

namespace OpenKh.Command.TexFooter.Subcommands
{
    [HelpOption]
    [Command(Description = "map file: export map or mdlx texture footer. map -> yml")]
    public class ExportCommand
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "Map file")]
        public string MapFile { get; set; }

        [Argument(1, Description = "Output dir")]
        public string OutputDir { get; set; }

        protected int OnExecute(CommandLineApplication app) => Execute();

        public int Execute()
        {
            var perTexture = new PerTexture();

            var barEntries = File.OpenRead(MapFile).Using(Bar.Read);
            foreach (var entry in barEntries
                .Where(entry => entry.Type == Bar.EntryType.ModelTexture && ModelTexture.IsValid(entry.Stream))
            )
            {
                entry.Stream.SetPosition(0);

                var modelTexture = ModelTexture.Read(entry.Stream);

                if (modelTexture.Images == null || !modelTexture.Images.Any())
                {
                    return 1;
                }

                var footerData = modelTexture.TextureFooterData;

                perTexture.Textures[entry.Name] = new TextureFooterDataIMEx(footerData);
            }

            var outDir = Path.Combine(
                Path.GetDirectoryName(MapFile),
                OutputDir ?? "."
            );
            var baseName = Path.GetFileNameWithoutExtension(MapFile);

            Directory.CreateDirectory(outDir);

            File.WriteAllText(
                Path.Combine(outDir, $"{baseName}.footer.yml"),
                new SerializerBuilder()
                    .WithTypeConverter(new UseJsonStyleArray<short>())
                    .WithTypeConverter(new UseJsonStyleArray<byte>())
                    .WithAttributeOverride<TextureFrame>(it => it.Data, new YamlIgnoreAttribute())
                    .Build()
                    .Serialize(ExportHelper.AlsoExportImages(outDir, baseName, perTexture))
            );

            return 0;
        }
    }
}

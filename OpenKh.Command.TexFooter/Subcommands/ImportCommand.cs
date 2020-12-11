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

        protected int OnExecute(CommandLineApplication app) => OnExecute();

        public int OnExecute()
        {
            var list = new List<string>();

            var barEntries = File.OpenRead(MapFile).Using(Bar.Read);
            var entry = barEntries.FirstOrDefault(entry => entry.Type == Bar.EntryType.ModelTexture);
            if (entry == null)
            {
                return 1;
            }
            if (entry.Stream.Length < 0x60)
            {
                return 1;
            }

            var modelTexture = ModelTexture.Read(entry.Stream);

            var footerData = new TextureFooterData();

            var outDir = Path.GetDirectoryName(MapFile);
            var baseName = Path.GetFileNameWithoutExtension(MapFile);

            var model = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithAttributeOverride<TextureFrame>(it => it.Data, new YamlIgnoreAttribute())
                .Build()
                .Deserialize<TextureFooterDataIMEx>(File.ReadAllText(Path.Combine(outDir, YmlFile ?? $"{baseName}.footer.yml")));

            var back = model.ConvertBack(
                pngFile =>
                {
                    return new SpriteBitmap(Path.Combine(outDir, pngFile));
                }
            );

            {
                var buffer = new MemoryStream();
                back.Write(buffer);

                modelTexture.SetFooterData(buffer.ToArray());
            }

            {
                var buffer = new MemoryStream();
                modelTexture.Write(buffer);
                buffer.Position = 0;

                entry.Stream = buffer;
            }

            {
                var buffer = new MemoryStream();
                Bar.Write(buffer, barEntries);

                File.WriteAllBytes(MapFile, buffer.ToArray());
            }

            return 0;
        }
    }
}

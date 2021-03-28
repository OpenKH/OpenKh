using McMaster.Extensions.CommandLineUtils;
using OpenKh.Command.TexFooter.Models;
using OpenKh.Command.TexFooter.TypeConverters;
using OpenKh.Command.TexFooter.Utils;
using OpenKh.Common;
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
    [Command(Description = "texture footer bin -> yml")]
    class BinToYmlCommand
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "Bin file")]
        public string BinFile { get; set; }

        [Argument(1, Description = "Output dir")]
        public string OutputDir { get; set; }

        protected int OnExecute(CommandLineApplication app) => Execute();

        public int Execute()
        {
            var footer = File.OpenRead(BinFile).Using(TextureFooterData.Read);

            var outDir = Path.Combine(
                Path.GetDirectoryName(BinFile),
                OutputDir ?? "."
            );
            var baseName = Path.GetFileNameWithoutExtension(BinFile);

            Directory.CreateDirectory(outDir);

            File.WriteAllText(
                Path.Combine(outDir, $"{baseName}.footer.yml"),
                new SerializerBuilder()
                    .WithTypeConverter(new UseJsonStyleArray<short>())
                    .WithTypeConverter(new UseJsonStyleArray<byte>())
                    .WithAttributeOverride<TextureFrame>(it => it.Data, new YamlIgnoreAttribute())
                    .Build()
                    .Serialize(
                        ExportHelper.AlsoExportImages(
                            outDir,
                            baseName,
                            new PerTexture
                            {
                                Textures = {
                                    ["MAP"] = new TextureFooterDataIMEx(footer)
                                },
                            }
                        )
                    )
            );

            return 0;
        }
    }
}

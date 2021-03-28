using McMaster.Extensions.CommandLineUtils;
using OpenKh.Command.TexFooter.Models;
using OpenKh.Command.TexFooter.Utils;
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
    [Command(Description = "yml -> texture footer bin")]
    class YmlToBinCommand
    {
        [Required]
        [FileExists]
        [Argument(0, Description = "Yml file")]
        public string YmlFile { get; set; }

        [Argument(1, Description = "Bin file")]
        public string BinFile { get; set; }

        protected int OnExecute(CommandLineApplication app) => Execute();

        public int Execute()
        {
            var perTexture = new DeserializerBuilder()
                .IgnoreUnmatchedProperties()
                .WithAttributeOverride<TextureFrame>(it => it.Data, new YamlIgnoreAttribute())
                .Build()
                .Deserialize<PerTexture>(File.ReadAllText(Path.Combine(YmlFile)));

            var model = perTexture.Textures.Single();

            var baseDir = Path.GetDirectoryName(YmlFile);

            var footer = new TextureFooterData();
            model.Value.ConvertBackTo(
                pngFile =>
                {
                    return new SpriteBitmap(Path.Combine(baseDir, pngFile));
                },
                footer
            );

            var toFile = new MemoryStream();
            footer.Write(toFile);

            File.WriteAllBytes(BinFile ?? Path.ChangeExtension(YmlFile, ".bin"), toFile.ToArray());
            return 0;
        }
    }
}

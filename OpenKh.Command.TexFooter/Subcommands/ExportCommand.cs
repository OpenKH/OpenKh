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

            if (modelTexture.Images == null || !modelTexture.Images.Any())
            {
                return 1;
            }

            var footerData = new TextureFooterData(
                new MemoryStream(modelTexture.GetFooterData())
            );

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
                    .Serialize(AlsoExportImages(outDir, baseName, new TextureFooterDataIMEx(footerData)))
            );

            return 0;
        }

        private TextureFooterDataIMEx AlsoExportImages(string outDir, string baseName, TextureFooterDataIMEx textureFooterData)
        {
            textureFooterData.TextureAnimationList?
                .Select((it, index) => (it, index))
                .ToList()
                .ForEach(
                    pair =>
                    {
                        var src = pair.it._source;
                        var bitmap = SpriteImageUtil.ToBitmap(
                            src.BitsPerPixel,
                            src.SpriteWidth,
                            src.SpriteHeight,
                            src.NumSpritesInImageData,
                            src.SpriteStride,
                            src.SpriteImage
                        );
                        var pngFile = Path.Combine(outDir, $"{baseName}.footer{pair.index}.png");
                        bitmap.Save(pngFile, ImageFormat.Png);

                        pair.it.SpriteImageFile = "./" + Path.GetFileName(pngFile);
                    }
                );

            return textureFooterData;
        }
    }
}

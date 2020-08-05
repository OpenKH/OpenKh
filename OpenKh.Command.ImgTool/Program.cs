using McMaster.Extensions.CommandLineUtils;
using OpenKh.Command.ImgTool.Interfaces;
using OpenKh.Command.ImgTool.Utils;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Utils;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks.Dataflow;

namespace OpenKh.Command.ImgTool
{
    [Command("OpenKh.Command.ImgTool")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(UnimdCommand), typeof(UnimzCommand), typeof(ImdCommand), typeof(ImzCommand)
        , typeof(ScanImdCommand), typeof(ScanImzCommand)
        , typeof(FixImzCommand), typeof(SwapImzPixelCommand))]
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
                return 1;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
                return 1;
            }
        }

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }


        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [HelpOption]
        [Command(Description = "imd file -> png file")]
        private class UnimdCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Input imd")]
            public string ImdFile { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Output png. Default is current dir, and file extension to png.", ShortName = "o", LongName = "output")]
            public string OutputPng { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var inputFile = ImdFile;
                var outputFile = OutputPng ?? Path.GetFullPath(Path.GetFileName(Path.ChangeExtension(inputFile, ".png")));

                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

                using (var stream = File.OpenRead(inputFile))
                {
                    var imgd = Imgd.Read(stream);
                    var bitmap = ImgdBitmapUtil.ToBitmap(imgd);
                    bitmap.Save(outputFile);
                }
                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "imz file -> png or imd files")]
        private class UnimzCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Input imz")]
            public string ImzFile { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Output dir. Default is current dir.", ShortName = "o", LongName = "output")]
            public string OutputDir { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Export as imd instead of png", ShortName = "m", LongName = "imd")]
            public bool ExportImd { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var inputFile = ImzFile;
                var outputDir = OutputDir ?? Environment.CurrentDirectory;

                using (var stream = File.OpenRead(inputFile))
                {
                    var pairs = Imgz.Read(stream).Select((imgd, index) => (imgd, index));

                    Directory.CreateDirectory(outputDir);

                    foreach (var (imgd, index) in pairs)
                    {
                        if (ExportImd)
                        {
                            var outputFile = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(inputFile)}-{1 + index}.imd");

                            var buffer = new MemoryStream();
                            imgd.Write(buffer);
                            File.WriteAllBytes(outputFile, buffer.ToArray());
                        }
                        else
                        {
                            var outputFile = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(inputFile)}-{1 + index}.png");

                            var bitmap = ImgdBitmapUtil.ToBitmap(imgd);
                            bitmap.Save(outputFile);
                        }
                    }
                }
                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "png file -> imd file")]
        private class ImdCommand : ICommonQuantizerParam
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Input png")]
            public string PngFile { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Output imd. Default is current dir, and file extension to imd.", ShortName = "o", LongName = "output")]
            public string OutputImd { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Set bits per pixel: 4, 8, or 32", ShortName = "b", LongName = "bpp")]
            public int BitsPerPixel { get; set; } = 8;

            [Option(CommandOptionType.NoValue, Description = "Use `pngquant.exe` -- lossy PNG compressor. Download `pngquant.exe` and place it beside this program, current directory, or PATH.", ShortName = "p", LongName = "pngquant")]
            public bool PngQuant { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Use embedded nQuant.Core color quantizer. This is default selection.", ShortName = "n", LongName = "nquant")]
            public bool nQuant { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Enable swizzle.", ShortName = "s", LongName = "swizzle")]
            public bool Swizzle { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var inputFile = PngFile;
                var outputFile = OutputImd ?? Path.GetFullPath(Path.GetFileName(Path.ChangeExtension(inputFile, ".imd")));

                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

                // Alpha enabled png → always 32 bpp
                using (var bitmap = new Bitmap(inputFile))
                {
                    var imgd = ImgdBitmapUtil.ToImgd(bitmap, BitsPerPixel, QuantizerFactory.MakeFrom(this), Swizzle);

                    var buffer = new MemoryStream();
                    imgd.Write(buffer);
                    File.WriteAllBytes(outputFile, buffer.ToArray());
                }
                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "png, imd or imz files -> imz file")]
        private class ImzCommand : ICommonQuantizerParam
        {
            [Required]
            [Option(CommandOptionType.MultipleValue, Description = "Input png/imd/imz file", ShortName = "i", LongName = "input")]
            public string[] InputFile { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Output imz file. Default is current dir, and first file extension to imz.", ShortName = "o", LongName = "output")]
            public string OutputImz { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Set bits per pixel for every png: 4, 8, or 32", ShortName = "b", LongName = "bpp")]
            public int BitsPerPixel { get; set; } = 8;

            [Option(CommandOptionType.NoValue, Description = "Use `pngquant.exe` -- lossy PNG compressor. Download `pngquant.exe` and deploy it beside this program, current directory, or PATH.", ShortName = "p", LongName = "pngquant")]
            public bool PngQuant { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Use embedded nQuant.Core color quantizer. This is default selection.", ShortName = "n", LongName = "nquant")]
            public bool nQuant { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Try to append to imz", ShortName = "a", LongName = "append")]
            public bool Append { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Enable swizzle for png input.", ShortName = "s", LongName = "swizzle")]
            public bool Swizzle { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                OutputImz = OutputImz ?? Path.GetFullPath(Path.GetFileName(Path.ChangeExtension(InputFile.First(), ".imz")));

                Directory.CreateDirectory(Path.GetDirectoryName(OutputImz));

                var prependImgdList = (Append && File.Exists(OutputImz))
                    ? File.OpenRead(OutputImz).Using(stream => Imgz.Read(stream).ToArray())
                    : new Imgd[0];

                var buffer = new MemoryStream();
                Imgz.Write(
                    buffer,
                    prependImgdList
                        .Concat(
                            InputFile
                                .SelectMany(imdFile => ImgdBitmapUtil.FromFileToImgdList(imdFile, BitsPerPixel, QuantizerFactory.MakeFrom(this), Swizzle))
                        )
                        .ToArray()
                );
                File.WriteAllBytes(OutputImz, buffer.ToArray());

                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "imd file -> display summary")]
        private class ScanImdCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Input imd file")]
            public string InputFile { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var target = File.OpenRead(InputFile).Using(stream => Imgd.Read(stream));

                Console.WriteLine(
                    FormatterFactory.GetFormatterPairs()
                        .First()
                        .FormatToString(ImgdSummary.From(target))
                );

                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "imz file -> display summary")]
        private class ScanImzCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Input imz file")]
            public string InputFile { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var targets = File.OpenRead(InputFile).Using(stream => Imgz.Read(stream).ToArray());

                Console.WriteLine(
                    FormatterFactory.GetFormatterPairs()
                        .First()
                        .FormatToString(
                            targets
                                .Select(one => ImgdSummary.From(one))
                                .Select((one, index) => (one, index))
                                .ToDictionary(
                                    pair => pair.index.ToString(),
                                    pair => pair.one
                                )
                        )
                );

                return 0;
            }
        }

        class FormatterPair
        {
            internal string Type;
            internal Func<object, string> FormatToString;
        }

        class FormatterFactory
        {
            internal static IEnumerable<FormatterPair> GetFormatterPairs() => new FormatterPair[]
            {
                new FormatterPair
                {
                    Type = "yaml",
                    FormatToString =
                        (obj) => new YamlDotNet.Serialization.SerializerBuilder()
                            .Build()
                            .Serialize(obj),
                },
                new FormatterPair
                {
                    Type = "json",
                    FormatToString =
                        (obj) => JsonSerializer.Serialize(
                            obj,
                            new JsonSerializerOptions
                            {
                                WriteIndented = true
                            }
                        ),
                },
            };
        }

        class ImgdSanityChecking
        {
            internal static int? GetExpectedDataLen(Imgd it)
            {
                switch (it.PixelFormat)
                {
                    case Imaging.PixelFormat.Indexed4:
                        return (it.Size.Width * it.Size.Height + 1) / 2;
                    case Imaging.PixelFormat.Indexed8:
                        return (it.Size.Width * it.Size.Height);
                    case Imaging.PixelFormat.Rgb888:
                        return (3 * it.Size.Width * it.Size.Height);
                    case Imaging.PixelFormat.Rgba8888:
                    case Imaging.PixelFormat.Rgbx8888:
                        return (4 * it.Size.Width * it.Size.Height);
                }
                return null;
            }
        }

        class ImgdSummary
        {
            public string PixelFormat { get; private set; }
            public int Width { get; private set; }
            public int Height { get; private set; }
            public bool IsSwizzled { get; private set; }
            public int? ExpectedDataLen { get; private set; }
            public int ActualDataLen { get; private set; }
            public int? ActualClutLen { get; private set; }

            internal static ImgdSummary From(Imgd it)
            {
                return new ImgdSummary
                {
                    PixelFormat = it.PixelFormat.ToString(),
                    Width = it.Size.Width,
                    Height = it.Size.Height,
                    IsSwizzled = it.IsSwizzled,
                    ExpectedDataLen = ImgdSanityChecking.GetExpectedDataLen(it),
                    ActualDataLen = it.Data.Length,
                    ActualClutLen = it.Clut?.Length,
                };
            }
        }

        [HelpOption]
        [Command(Description = "imz file -> imz file (fix 4-bpp doubled bitmap size)")]
        private class FixImzCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Input (and output) imz file")]
            public string InputFile { get; set; }

            [Option(CommandOptionType.SingleOrNoValue, Description = "Output imz file. Default is input imz.", ShortName = "o", LongName = "output")]
            public string OutputImz { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                OutputImz = OutputImz ?? InputFile;

                Directory.CreateDirectory(Path.GetDirectoryName(OutputImz));

                var images = File.OpenRead(OutputImz).Using(stream => Imgz.Read(stream).ToArray());

                var fixedCount = 0;

                var fixedImages = images
                    .Select(
                        image =>
                        {
                            var expetectedLen = ImgdSanityChecking.GetExpectedDataLen(image);
                            if (true
                                && expetectedLen != null
                                && expetectedLen < image.Data.Length
                                && Imaging.PixelFormat.Indexed4 == image.PixelFormat
                            )
                            {
                                var fixedData = new byte[expetectedLen.Value];

                                Buffer.BlockCopy(image.Data, 0, fixedData, 0, expetectedLen.Value);

                                fixedCount++;

                                return new Imgd(
                                    image.Size,
                                    image.PixelFormat,
                                    fixedData,
                                    image.Clut,
                                    isSwizzled: false
                                );
                            }
                            else
                            {
                                return image;
                            }
                        }
                    )
                    .ToArray();

                var buffer = new MemoryStream();

                Imgz.Write(buffer, fixedImages);

                File.WriteAllBytes(OutputImz, buffer.ToArray());

                Console.WriteLine($"Fixed {fixedCount} images.");

                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "imz file -> imz file (swap 4-bpp hi/lo pixel)")]
        private class SwapImzPixelCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Input (and output) imz file")]
            public string InputFile { get; set; }

            [Option(CommandOptionType.SingleOrNoValue, Description = "Output imz file. Default is input imz.", ShortName = "o", LongName = "output")]
            public string OutputImz { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                OutputImz = OutputImz ?? InputFile;

                Directory.CreateDirectory(Path.GetDirectoryName(OutputImz));

                var images = File.OpenRead(OutputImz).Using(stream => Imgz.Read(stream).ToArray());

                var fixedCount = 0;

                var fixedImages = images
                    .Select(
                        image =>
                        {
                            if (Imaging.PixelFormat.Indexed4 == image.PixelFormat)
                            {
                                var data = image.Data;

                                for (var x = 0; x < data.Length; x++)
                                {
                                    var swap = data[x];
                                    data[x] = (byte)((swap << 4) | (swap >> 4));
                                }

                                fixedCount++;
                            }
                            return image;
                        }
                    )
                    .ToArray();

                var buffer = new MemoryStream();

                Imgz.Write(buffer, fixedImages);

                File.WriteAllBytes(OutputImz, buffer.ToArray());

                Console.WriteLine($"Applied to {fixedCount} images.");

                return 0;
            }
        }

    }
}

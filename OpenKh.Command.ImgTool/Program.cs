using McMaster.Extensions.CommandLineUtils;
using OpenKh.Command.ImgTool.Utils;
using OpenKh.Common;
using OpenKh.Kh2;
using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Net.Http.Headers;
using System.Reflection;
using System.Text;

namespace OpenKh.Command.ImgTool
{
    [Command("OpenKh.Command.MsgTool")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(UnimdCommand), typeof(UnimzCommand), typeof(ImdCommand), typeof(ImzCommand))]
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

            [Option(CommandOptionType.SingleValue, Description = "Output png", ShortName = "o", LongName = "output")]
            public string OutputPng { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var inputFile = ImdFile;
                var outputFile = OutputPng ?? Path.Combine(
                    Path.GetDirectoryName(inputFile),
                    "extracted",
                    Path.GetFileName(
                        Path.ChangeExtension(inputFile, ".png")
                    )
                );

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

            [Option(CommandOptionType.SingleValue, Description = "Output dir", ShortName = "o", LongName = "output")]
            public string OutputDir { get; set; }

            [Option(CommandOptionType.NoValue, Description = "Export as imd instead of png", ShortName = "m", LongName = "imd")]
            public bool ExportImd { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var inputFile = ImzFile;
                var outputDir = OutputDir ?? Path.Combine(Path.GetDirectoryName(inputFile), "extracted");

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
        private class ImdCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Input png")]
            public string PngFile { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Output imd", ShortName = "o", LongName = "output")]
            public string OutputImd { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Set bits per pixel: 4, 8, or 32", ShortName = "b", LongName = "bpp")]
            public int BitsPerPixel { get; set; } = 8;

            [Option(CommandOptionType.NoValue, Description = "Try to append to imd", ShortName = "a", LongName = "append")]
            public bool Append { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var inputFile = PngFile;
                var outputFile = OutputImd ?? Path.Combine(
                    Path.GetDirectoryName(inputFile),
                    "converted",
                    Path.GetFileName(
                        Path.ChangeExtension(inputFile, ".imd")
                    )
                );

                Directory.CreateDirectory(Path.GetDirectoryName(outputFile));

                // Alpha enabled png → always 32 bpp
                using (var bitmap = new Bitmap(inputFile))
                {
                    var imgd = ImgdBitmapUtil.ToImgd(bitmap, BitsPerPixel);

                    var buffer = new MemoryStream();
                    imgd.Write(buffer);
                    File.WriteAllBytes(outputFile, buffer.ToArray());
                }
                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "png, imd or imz files -> imz file")]
        private class ImzCommand
        {
            [Required]
            [Option(CommandOptionType.MultipleValue, Description = "Input png/imd/imz file", ShortName = "i", LongName = "input")]
            public string[] InputFile { get; set; }

            [Required]
            [Option(CommandOptionType.SingleValue, Description = "Output imz file", ShortName = "o", LongName = "output")]
            public string OutputImz { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Set bits per pixel for every png: 4, 8, or 32", ShortName = "b", LongName = "bpp")]
            public int BitsPerPixel { get; set; } = 8;

            protected int OnExecute(CommandLineApplication app)
            {
                Directory.CreateDirectory(Path.GetDirectoryName(OutputImz));

                var buffer = new MemoryStream();
                Imgz.Write(
                    buffer,
                    InputFile
                        .SelectMany(imdFile => ImgdBitmapUtil.FromFileToImgdList(imdFile, BitsPerPixel))
                        .ToArray()
                );
                File.WriteAllBytes(OutputImz, buffer.ToArray());

                return 0;
            }
        }

    }
}

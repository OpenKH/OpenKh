using McMaster.Extensions.CommandLineUtils;
using OpenKh.Command.ImgTool.Utils;
using OpenKh.Common;
using OpenKh.Kh2;
using System;
using System.ComponentModel.DataAnnotations;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;

namespace OpenKh.Command.ImgTool
{
    [Command("OpenKh.Command.MsgTool")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(UnimdCommand), typeof(UnimzCommand), typeof(PackimdCommand))]
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
        [Command(Description = "Convert an imd file to a png file")]
        private class UnimdCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Input an imd file path")]
            public string ImdFile { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Save png to file path", ShortName = "o", LongName = "output")]
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
        [Command(Description = "Convert an imz file to png files")]
        private class UnimzCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Input an imz file path")]
            public string ImzFile { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Save png files to dir", ShortName = "o", LongName = "output")]
            public string OutputDir { get; set; }

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
                        var outputFile = Path.Combine(outputDir, $"{Path.GetFileNameWithoutExtension(inputFile)}-{1 + index}.png");

                        var bitmap = ImgdBitmapUtil.ToBitmap(imgd);
                        bitmap.Save(outputFile);
                    }
                }
                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "Convert a png file to an imd file")]
        private class PackimdCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "Input an png file path")]
            public string PngFile { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Save imd to file path", ShortName = "o", LongName = "output")]
            public string OutputImd { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "Force bits per pixel: 4, 8, 32", ShortName = "b", LongName = "bpp")]
            public int BitsPerPixel { get; set; }

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
                    File.Create(outputFile).Using(stream => imgd.Write(stream));
                }
                return 0;
            }
        }

    }
}

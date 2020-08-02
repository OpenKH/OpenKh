using OpenKh.Common;
using OpenKh.Kh2;
using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using OpenKh.Common.Exceptions;
using System.Linq;
using OpenKh.Engine.Parsers;
using System.Xml.Serialization;
using OpenKh.Command.MapGen.Models;
using Assimp;
using System.Numerics;
using Xe.Graphics;
using OpenKh.Command.MapGen.Utils;
using static OpenKh.Kh2.Mdlx;
using System.Collections.Generic;
using System.Diagnostics;
using NLog;
using System.ComponentModel;
using System.Drawing;

namespace OpenKh.Command.MapGen
{
    [Command("OpenKh.Command.MapGen")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    class Program
    {
        static int Main(string[] args)
        {
            try
            {
                return CommandLineApplication.Execute<Program>(args);
            }
            catch (InvalidFileException e)
            {
                Console.WriteLine(e.Message);
                return 3;
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
                return 2;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
                return -1;
            }
        }

        [Required]
        [FileExists]
        [Argument(0, Description = "Input file: mapdef.yml or model.{fbx,dae}")]
        public string InputFile { get; set; }

        [Argument(1, Description = "Output map file")]
        public string OutputMap { get; set; }

        private readonly Logger logger = LogManager.GetCurrentClassLogger();

        protected int OnExecute(CommandLineApplication app)
        {
#if false
            var colors = new System.Drawing.Color[]
            {
                System.Drawing.Color.FromArgb(  0,  0,255),
                System.Drawing.Color.FromArgb(  0,255,  0),
                System.Drawing.Color.FromArgb(  0,255,255),
                System.Drawing.Color.FromArgb(255,  0,  0),
                System.Drawing.Color.FromArgb(255,  0,255),
                System.Drawing.Color.FromArgb(255,255,  0),
                System.Drawing.Color.FromArgb(255,255,255),
                System.Drawing.Color.FromArgb(255,255,128),
                System.Drawing.Color.FromArgb(128,255,255),
                System.Drawing.Color.FromArgb(255,128,255),
                System.Drawing.Color.FromArgb(128,128,255),
                System.Drawing.Color.FromArgb(128,255,128),
                System.Drawing.Color.FromArgb(255,128,128),
            };
            for (int t = 0; t < colors.Length; t++)
            {
                var W = 128;
                var H = 128;
                var data = new byte[64 * 128];
                var clut = new byte[4 * 16];
                for (int y = 0; y < 4; y++)
                {
                    for (int x = 0; x < 4; x++)
                    {
                        var i = 4 * y + x;
                        var b = (byte)((i << 4) | i);
                        for (int by = 0; by < 24; by++)
                        {
                            for (int bx = 0; bx < 12; bx++)
                            {
                                data[12 * x + bx + 64 * (24 * y + by)] = b;
                            }
                        }
                    }
                }
                var color = colors[t];
                for (int i = 0; i < 16; i++)
                {
                    clut[4 * i + 0] = (byte)(color.R / 15.0f * i);
                    clut[4 * i + 1] = (byte)(color.G / 15.0f * i);
                    clut[4 * i + 2] = (byte)(color.B / 15.0f * i);
                    clut[4 * i + 3] = 255;
                }
                File.Create($@"H:\Proj\khkh_xldM\newMap\multi-4bpp\images2\{t}.imd").Using(
                    s => Imgd.Create(new System.Drawing.Size(W, H), Imaging.PixelFormat.Indexed4, data, clut, false).Write(s)
                );
            }
#endif

#if false
            var colors = new System.Drawing.Color[]
{
                System.Drawing.Color.FromArgb(  0,  0,255),
                System.Drawing.Color.FromArgb(  0,255,  0),
                System.Drawing.Color.FromArgb(  0,255,255),
                System.Drawing.Color.FromArgb(255,  0,  0),
                System.Drawing.Color.FromArgb(255,  0,255),
                System.Drawing.Color.FromArgb(255,255,  0),
                System.Drawing.Color.FromArgb(255,255,255),
                System.Drawing.Color.FromArgb(255,255,128),
                System.Drawing.Color.FromArgb(128,255,255),
                System.Drawing.Color.FromArgb(255,128,255),
                System.Drawing.Color.FromArgb(128,128,255),
                System.Drawing.Color.FromArgb(128,255,128),
                System.Drawing.Color.FromArgb(255,128,128),
};
            for (int t = 0; t < colors.Length; t++)
            {
                var W = 128;
                var H = 128;
                var data = new byte[128 * 128];
                var clut = new byte[4 * 256];
                for (int y = 0; y < 16; y++)
                {
                    for (int x = 0; x < 16; x++)
                    {
                        var i = 16 * y + x;
                        var b = (byte)i;
                        for (int by = 0; by < 6; by++)
                        {
                            for (int bx = 0; bx < 6; bx++)
                            {
                                data[6 * x + bx + 128 * (6 * y + by)] = b;
                            }
                        }
                    }
                }
                var color = colors[t];
                for (int i = 0; i < 256; i++)
                {
                    clut[4 * i + 0] = (byte)(color.R / 255.0f * i);
                    clut[4 * i + 1] = (byte)(color.G / 255.0f * i);
                    clut[4 * i + 2] = (byte)(color.B / 255.0f * i);
                    clut[4 * i + 3] = 255;
                }
                File.Create($@"H:\Proj\khkh_xldM\newMap\multi-8bpp\images\{t}.imd").Using(
                    s => Imgd.Create(new System.Drawing.Size(W, H), Imaging.PixelFormat.Indexed8, data, clut, false).Write(s)
                );
            }
            return 0;
#endif

            try
            {
                var yamlReader = new YamlDotNet.Serialization.DeserializerBuilder()
                    .IgnoreUnmatchedProperties()
                    .Build();

                MapBuilder builder;
                MapGenConfig config;
                string baseDir;
                string outMapFile;

                if (FileExtUtil.IsExtension(InputFile, ".yml"))
                {
                    var ymlFile = Path.GetFullPath(InputFile);
                    baseDir = Path.GetDirectoryName(ymlFile);

                    logger.Debug($"ymlFile is \"{ymlFile}\"");
                    logger.Debug($"baseDir is \"{baseDir}\"");

                    config = yamlReader.Deserialize<MapGenConfig>(File.ReadAllText(ymlFile));

                    var modelFile = Path.Combine(baseDir, config.inputFile);
                    outMapFile = Path.GetFullPath(OutputMap ?? Path.Combine(baseDir, config.outputFile));

                    builder = new MapBuilder(modelFile, config, CreateImageLoader(baseDir, config, FileLoader));
                }
                else
                {
                    var modelFile = Path.GetFullPath(InputFile);
                    baseDir = Path.GetDirectoryName(modelFile);
                    var ymlFile = Path.Combine(baseDir, "mapdef.yml");
                    outMapFile = Path.GetFullPath(OutputMap ?? Path.Combine(baseDir, Path.ChangeExtension(InputFile, ".map")));

                    logger.Debug($"ymlFile is \"{ymlFile}\"");
                    logger.Debug($"baseDir is \"{baseDir}\"");

                    config = File.Exists(ymlFile)
                        ? yamlReader.Deserialize<MapGenConfig>(File.ReadAllText(ymlFile))
                        : new MapGenConfig();

                    builder = new MapBuilder(modelFile, config, CreateImageLoader(baseDir, config, FileLoader));
                }

                logger.Debug("Building map file structure.");

                var buff = new MemoryStream();

                Bar.Write(
                    buff,
                    builder.GetBarEntries()
                        .Concat(LoadAdditionalBarEntries(config, CreateRawFileLoader(baseDir)))
                        .ToArray()
                );

                logger.Debug($"Writing to \"{outMapFile}\".");

                File.WriteAllBytes(outMapFile, buff.ToArray());

                logger.Debug("Done");

                return 0;
            }
            finally
            {
                LogManager.Shutdown();
            }
        }

        private Func<string, byte[]> CreateRawFileLoader(string baseDir)
        {
            return fileName =>
            {
                var filePath = Path.Combine(baseDir, fileName);

                logger.Debug($"Going to load file from \"{filePath}\"");

                return File.ReadAllBytes(filePath);
            };
        }

        private IEnumerable<Bar.Entry> LoadAdditionalBarEntries(MapGenConfig config, Func<string, byte[]> fileLoader)
        {
            foreach (var addFile in config.addFiles)
            {
                var data = fileLoader(addFile.fromFile);
                yield return new Bar.Entry
                {
                    Name = addFile.name,
                    Type = (Bar.EntryType)addFile.type,
                    Stream = new MemoryStream(data),
                    Index = addFile.index,
                };
            }
        }

        private Imgd FileLoader(string filePath, MaterialDef matDef, MapGenConfig config)
        {
            logger.Debug($"Load image from \"{filePath}\"");

            if (FileExtUtil.IsExtension(filePath, ".imd"))
            {
                return File.OpenRead(filePath).Using(s => Imgd.Read(s));
            }
            if (FileExtUtil.IsExtension(filePath, ".png"))
            {
                var imdFile = Path.ChangeExtension(filePath, ".imd");

                if (config.skipConversionIfExists && File.Exists(imdFile))
                {
                    logger.Debug($"Skipping png to imd conversion, due to imd file existence and skipConversionIfExists option.");
                }
                else
                {
                    try
                    {
                        logger.Debug($"Using ImgTool for png to imd conversion.");

                        var imgtoolOptions = matDef.imgtoolOptions ?? config.imgtoolOptions ?? "-b 8";

                        var result = new RunCmd(
                            "OpenKh.Command.ImgTool.exe",
                            $"imd \"{filePath}\" -o \"{imdFile}\" {imgtoolOptions}"
                        );

                        if (result.ExitCode != 0)
                        {
                            throw new Exception($"ImgTool failed ({result.ExitCode})");
                        }
                    }
                    catch (Win32Exception ex)
                    {
                        throw new Exception("ImgTool failed.", ex);
                    }
                }

                return FileLoader(imdFile, matDef, config);
            }

            throw new NotSupportedException(Path.GetExtension(filePath));
        }

        class RunCmd
        {
            private Process p;

            public string App => p.StartInfo.FileName;
            public int ExitCode => p.ExitCode;

            public RunCmd(string app, string arg)
            {
                var psi = new ProcessStartInfo(app, arg);
                psi.UseShellExecute = false;
                var p = Process.Start(psi);
                p.WaitForExit();
                this.p = p;
            }
        }

        private Func<MaterialDef, Imgd> CreateImageLoader(string baseDir, MapGenConfig config, Func<string, MaterialDef, MapGenConfig, Imgd> fileLoader)
        {
            return (matDef) =>
            {
                logger.Debug($"Going to load material \"{matDef.name}\".");

                var fileNames = new string[] {
                    matDef.fromFile,
                    matDef.fromFile2,
                    matDef.name + ".imd",
                    matDef.name + ".png",
                }
                    .Where(it => !string.IsNullOrWhiteSpace(it))
                    .ToList();

                var allImageDirs = new string[] { }
                    .Concat(
                        (config.imageDirs ?? new string[0])
                            .Select(imageDir => Path.Combine(baseDir, imageDir))
                    )
                    .Concat(
                        new string[] { baseDir }
                    )
                    .ToArray();

                var pathList =
                    allImageDirs
                        .SelectMany(
                            imageDir => fileNames.Select(fileName => Path.Combine(imageDir, fileName))
                        )
                        .ToArray();

                var found = pathList
                    .FirstOrDefault(File.Exists);

                Imgd loaded = null;

                if (found != null && File.Exists(found))
                {
                    try
                    {
                        loaded = fileLoader(found, matDef, config);
                    }
                    catch (Exception ex)
                    {
                        logger.Warn(ex, "Load image failed!");
                    }
                }

                if (loaded == null)
                {
                    logger.Warn($"File not found of material \"{matDef.name}\", or error thrown. Using fallback image instead.");

                    // fallback
                    var pixels = new byte[128 * 128];
                    for (int x = 0; x < pixels.Length; x++)
                    {
                        var y = x / 128;
                        var set = 0 != ((1 & (x / 8)) ^ (1 & (y / 8)));
                        pixels[x] = (byte)(set ? 255 : 95);
                    }

                    var palette = new byte[4 * 256];
                    for (int x = 0; x < 256; x++)
                    {
                        var v = (byte)x;
                        palette[4 * x + 0] = v;
                        palette[4 * x + 1] = v;
                        palette[4 * x + 2] = v;
                        palette[4 * x + 3] = 255;
                    }

                    return Imgd.Create(
                        new Size(128, 128),
                        Imaging.PixelFormat.Indexed8,
                        pixels,
                        palette,
                        false
                    );
                }

                return loaded;
            };
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;
    }
}

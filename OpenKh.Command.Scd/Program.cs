using System;
using System.ComponentModel.DataAnnotations;
using System.IO;
using System.Reflection;
using McMaster.Extensions.CommandLineUtils;
using OpenKh.Common;
using Xe.BinaryMapper;
using static OpenKh.Audio.Scd;

namespace OpenKh.Command.Scd
{
    [Command("OpenKh.Command.Scd")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(ExtractCommand), typeof(ExportCommand))]
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
                return 2;
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
                return -1;
            }
        }

        protected int OnExecute(CommandLineApplication app)
        {
            app.ShowHelp();
            return 1;
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [Command(Description = "Extract sound files (when possible in a playable format)")]
        private class ExtractCommand
        {
            [Required]
            [Argument(0, Description = "Input file")]
            public string Input { get; set; }

            protected void OnExecute(CommandLineApplication app)
            {
                Audio.Scd file = Read(File.OpenRead(Input));
                int counter = 1;
                
                var outDir = Path.GetFileNameWithoutExtension(Input);
                Directory.CreateDirectory(outDir);

                foreach (var entry in file.StreamFileEntries)
                {
                    switch (entry.Codec)
                    {
                        case Codec.OggVorbis:
                            var mem = new MemoryStream(entry.ExtraData);
                            var extraData = BinaryMapping.ReadObject<OggExtraData>(mem);
                            
                            mem.Position = 32 + extraData.SeekTableSize;
                            extraData.VorbisHeader = new byte[extraData.VorbisHeaderSize];
                            mem.Read(extraData.VorbisHeader, 0, extraData.VorbisHeaderSize);

                            Xor(extraData.VorbisHeader, extraData.EncodeByte);

                            var outFileName = Path.Combine(outDir, $"{entry.Name ?? counter.ToString()}.ogg");
                            using (var outStream = File.Create(outFileName))
                            {
                                outStream.Write(extraData.VorbisHeader);
                                outStream.Write(entry.Data);
                            }
                            counter++;
                            break;
                        default:
                            break;
                    }
                }
            }
        }

        [Command(Description = "Export stream informations in yaml format")]
        private class ExportCommand
        {
            [Required]
            [Argument(0, Description = "Input file")]
            public string Input { get; set; }

            protected void OnExecute(CommandLineApplication app)
            {
                Audio.Scd file = Read(File.OpenRead(Input));
                int counter = 1;

                var outDir = Path.GetFileNameWithoutExtension(Input);
                Directory.CreateDirectory(outDir);

                File.WriteAllText("test.yml", Helpers.YamlSerialize(file.StreamFileEntries));

                //foreach (var entry in file.StreamFileEntries)
                //{
                //    var outFileName = Path.Combine(outDir, $"{entry.Name ?? counter.ToString()}");
                //    File.WriteAllText($"{outFileName}.yml", Helpers.YamlSerialize(entry));
                //    File.WriteAllBytes($"{outFileName}_extraData.bin", entry.ExtraData);
                //    File.WriteAllBytes($"{outFileName}_data.bin", entry.Data);
                    //switch (entry.Codec)
                    //{
                    //    case Codec.OggVorbis:
                    //        var mem = new MemoryStream(entry.ExtraData);
                    //        var extraData = BinaryMapping.ReadObject<OggExtraData>(mem);

                    //        mem.Position = 32 + extraData.SeekTableSize;
                    //        extraData.VorbisHeader = new byte[extraData.VorbisHeaderSize];
                    //        mem.Read(extraData.VorbisHeader, 0, extraData.VorbisHeaderSize);

                    //        Xor(extraData.VorbisHeader, extraData.EncodeByte);

                    //        var outFileName = Path.Combine(outDir, $"{entry.Name ?? counter.ToString()}.ogg");
                    //        using (var outStream = File.Create(outFileName))
                    //        {
                    //            outStream.Write(extraData.VorbisHeader);
                    //            outStream.Write(entry.Data);
                    //        }
                    //        counter++;
                    //        break;
                    //    default:
                    //        break;
                    //}
                //}
            }
        }
    }
}

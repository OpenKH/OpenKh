using McMaster.Extensions.CommandLineUtils;
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

namespace OpenKh.Command.CoctChanger
{
    [Command("OpenKh.Command.CoctChanger")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(CreateDummyCoctCommand), typeof(UseThisCoctCommand))]
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
        [Command(Description = "coct file: create single room")]
        private class CreateDummyCoctCommand
        {
            [Required]
            [Argument(0, Description = "Output coct")]
            public string CoctOut { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "bbox: minX,Y,Z,maxX,Y,Z (default: ...)", ShortName = "b", LongName = "bbox")]
            public string BBox { get; set; } = "-18000,-1500,-18000,18000,500,18000";

            protected int OnExecute(CommandLineApplication app)
            {
                var coct = new Coct();

                var bbox = BBox.Split(',')
                    .Select(one => short.Parse(one))
                    .ToArray();

                var minX = bbox[0];
                var minY = bbox[1];
                var minZ = bbox[2];
                var maxX = bbox[3];
                var maxY = bbox[4];
                var maxZ = bbox[5];

                var ent1 = new Coct.Co1
                {
                    MinX = minX,
                    MinY = minY,
                    MinZ = minZ,
                    MaxX = maxX,
                    MaxY = maxY,
                    MaxZ = maxZ,
                    Collision2Start = 0,
                    Collision2End = 1,
                };
                coct.Collision1.Add(ent1);

                var ent2 = new Coct.CollisionMesh
                {
                    MinX = minX,
                    MinY = minY,
                    MinZ = minZ,
                    MaxX = maxX,
                    MaxY = maxY,
                    MaxZ = maxZ,
                    Collision3Start = 0,
                    Collision3End = 1,
                    v10 = 0x0100,
                    v12 = 0,
                };
                coct.Collision2.Add(ent2);

                var ent3 = new Coct.Co3
                {
                    v00 = 0,
                    Vertex1 = 0,
                    Vertex2 = 1,
                    Vertex3 = 2,
                    Vertex4 = 3,
                    Co5Index = 0,
                    Co6Index = 0,
                    Co7Index = 0,
                };
                coct.Collision3.Add(ent3);

                coct.CollisionVertices.AddRange(
                    new Coct.Vector4[]
                    {
                        new Coct.Vector4 { X = minX, Y = minY, Z = minZ, },
                        new Coct.Vector4 { X = maxX, Y = minY, Z = minZ, },
                        new Coct.Vector4 { X = maxX, Y = minY, Z = maxZ, },
                        new Coct.Vector4 { X = minX, Y = minY, Z = maxZ, },
                    }
                );

                var ent5 = new Coct.Co5
                {
                    X = 0,
                    Y = -1,
                    Z = 0,
                    D = -minY,
                };
                coct.Collision5.Add(ent5);

                var ent6 = new Coct.Co6
                {
                    MinX = minX,
                    MinY = minY,
                    MinZ = minZ,
                    MaxX = maxX,
                    MaxY = maxY,
                    MaxZ = maxZ,
                };
                coct.Collision6.Add(ent6);

                var ent7 = new Coct.Co7
                {
                    Unknown = 0x000003F1,
                };
                coct.Collision7.Add(ent7);

                var buff = new MemoryStream();
                coct.Write(buff);
                buff.Position = 0;
                File.WriteAllBytes(CoctOut, buff.ToArray());

                return 0;
            }
        }

        [HelpOption]
        [Command(Description = "map file: replace coct with your coct")]
        private class UseThisCoctCommand
        {
            [Required]
            [DirectoryExists]
            [Argument(0, Description = "Input map dir")]
            public string Input { get; set; }

            [Required]
            [DirectoryExists]
            [Argument(1, Description = "Output map dir")]
            public string Output { get; set; }

            [Required]
            [FileExists]
            [Argument(2, Description = "COCT file input")]
            public string CoctIn { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var coctBin = File.ReadAllBytes(CoctIn);

                foreach (var mapIn in Directory.GetFiles(Input, "*.map"))
                {
                    Console.WriteLine(mapIn);

                    var mapOut = Path.Combine(Output, Path.GetFileName(mapIn));

                    var entries = File.OpenRead(mapIn).Using(s => Bar.Read(s))
                        .Select(
                            it =>
                            {
                                if (it.Type == Bar.EntryType.MapCollision)
                                {
                                    it.Stream = new MemoryStream(coctBin, false);
                                }

                                return it;
                            }
                        )
                        .ToArray();

                    File.Create(mapOut).Using(s => Bar.Write(s, entries));
                }
                return 0;
            }
        }
    }
}

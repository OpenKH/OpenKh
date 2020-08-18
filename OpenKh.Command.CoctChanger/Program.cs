using McMaster.Extensions.CommandLineUtils;
using McMaster.Extensions.CommandLineUtils.Conventions;
using OpenKh.Command.CoctChanger.Utils;
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
using System.Numerics;
using System.Reflection;
using System.Text;

namespace OpenKh.Command.CoctChanger
{
    [Command("OpenKh.Command.CoctChanger")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    [Subcommand(typeof(CreateRoomCoctCommand), typeof(UseThisCoctCommand), typeof(ShowStatsCommand)
        , typeof(DumpCoctCommand))]
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
        [Command(Description = "coct file: create single closed room")]
        private class CreateRoomCoctCommand
        {
            [Required]
            [Argument(0, Description = "Output coct")]
            public string CoctOut { get; set; }

            [Option(CommandOptionType.SingleValue, Description = "bbox in model 3D space: minX,Y,Z,maxX,Y,Z (default: ...)", ShortName = "b", LongName = "bbox")]
            public string BBox { get; set; } = "-1000,-1000,-1000,1000,1500,1000";

            protected int OnExecute(CommandLineApplication app)
            {
                var coct = new Coct();

                var bbox = BBox.Split(',')
                    .Select(one => short.Parse(one))
                    .ToArray();

                var invMinX = bbox[0];
                var invMinY = bbox[1];
                var invMinZ = bbox[2];
                var invMaxX = bbox[3];
                var invMaxY = bbox[4];
                var invMaxZ = bbox[5];

                var minX = -invMinX;
                var minY = -invMinY;
                var minZ = -invMinZ;
                var maxX = -invMaxX;
                var maxY = -invMaxY;
                var maxZ = -invMaxZ;

                var builder = new Coct.BuildHelper(coct);

                // (forwardVec)
                // +Z
                // A  / +Y (upVec)
                // | / 
                // |/
                // +--> +X (rightVec)

                //   7 == 6  top
                //   |    |  top
                //   4 == 5  top
                //
                // 3 == 2  bottom
                // |    |  bottom
                // 0 == 1  bottom

                var table4Idxes = new short[]
                {
                    builder.AllocateVertex(minX, minY, minZ, 1),
                    builder.AllocateVertex(maxX, minY, minZ, 1),
                    builder.AllocateVertex(maxX, minY, maxZ, 1),
                    builder.AllocateVertex(minX, minY, maxZ, 1),
                    builder.AllocateVertex(minX, maxY, minZ, 1),
                    builder.AllocateVertex(maxX, maxY, minZ, 1),
                    builder.AllocateVertex(maxX, maxY, maxZ, 1),
                    builder.AllocateVertex(minX, maxY, maxZ, 1),
                };

                // side:
                // 0 bottom
                // 1 top
                // 2 west
                // 3 east
                // 4 south
                // 5 north

                var planes = new Plane[]
                {
                    new Plane( 0,-1, 0,+minY), //bottom
                    new Plane( 0,+1, 0,-maxY), //up
                    new Plane(-1, 0, 0,+minX), //west
                    new Plane(+1, 0, 0,-maxX), //east
                    new Plane( 0, 0,-1,+minZ), //south
                    new Plane( 0, 0,+1,-maxZ), //north
                };

                var faceVertexOrders = new int[,]
                {
                    {0,1,2,3}, //bottom
                    {4,7,6,5}, //top
                    {3,7,4,0}, //west
                    {1,5,6,2}, //east
                    {2,6,7,3}, //south
                    {0,4,5,1}, //north
                };

                var collisionMesh = new Coct.CollisionMesh
                {
                    Collisions = new List<Coct.Collision>(),
                    v10 = 0,
                    v12 = 0,
                };

                for (var side = 0; side < 6; side++)
                {
                    var collision = new Coct.Collision
                    {
                        v00 = 0,
                        Vertex1 = table4Idxes[faceVertexOrders[side, 0]],
                        Vertex2 = table4Idxes[faceVertexOrders[side, 1]],
                        Vertex3 = table4Idxes[faceVertexOrders[side, 2]],
                        Vertex4 = table4Idxes[faceVertexOrders[side, 3]],
                        Plane = planes[side],
                        BoundingBox = BoundingBoxInt16.Invalid,
                        SurfaceFlags = new Coct.SurfaceFlags() { Flags = 0x3F1 },
                    };
                    coct.Complete(collision);
                    collisionMesh.Collisions.Add(collision);
                }

                coct.Complete(collisionMesh);

                coct.CompleteAndAdd(
                    new Coct.CollisionMeshGroup
                    {
                        Meshes = new List<Coct.CollisionMesh>() { collisionMesh }
                    }
                );

                var buff = new MemoryStream();
                coct.Write(buff);
                buff.Position = 0;
                File.WriteAllBytes(CoctOut, buff.ToArray());

                return 0;
            }
        }

        class ShortVertex3
        {
            public short X { get; set; }
            public short Y { get; set; }
            public short Z { get; set; }
        }

        class ShortBBox
        {
            public ShortVertex3 Min { get; set; }
            public ShortVertex3 Max { get; set; }
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

        [HelpOption]
        [Command(Description = "coct file: show stats")]
        private class ShowStatsCommand
        {
            private class Report
            {
                public int NodeCount;
                public int LeafCount;
                public int TreeDepth;
                public int MeshCount;
                public int CollisionCount;
                public int VertexCount;
            }

            [Required]
            [FileExists]
            [Argument(0, Description = "Input map/coct file (decided by file extension: `.map` or not)")]
            public string InputFile { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var isMap = Path.GetExtension(InputFile).ToLowerInvariant() == ".map";

                if (isMap)
                {
                    foreach (var entry in File.OpenRead(InputFile).Using(Bar.Read)
                        .Where(entry => false
                            || entry.Type == Bar.EntryType.MapCollision
                            || entry.Type == Bar.EntryType.CameraCollision
                            || entry.Type == Bar.EntryType.LightData
                            || entry.Type == Bar.EntryType.MapCollision2
                            || entry.Type == Bar.EntryType.ModelCollision
                        )
                    )
                    {
                        Console.WriteLine($"# {entry.Name}:{entry.Index} ({entry.Type})");
                        PrintSummary(Coct.Read(entry.Stream));
                        Console.WriteLine();
                    }
                }
                else
                {
                    PrintSummary(File.OpenRead(InputFile).Using(Coct.Read));
                }

                return 0;
            }

            private void PrintSummary(Coct coct)
            {
                var report = new Report();
                GenerateReport(coct, 1, 0, report);

                Console.WriteLine($"Node count: {report.NodeCount,8:#,##0}");
                Console.WriteLine($"Leaf count: {report.LeafCount,8:#,##0}");
                Console.WriteLine($"Tree depth: {report.TreeDepth,8:#,##0}");
                Console.WriteLine($"Mesh count: {report.MeshCount,8:#,##0}");
                Console.WriteLine($"Coll count: {report.CollisionCount,8:#,##0}");
                Console.WriteLine($"Vert count: {report.VertexCount,8:#,##0}");
            }

            private static void GenerateReport(Coct coct, int depth, int index, Report report)
            {
                if (index == -1) return;

                report.NodeCount++;
                report.TreeDepth = Math.Max(report.TreeDepth, depth);
                var meshGroup = coct.CollisionMeshGroupList[index];
                if (meshGroup.Child1 >= 0)
                {
                    var childDepth = depth + 1;
                    GenerateReport(coct, childDepth, meshGroup.Child1, report);
                    GenerateReport(coct, childDepth, meshGroup.Child2, report);
                    GenerateReport(coct, childDepth, meshGroup.Child3, report);
                    GenerateReport(coct, childDepth, meshGroup.Child4, report);
                    GenerateReport(coct, childDepth, meshGroup.Child5, report);
                    GenerateReport(coct, childDepth, meshGroup.Child6, report);
                    GenerateReport(coct, childDepth, meshGroup.Child7, report);
                    GenerateReport(coct, childDepth, meshGroup.Child8, report);
                }
                else
                    report.LeafCount++;

                foreach (var mesh in meshGroup.Meshes)
                {
                    report.MeshCount++;
                    foreach (var collision in mesh.Collisions)
                    {
                        report.CollisionCount++;
                        report.VertexCount += 3;
                        if (collision.Vertex4 >= 0)
                            report.VertexCount++;
                    }
                }
            }
        }

        [HelpOption]
        [Command(Description = "coct file: dump")]
        private class DumpCoctCommand
        {
            [Required]
            [FileExists]
            [Argument(0, Description = "COCT file input")]
            public string CoctIn { get; set; }

            protected int OnExecute(CommandLineApplication app)
            {
                var coct = File.OpenRead(CoctIn).Using(Coct.Read);

                new DumpCoctUtil(coct, Console.Out);

                return 0;
            }
        }
    }
}

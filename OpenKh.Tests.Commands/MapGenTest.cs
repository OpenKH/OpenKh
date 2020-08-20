using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using System.Linq;
using Xunit;
using OpenKh.Command.MapGen.Utils;
using OpenKh.Command.DoctChanger.Utils;
using OpenKh.Command.CoctChanger.Utils;

namespace OpenKh.Tests.Commands
{
    public class MapGenTest
    {
        [Theory]
        [InlineData("res/mapgen/diagnoal/diagnoal.fbx")]
        [InlineData("res/mapgen/multi-4bpp/multi-4bpp.fbx")]
        [InlineData("res/mapgen/multi-8bpp/multi-8bpp.fbx")]
        [InlineData("res/mapgen/terrain/terrain.fbx")]
        public void TestMapGenUtil(string inputModel)
        {
            var outMap = Path.ChangeExtension(inputModel, ".map");
            using var disposer = new FileDisposer(outMap);

            new MapGenUtil().Run(inputModel, outMap);

            var barEntries = File.OpenRead(outMap).Using(Bar.Read);

            {
                var doct = Doct.Read(barEntries.Single(it => it.Type == Bar.EntryType.MeshOcclusion).Stream);
                var writer = new StringWriter();
                new DumpDoctUtil(doct, writer);

                var doctDumpFile = Path.ChangeExtension(inputModel, ".doct.dump");

                Assert.Equal(
                    expected: File.ReadAllText(doctDumpFile),
                    actual: writer.ToString(),
                    ignoreLineEndingDifferences: true
                );
            }

            {
                var coct = Coct.Read(barEntries.Single(it => it.Type == Bar.EntryType.MapCollision).Stream);
                var writer = new StringWriter();
                new DumpCoctUtil(coct, writer);

                var coctDumpFile = Path.ChangeExtension(inputModel, ".coct.dump");

                Assert.Equal(
                    expected: File.ReadAllText(coctDumpFile),
                    actual: writer.ToString(),
                    ignoreLineEndingDifferences: true
                );
            }
        }
    }
}

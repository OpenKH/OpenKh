using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using System.Linq;
using Xunit;
using System;
using System.Collections.Generic;
using System.Text;
using OpenKh.Command.MapGen.Utils;
using OpenKh.Command.DoctChanger.Utils;

namespace OpenKh.Tests.kh2
{
    public class MapGenTest
    {
        [Theory]
        [InlineData(@"kh2/res/mapgen/diagnoal/diagnoal.fbx")]
        [InlineData(@"kh2/res/mapgen/multi-4bpp/multi-4bpp.fbx")]
        [InlineData(@"kh2/res/mapgen/multi-8bpp/multi-8bpp.fbx")]
        [InlineData(@"kh2/res/mapgen/terrain/terrain.fbx")]
        public void TestMapGenUtil(string inputModel)
        {
            var outMap = Path.ChangeExtension(inputModel, ".map");
            new MapGenUtil().Run(inputModel, outMap);

            var barEntries = File.OpenRead(outMap).Using(Bar.Read);

            {
                var doct = Doct.Read(barEntries.Single(it => it.Type == Bar.EntryType.MeshOcclusion).Stream);
                var writer = new StringWriter();
                new DumpDoctUtil(doct, writer);

                var doctDumpFile = Path.ChangeExtension(inputModel, ".doct.dump");

                Assert.Equal(
                    expected: File.ReadAllText(doctDumpFile),
                    actual: writer.ToString()
                );
                //File.WriteAllText(doctDumpFile, writer.ToString());
            }
        }
    }
}

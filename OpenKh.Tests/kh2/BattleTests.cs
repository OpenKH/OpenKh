using OpenKh.Kh2.Battle;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class BattleTests
    {
        public class FmlvTests
        {
            [Fact]
            public void CheckStandardFile() => Common.FileOpenRead(@"kh2/res/fmlv_de.bin", stream =>
            {
                var table = Fmlv.Read(stream);

                Assert.Equal(0x26, table.Count);

                Assert.Equal(6, table.GroupBy(x => x.FormId).ToList().Count);

                Assert.Equal(0x5A, table.FirstOrDefault(x => x.FormId == 2 && x.FormLevel == 4).Exp);
            });

            [Fact]
            public void CheckFinalMixFile() => Common.FileOpenRead(@"kh2/res/fmlv_fm.bin", stream =>
            {
                var table = Fmlv.Read(stream);

                Assert.Equal(0x2D, table.Count);

                Assert.Equal(7, table.GroupBy(x => x.FormId).ToList().Count);

                Assert.Equal(0x4C, table.FirstOrDefault(x => x.FormId == 2 && x.FormLevel == 4).Exp);
            });

            [Fact]
            public void WriteTest() => Common.FileOpenRead(@"kh2/res/fmlv_fm.bin", stream =>
                Helpers.AssertStream(stream, inStream =>
                {
                    var outStream = new MemoryStream();
                    Fmlv.Write(outStream, Fmlv.Read(inStream));

                    return outStream;
                })
            );
        }
    }
}
using OpenKh.Common;
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
            public void CheckStandardNewImpl() => File.OpenRead(@"kh2/res/fmlv_de.bin").Using(stream =>
            {
                var table = Fmlv.Read(stream);

                Assert.Equal(0x26, table.Count);

                Assert.Equal(6, table.GroupBy(x => x.FormId).ToList().Count);

                Assert.Equal(0x5A, table.FirstOrDefault(x => x.FormId == 2 && x.FormLevel == 4).Exp);
            });

            [Fact]
            public void CheckFinalMixNewImpl() => File.OpenRead(@"kh2/res/fmlv_fm.bin").Using(stream =>
            {
                var table = Fmlv.Read(stream);

                Assert.Equal(0x2D, table.Count);

                Assert.Equal(7, table.GroupBy(x => x.FormId).ToList().Count);

                Assert.Equal(0x4C, table.FirstOrDefault(x => x.FormId == 2 && x.FormLevel == 4).Exp);
            });

            [Theory]
            [InlineData(@"kh2/res/fmlv_de.bin")]
            [InlineData(@"kh2/res/fmlv_fm.bin")]
            public void WriteTest(string fileName) => File.OpenRead(fileName).Using(stream =>
                Helpers.AssertStream(stream, inStream =>
                {
                    var outStream = new MemoryStream();
                    Fmlv.Write(outStream, Fmlv.Read(inStream));

                    return outStream;
                })
            );
        }

        public class EnemyTest
        {
            [Fact]
            public void ReadTest() => File.OpenRead(@"kh2/res/enmp.bin").Using(stream =>
            {
                var table = Enmp.Read(stream);

                Assert.Equal(229, table.Count);

                var roxas = table.FirstOrDefault(enemy => enemy.Id == 242);
                Assert.Equal(99, roxas.Level);
                Assert.Equal(1750, roxas.Health[0]);
                Assert.Equal(86, roxas.Unknown44); // 56
                Assert.Equal(28, roxas.Unknown46);
                Assert.Equal(100, roxas.PhysicalWeakness);
                Assert.Equal(25, roxas.FireWeakness);
                Assert.Equal(25, roxas.IceWeakness);
                Assert.Equal(25, roxas.ThunderWeakness);
                Assert.Equal(25, roxas.DarkWeakness);
                Assert.Equal(25, roxas.Unknown52);
                Assert.Equal(100, roxas.ReflectWeakness);
            });

            [Fact]
            public void WriteTest() => File.OpenRead(@"kh2/res/enmp.bin").Using(stream =>
            {
                Helpers.AssertStream(stream, inStream =>
                {
                    var outStream = new MemoryStream();
                    Enmp.Write(outStream, Enmp.Read(inStream));
                    return outStream;
                });
            });
        }

        public class LvupTests
        {
            [Fact]
            public void LvupTableTest() => File.OpenRead(@"kh2/res/lvup_fm.bin").Using(stream =>
            {
                Helpers.AssertStream(stream, inStream =>
                {
                    var outStream = new MemoryStream();
                    Lvup.Read(inStream).Write(outStream);
                    return outStream;
                });
            });

            [Fact]
            public void WriteTest() => File.OpenRead(@"kh2/res/lvup_fm.bin").Using(stream =>
            {
                Helpers.AssertStream(stream, inStream =>
                {
                    var outStream = new MemoryStream();
                    Lvup.Read(inStream).Write(outStream);
                    return outStream;
                });
            });
        }

        [Fact]
        public void BonsTableTest() => File.OpenRead(@"kh2/res/bons_fm.bin").Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                Bons.Write(outStream, Bons.Read(inStream));
                return outStream;
            });
        });

        [Fact]
        public void PrztTableTest() => File.OpenRead(@"kh2/res/przt.bin").Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                Przt.Write(outStream, Przt.Read(inStream));
                return outStream;
            });
        });

        [Fact]
        public void VtblTableTest() => File.OpenRead(@"kh2/res/vtbl.bin").Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                Vtbl.Write(outStream, Vtbl.Read(inStream));
                return outStream;
            });
        });

    }
}

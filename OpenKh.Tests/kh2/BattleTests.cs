using System.IO;
using OpenKh.Common;
using OpenKh.Kh2.Battle;
using System.Linq;
using Xunit;
using OpenKh.Kh2;

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

            [Fact]
            public void WriteTest() => File.OpenRead(@"kh2/res/fmlv_fm.bin").Using(stream =>
                Helpers.AssertStream(stream, inStream =>
                {
                    var outStream = new MemoryStream();
                    Fmlv.Write(outStream, Fmlv.Read(inStream));

                    return outStream;
                })
            );
        }

        [Fact]
        public void EnemyTableTest() => File.OpenRead(@"kh2/res/enmp.bin").Using(stream =>
        {
            var table = BaseTable<Enmp>.Read(stream);

            Assert.Equal(2, table.Id);
            Assert.Equal(229, table.Count);
            Assert.Equal(229, table.Items.Count);

            var roxas = table.Items.FirstOrDefault(enemy => enemy.Id == 242);
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
        public void BonsTableTest() => File.OpenRead(@"kh2/res/bons_fm.bin").Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                BaseTable<Bons>.Read(inStream).Write(outStream);
                return outStream;
            });
        });

        [Fact]
        public void PrztTableTest() => File.OpenRead(@"kh2/res/przt.bin").Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                BaseTable<Przt>.Read(inStream).Write(outStream);
                return outStream;
            });
        });

        [Fact]
        public void VtblTableTest() => File.OpenRead(@"kh2/res/vtbl.bin").Using(stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                BaseTable<Vtbl>.Read(inStream).Write(outStream);
                return outStream;
            });
        });

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
    }
}

using OpenKh.Common;
using OpenKh.Kh2.Battle;
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
                var table = new Fmlv(stream);

                Assert.Equal(0x26, table.Count);
                Assert.Equal(0x26, table.Levels.Count);

                Assert.Equal(6, table.Levels.GroupBy(x => x.FormId).ToList().Count);

                Assert.Equal(0x5A, table.Levels.FirstOrDefault(x => x.FormId == 2 && x.FormLevel == 4).Exp);
            });

            [Fact]
            public void CheckFinalMixFile() => Common.FileOpenRead(@"kh2/res/fmlv_fm.bin", stream =>
            {
                var table = new Fmlv(stream);

                Assert.Equal(0x2D, table.Count);
                Assert.Equal(0x2D, table.Levels.Count);

                Assert.Equal(7, table.Levels.GroupBy(x => x.FormId).ToList().Count);

                Assert.Equal(0x4C, table.Levels.FirstOrDefault(x => x.FormId == 2 && x.FormLevel == 4).Exp);
            });
        }

        [Fact]
        public void EnemyTableTest() => Common.FileOpenRead(@"kh2/res/enmp.bin", x => x.Using(stream =>
        {
            var table = BaseBattle<Enmp>.Read(stream);

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
        }));
    }
}

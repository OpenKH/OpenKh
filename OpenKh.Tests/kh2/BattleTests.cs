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

        public class BonsTests
        {
            [Fact]
            public void CheckHeaderSize() => Common.FileOpenRead(@"kh2/res/bons_fm.bin", stream =>
            {
                var table = new Bons(stream);

                Assert.Equal(0xB3, table.BonusLevels.Count);
            });
        }

        public class PrztTests
        {
            [Fact]
            public void CheckHeaderSize() => Common.FileOpenRead(@"E:\HAX\KH Hacking\00battle\przt_fm.bin", stream =>
            {
                var table = new Przt(stream);

                Assert.Equal(0xB8, table.Drops.Count);
            });
        }
    }
}
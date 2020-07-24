using OpenKh.Common;
using OpenKh.Kh2.Ard;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ArdTests
    {
        public class SpawnScriptTests
        {
            const string FileName = "kh2/res/map.spawnscript";

            [Fact]
            public void ReadTest()
            {
                var spawns = File.OpenRead(FileName).Using(SpawnScript.Read);
                var strProgram = spawns.First(x => x.ProgramId == 6).ToString();

                Assert.Equal(31, spawns.Count);
            }

            [Fact]
            public void WriteTest() => File.OpenRead(FileName).Using(x => Helpers.AssertStream(x, stream =>
            {
                var outStream = new MemoryStream();
                SpawnScript.Write(outStream, SpawnScript.Read(stream));

                return outStream;
            }));
        }

        public class SpawnPointTests
        {
            const string FileNameM00 = "kh2/res/m_00.spawnpoint";
            const string FileNameM10 = "kh2/res/m_10.spawnpoint";
            const string FileNameY73 = "kh2/res/y73_.spawnpoint";

            [Fact]
            public void ReadM00Test()
            {
                var spawns = File.OpenRead(FileNameM00).Using(SpawnPoint.Read);

                Assert.Equal(4, spawns.Count);
                Assert.Equal(3, spawns[0].SpawnEntityGroupCount);
                Assert.Equal(0x236, spawns[0].SpawnEntiyGroup[0].ObjectId);
            }

            [Theory]
            [InlineData(FileNameM00)]
            [InlineData(FileNameM10)]
            [InlineData(FileNameY73)]
            public void WriteTest(string fileName) =>
                File.OpenRead(fileName).Using(x => Helpers.AssertStream(x, stream =>
            {
                var outStream = new MemoryStream();
                SpawnPoint.Write(outStream, SpawnPoint.Read(stream));

                return outStream;
            }));
        }
    }
}

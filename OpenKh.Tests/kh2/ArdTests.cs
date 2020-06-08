using OpenKh.Common;
using OpenKh.Kh2.Ard;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ArdTests
    {
        public class SpawnPointTests
        {
            const string FileName = "kh2/res/m_00.spawnpoint";

            [Fact]
            public void ReadTest()
            {
                var spawns = File.OpenRead(FileName).Using(SpawnPoint.Read);

                Assert.Equal(4, spawns.Count);
                Assert.Equal(3, spawns[0].EntityCount);
            }

            [Fact]
            public void WriteTest() => File.OpenRead(FileName).Using(x => Helpers.AssertStream(x, stream =>
            {
                var outStream = new MemoryStream();
                SpawnPoint.Write(outStream, SpawnPoint.Read(stream));

                return outStream;
            }));
        }
    }
}

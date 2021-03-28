using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ObjectCollisionTests
    {
        private const string FileName = "kh2/res/object.knc";

        [Fact]
        public void ReadTest()
        {
            var list = File.OpenRead(FileName).Using(ObjectCollision.Read);
            Assert.Equal(3, list.Count);
        }

        [Fact]
        public void WriteTest() => File.OpenRead(FileName).Using(x => Helpers.AssertStream(x, stream =>
        {
            var outStream = new MemoryStream();
            ObjectCollision.Write(outStream, ObjectCollision.Read(stream));

            return outStream;
        }));
    }
}

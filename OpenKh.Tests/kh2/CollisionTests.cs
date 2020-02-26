using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class CollisionTests
    {
        private const string FileName = "kh2/res/map-collision.coct";

        [Fact]
        public void IsValid() => File.OpenRead(FileName).Using(stream =>
            Coct.IsValid(stream));

        [Fact]
        public void ReadCollision() => File.OpenRead(FileName).Using(stream =>
        {
            var collision = Coct.Read(stream);

            Assert.Equal(1, collision.Collision1.Count);
            Assert.Equal(1, collision.Collision2.Count);
            Assert.Equal(16, collision.Collision3.Count);
            Assert.Equal(14, collision.Collision4.Count);
            Assert.Equal(8, collision.Collision5.Count);
            Assert.Equal(8, collision.Collision6.Count);
            Assert.Equal(2, collision.Collision7.Count);
        });

        [Fact]
        public void WriteCollision() => File.OpenRead(FileName).Using(x =>
        Helpers.AssertStream(x, inStream =>
            {
                var outStream = new MemoryStream();
                Coct.Read(inStream).Write(outStream);

                return outStream;
            }));
    }
}
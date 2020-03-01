using OpenKh.Common;
using OpenKh.Kh2;
using System.IO;
using System.Linq;
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

            Assert.Equal(188, collision.Collision1.Count);
            Assert.Equal(7, collision.Collision1[5].v00);
            Assert.Equal(6, collision.Collision1[5].v02);
            Assert.Equal(6, collision.Collision1[5].v02);
            Assert.Equal(-4551, collision.Collision1[5].v10);
            Assert.Equal(3, collision.Collision1[5].Collision2Index);
            Assert.Equal(4, collision.Collision1[5].Collision2Count);

            Assert.Equal(152, collision.Collision2.Count);
            Assert.Equal(99, collision.Collision2[4].v02);
            Assert.Equal(-3449, collision.Collision2[4].v06);
            Assert.Equal(6, collision.Collision2[4].Collision3Index);
            Assert.Equal(7, collision.Collision2[4].Collision3Count);

            Assert.Equal(319, collision.Collision3.Count);
            Assert.Equal(24, collision.Collision3[7].Vertex1);
            Assert.Equal(25, collision.Collision3[7].Vertex2);
            Assert.Equal(14, collision.Collision3[7].Vertex3);
            Assert.Equal(18, collision.Collision3[7].Vertex4);
            Assert.Equal(6, collision.Collision3[7].v0a);
            Assert.Equal(4, collision.Collision3[7].v0c);
            Assert.Equal(2, collision.Collision3[7].v0e);

            Assert.Equal(549, collision.CollisionVertices.Count);
            Assert.Equal(233, collision.Collision5.Count);
            Assert.Equal(240, collision.Collision6.Count);
            Assert.Equal(9, collision.Collision7.Count);
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
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
            var collision = new CoctLogical(Coct.Read(stream));

            Assert.Equal(188, collision.Collision1.Count);
            Assert.Equal(7, collision.Collision1[5].Child1);
            Assert.Equal(6, collision.Collision1[5].Child2);
            Assert.Equal(6, collision.Collision1[5].Child2);
            Assert.Equal(-4551, collision.Collision1[5].MinX);

            Assert.Equal(1, collision.Collision1[5].Meshes.Count);

            Assert.Equal(99, collision.Collision1[6].Meshes[0].MinY);
            Assert.Equal(-3449, collision.Collision1[6].Meshes[0].MaxX);
            Assert.Equal(1, collision.Collision1[6].Meshes[0].Items.Count);

            Assert.Equal(24, collision.Collision1[7].Meshes[0].Items[0].Vertex1);
            Assert.Equal(25, collision.Collision1[7].Meshes[0].Items[0].Vertex2);
            Assert.Equal(14, collision.Collision1[7].Meshes[0].Items[0].Vertex3);
            Assert.Equal(18, collision.Collision1[7].Meshes[0].Items[0].Vertex4);
            Assert.Equal(6, collision.Collision1[7].Meshes[0].Items[0].Co5Index);
            Assert.Equal(4, collision.Collision1[7].Meshes[0].Items[0].Co6Index);
            Assert.Equal(2, collision.Collision1[7].Meshes[0].Items[0].Co7Index);

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

        [Fact]
        public void TestLogicalReadWrite()
        {
            File.OpenRead(FileName).Using(
                inStream =>
                {
                    var outStream = new MemoryStream();
                    var coct = Coct.Read(inStream);
                    var coctLogical = new CoctLogical(coct);
                    var coctBack = coctLogical.CreateCoct();
                    coctBack.Write(outStream);

                    outStream.Position = 0;

                    {
                        var collision = new CoctLogical(Coct.Read(outStream));

                        Assert.Equal(188, collision.Collision1.Count);
                        Assert.Equal(7, collision.Collision1[5].Child1);
                        Assert.Equal(6, collision.Collision1[5].Child2);
                        Assert.Equal(6, collision.Collision1[5].Child2);
                        Assert.Equal(-4551, collision.Collision1[5].MinX);

                        Assert.Equal(1, collision.Collision1[5].Meshes.Count);

                        Assert.Equal(99, collision.Collision1[6].Meshes[0].MinY);
                        Assert.Equal(-3449, collision.Collision1[6].Meshes[0].MaxX);
                        Assert.Equal(1, collision.Collision1[6].Meshes[0].Items.Count);

                        Assert.Equal(24, collision.Collision1[7].Meshes[0].Items[0].Vertex1);
                        Assert.Equal(25, collision.Collision1[7].Meshes[0].Items[0].Vertex2);
                        Assert.Equal(14, collision.Collision1[7].Meshes[0].Items[0].Vertex3);
                        Assert.Equal(18, collision.Collision1[7].Meshes[0].Items[0].Vertex4);
                        Assert.Equal(6, collision.Collision1[7].Meshes[0].Items[0].Co5Index);
                        Assert.Equal(4, collision.Collision1[7].Meshes[0].Items[0].Co6Index);
                        Assert.Equal(2, collision.Collision1[7].Meshes[0].Items[0].Co7Index);

                        Assert.Equal(549, collision.CollisionVertices.Count);
                        Assert.Equal(233, collision.Collision5.Count);
                        Assert.Equal(240, collision.Collision6.Count);
                        Assert.Equal(9, collision.Collision7.Count);
                    }
                }
            );
        }
    }
}
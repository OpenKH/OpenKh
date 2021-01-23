using OpenKh.Common;
using OpenKh.Recom;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Recom
{
    public class MdlTests
    {
        private const string FileName = @"D:\Hacking\KHReCoM\models\ps4_stripped\PL0001.MDL";

        [Fact]
        public void ReadTest()
        {
            var mdl = File.OpenRead(FileName).Using(Mdl.Read);

            Assert.Equal(2, mdl.Count);

            var submodel = mdl.First();
            Assert.Equal(135, submodel.Bones.Count);
            Assert.Equal("Root", submodel.Bones[0].Name);
            Assert.Equal("ex_0010_sk", submodel.Bones[1].Name);
            Assert.Equal("hip", submodel.Bones[2].Name);

            Assert.Equal(2, submodel.Materials.Count);
            Assert.Equal("n_zz100_02", submodel.Materials[0].Name);
            Assert.Equal("n_zz100_01", submodel.Materials[1].Name);

            Assert.Equal(135, submodel.Meshes.Count);
            Assert.Equal(32, submodel.Meshes[0].Vertices.Length);
            Assert.Equal(32, submodel.Meshes[1].Vertices.Length);
            Assert.Equal(33, submodel.Meshes[2].Vertices.Length);

            var aaa = submodel.Meshes
                .GroupBy(x => x.VertexBufferStride)
                .ToList();
        }

        [Fact]
        public void WritesBackCorrectly() => File.OpenRead(FileName).Using(stream =>
            Helpers.AssertStream(stream, x =>
            {
                var outStream = new MemoryStream();
                Mdl.Write(outStream, Mdl.Read(stream));

                return outStream;
            }));
    }
}

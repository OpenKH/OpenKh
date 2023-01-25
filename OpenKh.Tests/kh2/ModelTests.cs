using OpenKh.Common;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ModelTests
    {
        public class BackgroundTests
        {
            private const string FileName = "kh2/res/map.model";
            private readonly Model _model;

            public BackgroundTests()
            {
                _model = File.OpenRead(FileName).Using(Model.Read);
            }

            [Fact]
            public void IsBackground() => Assert.IsType<ModelBackground>(_model);

            [Fact]
            public void ReadGroupCount() => Assert.Equal(0x61, _model.GroupCount);

            [Fact]
            public void ReadFlags() => Assert.Equal(3, _model.Flags);

            [Fact]
            public void GetPolygonCount() => Assert.Equal(0x699,
                _model.GetDrawPolygonCount(CreateDisplayFlags(0x61, 1)));

            [Fact]
            public void ShouldWriteBackTheExactSameFile() => File.OpenRead(FileName)
                .Using(stream => Helpers.AssertStream(stream, _ =>
                {
                    var outStream = new MemoryStream();
                    _model.Write(outStream);

                    return outStream;
                }));
        }

        private static IList<byte> CreateDisplayFlags(int count, byte flag) =>
            Enumerable.Range(0, count).Select(x => flag).ToList();
    }
}

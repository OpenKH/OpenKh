using OpenKh.Common;
using OpenKh.Kh3;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Kh3
{
    public class ConditionalLoadTeast
    {
        private const string Sample1 = "kh3/res/conditionalload-sample1.bin";
        private const string Sample2 = "kh3/res/conditionalload-sample2.bin";

        [Fact]
        public void InitialPlayerRead()
        {
            var conditions = File.OpenRead(Sample1).Using(ConditionalLoad.Read);

            Assert.Equal(3, conditions.Count);
            Assert.Equal(
                "For pooh, load Sora with P_PO100 if less than PO@100",
                conditions[0].ToString());
            Assert.Equal(
                "For zz, load Debug with P_ZZ100 if greater than debug@DBG_EVT",
                conditions[1].ToString());
            Assert.Equal(
                "For *, load Xeeynamo with P_EX999",
                conditions[2].ToString());
        }

        [Theory]
        [InlineData(Sample1)]
        [InlineData(Sample2)]
        public void WriteTest(string fileName) => Common.FileOpenRead(fileName, stream =>
        {
            Helpers.AssertStream(stream, inStream =>
            {
                var outStream = new MemoryStream();
                ConditionalLoad.Write(outStream, ConditionalLoad.Read(inStream));

                return outStream;
            });
        });
    }
}

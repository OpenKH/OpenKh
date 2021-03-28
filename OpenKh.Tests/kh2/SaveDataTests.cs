using OpenKh.Common;
using OpenKh.Kh2.SaveData;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class SaveDataTests
    {
        [Theory]
        [InlineData("kh2us")]
        [InlineData("kh2fm")]
        public void WriteSameFile(string name) => File
            .OpenRead($"kh2/res/{name}.bin").Using(stream =>
            {
                Helpers.AssertStream(stream, inStream =>
                {
                    var outStream = new MemoryStream();
                    SaveDataFactory.Write(outStream, SaveDataFactory.Read(stream));

                    return outStream;
                });
            });
    }
}

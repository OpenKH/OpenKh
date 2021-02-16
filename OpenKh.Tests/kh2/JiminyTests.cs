using OpenKh.Common;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class JiminyTests
    {
        public class CharTests
        {
            [Fact]
            public void CheckForLength() => File.OpenRead("kh2/res/char.jimidata").Using(stream =>
            {
                var entries = Kh2.Jiminy.Char.Read(stream);
                var grouped = entries.GroupBy(x => x.Unk10).ToList();
                Assert.Equal(0x152, entries.Count);
            });
        }
    }
}

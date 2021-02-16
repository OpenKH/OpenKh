using OpenKh.Kh2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class DoctTest
    {
        [Fact]
        public void CreateDummyDoctAndReadIt()
        {
            using var stream = new MemoryStream();
            var doct = new Doct();
            doct.Entry1List.Add(new Doct.Entry1 { });
            doct.Entry2List.Add(new Doct.Entry2 { });

            Doct.Write(stream, doct);

            stream.Position = 0;

            Doct.Read(stream); // confirm that it throws nothing.
        }
    }
}

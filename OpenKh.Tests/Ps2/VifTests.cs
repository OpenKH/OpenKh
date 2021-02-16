using OpenKh.Common;
using OpenKh.Ps2;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Ps2
{
    public class VifTests
    {
        const string VifFileName = "./Ps2/res/VifPacked.bin";
        const string Vu1FileName = "./Ps2/res/Vu1memory.bin";

        [Fact]
        public void UnpackVifPacketToVu1() => File.OpenRead(Vu1FileName).Using(stream =>
        {
            Helpers.AssertStream(stream, _ =>
            {
                var vifContent = File.ReadAllBytes(VifFileName);
                var vifEmulator = new VifUnpacker(vifContent);
                vifEmulator.Run();

                return new MemoryStream(vifEmulator.Memory);
            });
        });
    }
}

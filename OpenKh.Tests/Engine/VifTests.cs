using OpenKh.Common;
using OpenKh.Engine.Parsers;
using System.IO;
using Xunit;

namespace OpenKh.Tests.Engine
{
    public class VifTests
    {
        const string VifFileName = "./engine/res/VifPacked.bin";
        const string Vu1FileName = "./engine/res/Vu1memory.bin";

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

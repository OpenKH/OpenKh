using OpenKh.Common;
using System.IO;

namespace OpenKh.Kh2
{
    public partial class Dpd
    {
        public class Vsf
        {
            public byte[] Unknown { get; set; } // Structure is unknown so the binary is stored for now

            public Vsf()
            {

            }

            public Vsf(byte[] vsf)
            {
                Unknown = vsf;
            }

            public Vsf(Stream vsfStream)
            {
                long initialPosition = vsfStream.Position;
                Unknown = vsfStream.ReadAllBytes();
                vsfStream.Position = initialPosition;
            }

            public Stream getAsStream()
            {
                Stream fileStream = new MemoryStream(Unknown);
                fileStream.Position = 0;
                return fileStream;
            }
        }
    }
}

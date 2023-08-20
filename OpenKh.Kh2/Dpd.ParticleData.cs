using OpenKh.Common;
using System.IO;

namespace OpenKh.Kh2
{
    public partial class Dpd
    {
        public class ParticleData
        {
            public byte[] Unknown { get; set; } // Structure is unknown so the binary is stored for now

            public ParticleData()
            {

            }

            public ParticleData(byte[] particleData)
            {
                Unknown = particleData;
            }

            public ParticleData(Stream particleDataStream)
            {
                long initialPosition = particleDataStream.Position;
                Unknown = particleDataStream.ReadAllBytes();
                particleDataStream.Position = initialPosition;
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

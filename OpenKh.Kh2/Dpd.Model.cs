using OpenKh.Common;
using System.IO;

namespace OpenKh.Kh2
{
    public partial class Dpd
    {
        public class Model
        {
            public byte[] Unknown { get; set; } // Structure is unknown so the binary is stored for now

            public Model()
            {

            }

            public Model(byte[] model)
            {
                Unknown = model;
            }

            public Model(Stream modelStream)
            {
                long initialPosition = modelStream.Position;
                Unknown = modelStream.ReadAllBytes();
                modelStream.Position = initialPosition;
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

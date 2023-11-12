using OpenKh.Common;
using OpenKh.Kh2.Utils;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Kh2
{
    public class DpdVsf
    {
        public int VsfNo { get; set; }
        public int ModelNumber { get; set; }
        List<ushort> Indices { get; set; }

        public DpdVsf(Stream vsfStream)
        {
            VsfNo = vsfStream.ReadInt32();
            ModelNumber = vsfStream.ReadInt32();
            Indices = new List<ushort>();
            int indexCount = vsfStream.ReadInt32();
            for(int i = 0; i < indexCount; i++)
            {
                Indices.Add(vsfStream.ReadUInt16());
            }
        }

        public Stream getAsStream()
        {
            Stream fileStream = new MemoryStream();
            fileStream.Position = 0;

            fileStream.Write(VsfNo);
            fileStream.Write(ModelNumber);
            fileStream.Write((int)Indices.Count);
            foreach (ushort index in Indices)
            {
                fileStream.Write(index);
            }
            ReadWriteUtils.alignStreamToByte(fileStream, 16);

            fileStream.Position = 0;
            return fileStream;
        }
    }
}

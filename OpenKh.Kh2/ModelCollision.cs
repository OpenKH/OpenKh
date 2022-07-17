using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class ModelCollision
    {
        public static int reservedSize = 56;
        [Data] public int EntryCount { get; set; }
        [Data] public int Enable { get; set; }
        [Data] public List<ObjectCollision> EntryList { get; set; }

        bool EnableFlag
        {
            get
            {
                return (Enable == 1);
            }
            set
            {
                Enable = (value) ? 1 : 0;
            }
        }

        public ModelCollision(Stream stream)
        {
            this.EntryCount = stream.ReadInt32();
            this.Enable = stream.ReadInt32();
            stream.Position += reservedSize;
            this.EntryList = Enumerable
                                        .Range(0, this.EntryCount)
                                        .Select(x => BinaryMapping.ReadObject<ObjectCollision>(stream))
                                        .ToList();
        }

        public Stream toStream()
        {
            Stream outStream = new MemoryStream();
            outStream.Write(EntryCount);
            outStream.Write(Enable);

            outStream.Write(new byte[reservedSize], 0, reservedSize);

            foreach (var item in EntryList)
                BinaryMapping.WriteObject(outStream, item);

            return outStream;
        }
    }
}

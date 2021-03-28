using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class ObjectCollision
    {
        [Data] public byte Group { get; set; }
        [Data] public byte Parts { get; set; }
        [Data] public short Argument { get; set; }
        [Data] public byte Type { get; set; }
        [Data] public byte Shape { get; set; }
        [Data] public short Bone { get; set; }
        [Data] public short PositionX { get; set; }
        [Data] public short PositionY { get; set; }
        [Data] public short PositionZ { get; set; }
        [Data] public short PositionHeight { get; set; }
        [Data] public short Radius { get; set; }
        [Data] public short Height { get; set; }

        public static List<ObjectCollision> Read(Stream stream)
        {
            var count = stream.ReadInt32();
            stream.Position += 0x40 - 4;
            return Enumerable
                .Range(0, count)
                .Select(x => BinaryMapping.ReadObject<ObjectCollision>(stream))
                .ToList();
        }

        public static void Write(Stream stream, ICollection<ObjectCollision> collisions)
        {
            stream.Write(collisions.Count);
            stream.Write(1);
            stream.Position += 0x40 - 8;

            foreach (var item in collisions)
                BinaryMapping.WriteObject(stream, item);
        }
    }
}

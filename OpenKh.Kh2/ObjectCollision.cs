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
        [Data] public TypeEnum Type { get; set; }
        [Data] public ShapeEnum Shape { get; set; }
        [Data] public short Bone { get; set; }
        [Data] public short PositionX { get; set; }
        [Data] public short PositionY { get; set; }
        [Data] public short PositionZ { get; set; }
        [Data] public short PositionHeight { get; set; }
        [Data] public short Radius { get; set; }
        [Data] public short Height { get; set; }

        public enum TypeEnum : byte
        {
            BG = 0x0,
            OBJ = 0x1,
            HIT = 0x2,
            TARGET = 0x3,
            BG_PLAYER = 0x4,
            REACTION = 0x5,
            ATTACK = 0x6,
            CAMERA = 0x7,
            CAST_ITEM = 0x8,
            ITEM = 0x9,
            IK = 0xa,
            IK_DOWN = 0xb,
            NECK = 0xc,
            GUARD = 0xd,
            REF_RC = 0xe,
            WEAPON_TOP = 0xf,
            STUN = 0x10,
            HEAD = 0x11,
            BLIND = 0x12,
            TALKCAMERA = 0x13,
            RTN_NECK = 0x14,
        }

        public enum ShapeEnum : byte
        {
            ELLIPSOID = 0x0,
            COLUMN = 0x1,
            CUBE = 0x2,
            SPHERE = 0x3,
        }

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

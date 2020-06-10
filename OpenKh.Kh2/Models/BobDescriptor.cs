using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Models
{
    public class BobDescriptor
    {
        private const int Identifier = 8;

        [Data] public float PositionX { get; set; }
        [Data] public float PositionY { get; set; }
        [Data] public float PositionZ { get; set; }
        [Data] public float RotationX { get; set; }
        [Data] public float RotationY { get; set; }
        [Data] public float RotationZ { get; set; }
        [Data] public float ScalingX { get; set; }
        [Data] public float ScalingY { get; set; }
        [Data] public float ScalingZ { get; set; }
        [Data] public int BobIndex { get; set; }
        [Data] public int Unknown28 { get; set; }
        [Data] public int Unknown2c { get; set; }
        [Data] public int Unknown30 { get; set; }
        [Data] public int Unknown34 { get; set; }
        [Data] public float Unknown38 { get; set; }
        [Data] public float Unknown3c { get; set; }
        [Data] public float Unknown40 { get; set; }
        [Data] public float Unknown44 { get; set; }
        [Data] public float Unknown48 { get; set; }
        [Data] public float Unknown4c { get; set; }
        [Data] public float Unknown50 { get; set; }
        [Data] public float Unknown54 { get; set; }
        [Data] public float Unknown58 { get; set; }
        [Data] public float Unknown5c { get; set; }
        [Data] public float Unknown60 { get; set; }
        [Data] public float Unknown64 { get; set; }

        public override string ToString() =>
            $"POS({PositionX:F0}, {PositionY:F0},{PositionZ:F0}) " +
            $"ROT({RotationX:F0}, {RotationY:F0},{RotationZ:F0}) " +
            $"SCA({ScalingX:F0}, {ScalingY:F0},{ScalingZ:F0}) " +
            $"IDX({BobIndex}";

        public static bool IsValid(Stream stream) =>
            stream.ReadInt32() == Identifier;

        public static List<BobDescriptor> Read(Stream stream)
        {
            var id = stream.ReadInt32();
            if (id != Identifier)
                throw new InvalidDataException($"Expected {Identifier} as identifier but found {id}");

            var entryCount = stream.ReadInt32() / 0x68;
            return Enumerable.Range(0, entryCount)
                .Select(_ => BinaryMapping.ReadObject<BobDescriptor>(stream))
                .ToList();
        }

        public static void Write(Stream stream, IEnumerable<BobDescriptor> bobDescriptors)
        {
            var items = bobDescriptors.ToList();

            stream.Write(8);
            stream.Write(items.Count * 0x68);
            foreach (var item in bobDescriptors)
                BinaryMapping.WriteObject(stream, item);
        }
    }
}

using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Ard
{
    public class SpawnPoint
    {
        public class Entity
        {
            [Data] public int ObjectId { get; set; }
            [Data] public float PositionX { get; set; }
            [Data] public float PositionY { get; set; }
            [Data] public float PositionZ { get; set; }
            [Data] public float RotationX { get; set; }
            [Data] public float RotationY { get; set; }
            [Data] public float RotationZ { get; set; }
            [Data] public short Unk1c { get; set; }
            [Data] public short Unk1e { get; set; }
            [Data] public int Unk20 { get; set; }
            [Data] public int Unk24 { get; set; }
            [Data] public int Unk28 { get; set; }
            [Data] public int Unk2c { get; set; }
            [Data] public int Unk30 { get; set; }
            [Data] public int Unk34 { get; set; }
            [Data] public int Unk38 { get; set; }
            [Data] public int Unk3c { get; set; }

            public static Entity Read(Stream stream) =>
                BinaryMapping.ReadObject<Entity>(stream);

            public static void Write(Stream stream, Entity entity) =>
                BinaryMapping.WriteObject(stream, entity);

            public override string ToString() =>
                $"ID {ObjectId} POS({PositionX:F0}, {PositionY:F0}, {PositionZ:F0}) ROT({RotationX:F0}, {RotationY:F0}, {RotationZ:F0}) " + 
                $"UNK {Unk1c:X} {Unk1e:X} {Unk20:X} {Unk24:X} {Unk28:X} {Unk2c:X} {Unk30:X} {Unk34:X} {Unk38:X} {Unk3c:X}";
        }

        [Data] public short Unk00 { get; set; }
        [Data] public short Unk02 { get; set; }
        [Data] public int EntityCount { get; set; }
        [Data] public int Unk08 { get; set; }
        [Data] public int Unk0c { get; set; }
        [Data] public int Unk10 { get; set; }
        [Data] public int Unk14 { get; set; }
        [Data] public int Unk18 { get; set; }
        [Data] public int Unk1c { get; set; }
        [Data] public int Unk20 { get; set; }
        [Data] public int Unk24 { get; set; }
        [Data] public int Unk28 { get; set; }

        public List<Entity> Entities { get; set; }

        public override string ToString() =>
            $"{Unk00:X} {Unk02:X}\n{string.Join("\n", Entities.Select(x => x.ToString()))}";

        public static List<SpawnPoint> Read(Stream stream)
        {
            var typeId = stream.ReadInt32();
            var itemCount = stream.ReadInt32();
            return Enumerable.Range(0, itemCount)
                .Select(x => ReadSingle(stream))
                .ToList();
        }

        public static void Write(Stream stream, List<SpawnPoint> items)
        {
            stream.Write(2);
            stream.Write(items.Count);
            foreach (var item in items)
            {
                item.EntityCount = item.Entities.Count;
                BinaryMapping.WriteObject(stream, item);
                foreach (var spawnPoint in item.Entities)
                    Entity.Write(stream, spawnPoint);
            }
        }

        private static SpawnPoint ReadSingle(Stream stream)
        {
            var spawn = BinaryMapping.ReadObject<SpawnPoint>(stream);
            spawn.Entities = Enumerable.Range(0, spawn.EntityCount)
                .Select(x => Entity.Read(stream))
                .ToList();

            return spawn;
        }
    }
}

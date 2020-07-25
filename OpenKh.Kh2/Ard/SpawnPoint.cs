using OpenKh.Common;
using System;
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

        public class Unknown08
        {
            [Data] public short Unk00 { get; set; }
            [Data] public short Count { get; set; }
            [Data] public short Unk04 { get; set; }
            [Data] public short Unk06 { get; set; }
            public List<Position> Positions { get; set; }

            public static Unknown08 Read(Stream stream)
            {
                var table = BinaryMapping.ReadObject<Unknown08>(stream);
                table.Positions = Enumerable.Range(0, table.Count)
                    .Select(_ => BinaryMapping.ReadObject<Position>(stream))
                    .ToList();

                return table;
            }

            public static void Write(Stream stream, Unknown08 entity)
            {
                BinaryMapping.WriteObject(stream, entity);
                foreach (var position in entity.Positions)
                    BinaryMapping.WriteObject(stream, position);
            }

            public override string ToString() => $"{Unk00:X} {Unk04:X} {Unk06:X}";
        }

        public class Position
        {
            [Data] public float X { get; set; }
            [Data] public float Y { get; set; }
            [Data] public float Z { get; set; }

            public override string ToString() => $"{X:F0} {Y:F0} {Z:F0}";
        }

        public class Unknown0a
        {
            [Data] public byte Unk00 { get; set; }
            [Data] public byte Unk01 { get; set; }
            [Data] public byte Unk02 { get; set; }
            [Data] public byte Unk03 { get; set; }
            [Data] public byte Unk04 { get; set; }
            [Data] public byte Unk05 { get; set; }
            [Data] public short Unk06 { get; set; }
            [Data] public int Unk08 { get; set; }
            [Data] public int Unk0c { get; set; }

            public static Unknown0a Read(Stream stream) =>
                BinaryMapping.ReadObject<Unknown0a>(stream);

            public static void Write(Stream stream, Unknown0a entity) =>
                BinaryMapping.WriteObject(stream, entity);
        }

        public class Unknown0c
        {
            [Data] public int Unk00 { get; set; }
            [Data] public int Unk04 { get; set; }

            public static Unknown0c Read(Stream stream) =>
                BinaryMapping.ReadObject<Unknown0c>(stream);

            public static void Write(Stream stream, Unknown0c entity) =>
                BinaryMapping.WriteObject(stream, entity);
        }

        // loader 00199AA0
        
        [Data] public short Unk00 { get; set; }
        [Data] public short Unk02 { get; set; }
        [Data] public short SpawnEntityGroupCount { get; set; }
        [Data] public short EntityGroup2Count { get; set; }
        [Data] public short Unk08Count { get; set; }
        [Data] public short Unk0aCount { get; set; }
        [Data] public int Unk0cCount { get; set; }
        [Data] public int Unk10 { get; set; }
        [Data] public int Unk14 { get; set; }
        [Data] public int Unk18 { get; set; }
        [Data] public int Unk1c { get; set; }
        [Data] public int Unk20 { get; set; }
        [Data] public int Unk24 { get; set; }
        [Data] public int Unk28 { get; set; }

        public List<Entity> SpawnEntiyGroup { get; set; }
        public List<Entity> EntityGroup2 { get; set; }
        public List<Unknown08> Unknown08Table { get; set; }
        public List<Unknown0a> Unknown0aTable { get; set; }
        public List<Unknown0c> Unknown0cTable { get; set; }

        public override string ToString() =>
            $"{Unk00:X} {Unk02:X}\n{string.Join("\n", SpawnEntiyGroup.Select(x => x.ToString()))}";

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
                item.SpawnEntityGroupCount = (short)item.SpawnEntiyGroup.Count;
                item.EntityGroup2Count = (short)item.EntityGroup2.Count;
                item.Unk08Count = (short)item.Unknown08Table.Count;
                item.Unk0aCount = (short)item.Unknown0aTable.Count;
                item.Unk0cCount = (short)item.Unknown0cTable.Count;
                item.Unk10 = 0;
                item.Unk14 = 0;
                item.Unk18 = 0;
                item.Unk28 = 0;

                BinaryMapping.WriteObject(stream, item);
                foreach (var spawnPoint in item.SpawnEntiyGroup)
                    Entity.Write(stream, spawnPoint);
                foreach (var spawnPoint in item.EntityGroup2)
                    Entity.Write(stream, spawnPoint);
                foreach (var unk in item.Unknown08Table)
                    Unknown08.Write(stream, unk);
                foreach (var unk in item.Unknown0aTable)
                    Unknown0a.Write(stream, unk);
                foreach (var unk in item.Unknown0cTable)
                    Unknown0c.Write(stream, unk);
            }
        }

        private static SpawnPoint ReadSingle(Stream stream)
        {
            var spawn = BinaryMapping.ReadObject<SpawnPoint>(stream);
            spawn.SpawnEntiyGroup = ReadList(stream, spawn.SpawnEntityGroupCount, Entity.Read);
            spawn.EntityGroup2 = ReadList(stream, spawn.EntityGroup2Count, Entity.Read);
            spawn.Unknown08Table = ReadList(stream, spawn.Unk08Count, Unknown08.Read);
            spawn.Unknown0aTable = ReadList(stream, spawn.Unk0aCount, Unknown0a.Read);
            spawn.Unknown0cTable = ReadList(stream, spawn.Unk0cCount, Unknown0c.Read);

            return spawn;
        }

        private static List<T> ReadList<T>(Stream stream, int count, Func<Stream, T> reader) =>
            Enumerable.Range(0, count).Select(x => reader(stream)).ToList();
    }
}

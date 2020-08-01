using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;
using Xe.IO;

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
            [Data] public int TalkMessage { get; set; }
            [Data] public int ReactionCommand { get; set; }
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
                $"UNK {Unk1c:X} {Unk1e:X} {Unk20:X} {Unk24:X} {TalkMessage:X} {ReactionCommand:X} {Unk30:X} {Unk34:X} {Unk38:X} {Unk3c:X}";
        }

        public class Unknown08
        {
            private class Raw
            {
                [Data] public short Unk00 { get; set; }
                [Data] public short Count { get; set; }
                [Data] public short Unk04 { get; set; }
                [Data] public short Unk06 { get; set; }
            }

            public short Unk00 { get; set; }
            public short Unk04 { get; set; }
            public short Unk06 { get; set; }
            public List<Position> Positions { get; set; }

            public static Unknown08 Read(Stream stream)
            {
                var header = BinaryMapping.ReadObject<Raw>(stream);
                return new Unknown08
                {
                    Unk00 = header.Unk00,
                    Unk04 = header.Unk04,
                    Unk06 = header.Unk06,
                    Positions = Enumerable.Range(0, header.Count)
                    .Select(_ => BinaryMapping.ReadObject<Position>(stream))
                    .ToList()
                };
            }

            public static void Write(Stream stream, Unknown08 entity)
            {
                BinaryMapping.WriteObject(stream, new Raw
                {
                    Unk00 = entity.Unk00,
                    Count = (short)entity.Positions.Count,
                    Unk04 = entity.Unk04,
                    Unk06 = entity.Unk06,
                });
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
        
        private class Raw
        {
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
        }

        public short Unk00 { get; set; }
        public short Unk02 { get; set; }
        public int Unk1c { get; set; }
        public int Unk20 { get; set; }
        public int Unk24 { get; set; }

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
                BinaryMapping.WriteObject(stream, new Raw
                {
                    Unk00 = item.Unk00,
                    Unk02 = item.Unk02,
                    SpawnEntityGroupCount = (short)item.SpawnEntiyGroup.Count,
                    EntityGroup2Count = (short)item.EntityGroup2.Count,
                    Unk08Count = (short)item.Unknown08Table.Count,
                    Unk0aCount = (short)item.Unknown0aTable.Count,
                    Unk0cCount = (short)item.Unknown0cTable.Count,
                    Unk10 = 0,
                    Unk14 = 0,
                    Unk18 = 0,
                    Unk1c = item.Unk1c,
                    Unk20 = item.Unk20,
                    Unk24 = item.Unk24,
                    Unk28 = 0,
                });

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
            var raw = BinaryMapping.ReadObject<Raw>(stream);
            var spawn = new SpawnPoint
            {
                Unk00 = raw.Unk00,
                Unk02 = raw.Unk02,
                Unk1c = raw.Unk1c,
                Unk20 = raw.Unk20,
                Unk24 = raw.Unk24,
            };

            spawn.SpawnEntiyGroup = ReadList(stream, raw.SpawnEntityGroupCount, Entity.Read);
            spawn.EntityGroup2 = ReadList(stream, raw.EntityGroup2Count, Entity.Read);
            spawn.Unknown08Table = ReadList(stream, raw.Unk08Count, Unknown08.Read);
            spawn.Unknown0aTable = ReadList(stream, raw.Unk0aCount, Unknown0a.Read);
            spawn.Unknown0cTable = ReadList(stream, raw.Unk0cCount, Unknown0c.Read);

            return spawn;
        }

        private static List<T> ReadList<T>(Stream stream, int count, Func<Stream, T> reader) =>
            Enumerable.Range(0, count).Select(x => reader(stream)).ToList();
    }
}

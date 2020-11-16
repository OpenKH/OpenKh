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
            private class Raw
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
                [Data] public int AiParameter { get; set; }
                [Data] public int TalkMessage { get; set; }
                [Data] public int ReactionCommand { get; set; }
                [Data] public int Unk30 { get; set; }
                [Data] public int Unk34 { get; set; }
                [Data] public int Unk38 { get; set; }
                [Data] public int Unk3c { get; set; }
            }

            public int ObjectId { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float PositionZ { get; set; }
            public float RotationX { get; set; }
            public float RotationY { get; set; }
            public float RotationZ { get; set; }
            public short Unk1c { get; set; }
            public short Unk1e { get; set; }
            public int Unk20 { get; set; }
            public int AiParameter { get; set; }
            public int TalkMessage { get; set; }
            public int ReactionCommand { get; set; }
            public int Unk30 { get; set; }

            public static Entity Read(Stream stream)
            {
                var raw = BinaryMapping.ReadObject<Raw>(stream);
                return new Entity
                {
                    ObjectId = raw.ObjectId,
                    PositionX = raw.PositionX,
                    PositionY = raw.PositionY,
                    PositionZ = raw.PositionZ,
                    RotationX = raw.RotationX,
                    RotationY = raw.RotationY,
                    RotationZ = raw.RotationZ,
                    Unk1c = raw.Unk1c,
                    Unk1e = raw.Unk1e,
                    Unk20 = raw.Unk20,
                    AiParameter = raw.AiParameter,
                    TalkMessage = raw.TalkMessage,
                    ReactionCommand = raw.ReactionCommand,
                    Unk30 = raw.Unk30,
                };
            }

            public static void Write(Stream stream, Entity entity) =>
                BinaryMapping.WriteObject(stream, new Raw
                {
                    ObjectId = entity.ObjectId,
                    PositionX = entity.PositionX,
                    PositionY = entity.PositionY,
                    PositionZ = entity.PositionZ,
                    RotationX = entity.RotationX,
                    RotationY = entity.RotationY,
                    RotationZ = entity.RotationZ,
                    Unk1c = entity.Unk1c,
                    Unk1e = entity.Unk1e,
                    Unk20 = entity.Unk20,
                    AiParameter = entity.AiParameter,
                    TalkMessage = entity.TalkMessage,
                    ReactionCommand = entity.ReactionCommand,
                    Unk30 = entity.Unk30,
                });

            public override string ToString() =>
                $"ID {ObjectId} POS({PositionX:F0}, {PositionY:F0}, {PositionZ:F0}) ROT({RotationX:F0}, {RotationY:F0}, {RotationZ:F0}) " +
                $"UNK {Unk1c:X} {Unk1e:X} {Unk20:X} {AiParameter:X} {TalkMessage:X} {ReactionCommand:X} {Unk30:X}";
        }

        public class EventActivator
        {
            private class Raw
            {
                [Data] public int Unk00 { get; set; }
                [Data] public float PositionX { get; set; }
                [Data] public float PositionY { get; set; }
                [Data] public float PositionZ { get; set; }
                [Data] public float ScaleX { get; set; }
                [Data] public float ScaleY { get; set; }
                [Data] public float ScaleZ { get; set; }
                [Data] public float RotationX { get; set; }
                [Data] public float RotationY { get; set; }
                [Data] public float RotationZ { get; set; }
                [Data] public int Unk28 { get; set; }
                [Data] public int Unk2c { get; set; }
                [Data] public int Unk30 { get; set; }
                [Data] public int Unk34 { get; set; }
                [Data] public int Unk38 { get; set; }
                [Data] public int Unk3c { get; set; }
            }

            public int Unk00 { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float PositionZ { get; set; }
            public float ScaleX { get; set; }
            public float ScaleY { get; set; }
            public float ScaleZ { get; set; }
            public float RotationX { get; set; }
            public float RotationY { get; set; }
            public float RotationZ { get; set; }
            public int Unk28 { get; set; }
            public int Unk2c { get; set; }

            public static EventActivator Read(Stream stream)
            {
                var raw = BinaryMapping.ReadObject<Raw>(stream);
                return new EventActivator
                {
                    Unk00 = raw.Unk00,
                    PositionX = raw.PositionX,
                    PositionY = raw.PositionY,
                    PositionZ = raw.PositionZ,
                    ScaleX = raw.ScaleX,
                    ScaleY = raw.ScaleY,
                    ScaleZ = raw.ScaleZ,
                    RotationX = raw.RotationX,
                    RotationY = raw.RotationY,
                    RotationZ = raw.RotationZ,
                    Unk28 = raw.Unk28,
                    Unk2c = raw.Unk2c,
                };
            }

            public static void Write(Stream stream, EventActivator unk) =>
                BinaryMapping.WriteObject(stream, new Raw
                {
                    Unk00 = unk.Unk00,
                    PositionX = unk.PositionX,
                    PositionY = unk.PositionY,
                    PositionZ = unk.PositionZ,
                    ScaleX = unk.ScaleX,
                    ScaleY = unk.ScaleY,
                    ScaleZ = unk.ScaleZ,
                    RotationX = unk.RotationX,
                    RotationY = unk.RotationY,
                    RotationZ = unk.RotationZ,
                    Unk28 = unk.Unk28,
                    Unk2c = unk.Unk2c,
                });

            public override string ToString() =>
                $"ID {Unk00} POS({PositionX:F0}, {PositionY:F0}, {PositionZ:F0}) SCL({ScaleX:F0}, {ScaleY:F0}, {ScaleZ:F0}) ROT({RotationX:F0}, {RotationY:F0}, {RotationZ:F0}) " +
                $"UNK {RotationX:X} {RotationY:F0} {RotationZ:X} {Unk28:X} {Unk2c:X}";
        }

        public class WalkPathDesc
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

            public static WalkPathDesc Read(Stream stream)
            {
                var header = BinaryMapping.ReadObject<Raw>(stream);
                return new WalkPathDesc
                {
                    Unk00 = header.Unk00,
                    Unk04 = header.Unk04,
                    Unk06 = header.Unk06,
                    Positions = Enumerable.Range(0, header.Count)
                    .Select(_ => BinaryMapping.ReadObject<Position>(stream))
                    .ToList()
                };
            }

            public static void Write(Stream stream, WalkPathDesc entity)
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
            [Data] public short EntityCount { get; set; }
            [Data] public short EventActivatorCount { get; set; }
            [Data] public short Unk08Count { get; set; }
            [Data] public short Unk0aCount { get; set; }
            [Data] public int Unk0cCount { get; set; }
            [Data] public int Unk10 { get; set; }
            [Data] public int Unk14 { get; set; }
            [Data] public int Unk18 { get; set; }
            [Data] public byte Place { get; set; }
            [Data] public byte Door { get; set; }
            [Data] public byte World { get; set; }
            [Data] public byte Unk1f { get; set; }
            [Data] public int Unk20 { get; set; }
            [Data] public int Unk24 { get; set; }
            [Data] public int Unk28 { get; set; }
        }

        public class TeleportDesc
        {
            public byte Place { get; set; }
            public byte Door { get; set; }
            public byte World { get; set; }
            public byte Unknown { get; set; }
        }

        public short Unk00 { get; set; }
        public short Unk02 { get; set; }
        public TeleportDesc Teleport { get; set; }
        public int Unk20 { get; set; }
        public int Unk24 { get; set; }

        public List<Entity> Entities { get; set; }
        public List<EventActivator> EventActivators { get; set; }
        public List<WalkPathDesc> WalkPath { get; set; }
        public List<Unknown0a> Unknown0aTable { get; set; }
        public List<Unknown0c> Unknown0cTable { get; set; }

        public override string ToString() =>
            $"{Unk00:X} {Unk02:X}\n{string.Join("\n", Entities.Select(x => x.ToString()))}";

        public static List<SpawnPoint> Read(Stream stream)
        {
            var typeId = stream.ReadInt32();
            var itemCount = stream.ReadInt32();
            if (itemCount <= 0)
                return new List<SpawnPoint>();
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
                    EntityCount = (short)item.Entities.Count,
                    EventActivatorCount = (short)item.EventActivators.Count,
                    Unk08Count = (short)item.WalkPath.Count,
                    Unk0aCount = (short)item.Unknown0aTable.Count,
                    Unk0cCount = (short)item.Unknown0cTable.Count,
                    Unk10 = 0,
                    Unk14 = 0,
                    Unk18 = 0,
                    Place = item.Teleport.Place,
                    Door = item.Teleport.Door,
                    World = item.Teleport.World,
                    Unk1f = item.Teleport.Unknown,
                    Unk20 = item.Unk20,
                    Unk24 = item.Unk24,
                    Unk28 = 0,
                });

                foreach (var spawnPoint in item.Entities)
                    Entity.Write(stream, spawnPoint);
                foreach (var unk in item.EventActivators)
                    EventActivator.Write(stream, unk);
                foreach (var unk in item.WalkPath)
                    WalkPathDesc.Write(stream, unk);
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
                Teleport = new TeleportDesc
                {
                    Place = raw.Place,
                    Door = raw.Door,
                    World = raw.World,
                    Unknown = raw.Unk1f
                },
                Unk20 = raw.Unk20,
                Unk24 = raw.Unk24,
            };

            spawn.Entities = ReadList(stream, raw.EntityCount, Entity.Read);
            spawn.EventActivators = ReadList(stream, raw.EventActivatorCount, EventActivator.Read);
            spawn.WalkPath = ReadList(stream, raw.Unk08Count, WalkPathDesc.Read);
            spawn.Unknown0aTable = ReadList(stream, raw.Unk0aCount, Unknown0a.Read);
            spawn.Unknown0cTable = ReadList(stream, raw.Unk0cCount, Unknown0c.Read);

            return spawn;
        }

        private static List<T> ReadList<T>(Stream stream, int count, Func<Stream, T> reader) =>
            Enumerable.Range(0, count).Select(x => reader(stream)).ToList();
    }
}

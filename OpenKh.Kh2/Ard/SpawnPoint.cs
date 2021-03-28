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
                [Data] public byte SpawnType { get; set; }
                [Data] public byte SpawnArgument { get; set; }
                [Data] public short Serial { get; set; }
                [Data] public int Argument1 { get; set; }
                [Data] public int Argument2 { get; set; }
                [Data] public short ReactionCommand { get; set; }
                [Data] public short SpawnDelay { get; set; }
                [Data] public short Command { get; set; }
                [Data] public short SpawnRange { get; set; }
                [Data] public byte Level { get; set; }
                [Data] public byte Medal { get; set; }
                [Data] public short Reserved32 { get; set; }
                [Data] public int Reserved34 { get; set; }
                [Data] public int Reserved38 { get; set; }
                [Data] public int Reserved3c { get; set; }
            }

            public int ObjectId { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float PositionZ { get; set; }
            public float RotationX { get; set; }
            public float RotationY { get; set; }
            public float RotationZ { get; set; }
            public byte SpawnType { get; set; }
            public byte SpawnArgument { get; set; }
            public short Serial { get; set; }
            public int Argument1 { get; set; }
            public int Argument2 { get; set; }
            public short ReactionCommand { get; set; }
            public short SpawnDelay { get; set; }
            public short Command { get; set; }
            public short SpawnRange { get; set; }
            public byte Level { get; set; }
            public byte Medal { get; set; }

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
                    SpawnType = raw.SpawnType,
                    SpawnArgument = raw.SpawnArgument,
                    Serial = raw.Serial,
                    Argument1 = raw.Argument1,
                    Argument2 = raw.Argument2,
                    ReactionCommand = raw.ReactionCommand,
                    SpawnDelay = raw.SpawnDelay,
                    Command = raw.Command,
                    SpawnRange = raw.SpawnRange,
                    Level = raw.Level,
                    Medal = raw.Medal,
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
                    SpawnType = entity.SpawnType,
                    SpawnArgument = entity.SpawnArgument,
                    Serial = entity.Serial,
                    Argument1 = entity.Argument1,
                    Argument2 = entity.Argument2,
                    ReactionCommand = entity.ReactionCommand,
                    SpawnDelay = entity.SpawnDelay,
                    Command = entity.Command,
                    SpawnRange = entity.SpawnRange,
                    Level = entity.Level,
                    Medal = entity.Medal,
                });

            public override string ToString() =>
                $"ID {ObjectId} POS({PositionX:F0}, {PositionY:F0}, {PositionZ:F0}) ROT({RotationX:F0}, {RotationY:F0}, {RotationZ:F0})";
        }

        public class EventActivator
        {
            private class Raw
            {
                [Data] public short Shape { get; set; }
                [Data] public short Option { get; set; }
                [Data] public float PositionX { get; set; }
                [Data] public float PositionY { get; set; }
                [Data] public float PositionZ { get; set; }
                [Data] public float ScaleX { get; set; }
                [Data] public float ScaleY { get; set; }
                [Data] public float ScaleZ { get; set; }
                [Data] public float RotationX { get; set; }
                [Data] public float RotationY { get; set; }
                [Data] public float RotationZ { get; set; }
                [Data] public int Flags { get; set; }
                [Data] public short Type { get; set; }
                [Data] public byte OnBgGroup { get; set; }
                [Data] public byte OffBgGroup { get; set; }
                [Data] public int Padding30 { get; set; }
                [Data] public int Padding34 { get; set; }
                [Data] public int Padding38 { get; set; }
                [Data] public int Padding3c { get; set; }
            }

            public short Shape { get; set; }
            public short Option { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float PositionZ { get; set; }
            public float ScaleX { get; set; }
            public float ScaleY { get; set; }
            public float ScaleZ { get; set; }
            public float RotationX { get; set; }
            public float RotationY { get; set; }
            public float RotationZ { get; set; }
            public int Flags { get; set; }
            public short Type { get; set; }
            public byte OnBgGroup { get; set; }
            public byte OffBgGroup { get; set; }

            public static EventActivator Read(Stream stream)
            {
                var raw = BinaryMapping.ReadObject<Raw>(stream);
                return new EventActivator
                {
                    Shape = raw.Shape,
                    Option = raw.Option,
                    PositionX = raw.PositionX,
                    PositionY = raw.PositionY,
                    PositionZ = raw.PositionZ,
                    ScaleX = raw.ScaleX,
                    ScaleY = raw.ScaleY,
                    ScaleZ = raw.ScaleZ,
                    RotationX = raw.RotationX,
                    RotationY = raw.RotationY,
                    RotationZ = raw.RotationZ,
                    Flags = raw.Flags,
                    Type = raw.Type,
                    OnBgGroup = raw.OnBgGroup,
                    OffBgGroup = raw.OffBgGroup,
                };
            }

            public static void Write(Stream stream, EventActivator activator) =>
                BinaryMapping.WriteObject(stream, new Raw
                {
                    Shape = activator.Shape,
                    Option = activator.Option,
                    PositionX = activator.PositionX,
                    PositionY = activator.PositionY,
                    PositionZ = activator.PositionZ,
                    ScaleX = activator.ScaleX,
                    ScaleY = activator.ScaleY,
                    ScaleZ = activator.ScaleZ,
                    RotationX = activator.RotationX,
                    RotationY = activator.RotationY,
                    RotationZ = activator.RotationZ,
                    Flags = activator.Flags,
                    Type = activator.Type,
                    OnBgGroup = activator.OnBgGroup,
                    OffBgGroup = activator.OffBgGroup,
                });

            public override string ToString() =>
                $"Shape {Shape} Option {Option} POS({PositionX:F0}, {PositionY:F0}, {PositionZ:F0}) SCL({ScaleX:F0}, {ScaleY:F0}, {ScaleZ:F0}) ROT({RotationX:F0}, {RotationY:F0}, {RotationZ:F0}) " +
                $"Flags {Flags:X}, Type {Type}, OnBg {OnBgGroup:X}, OffBg {OffBgGroup:X}";
        }

        public class WalkPathDesc
        {
            private class Raw
            {
                [Data] public short Serial { get; set; }
                [Data] public short Count { get; set; }
                [Data] public byte Flag { get; set; }
                [Data] public byte Id { get; set; }
                [Data] public short Reserved { get; set; }
            }

            public short Serial { get; set; }
            public byte Flag { get; set; }
            public byte Id { get; set; }
            public List<Position> Positions { get; set; }

            public static WalkPathDesc Read(Stream stream)
            {
                var header = BinaryMapping.ReadObject<Raw>(stream);
                return new WalkPathDesc
                {
                    Serial = header.Serial,
                    Flag = header.Flag,
                    Id = header.Id,
                    Positions = Enumerable.Range(0, header.Count)
                        .Select(_ => BinaryMapping.ReadObject<Position>(stream))
                        .ToList()
                };
            }

            public static void Write(Stream stream, WalkPathDesc entity)
            {
                BinaryMapping.WriteObject(stream, new Raw
                {
                    Serial = entity.Serial,
                    Flag = entity.Flag,
                    Count = (short)entity.Positions.Count,
                    Id = entity.Id,
                });

                foreach (var position in entity.Positions)
                    BinaryMapping.WriteObject(stream, position);
            }

            public override string ToString() => $"Serial {Serial:X} Flag {Flag:X} Id {Id:X}";
        }

        public class Position
        {
            [Data] public float X { get; set; }
            [Data] public float Y { get; set; }
            [Data] public float Z { get; set; }

            public override string ToString() => $"{X:F0} {Y:F0} {Z:F0}";
        }

        public class ReturnParameter
        {
            [Data] public byte Id { get; set; }
            [Data] public byte Type { get; set; }
            [Data] public byte Rate { get; set; }
            [Data] public byte EntryType { get; set; }
            [Data] public int Argument04 { get; set; }
            [Data] public int Argument08 { get; set; }
            [Data] public int Argument0c { get; set; }

            public static ReturnParameter Read(Stream stream) =>
                BinaryMapping.ReadObject<ReturnParameter>(stream);

            public static void Write(Stream stream, ReturnParameter entity) =>
                BinaryMapping.WriteObject(stream, entity);
        }

        public class Signal
        {
            [Data] public ushort SignalId { get; set; }
            [Data] public ushort Argument { get; set; }
            [Data] public byte Action { get; set; }
            [Data] public byte Padding05 { get; set; }
            [Data] public byte Padding06 { get; set; }
            [Data] public byte Padding07 { get; set; }

            public static Signal Read(Stream stream) =>
                BinaryMapping.ReadObject<Signal>(stream);

            public static void Write(Stream stream, Signal entity) =>
                BinaryMapping.WriteObject(stream, entity);
        }

        private class Raw
        {
            [Data] public byte Type { get; set; }
            [Data] public byte Flag { get; set; }
            [Data] public short Id { get; set; }
            [Data] public short EntityCount { get; set; }
            [Data] public short EventActivatorCount { get; set; }
            [Data] public short WalkPathCount { get; set; }
            [Data] public short ReturnParameterCount { get; set; }
            [Data] public short SignalCount { get; set; }
            [Data] public short Reserved0e { get; set; }
            [Data] public int Reserved10 { get; set; }
            [Data] public int Reserved14 { get; set; }
            [Data] public int Reserved18 { get; set; }
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

        public byte Type { get; set; }
        public byte Flag { get; set; }
        public short Id { get; set; }
        public TeleportDesc Teleport { get; set; }
        public int Unk20 { get; set; }
        public int Unk24 { get; set; }

        public List<Entity> Entities { get; set; }
        public List<EventActivator> EventActivators { get; set; }
        public List<WalkPathDesc> WalkPath { get; set; }
        public List<ReturnParameter> ReturnParameters { get; set; }
        public List<Signal> Signals { get; set; }

        public override string ToString() =>
            $"Type {Type:X} Flag {Flag:X} {Id:X}\n{string.Join("\n", Entities.Select(x => x.ToString()))}";

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
                    Type = item.Type,
                    Flag = item.Flag,
                    Id = item.Id,
                    EntityCount = (short)item.Entities.Count,
                    EventActivatorCount = (short)item.EventActivators.Count,
                    WalkPathCount = (short)item.WalkPath.Count,
                    ReturnParameterCount = (short)item.ReturnParameters.Count,
                    SignalCount = (short)item.Signals.Count,
                    Reserved10 = 0,
                    Reserved14 = 0,
                    Reserved18 = 0,
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
                foreach (var unk in item.ReturnParameters)
                    ReturnParameter.Write(stream, unk);
                foreach (var unk in item.Signals)
                    Signal.Write(stream, unk);
            }
        }

        private static SpawnPoint ReadSingle(Stream stream)
        {
            var raw = BinaryMapping.ReadObject<Raw>(stream);
            var spawn = new SpawnPoint
            {
                Type = raw.Type,
                Flag = raw.Flag,
                Id = raw.Id,
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
            spawn.WalkPath = ReadList(stream, raw.WalkPathCount, WalkPathDesc.Read);
            spawn.ReturnParameters = ReadList(stream, raw.ReturnParameterCount, ReturnParameter.Read);
            spawn.Signals = ReadList(stream, raw.SignalCount, Signal.Read);

            return spawn;
        }

        private static List<T> ReadList<T>(Stream stream, int count, Func<Stream, T> reader) =>
            Enumerable.Range(0, count).Select(x => reader(stream)).ToList();
    }
}

using OpenKh.Common;
using OpenKh.Common.Ps2;
using OpenKh.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;
using Xe.IO;

namespace OpenKh.Kh2
{
    public abstract class Model
    {
        protected enum Type : int
        {
            Multiple = -1,
            Unknown = 0,
            Legacy = 1,
            Background = 2,
            Skeleton = 3,
        }

        private const int ReservedArea = 0x90;

        public abstract int GroupCount { get; }
        public int Flags { get; protected set; }

        public virtual int GetDrawPolygonCount(IList<byte> displayFlags) => 0;

        public void Write(Stream stream)
        {
            var tempStream = new MemoryStream();
            InternalWrite(tempStream);

            stream.Position = ReservedArea;
            stream.Write(tempStream.GetBuffer(), 0, (int)tempStream.Length);
        }

        protected abstract void InternalWrite(Stream stream);

        public static Model Read(Stream stream)
        {
            var type = ReadModelType(stream);
            stream.SetPosition(ReservedArea);
            var subStream = new SubStream(stream, ReservedArea, stream.Length - ReservedArea);

            return type switch
            {
                Type.Multiple => new ModelMultiple(subStream),
                Type.Legacy => new ModelLegacy(subStream),
                Type.Background => new ModelBackground(subStream),
                Type.Skeleton => new ModelSkeleton(subStream),
                _ => throw new Exception($"Model type {type} not recognized")
            };
        }

        private static Type ReadModelType(Stream stream) =>
            (Type)stream.SetPosition(ReservedArea).ReadInt32();

        protected static List<T> For<T>(int count, Func<T> func) =>
            Enumerable.Range(0, count).Select(_ => func()).ToList();

        protected static IEnumerable<ushort> ReadUInt16List(Stream stream)
        {
            while (true)
            {
                var data = stream.ReadUInt16();
                if (data == 0xFFFF)
                    break;
                yield return data;
            }
        }

        protected static void WriteUInt16List(Stream stream, IEnumerable<ushort> alb2t2)
        {
            foreach (var data in alb2t2)
                stream.Write(data);
            stream.Write((ushort)0xFFFF);
        }
    }
}

using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    
    public class Pmp
    {
        private static readonly IBinaryMapping Mapping =
                                MappingConfiguration.DefaultConfiguration()
                                .ForType<Vector3>(x => new Vector3(
                                x.Reader.ReadSingle(),
                                x.Reader.ReadSingle(),
                                x.Reader.ReadSingle()),
                                x =>
                                {
                                    var vector = (Vector3)x.Item;
                                    x.Writer.Write(vector.X);
                                    x.Writer.Write(vector.Y);
                                    x.Writer.Write(vector.Z);
                                })
                                .Build();

        private const UInt32 MagicCode = 0x504D50;

        public class Header
        {
            [Data] public UInt32 MagicCode { get; set; }
            [Data(Count = 3)] public int[] Unk1 { get; set; }
            [Data] public ushort ObjectCount { get; set; }
            [Data] public ushort Unk2 { get; set; }
            [Data] public uint Unk3 { get; set; }
            [Data] public ushort Unk4 { get; set; }
            [Data] public ushort TextureCount { get; set; }
            [Data] public uint TextureListOffset { get; set; }
        }

        public class ObjectInfo
        {
            [Data] public Vector3 Position { get; set; }
            [Data] public Vector3 Rotation { get; set; }
            [Data] public Vector3 Scale { get; set; }
            [Data] public uint PMO_Offset { get; set; }
            [Data] public uint Unk1 { get; set; }
            [Data] public ushort ObjectFlags { get; set; }
            // Unknown use.
            [Data] public ushort _ObjectID { get; set; }
        }

        public Header header = new Header();
        public List<ObjectInfo> objectInfo = new List<ObjectInfo>();
        public List<Pmo> PmoList = new List<Pmo>();

        public static Pmp Read(Stream stream)
        {
            Pmp pmp = new Pmp();
            pmp.header = BinaryMapping.ReadObject<Header>(stream);

            for(int i = 0; i < pmp.header.ObjectCount; i++)
            {
                pmp.objectInfo.Add(Mapping.ReadObject<ObjectInfo>(stream));
            }

            for(int p = 0; p < pmp.header.ObjectCount; p++)
            {
                if(pmp.objectInfo[p].PMO_Offset != 0)
                {
                    stream.Seek(pmp.objectInfo[p].PMO_Offset, SeekOrigin.Begin);
                    pmp.PmoList.Add(Pmo.Read(stream));
                }
            }

            return pmp;
        }

        public static bool IsValid(Stream stream) =>
           stream.Length >= 0x20 &&
           stream.SetPosition(0).ReadUInt32() == MagicCode;
    }
}

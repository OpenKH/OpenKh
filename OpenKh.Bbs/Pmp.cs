using Microsoft.VisualBasic.CompilerServices;
using OpenKh.Common;
using OpenKh.Common.Utils;
using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
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
            [Data] public ushort Version { get; set; }
            [Data(Count = 2)] public int[] Padding { get; set; }
            [Data] public byte Padding2 { get; set; }
            [Data] public byte MapFlag { get; set; }
            [Data] public ushort ObjectCount { get; set; }
            [Data] public ushort ModelCount { get; set; }
            [Data] public uint Padding3 { get; set; }
            [Data] public ushort Padding4 { get; set; }
            [Data] public ushort TextureCount { get; set; }
            [Data] public uint TextureListOffset { get; set; }
        }

        public enum MapFlags
        {
            FLAG_NONE = 0,
            FLAG_DISPOFF = 1,
            FLAG_PRESETOFF = 2,
            FLAG_SYSPRESETOFF = 4
        }

        public class ObjectInfo
        {
            [Data] public float PositionX { get; set; }
            [Data] public float PositionY { get; set; }
            [Data] public float PositionZ { get; set; }
            [Data] public float RotationX { get; set; }
            [Data] public float RotationY { get; set; }
            [Data] public float RotationZ { get; set; }
            [Data] public float ScaleX { get; set; }
            [Data] public float ScaleY { get; set; }
            [Data] public float ScaleZ { get; set; }
            [Data] public uint PMO_Offset { get; set; }
            [Data] public uint Unk1 { get; set; }
            [Data] public ushort ObjectFlags { get; set; }
            [Data] public ushort ObjectID { get; set; }
        }

        public class PMPTextureInfo
        {
            [Data] public uint Offset { get; set; }
            [Data(Count = 12)] public string TextureName { get; set; }
            [Data] public float AnimateUV_X { get; set; }
            [Data] public float AnimateUV_Y { get; set; }
            [Data(Count = 2)] public uint[] Unknown { get; set; }
        }

        public Header header = new Header();
        public List<ObjectInfo> objectInfo = new List<ObjectInfo>();
        public List<Pmo> PmoList = new List<Pmo>();
        public List<Tm2> TextureDataList = new List<Tm2>();
        public List<PMPTextureInfo> TextureList = new List<PMPTextureInfo>();
        public List<bool> hasDifferentMatrix = new List<bool>();

        public static Pmp Read(Stream stream)
        {
            Pmp pmp = new Pmp();
            pmp.header = BinaryMapping.ReadObject<Header>(stream);

            // Read Object List.
            for (int i = 0; i < pmp.header.ObjectCount; i++)
            {
                pmp.objectInfo.Add(Mapping.ReadObject<ObjectInfo>(stream));
                pmp.hasDifferentMatrix.Add(BitsUtil.Int.GetBit(pmp.objectInfo[i].ObjectFlags, 0));
            }

            // Read PMO list.
            for (int p = 0; p < pmp.header.ObjectCount; p++)
            {
                ObjectInfo currentPmoInfo = pmp.objectInfo[p];
                if (currentPmoInfo.PMO_Offset != 0)
                {
                    stream.Seek(currentPmoInfo.PMO_Offset, SeekOrigin.Begin);
                    pmp.PmoList.Add(Pmo.Read(stream));
                }
            }

            stream.Seek(pmp.header.TextureListOffset, SeekOrigin.Begin);

            for (int t = 0; t < pmp.header.TextureCount; t++)
            {
                pmp.TextureList.Add(BinaryMapping.ReadObject<PMPTextureInfo>(stream));
            }

            // Read textures.
            for (int k = 0; k < pmp.TextureList.Count; k++)
            {
                stream.Seek(pmp.TextureList[k].Offset + 0x10, SeekOrigin.Begin);
                uint tm2size = stream.ReadUInt32() + 0x10;
                stream.Seek(pmp.TextureList[k].Offset, SeekOrigin.Begin);

                pmp.TextureDataList.Add(Tm2.Read(stream, true).First());
            }

            return pmp;
        }

        public static void Write(Stream stream, Pmp pmp)
        {
            stream.Position = 0;
            BinaryMapping.WriteObject<Header>(stream, pmp.header);

            for (int i = 0; i < pmp.objectInfo.Count; i++)
            {
                BinaryMapping.WriteObject<ObjectInfo>(stream, pmp.objectInfo[i]);
            }

            List<Pmo> nPmoList = pmp.PmoList;
            nPmoList.Sort((l, r) => l.PMO_StartPosition.CompareTo(r.PMO_StartPosition));

            for (int p = 0; p < nPmoList.Count; p++)
            {
                BinaryMapping.WriteObject<Pmo.Header>(stream, nPmoList[p].header);

                for (int g = 0; g < nPmoList[p].textureInfo.Length; g++)
                {
                    BinaryMapping.WriteObject<Pmo.TextureInfo>(stream, nPmoList[p].textureInfo[g]);
                }

                Pmo.WriteMeshData(stream, nPmoList[p]);
            }

            for (int tl = 0; tl < pmp.TextureList.Count; tl++)
            {
                BinaryMapping.WriteObject<PMPTextureInfo>(stream, pmp.TextureList[tl]);
            }

            for (int td = 0; td < pmp.TextureList.Count; td++)
            {
                List<Tm2> l = new List<Tm2>();
                l.Add(pmp.TextureDataList[td]);
                Tm2.Write(stream, l);
            }
        }

        public static bool IsValid(Stream stream) =>
           stream.Length >= 0x20 &&
           stream.SetPosition(0).ReadUInt32() == MagicCode;
    }
}

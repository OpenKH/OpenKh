using OpenKh.Common;
using OpenKh.Common.Utils;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xe.BinaryMapper;

namespace OpenKh.Ddd
{
    public class PmpDdd
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

            public override string ToString()
            {
                Vector3 scale = new Vector3(ScaleX, ScaleY, ScaleZ);
                Vector3 rotation = new Vector3(RotationX, RotationY, RotationZ);
                Vector3 translation = new Vector3(PositionX, PositionY, PositionZ);
                return "S " + scale + " R " + rotation + " T " + translation;
            }

            public Matrix4x4 GetTransformationMatrix()
            {
                Vector3 scale = new Vector3(ScaleX, ScaleY, ScaleZ);
                Vector3 translation = new Vector3(PositionX, PositionY, PositionZ);
                Matrix4x4 scaleMatrix = Matrix4x4.CreateScale(scale);
                Matrix4x4 rotationMatrix;
                Matrix4x4 rotationMatrixX = Matrix4x4.CreateRotationX(RotationX);
                Matrix4x4 rotationMatrixY = Matrix4x4.CreateRotationY(RotationY);
                Matrix4x4 rotationMatrixZ = Matrix4x4.CreateRotationZ(RotationZ);
                rotationMatrix = rotationMatrixZ * rotationMatrixY * rotationMatrixX;
                Matrix4x4 translationMatrix = Matrix4x4.CreateTranslation(translation);

                Matrix4x4 AbsoluteTransformationMatrix = scaleMatrix * rotationMatrix * translationMatrix;
                return AbsoluteTransformationMatrix;
            }

            public bool ShouldMirrorFaces()
            {
                int mirrorCount = 0;
                if(ScaleX < 0) mirrorCount++;
                if(ScaleY < 0) mirrorCount++;
                if(ScaleZ < 0) mirrorCount++;

                return (mirrorCount % 2 != 0);
            }
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
        public List<PmoV4_2> PmoList = new List<PmoV4_2>();
        //public List<Tm2> TextureDataList = new List<Tm2>();
        public List<PMPTextureInfo> TextureList = new List<PMPTextureInfo>();
        public List<bool> hasDifferentMatrix = new List<bool>();

        public static PmpDdd Read(Stream stream)
        {
            PmpDdd pmp = new PmpDdd();
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
                    pmp.PmoList.Add(PmoV4_2.Read(stream, true));
                }
                else
                {
                    pmp.PmoList.Add(null);
                }
            }

            stream.Seek(pmp.header.TextureListOffset, SeekOrigin.Begin);

            for (int t = 0; t < pmp.header.TextureCount; t++)
            {
                pmp.TextureList.Add(BinaryMapping.ReadObject<PMPTextureInfo>(stream));
            }

            // Read textures
            for (int k = 0; k < pmp.TextureList.Count; k++)
            {
                //stream.Seek(pmp.TextureList[k].Offset + 0x10, SeekOrigin.Begin);
                //uint tm2size = stream.ReadUInt32() + 0x10;
                //stream.Seek(pmp.TextureList[k].Offset, SeekOrigin.Begin);
                //
                //pmp.TextureDataList.Add(Tm2.Read(stream, true).First());
            }

            return pmp;
        }

        public static PmpDdd Read(string filepath)
        {
            PmpDdd thisPmp = new PmpDdd();
            using (FileStream stream = new FileStream(filepath, FileMode.Open))
            {
                thisPmp = Read(stream);
            }
            return thisPmp;
        }

        public static bool IsValid(Stream stream) =>
           stream.Length >= 0x20 &&
           stream.SetPosition(0).ReadUInt32() == MagicCode;
    }
}

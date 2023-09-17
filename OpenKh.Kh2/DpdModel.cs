using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class DpdModel
    {
        public byte[] Unknown { get; set; } // Structure is unknown so the binary is stored for now
        public DpdModelHeader Header { get; set; }
        public OmdHeader OmdHeade { get; set; }
        public List<OmdPrimitive> OmdPrimitives { get; set; }
        public List<DpdVector> UnknownVectors1 { get; set; }
        public List<DpdVector> UnknownVectors2 { get; set; }


        public DpdModel(Stream modelStream)
        {
            long initialPosition = modelStream.Position;

            // Actual reading - Complete structure is not yet known
            Header = BinaryMapping.ReadObject<DpdModelHeader>(modelStream);
            OmdHeade = BinaryMapping.ReadObject<OmdHeader>(modelStream);

            // Then comes a list of OmdPrimitives. The first 2 bytes are read and if they are 0xFFFF (-1) there are no more OmdPrimitives
            // Each OmdPrimitive contains a list of primitives. The primitive stored depends on the type
            // Example: Sora's DPD 2 model 14 has multiple OmdPrimitives
            // After the list comes what seems to be a padding to 16 bytes, however Sora's DPD 2 model 13 has 0x0101

            // Then comes a DpdVector list (Which may be a normal vector list) As many as OmdHeader.VCount
            // Then comes another DpdVector list of size unknown since it doesn't match OmdHeader.NCount
            // End of model, padding to 16 bytes

            // File as bytes to be able to write it back
            modelStream.Position = initialPosition;
            Unknown = modelStream.ReadAllBytes();

            modelStream.Position = initialPosition;
        }

        public Stream getAsStream()
        {
            Stream fileStream = new MemoryStream(Unknown);
            fileStream.Position = 0;
            return fileStream;
        }

        public class DpdModelHeader // DPDMODEL
        {
            [Data] public short Flag { get; set; }
            [Data] public short ShRsdNo { get; set; }
            [Data] public short ShTexpBase { get; set; }
            [Data] public short ShClutpBase { get; set; }
            [Data(Count = 2)] public byte[] AbRes0 { get; set; }
            [Data] public int NPacketSize { get; set; }
            [Data(Count = 4)] public byte[] AbRes1 { get; set; }
        }

        public class OmdHeader // OMD_H
        {
            [Data] public int Flag { get; set; }
            [Data] public int PAddress { get; set; } // Primitives (vertices)
            [Data] public int VAddress { get; set; } // Normals
            [Data] public int NAddress { get; set; }
            [Data] public short PCount { get; set; } // Sum of all primitives on all OmdPrimitives
            [Data] public short VCount { get; set; }
            [Data] public short NCount { get; set; }
            [Data] public byte Alpha { get; set; }
            [Data] public byte AlphaFix { get; set; }
            [Data] public short Res1 { get; set; }
            [Data] public short Res2 { get; set; }
            [Data] public int Vna { get; set; }
        }

        public class OmdPrimitive
        {
            public OmdPrimitiveHeader Header { get; set; }
            // List<Primitives> | Depends on Header Type. As many as PCount

            public class OmdPrimitiveHeader // OMD_PRIM_H
            {
                [Data] public byte Flag { get; set; }
                [Data] public byte Type { get; set; }
                [Data] public short PCount { get; set; }
                [Data] public byte TNo { get; set; }
                [Data] public byte TimPosX { get; set; }
                [Data] public byte Res1 { get; set; }
                [Data] public byte Res2 { get; set; }
                [Data] public uint Tex0L { get; set; }
                [Data] public uint Tex0H { get; set; }
            }

            public class Primitive0 // OMD_G3
            {
                [Data] public byte r0 { get; set; }
                [Data] public byte g0 { get; set; }
                [Data] public byte b0 { get; set; }
                [Data] public byte a0 { get; set; }
                [Data] public byte r1 { get; set; }
                [Data] public byte g1 { get; set; }
                [Data] public byte b1 { get; set; }
                [Data] public byte a1 { get; set; }
                [Data] public byte r2 { get; set; }
                [Data] public byte g2 { get; set; }
                [Data] public byte b2 { get; set; }
                [Data] public byte a2 { get; set; }
                [Data] public ushort v0 { get; set; }
                [Data] public ushort v1 { get; set; }
                [Data] public ushort v2 { get; set; }
                [Data] public ushort flag { get; set; }
            }
            public class Primitive1 // OMD_G3L
            {
                [Data] public byte r0 { get; set; }
                [Data] public byte g0 { get; set; }
                [Data] public byte b0 { get; set; }
                [Data] public byte a0 { get; set; }
                [Data] public byte r1 { get; set; }
                [Data] public byte g1 { get; set; }
                [Data] public byte b1 { get; set; }
                [Data] public byte a1 { get; set; }
                [Data] public byte r2 { get; set; }
                [Data] public byte g2 { get; set; }
                [Data] public byte b2 { get; set; }
                [Data] public byte a2 { get; set; }
                [Data] public ushort v0 { get; set; }
                [Data] public ushort v1 { get; set; }
                [Data] public ushort v2 { get; set; }
                [Data] public ushort flag { get; set; }
                [Data] public ushort n0 { get; set; }
                [Data] public ushort n1 { get; set; }
                [Data] public ushort n2 { get; set; }
                [Data] public ushort res { get; set; }
            }
            public class Primitive2 // OMD_GT3
            {
                [Data] public byte r0 { get; set; }
                [Data] public byte g0 { get; set; }
                [Data] public byte b0 { get; set; }
                [Data] public byte a0 { get; set; }
                [Data] public byte r1 { get; set; }
                [Data] public byte g1 { get; set; }
                [Data] public byte b1 { get; set; }
                [Data] public byte a1 { get; set; }
                [Data] public byte r2 { get; set; }
                [Data] public byte g2 { get; set; }
                [Data] public byte b2 { get; set; }
                [Data] public byte a2 { get; set; }
                [Data] public ushort v0 { get; set; }
                [Data] public ushort v1 { get; set; }
                [Data] public ushort v2 { get; set; }
                [Data] public ushort flag { get; set; }
                [Data] public ushort s0 { get; set; }
                [Data] public ushort t0 { get; set; }
                [Data] public ushort s1 { get; set; }
                [Data] public ushort t1 { get; set; }
                [Data] public ushort s2 { get; set; }
                [Data] public ushort t2 { get; set; }
            }
            public class Primitive3 // OMD_GT3L
            {
                [Data] public byte r0 { get; set; }
                [Data] public byte g0 { get; set; }
                [Data] public byte b0 { get; set; }
                [Data] public byte a0 { get; set; }
                [Data] public byte r1 { get; set; }
                [Data] public byte g1 { get; set; }
                [Data] public byte b1 { get; set; }
                [Data] public byte a1 { get; set; }
                [Data] public byte r2 { get; set; }
                [Data] public byte g2 { get; set; }
                [Data] public byte b2 { get; set; }
                [Data] public byte a2 { get; set; }
                [Data] public ushort v0 { get; set; }
                [Data] public ushort v1 { get; set; }
                [Data] public ushort v2 { get; set; }
                [Data] public ushort flag { get; set; }
                [Data] public ushort s0 { get; set; }
                [Data] public ushort t0 { get; set; }
                [Data] public ushort s1 { get; set; }
                [Data] public ushort t1 { get; set; }
                [Data] public ushort s2 { get; set; }
                [Data] public ushort t2 { get; set; }
                [Data] public ushort n0 { get; set; }
                [Data] public ushort n1 { get; set; }
                [Data] public ushort n2 { get; set; }
                [Data] public ushort res { get; set; }
            }
            public class Primitive4 // OMD_G4
            {
                [Data] public byte r0 { get; set; }
                [Data] public byte g0 { get; set; }
                [Data] public byte b0 { get; set; }
                [Data] public byte a0 { get; set; }
                [Data] public byte r1 { get; set; }
                [Data] public byte g1 { get; set; }
                [Data] public byte b1 { get; set; }
                [Data] public byte a1 { get; set; }
                [Data] public byte r2 { get; set; }
                [Data] public byte g2 { get; set; }
                [Data] public byte b2 { get; set; }
                [Data] public byte a2 { get; set; }
                [Data] public byte r3 { get; set; }
                [Data] public byte g3 { get; set; }
                [Data] public byte b3 { get; set; }
                [Data] public byte a3 { get; set; }
                [Data] public ushort v0 { get; set; }
                [Data] public ushort v1 { get; set; }
                [Data] public ushort v2 { get; set; }
                [Data] public ushort v3 { get; set; }
            }
            public class Primitive5 // OMD_G4L
            {
                [Data] public byte r0 { get; set; }
                [Data] public byte g0 { get; set; }
                [Data] public byte b0 { get; set; }
                [Data] public byte a0 { get; set; }
                [Data] public byte r1 { get; set; }
                [Data] public byte g1 { get; set; }
                [Data] public byte b1 { get; set; }
                [Data] public byte a1 { get; set; }
                [Data] public byte r2 { get; set; }
                [Data] public byte g2 { get; set; }
                [Data] public byte b2 { get; set; }
                [Data] public byte a2 { get; set; }
                [Data] public byte r3 { get; set; }
                [Data] public byte g3 { get; set; }
                [Data] public byte b3 { get; set; }
                [Data] public byte a3 { get; set; }
                [Data] public ushort v0 { get; set; }
                [Data] public ushort v1 { get; set; }
                [Data] public ushort v2 { get; set; }
                [Data] public ushort v3 { get; set; }
                [Data] public ushort n0 { get; set; }
                [Data] public ushort n1 { get; set; }
                [Data] public ushort n2 { get; set; }
                [Data] public ushort n3 { get; set; }
            }
            public class Primitive6 // OMD_GT4
            {
                [Data] public byte r0 { get; set; }
                [Data] public byte g0 { get; set; }
                [Data] public byte b0 { get; set; }
                [Data] public byte a0 { get; set; }
                [Data] public byte r1 { get; set; }
                [Data] public byte g1 { get; set; }
                [Data] public byte b1 { get; set; }
                [Data] public byte a1 { get; set; }
                [Data] public byte r2 { get; set; }
                [Data] public byte g2 { get; set; }
                [Data] public byte b2 { get; set; }
                [Data] public byte a2 { get; set; }
                [Data] public byte r3 { get; set; }
                [Data] public byte g3 { get; set; }
                [Data] public byte b3 { get; set; }
                [Data] public byte a3 { get; set; }
                [Data] public ushort v0 { get; set; }
                [Data] public ushort v1 { get; set; }
                [Data] public ushort v2 { get; set; }
                [Data] public ushort v3 { get; set; }
                [Data] public ushort s0 { get; set; }
                [Data] public ushort t0 { get; set; }
                [Data] public ushort s1 { get; set; }
                [Data] public ushort t1 { get; set; }
                [Data] public ushort s2 { get; set; }
                [Data] public ushort t2 { get; set; }
                [Data] public ushort s3 { get; set; }
                [Data] public ushort t3 { get; set; }
            }
            public class Primitive7 // OMD_GT4L
            {
                [Data] public byte r0 { get; set; }
                [Data] public byte g0 { get; set; }
                [Data] public byte b0 { get; set; }
                [Data] public byte a0 { get; set; }
                [Data] public byte r1 { get; set; }
                [Data] public byte g1 { get; set; }
                [Data] public byte b1 { get; set; }
                [Data] public byte a1 { get; set; }
                [Data] public byte r2 { get; set; }
                [Data] public byte g2 { get; set; }
                [Data] public byte b2 { get; set; }
                [Data] public byte a2 { get; set; }
                [Data] public byte r3 { get; set; }
                [Data] public byte g3 { get; set; }
                [Data] public byte b3 { get; set; }
                [Data] public byte a3 { get; set; }
                [Data] public ushort v0 { get; set; }
                [Data] public ushort v1 { get; set; }
                [Data] public ushort v2 { get; set; }
                [Data] public ushort v3 { get; set; }
                [Data] public ushort s0 { get; set; }
                [Data] public ushort t0 { get; set; }
                [Data] public ushort s1 { get; set; }
                [Data] public ushort t1 { get; set; }
                [Data] public ushort s2 { get; set; }
                [Data] public ushort t2 { get; set; }
                [Data] public ushort s3 { get; set; }
                [Data] public ushort t3 { get; set; }
                [Data] public ushort n0 { get; set; }
                [Data] public ushort n1 { get; set; }
                [Data] public ushort n2 { get; set; }
                [Data] public ushort n3 { get; set; }
            }
        }

        public class DpdVector // SVECTOR
        {
            [Data] public short X { get; set; }
            [Data] public short Y { get; set; }
            [Data] public short Z { get; set; }
        }
    }
}

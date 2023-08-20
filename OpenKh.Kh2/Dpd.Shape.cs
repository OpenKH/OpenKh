using OpenKh.Kh2.Utils;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public partial class Dpd
    {
        public class Shape
        {
            [Data] public PacketHeader PackHeader { get; set; }
            [Data] public AnmHeader AnimHeader { get; set; }
            public List<AnmSeq> AnmSeqs { get; set; }

            public Shape()
            {

            }

            internal Shape(Stream shapeStream)
            {
                PackHeader = BinaryMapping.ReadObject<PacketHeader>(shapeStream);
                AnimHeader = BinaryMapping.ReadObject<AnmHeader>(shapeStream);

                AnmSeqs = new List<AnmSeq>();
                for(int i = 0; i < AnimHeader.ShCount; i++)
                {
                    AnmSeqs.Add(BinaryMapping.ReadObject<AnmSeq>(shapeStream));
                }
                if(shapeStream.Position % 16 != 0)
                {
                    shapeStream.Position += 8;
                }

                //foreach(AnmSeq sequence in AnmSeqs)
                for(int i = 0; i < AnmSeqs.Count; i++)
                {
                    AnmSeq sequence = AnmSeqs[i];
                    sequence.shapeVu = BinaryMapping.ReadObject<ShapeVu1>(shapeStream);
                    sequence.sprites1R = new List<SpriteVu1R>();
                    sequence.sprites1 = new List<SpriteVu1>();
                    sequence.sprites1A = new List<SpriteVu1A>();

                    for (int j = 0; j < sequence.shapeVu.Counter; j++)
                    {
                        if (sequence.shapeVu.isType1) // First byte in the flag
                        {
                            sequence.sprites1R.Add(BinaryMapping.ReadObject<SpriteVu1R>(shapeStream));
                        }
                        else
                        {
                            sequence.sprites1.Add(BinaryMapping.ReadObject<SpriteVu1>(shapeStream));
                        }
                    }
                    for (int j = 0; j < sequence.shapeVu.Counter; j++)
                    {
                        sequence.sprites1A.Add(BinaryMapping.ReadObject<SpriteVu1A>(shapeStream));
                    }
                    if (shapeStream.Position % 16 != 0)
                    {
                        shapeStream.Position += 8;
                    }
                }
            }

            public Stream getAsStream()
            {
                Stream fileStream = new MemoryStream();

                long POINTER_shapeSize = 30;

                bool headerRequiresPadding = AnmSeqs.Count % 2 != 0;

                BinaryWriter writer = new BinaryWriter(fileStream);

                BinaryMapping.WriteObject(fileStream, PackHeader); // PacketSize not taken into account, we used the same it started with
                BinaryMapping.WriteObject(fileStream, AnimHeader);

                List<long> POINTER_addresses = new List<long>();
                List<long> addresses = new List<long>();
                foreach(AnmSeq seq in AnmSeqs)
                {
                    POINTER_addresses.Add(fileStream.Position);
                    BinaryMapping.WriteObject(fileStream, seq);
                }
                if (headerRequiresPadding)
                {
                    ReadWriteUtils.alignStreamToByte(fileStream, 16);
                }

                foreach (AnmSeq seq in AnmSeqs)
                {
                    addresses.Add(fileStream.Position - 16); // First header not included
                    BinaryMapping.WriteObject(fileStream, seq.shapeVu);
                    if (seq.shapeVu.isType1)
                    {
                        foreach(SpriteVu1R sprite in seq.sprites1R)
                        {
                            BinaryMapping.WriteObject(fileStream, sprite);
                        }
                    }
                    else
                    {
                        foreach(SpriteVu1 sprite in seq.sprites1)
                        {
                            BinaryMapping.WriteObject(fileStream, sprite);
                        }
                    }
                    foreach(SpriteVu1A sprite in seq.sprites1A)
                    {
                        BinaryMapping.WriteObject(fileStream, sprite);
                    }
                    ReadWriteUtils.alignStreamToByte(fileStream, 16);
                }

                // Shape size
                long shapeSize = fileStream.Length - 16; // First header not included
                fileStream.Position = POINTER_shapeSize;
                writer.Write((int)shapeSize);

                // Addresses
                for(int i = 0; i < POINTER_addresses.Count; i++)
                {
                    fileStream.Position = POINTER_addresses[i];
                    writer.Write((short)addresses[i]);
                }

                fileStream.Position = 0;
                return fileStream;
            }

            public class PacketHeader
            {
                [Data] public short ShShpNumber { get; set; }
                [Data] public short ShTexpBase { get; set; }
                [Data] public short ShClutpBase { get; set; }
                [Data] public byte AbRes0_1 { get; set; }
                [Data] public byte AbRes0_2 { get; set; }
                [Data] public int PacketSize { get; set; } // 0x0160 for AnmSeq flag 0x08, 0x00F0 for others
                [Data(Count = 4)] public byte[] AbRes1 { get; set; }
            }
            public class AnmHeader
            {
                [Data] public byte Id { get; set; }
                [Data] public byte Version { get; set; }
                [Data] public short Flag { get; set; }
                [Data] public short ShCount { get; set; }
                [Data] public short SeqCount { get; set; }
                [Data] public short X { get; set; }
                [Data] public short Y { get; set; }
                [Data] public short Res { get; set; }
                [Data] public ushort Size { get; set; } // Size of this header + anything below in bytes
            }
            // VV Size: 8 bytes VV
            public class AnmSeq
            {
                [Data] public short Address { get; set; }
                [Data] public short Wait { get; set; }
                [Data] public byte Flag { get; set; }
                [Data(Count = 3)] public byte[] Res { get; set; }
                public ShapeVu1 shapeVu { get; set; }
                public List<SpriteVu1> sprites1 { get; set; }
                public List<SpriteVu1R> sprites1R { get; set; }
                public List<SpriteVu1A> sprites1A { get; set; }

            }
            // VV Size: 64 bytes VV
            public class ShapeVu1
            {
                [Data] public short Flag { get; set; }
                [Data] public short Counter { get; set; }
                [Data] public int SpVula { get; set; }
                [Data] public byte TimPosX { get; set; }
                [Data(Count = 7)] public byte[] Res { get; set; }
                [Data] public int Tex0_L { get; set; }
                [Data] public int Tex0_H { get; set; }
                [Data(Count = 2)] public int[] Res2 { get; set; }
                public bool isType1 { get { return (Flag & 8) == 8; } }
            }
            public class SpriteVu1
            {
                [Data] public short X0 { get; set; }
                [Data] public short Y0 { get; set; }
                [Data] public short X3 { get; set; }
                [Data] public short Y3 { get; set; }
                [Data] public short S { get; set; }
                [Data] public short T { get; set; }
                [Data] public short W { get; set; }
                [Data] public short H { get; set; }
            }
            public class SpriteVu1R
            {
                [Data] public short X0 { get; set; }
                [Data] public short Y0 { get; set; }
                [Data] public short X1 { get; set; }
                [Data] public short Y1 { get; set; }
                [Data] public short X2 { get; set; }
                [Data] public short Y2 { get; set; }
                [Data] public short X3 { get; set; }
                [Data] public short Y3 { get; set; }
                [Data] public short S { get; set; }
                [Data] public short T { get; set; }
                [Data] public short W { get; set; }
                [Data] public short H { get; set; }
            }
            public class SpriteVu1A
            {
                [Data] public byte A { get; set; }
                [Data] public byte B { get; set; }
                [Data] public byte G { get; set; }
                [Data] public byte R { get; set; }
                [Data] public byte Alpha { get; set; }
                [Data] public byte AlphaFix { get; set; }
                [Data] public byte AlphaCode { get; set; }
                [Data] public byte Pa1 { get; set; }
            }
        }
    }
}

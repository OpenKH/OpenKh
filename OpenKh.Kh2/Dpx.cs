using OpenKh.Common;
using OpenKh.Kh2.Utils;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public partial class Dpx
    {
        public DpxHeader Header { get; set; }
        public List<DpxParticleEffect> ParticleEffects { get; set; }
        public List<Dpd> DpdList { get; set; }

        public class DpxHeader
        {
            [Data] public int DpxVersion { get; set; }
            [Data] public int Reserve1 { get; set; }
            [Data] public int Reserve2 { get; set; }
            [Data] public int ParticleEffectCount { get; set; }
        }

        public class DpxParticleEffect
        {
            [Data] public uint DpdOffset { get; set; } // Identifies the associated DPD
            [Data] public uint ParticleDataId { get; set; } // Particle Data to be used inside of the DPD
            [Data] public uint EffectNumber { get; set; } // Identifier
            [Data] public int Reserve1 { get; set; }
            [Data] public uint CategoryFlag1 { get; set; }
            [Data] public uint CategoryFlag2 { get; set; }
            [Data] public ulong CategoryFlag3 { get; set; }
            public int DpdId { get; set; } // Based on offset
            public override string ToString()
            {
                return "Effect number: " + EffectNumber + " | DPD Id: " + DpdId + " | DPD Offset: " + DpdOffset + " | ParticleData Id: " + ParticleDataId ;
            }
        }


        public Dpx(Stream stream)
        {
            long basePosition = stream.Position;

            Header = BinaryMapping.ReadObject<DpxHeader>(stream);

            ParticleEffects = new List<DpxParticleEffect>();
            for (int i = 0; i < Header.ParticleEffectCount; i++)
            {
                ParticleEffects.Add(BinaryMapping.ReadObject<DpxParticleEffect>(stream));
            }

            // DPD offsets
            List<int> dpdOffsets = new List<int>();
            int dpdOffsetsCount = stream.ReadInt32();
            for (int i = 0; i < dpdOffsetsCount; i++)
            {
                dpdOffsets.Add(stream.ReadInt32());
            }
            foreach(DpxParticleEffect effect in ParticleEffects)
            {
                effect.DpdId = dpdOffsets.IndexOf((int)effect.DpdOffset);
            }
            // Then there's a padding to 16 bytes, 4 x 16 0 bytes, 2 x 16 FF bytes

            // DPD list
            DpdList = new List<Dpd>();
            if (dpdOffsets.Count == 0)
            {
                return;
            }

            stream.Position = basePosition + dpdOffsets[0];

            for (int i = 0; i < dpdOffsets.Count; i++)
            {
                long startOffset = basePosition + dpdOffsets[i];
                long nextOffset = 0;
                if(i + 1 < dpdOffsets.Count)
                {
                    nextOffset = basePosition + dpdOffsets[i + 1];
                }
                else
                {
                    nextOffset = basePosition + stream.Length;
                }

                // Load data to a stream
                stream.Position = dpdOffsets[i];
                byte[] data = stream.ReadBytes((int)(nextOffset - startOffset));
                MemoryStream dpdStream = new MemoryStream();
                dpdStream.Write(data, 0, data.Length);
                dpdStream.Position = 0;

                DpdList.Add(new Dpd(dpdStream));
            }
        }

        public Stream getAsStream()
        {
            Stream fileStream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(fileStream);

            Header.ParticleEffectCount = ParticleEffects.Count;
            BinaryMapping.WriteObject(fileStream, Header);

            long POINTER_particleEffectList = fileStream.Position;
            fileStream.Position += 32 * ParticleEffects.Count;

            writer.Write((int)DpdList.Count);
            long POINTER_dpdOffsets = fileStream.Position;
            List<long> dpdOffsets = new List<long>();
            for (int i = 0; i < DpdList.Count; i++)
            {
                writer.Write((int)0);
            }
            ReadWriteUtils.alignStreamToByte(fileStream, 16);
            ReadWriteUtils.addBytesToStream(fileStream, 64, 0x00);
            ReadWriteUtils.addBytesToStream(fileStream, 32, 0xFF);

            foreach(Dpd dpd in DpdList)
            {
                dpdOffsets.Add(fileStream.Position);
                writer.Write(((MemoryStream)dpd.getAsStream()).ToArray());
            }
            
            fileStream.Position = POINTER_dpdOffsets;
            foreach(long dpdOffset in dpdOffsets)
            {
                writer.Write((int)dpdOffset);
            }

            fileStream.Position = POINTER_particleEffectList;
            foreach (DpxParticleEffect dpxPE in ParticleEffects)
            {
                dpxPE.DpdOffset = (uint)dpdOffsets[dpxPE.DpdId];
                BinaryMapping.WriteObject(fileStream, dpxPE);
            }

            fileStream.Position = 0;
            return fileStream;
        }
    }
}

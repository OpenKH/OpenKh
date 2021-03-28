using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class Bep
    {
        private const uint MagicCode = 0x50454240;
        private const uint version = 2;

        public class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public uint version { get; set; }
            [Data] public uint BaseParametersCount { get; set; }
            [Data] public uint BaseParametersOffset { get; set; }
            [Data] public uint DisappearParametersCount { get; set; }
            [Data] public uint DisappearParametersOffset { get; set; }
        }

        public class BaseParameter
        {
            [Data] public ushort BattleLevel { get; set; }
            [Data] public ushort BaseAttack { get; set; }
            [Data] public ushort Defense { get; set; }
            [Data] public byte DamageCeiling { get; set; }
            [Data] public byte DamageFloor { get; set; }
            [Data] public uint BaseHP { get; set; }
            [Data] public uint BaseEXP { get; set; }
        }

        public class DisappearParameter
        {
            [Data] public ushort WorldID { get; set; }
            [Data] public ushort RoomID { get; set; }
            [Data] public float Distance { get; set; }
        }

        public Header header = new Header();
        public List<BaseParameter> baseParameters = new List<BaseParameter>();
        public List<DisappearParameter> disappearParameters = new List<DisappearParameter>();

        public static Bep Read(Stream stream)
        {
            Bep bep = new Bep();
            bep.header = BinaryMapping.ReadObject<Header>(stream);

            stream.Seek(bep.header.BaseParametersOffset, SeekOrigin.Begin);
            bep.baseParameters = new List<BaseParameter>();
            for (int c = 0; c < bep.header.BaseParametersCount; c++)
            {
                bep.baseParameters.Add(BinaryMapping.ReadObject<BaseParameter>(stream));
            }

            stream.Seek(bep.header.DisappearParametersOffset, SeekOrigin.Begin);
            bep.disappearParameters = new List<DisappearParameter>();
            for (int d = 0; d < bep.header.DisappearParametersCount; d++)
            {
                bep.disappearParameters.Add(BinaryMapping.ReadObject<DisappearParameter>(stream));
            }

            return bep;
        }

        public static void Write(Stream stream, Bep bep)
        {
            BinaryMapping.WriteObject<Header>(stream, bep.header);

            for (int c = 0; c < bep.disappearParameters.Count; c++)
            {
                BinaryMapping.WriteObject<BaseParameter>(stream, bep.baseParameters[c]);
            }

            for (int d = 0; d < bep.disappearParameters.Count; d++)
            {
                BinaryMapping.WriteObject<DisappearParameter>(stream, bep.disappearParameters[d]);
            }
        }
    }
}

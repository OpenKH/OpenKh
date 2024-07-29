using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Mixdata
{
    public class LeveLP
    {
        private const int MagicCode = 0x564C494D;

        [Data] public ushort Title { get; set; } //Names from Sys.bar like "Amateur Moogle"
        [Data] public ushort Stat { get; set; } //Another text ID.
        [Data] public short Enable { get; set; }
        [Data] public ushort Padding { get; set; }
        [Data] public int Exp { get; set; }

        public static List<LeveLP> Read(Stream stream)
        {
            var leveLPList = new List<LeveLP>();
            using (var reader = new BinaryReader(stream, System.Text.Encoding.Default, true))
            {
                int magicCode = reader.ReadInt32();
                int version = reader.ReadInt32();
                int count = reader.ReadInt32();
                reader.ReadInt32(); // Skip padding

                for (int i = 0; i < count; i++)
                {
                    var leveLP = new LeveLP
                    {
                        Title = reader.ReadUInt16(),
                        Stat = reader.ReadUInt16(),
                        Enable = reader.ReadInt16(),
                        Padding = reader.ReadUInt16(),
                        Exp = reader.ReadInt32()
                    };
                    leveLPList.Add(leveLP);
                }
            }
            return leveLPList;
        }
        public static void Write(Stream stream, List<LeveLP> leveLPList)
        {
            stream.Position = 0;
            using (var writer = new BinaryWriter(stream, System.Text.Encoding.Default, true))
            {
                writer.Write(MagicCode);
                writer.Write(2); // Version number, hardcoded for example
                writer.Write(leveLPList.Count);
                writer.Write(0); // Padding

                foreach (var leveLP in leveLPList)
                {
                    writer.Write(leveLP.Title);
                    writer.Write(leveLP.Stat);
                    writer.Write(leveLP.Enable);
                    writer.Write(leveLP.Padding);
                    writer.Write(leveLP.Exp);
                }
            }
        }
    }
}

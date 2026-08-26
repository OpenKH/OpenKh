using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public class PrtyFile
    {
        public int Count { get; set; }
        public int[] Offsets { get; set; }
        public Dictionary<int, Prty> UniqueEntries { get; set; }

        public static PrtyFile Read(Stream stream)
        {
            stream.Position = 0;
            var reader = new BinaryReader(stream, Encoding.Default, true);

            int count = reader.ReadInt32();

            var offsets = new int[count];
            for (int i = 0; i < count; i++)
                offsets[i] = reader.ReadInt32();

            var unique = new Dictionary<int, Prty>();

            foreach (var offset in offsets.Skip(1).Distinct())
            {
                stream.Position = offset;
                var entry = BinaryMapping.ReadObject<Prty>(stream);
                unique[offset] = entry;
            }

            return new PrtyFile
            {
                Count = count,
                Offsets = offsets,
                UniqueEntries = unique
            };
        }

        public void Write(Stream stream)
        {
            stream.SetLength(0);
            stream.Position = 0;

            var writer = new BinaryWriter(stream, Encoding.Default, true);

            writer.Write(Count);

            foreach (var offset in Offsets)
                writer.Write(offset);

            foreach (var kvp in UniqueEntries)
            {
                stream.Position = kvp.Key;
                BinaryMapping.WriteObject(stream, kvp.Value);
            }
        }
    }

    public class Prty
    {
        //Structure: 0x4 bytes for Count, then for Count, offsets pointing to different characters in the file. 
        //0x00000000 seems to be some filler data? Unknown what purpose it serves.
        //Offsets also repeat. This is because the index of an offset is used for the NeoMoveset of a character.
        //Unsure why, but worlds with different costumes (NM, WI, TR, etc.) need to use different NeoMoveset values even if they read the exact same values.
        //For Sora, NeoMoveset also is linked to the PTYA entry to use.
        [Data] public float WalkSpeed { get; set; }
        [Data] public float RunSpeed { get; set; }
        [Data] public float JumpHeight { get; set; }
        [Data] public float TurnSpeed { get; set; }
        [Data] public float HangHeight { get; set; }
        [Data] public float HangMargin { get; set; }
        [Data] public float StunTime { get; set; }
        [Data] public float MpChargeTime { get; set; }
        [Data] public float UpDownSpeed { get; set; }
        [Data] public float DashSpeed { get; set; }
        [Data] public float Acceleration { get; set; }
        [Data] public float Brake { get; set; }
        [Data] public float Subjective { get; set; }

        public class PrtyEntries
        {
            [Data] public Dictionary<int, Prty> Entries { get; set; }
        }
        public static readonly Dictionary<string, int> CharacterMap = new Dictionary<string, int>
        {
            {"Sora", 0},
            {"ValorForm", 1},
            {"WisdomForm", 2},
            {"MasterForm", 3},
            {"FinalForm", 4},
            {"AntiForm", 5},
            {"SoraLK", 6},
            {"SoraLM", 7},
            {"Donald", 8},
            {"DonaldLK", 9},
            {"DonaldLM", 10},
            {"Goofy", 11},
            {"GoofyLK", 12},
            {"Aladdin", 13},
            {"Auron", 14},
            {"Mulan", 15},
            {"Ping", 16},
            {"Tron", 17},
            {"Mickey", 18},
            {"Beast", 19},
            {"Jack", 20},
            {"Simba", 21},
            {"Sparrow", 22},
            {"Riku", 23},
            {"MagicCarpet", 24},
            {"LightCycle", 25},
            {"SoraDie", 26},
            {"Unknown1", 27},
            {"Unknown2", 28},
            {"GummiShip", 29},
            {"RedRocket", 30},
            {"Neverland", 31},
            {"Session", 32},
            {"LimitForm", 33}
        };

        //Read/Write doesn't currently work. 

        /*
        public static (List<Prty> Items, List<int> Offsets) Read(Stream stream)
        {
            return BaseTableOffsetWithPaddings<Prty>.ReadWithOffsets(stream);
        }

        public static void Write(Stream stream, IEnumerable<Prty> entries, List<int> originalOffsets)
        {
            BaseTableOffsetWithPaddings<Prty>.WriteWithOffsets(stream, entries, originalOffsets);
        }
        */
    }
}

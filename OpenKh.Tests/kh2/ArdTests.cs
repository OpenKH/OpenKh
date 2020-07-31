using OpenKh.Common;
using OpenKh.Kh2.Ard;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class ArdTests
    {
        public class SpawnScriptTests
        {
            const string FileName = "kh2/res/map.spawnscript";

            [Fact]
            public void ReadTest()
            {
                var spawns = File.OpenRead(FileName).Using(SpawnScript.Read);
                var strProgram = spawns.First(x => x.ProgramId == 6).ToString();

                Assert.Equal(31, spawns.Count);
            }

            [Fact]
            public void WriteTest() => File.OpenRead(FileName).Using(x => Helpers.AssertStream(x, stream =>
            {
                var outStream = new MemoryStream();
                SpawnScript.Write(outStream, SpawnScript.Read(stream));

                return outStream;
            }));

            [Theory]
            [InlineData("Spawn \"ABcd\"", SpawnScript.Operation.Spawn, 0x64634241)]
            [InlineData("MapOcclusion 0xffffffff 0x00000001", SpawnScript.Operation.MapOcclusion, -1, 1)]
            [InlineData("MultipleSpawn \"0123\" \"4567\"", SpawnScript.Operation.MultipleSpawn, 0x33323130, 0x37363534)]
            [InlineData("Set05 123", (SpawnScript.Operation)5, 123)]
            [InlineData("Set06 123", (SpawnScript.Operation)6, 123)]
            [InlineData("Set07 123", (SpawnScript.Operation)7, 123)]
            [InlineData("Party NO_FRIEND", (SpawnScript.Operation)15, 0)]
            [InlineData("Party DEFAULT", (SpawnScript.Operation)15, 1)]
            [InlineData("Party W_FRIEND", (SpawnScript.Operation)15, 2)]
            [InlineData("Party W_FRIEND_IN", (SpawnScript.Operation)15, 3)]
            [InlineData("Party W_FRIEND_FIX", (SpawnScript.Operation)15, 4)]
            [InlineData("Party W_FRIEND_ONLY", (SpawnScript.Operation)15, 5)]
            [InlineData("Party DONALD_ONLY", (SpawnScript.Operation)15, 6)]
            [InlineData("Bgm 123 456", SpawnScript.Operation.Bgm, 0x01c8007b)]
            [InlineData("BgmDefault", SpawnScript.Operation.Bgm, 0)]
            [InlineData("Mission 0x1234 \"OPENKH IS OUR MISSION!\"", SpawnScript.Operation.Mission, 0x1234,
                0x4E45504F, 0x4920484B, 1431248979, 1229791314, 1330205523, 8526, 0, 0)]
            [InlineData("BattleLevel 99", SpawnScript.Operation.BattleLevel, 99)]
            [InlineData("Unkff 0x1234567 0x1ccccccc", (SpawnScript.Operation)0xff, 0x1234567, 0x1ccccccc)]
            public void ParseScriptAsText(string expected, SpawnScript.Operation operation, params int[] parameters) =>
                Assert.Equal(expected, SpawnScriptParser.AsText(new SpawnScript.Function
                {
                    Opcode = operation,
                    Parameters = parameters.ToList()
                }));

            //[Fact]
            //public void ForAllFiles()
            //{
            //    File.WriteAllText("D:\\out.csv", "MAP,00,01,02,03,04,05,06,07,08,09,0a,0b,0c,0d,0e,0f,10,11,12,13,14,15,16,17,18,19,1a,1b,1c,1d,1e,1f\n");
            //    foreach (var fileName in Directory.GetFiles(@"D:\Hacking\KH2\export_fm\ard\", "*.ard", SearchOption.AllDirectories))
            //    {
            //        var map = Path.GetFileNameWithoutExtension(fileName);
            //        var bar = File.OpenRead(fileName).Using(Kh2.Bar.Read);
            //        foreach (var entry in bar.Where(x => x.Type == Kh2.Bar.EntryType.SpawnScript).OrderBy(x => x.Name))
            //        {
            //            ForSpawnScript($"{map}_{entry.Name}", SpawnScript.Read(entry.Stream));
            //        }

            //    }
            //}

            //private static void ForSpawnScript(string prefix, List<SpawnScript> spawnScript)
            //{
            //    var opcodeCount = new int[0x20];
            //    foreach (var script in spawnScript)
            //    {
            //        File.WriteAllText($"D:\\KH2FM_map_scripts\\{prefix}_{script.ProgramId:X04}.txt", script.ToString());
            //        foreach (var function in script.Functions)
            //            opcodeCount[(ushort)function.Opcode]++;
            //    }
            //    File.AppendAllText("D:\\out.csv", $"{prefix}," + string.Join(",", opcodeCount.Select(x => x.ToString())) + "\n");
            //}
        }

        public class SpawnPointTests
        {
            const string FileNameM00 = "kh2/res/m_00.spawnpoint";
            const string FileNameM10 = "kh2/res/m_10.spawnpoint";
            const string FileNameY73 = "kh2/res/y73_.spawnpoint";

            [Fact]
            public void ReadM00Test()
            {
                var spawns = File.OpenRead(FileNameM00).Using(SpawnPoint.Read);

                Assert.Equal(4, spawns.Count);
                Assert.Equal(3, spawns[0].SpawnEntityGroupCount);
                Assert.Equal(0x236, spawns[0].SpawnEntiyGroup[0].ObjectId);
            }

            [Theory]
            [InlineData(FileNameM00)]
            [InlineData(FileNameM10)]
            [InlineData(FileNameY73)]
            public void WriteTest(string fileName) =>
                File.OpenRead(fileName).Using(x => Helpers.AssertStream(x, stream =>
            {
                var outStream = new MemoryStream();
                SpawnPoint.Write(outStream, SpawnPoint.Read(stream));

                return outStream;
            }));
        }
    }
}

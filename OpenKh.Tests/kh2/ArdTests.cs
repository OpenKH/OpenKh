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
            [InlineData("$Unk03 123 \"666\"", (SpawnScript.Operation)3, 123, 0x363636)]
            [InlineData("$Set_01c6053c 123", (SpawnScript.Operation)4, 123)]
            [InlineData("$Set_0034ecd0 123", (SpawnScript.Operation)5, 123)]
            [InlineData("$Set_0034ecd8 123", (SpawnScript.Operation)6, 123)]
            [InlineData("$Set_0034ecdc 123", (SpawnScript.Operation)7, 123)]
            [InlineData("$Unk09 \"666\"", (SpawnScript.Operation)9, 0x363636)]
            [InlineData("Party NO_FRIEND", SpawnScript.Operation.Party, 0)]
            [InlineData("Party DEFAULT", SpawnScript.Operation.Party, 1)]
            [InlineData("Party W_FRIEND", SpawnScript.Operation.Party, 2)]
            [InlineData("Party W_FRIEND_IN", SpawnScript.Operation.Party, 3)]
            [InlineData("Party W_FRIEND_FIX", SpawnScript.Operation.Party, 4)]
            [InlineData("Party W_FRIEND_ONLY", SpawnScript.Operation.Party, 5)]
            [InlineData("Party DONALD_ONLY", SpawnScript.Operation.Party, 6)]
            [InlineData("Bgm 123 456", SpawnScript.Operation.Bgm, 0x01c8007b)]
            [InlineData("BgmDefault", SpawnScript.Operation.Bgm, 0)]
            [InlineData("$SetFlag_0034f240_To4", SpawnScript.Operation.SetFlag4)]
            [InlineData("Mission 0x1234 \"OPENKH IS OUR MISSION!\"", SpawnScript.Operation.Mission, 0x1234,
                0x4E45504F, 0x4920484B, 1431248979, 1229791314, 1330205523, 8526, 0, 0)]
            [InlineData("Layout \"OPENKH IS OUR MISSION!\"", SpawnScript.Operation.Layout,
                0x4E45504F, 0x4920484B, 1431248979, 1229791314, 1330205523, 8526, 0, 0)]
            [InlineData("$SetFlag_0034f240_To10", SpawnScript.Operation.SetFlag10)]
            [InlineData("BattleLevel 99", SpawnScript.Operation.BattleLevel, 99)]
            [InlineData("$Unk1f \"666\"", (SpawnScript.Operation)0x1f, 0x363636)]
            [InlineData("$Unkff 0x1234567 0x1ccccccc", (SpawnScript.Operation)0xff, 0x1234567, 0x1ccccccc)]
            public void ParseScriptAsText(string expected, SpawnScript.Operation operation, params int[] parameters) =>
                Assert.Equal(expected, SpawnScriptParser.AsText(new SpawnScript.Function
                {
                    Opcode = operation,
                    Parameters = parameters.ToList()
                }));

            [Theory]
            [InlineData("Cutscene \"666\" 1", 0x20000, 0x363636, 1)]
            [InlineData("GetItem 666", 0x10006, 0x29a)]
            [InlineData("GetItem 666 777", 0x20006, 0x29a, 0x309)]
            [InlineData("End", 7)]
            public void ParseScriptRunAsText(string expected, params int[] parameters)
            {
                var aaa = SpawnScriptParser.AsText(new SpawnScript.Function
                {
                    Opcode = SpawnScript.Operation.Run,
                    Parameters = new int[] { 0 }.Concat(parameters).ToList()
                });
                Assert.Equal($"\t{expected}", aaa.Split('\n').LastOrDefault());
            }

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
                Assert.Equal(3, spawns[0].SpawnEntiyGroup.Count);
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

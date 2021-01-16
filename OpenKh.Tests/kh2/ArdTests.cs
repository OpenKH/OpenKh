using OpenKh.Common;
using OpenKh.Kh2;
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
            private const string FileName = "kh2/res/map.spawnscript";
            private static readonly byte[] SampleScript = new byte[]
            {
                    // Program header
                    0x00, 0x00, 0x40, 0x00,

                    // Opcode AreaSettings
                    0x0C, 0x00, 0x0e, 0x00, 0x00, 0x00, 0xFF, 0xFF,

                    // ProgressFlag
                    0x02, 0x00, 0x0A, 0x38,

                    // Event
                    0x00, 0x00, 0x02, 0x00, 0x31, 0x31, 0x30, 0x00,

                    // Jump
                    0x01, 0x00, 0x00, 0x00, 0x01, 0x00, 0x0E, 0x00,
                    0x05, 0x00, 0x00, 0x00, 0x00, 0x00, 0x01, 0x00,

                    // Party menu
                    0x07, 0x00, 0x00, 0x00,

                    // Progress flag
                    0x02, 0x00, 0x34, 0x12,

                    // UnkAreaSet05
                    0x05, 0x00, 0x22, 0x11,

                    // AddInventory
                    0x06, 0x00, 0x02, 0x00,
                    0x9A, 0x02, 0x00, 0x00,
                    0x09, 0x03, 0x00, 0x00,

                    // End program
                    0xFF, 0xFF, 0x00, 0x00,
            };
            private const string SampleScriptDecompiled =
                "Program 0x00\n" +
                "AreaSettings 0 -1\n" +
                "\tSetProgressFlag 0x380A\n" +
                "\tSetEvent \"110\" Type 2\n" +
                "\tSetJump Type 1 World NM Area 5 Entrance 0 LocalSet 0 FadeType 1\n" +
                "\tSetPartyMenu 0\n" +
                "\tSetProgressFlag 0x1234\n" +
                "\tSetUnk05 0x1122\n" +
                "\tSetInventory 666 777\n";

            [Fact]
            public void ReadTest()
            {
                var spawns = File.OpenRead(FileName).Using(AreaDataScript.Read);
                var strProgram = spawns.First(x => x.ProgramId == 6).ToString();

                Assert.Equal(31, spawns.Count);
            }

            [Fact]
            public void WriteTest() => File.OpenRead(FileName).Using(x => Helpers.AssertStream(x, stream =>
            {
                var outStream = new MemoryStream();
                AreaDataScript.Write(outStream, AreaDataScript.Read(stream));

                return outStream;
            }));

            [Theory]
            [InlineData("Spawn \"ABcd\"", 0, 0x64634241)]
            [InlineData("MapVisibility 0xffffffff 0x00000001", 1, -1, 1)]
            [InlineData("RandomSpawn \"0123\" \"4567\"", 2, 0x33323130, 0x37363534)]
            [InlineData("CasualSpawn 123 \"666\"", 3, 123, 0x363636)]
            [InlineData("Capacity 123", 4, 0x42F60000)]
            [InlineData("AllocEnemy 123", 5, 123)]
            [InlineData("Unk06 123", 6, 123)]
            [InlineData("Unk07 123", 7, 123)]
            [InlineData("SpawnAlt \"666\"", 9, 0x363636)]
            [InlineData("Party NO_FRIEND", 15, 0)]
            [InlineData("Party DEFAULT", 15, 1)]
            [InlineData("Party W_FRIEND", 15, 2)]
            [InlineData("Party W_FRIEND_IN", 15, 3)]
            [InlineData("Party W_FRIEND_FIX", 15, 4)]
            [InlineData("Party W_FRIEND_ONLY", 15, 5)]
            [InlineData("Party DONALD_ONLY", 15, 6)]
            [InlineData("Bgm 123 456", 16, 0x01c8007b)]
            [InlineData("Bgm Default Default", 16, 0)]
            [InlineData("StatusFlag3", 0x14)]
            [InlineData("Mission 0x1234 \"OPENKH IS OUR MISSION!\"", 0x15, 0x1234,
                0x4E45504F, 0x4920484B, 1431248979, 1229791314, 1330205523, 8526, 0, 0)]
            [InlineData("Layout \"OPENKH IS OUR MISSION!\"", 0x16,
                0x4E45504F, 0x4920484B, 1431248979, 1229791314, 1330205523, 8526, 0, 0)]
            [InlineData("StatusFlag5", 0x17)]
            [InlineData("BattleLevel 99", 0x1E, 99)]
            [InlineData("Unk1f \"666\"", 0x1f, 0x363636)]
            public void ParseScriptAsText(string expected, int operation, params int[] parameters)
            {
                const int ProgramId = 0;

                var writer = new BinaryWriter(new MemoryStream());
                writer.Write((short)ProgramId);
                writer.Write((short)(parameters.Length * 4 + 8));
                writer.Write((short)operation);
                writer.Write((short)parameters.Length);
                foreach (var item in parameters)
                    writer.Write(item);

                writer.BaseStream.Position = 0;
                writer.BaseStream.Flush();
                var command = AreaDataScript.Read(writer.BaseStream, ProgramId).First();

                Assert.Equal(expected, command.ToString());
            }

            [Fact]
            public void CreateProgramsCorrectly()
            {
                const string Input = @"
# This is a comment!
  Program     123 # This is our program entry
StatusFlag3

Program 0x123
StatusFlag5";
                var script = AreaDataScript.Compile(Input).ToArray();
                Assert.Equal(2, script.Length);

                Assert.Equal(123, script[0].ProgramId);
                Assert.Single(script[0].Functions);

                Assert.Equal(0x123, script[1].ProgramId);
                Assert.Single(script[1].Functions);
            }

            [Theory]
            [InlineData("Spawn \"ABcd\"")]
            [InlineData("MapVisibility 0xffffffff 0x00000001")]
            [InlineData("RandomSpawn \"0123\" \"4567\"")]
            [InlineData("CasualSpawn 123 \"666\"")]
            [InlineData("Capacity 123")]
            [InlineData("AllocEnemy 123")]
            [InlineData("Unk06 123")]
            [InlineData("Unk07 123")]
            [InlineData("SpawnAlt \"666\"")]
            [InlineData("Party NO_FRIEND")]
            [InlineData("Party DEFAULT")]
            [InlineData("Party W_FRIEND")]
            [InlineData("Party W_FRIEND_IN")]
            [InlineData("Party W_FRIEND_FIX")]
            [InlineData("Party W_FRIEND_ONLY")]
            [InlineData("Party DONALD_ONLY")]
            [InlineData("Bgm 123 456")]
            [InlineData("Bgm Default Default")]
            [InlineData("StatusFlag3")]
            [InlineData("Mission 0x1234 \"OPENKH IS OUR MISSION!\"")]
            [InlineData("Layout \"OPENKH IS OUR MISSION!\"")]
            [InlineData("StatusFlag5")]
            [InlineData("BattleLevel 99")]
            [InlineData("Unk1f \"666\"")]
            public void ParseTextAsScript(string input)
            {
                var code = $"Program 0x00\n{input}";
                var scripts = AreaDataScript.Compile(code).ToArray();
                Assert.Single(scripts);

                var decompiled = scripts.First().ToString();

                Assert.Equal(code, decompiled);
            }

            [Fact]
            public void WriteAreaSettings() =>
                Helpers.AssertStream(new MemoryStream(SampleScript), stream =>
                {
                    var dstStream = new MemoryStream();
                    var scripts = AreaDataScript.Read(stream);
                    AreaDataScript.Write(dstStream, scripts);

                    return dstStream;
                });

            [Fact]
            public void DecompileAreaSettings()
            {
                var scripts = AreaDataScript.Read(new MemoryStream(SampleScript));
                Assert.Equal(SampleScriptDecompiled, scripts.First().ToString());
            }

            [Fact]
            public void CompileAreaSettings() =>
                Helpers.AssertStream(new MemoryStream(SampleScript), _ =>
                {
                    var dstStream = new MemoryStream();
                    var scripts = AreaDataScript.Compile(SampleScriptDecompiled);
                    AreaDataScript.Write(dstStream, scripts);

                    return dstStream;
                });


            public static IEnumerable<object[]> ScriptSource()
            {
                if (!Directory.Exists(Path.Combine(Helpers.Kh2DataPath, "ard")))
                {
                    yield return new object[] { "", "" };
                    yield break;
                }

                foreach (var fileName in Directory.GetFiles(Helpers.Kh2DataPath, "ard/*.ard", SearchOption.AllDirectories))
                {
                    if (!File.OpenRead(fileName).Using(Bar.IsValid))
                        continue;

                    yield return new object[] { fileName, "map" };
                    yield return new object[] { fileName, "btl" };
                    yield return new object[] { fileName, "evt" };
                }
            }

            [SkippableTheory, MemberData(nameof(ScriptSource))]
            public void Batch_WriteAllScripts(string ardFile, string scriptSet)
            {
                Skip.If(ardFile == string.Empty, "No ARD files found");

                var binarcEntry = File.OpenRead(ardFile).Using(Bar.Read)
                    .FirstOrDefault(x => x.Name == scriptSet && x.Type == Bar.EntryType.AreaDataScript);
                if (binarcEntry == null)
                    return;

                Helpers.AssertStream(binarcEntry.Stream, stream =>
                {
                    var memStream = new MemoryStream();
                    AreaDataScript.Write(memStream, AreaDataScript.Read(stream));

                    return memStream;
                });
            }

            [SkippableTheory, MemberData(nameof(ScriptSource))]
            public void Batch_CompileAllScripts(string ardFile, string scriptSet)
            {
                Skip.If(ardFile == string.Empty, "No ARD files found");

                var binarcEntry = File.OpenRead(ardFile).Using(Bar.Read)
                    .FirstOrDefault(x => x.Name == scriptSet && x.Type == Bar.EntryType.AreaDataScript);
                if (binarcEntry == null)
                    return;

                var expected = AreaDataScript.Decompile(AreaDataScript.Read(binarcEntry.Stream));
                var actual = AreaDataScript.Decompile(AreaDataScript.Compile(expected));
                Assert.Equal(expected, actual);
            }
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
                Assert.Equal(3, spawns[0].Entities.Count);
                Assert.Equal(0x236, spawns[0].Entities[0].ObjectId);
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

using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using OpenKh.Patcher;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xunit;
using Xunit.Sdk;
using YamlDotNet.Serialization;

namespace OpenKh.Tests.Patcher
{
    public class PatcherTests : IDisposable
    {
        private const string AssetsInputDir = "original_input";
        private const string ModInputDir = "mod_input";
        private const string ModOutputDir = "mod_output";

        public PatcherTests()
        {
            Dispose();
            Directory.CreateDirectory(AssetsInputDir);
            Directory.CreateDirectory(ModInputDir);
            Directory.CreateDirectory(ModOutputDir);
        }

        public void Dispose()
        {
            if (Directory.Exists(AssetsInputDir))
                Directory.Delete(AssetsInputDir, true);
            if (Directory.Exists(ModInputDir))
                Directory.Delete(ModInputDir, true);
            if (Directory.Exists(ModOutputDir))
                Directory.Delete(ModOutputDir, true);
        }

        [Fact]
        public void Kh2CopyBinariesTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                {
                    new AssetFile
                    {
                        Name = "somedir/somefile.bin",
                        Method = "copy",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "somedir/somefile.bin"
                            }
                        }
                    }
                }
            };

            CreateFile(ModInputDir, patch.Assets[0].Name).Dispose();

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, patch.Assets[0].Name);
        }

        [Fact]
        public void Kh2CreateBinArcIfSourceDoesntExistsTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                {
                    new AssetFile
                    {
                        Name = "somedir/somefile.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "abcd",
                                Type = "list",
                                Method = "copy",
                                Source = new List<AssetFile>
                                {
                                    new AssetFile
                                    {
                                        Name = "somedir/somefile/abcd.bin"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            CreateFile(ModInputDir, "somedir/somefile/abcd.bin").Using(x =>
            {
                x.WriteByte(0);
                x.WriteByte(1);
                x.WriteByte(2);
                x.WriteByte(3);
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, patch.Assets[0].Name);
            AssertBarFile("abcd", entry =>
            {
                Assert.Equal(Bar.EntryType.List, entry.Type);
                Assert.Equal(4, entry.Stream.Length);
                Assert.Equal(0, entry.Stream.ReadByte());
                Assert.Equal(1, entry.Stream.ReadByte());
                Assert.Equal(2, entry.Stream.ReadByte());
                Assert.Equal(3, entry.Stream.ReadByte());
            }, ModOutputDir, patch.Assets[0].Name);
        }

        [Fact]
        public void Kh2MergeWithOriginalBinArcTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                {
                    new AssetFile
                    {
                        Name = "somedir/somefile.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "abcd",
                                Type = "list",
                                Method = "copy",
                                Source = new List<AssetFile>
                                {
                                    new AssetFile
                                    {
                                        Name = "somedir/somefile/abcd.bin"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            CreateFile(ModInputDir, "somedir/somefile/abcd.bin").Using(x =>
            {
                x.WriteByte(0);
                x.WriteByte(1);
                x.WriteByte(2);
                x.WriteByte(3);
            });

            CreateFile(AssetsInputDir, "somedir/somefile.bar").Using(x =>
            {
                Bar.Write(x, new Bar
                {
                    new Bar.Entry
                    {
                        Name = "nice",
                        Type = Bar.EntryType.Model,
                        Stream = new MemoryStream(new byte[] { 4, 5, 6, 7 })
                    }
                });
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, patch.Assets[0].Name);
            AssertBarFile("abcd", entry =>
            {
                Assert.Equal(Bar.EntryType.List, entry.Type);
                Assert.Equal(4, entry.Stream.Length);
                Assert.Equal(0, entry.Stream.ReadByte());
                Assert.Equal(1, entry.Stream.ReadByte());
                Assert.Equal(2, entry.Stream.ReadByte());
                Assert.Equal(3, entry.Stream.ReadByte());
            }, ModOutputDir, patch.Assets[0].Name);
            AssertBarFile("nice", entry =>
            {
                Assert.Equal(Bar.EntryType.Model, entry.Type);
                Assert.Equal(4, entry.Stream.Length);
                Assert.Equal(4, entry.Stream.ReadByte());
                Assert.Equal(5, entry.Stream.ReadByte());
                Assert.Equal(6, entry.Stream.ReadByte());
                Assert.Equal(7, entry.Stream.ReadByte());
            }, ModOutputDir, patch.Assets[0].Name);
        }

        [Fact]
        public void Kh2CreateImgdTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                {
                    new AssetFile
                    {
                        Name = "somedir/somefile.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "test",
                                Method = "imgd",
                                Type = "imgd",
                                Source = new List<AssetFile>
                                {
                                    new AssetFile
                                    {
                                        Name = "sample.png",
                                        IsSwizzled = false
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Copy("Imaging/res/png/32.png", Path.Combine(ModInputDir, "sample.png"));

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, patch.Assets[0].Name);
            AssertBarFile("test", entry =>
            {
                Assert.True(Imgd.IsValid(entry.Stream));
            }, ModOutputDir, patch.Assets[0].Name);
        }

        [Fact]
        public void Kh2MergeImzTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                {
                    new AssetFile
                    {
                        Name = "out.imz",
                        Method = "imgz",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "test.imd",
                                Index = 1,
                            }
                        }
                    }
                }
            };

            var tmpImd = Imgd.Create(new System.Drawing.Size(16, 16), PixelFormat.Indexed4, new byte[16 * 16 / 2], new byte[4], false);
            var patchImd = Imgd.Create(new System.Drawing.Size(32, 16), PixelFormat.Indexed4, new byte[32 * 16 / 2], new byte[4], false);
            CreateFile(AssetsInputDir, "out.imz").Using(x =>
            {
                Imgz.Write(x, new Imgd[]
                {
                    tmpImd,
                    tmpImd,
                    tmpImd,
                });
            });
            CreateFile(ModInputDir, "test.imd").Using(patchImd.Write);

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "out.imz");
            File.OpenRead(Path.Combine(ModOutputDir, "out.imz")).Using(x =>
            {
                var images = Imgz.Read(x).ToList();
                Assert.Equal(3, images.Count);
                Assert.Equal(16, images[0].Size.Width);
                Assert.Equal(32, images[1].Size.Width);
                Assert.Equal(16, images[2].Size.Width);
            });
        }

        [Fact]
        public void Kh2MergeImzInsideBarTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                {
                    new AssetFile
                    {
                        Name = "out.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "test",
                                Type = "imgz",
                                Method = "imgz",
                                Source = new List<AssetFile>
                                {
                                    new AssetFile
                                    {
                                        Name = "test.imd",
                                        Index = 1
                                    }
                                },
                            }
                        }
                    }
                }
            };

            var tmpImd = Imgd.Create(new System.Drawing.Size(16, 16), PixelFormat.Indexed4, new byte[16 * 16 / 2], new byte[4], false);
            var patchImd = Imgd.Create(new System.Drawing.Size(32, 16), PixelFormat.Indexed4, new byte[32 * 16 / 2], new byte[4], false);
            CreateFile(AssetsInputDir, "out.bar").Using(x =>
            {
                using var memoryStream = new MemoryStream();
                Imgz.Write(memoryStream, new Imgd[]
                {
                    tmpImd,
                    tmpImd,
                    tmpImd,
                });

                Bar.Write(x, new Bar
                {
                    new Bar.Entry
                    {
                        Name = "test",
                        Type = Bar.EntryType.Imgz,
                        Stream = memoryStream
                    }
                });
            });
            CreateFile(ModInputDir, "test.imd").Using(patchImd.Write);

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "out.bar");
            AssertBarFile("test", x =>
            {
                var images = Imgz.Read(x.Stream).ToList();
                Assert.Equal(3, images.Count);
                Assert.Equal(16, images[0].Size.Width);
                Assert.Equal(32, images[1].Size.Width);
                Assert.Equal(16, images[2].Size.Width);
            }, ModOutputDir, "out.bar");
        }

        [Fact]
        public void MergeKh2MsgTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                {
                    new AssetFile
                    {
                        Name = "msg/us/sys.msg",
                        Method = "kh2msg",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "sys.yml",
                                Language = "en",
                            }
                        }
                    },
                    new AssetFile
                    {
                        Name = "msg/it/sys.msg",
                        Method = "kh2msg",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "sys.yml",
                                Language = "it",
                            }
                        }
                    },
                    new AssetFile
                    {
                        Name = "msg/jp/sys.msg",
                        Method = "kh2msg",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "sys.yml",
                                Language = "jp",
                            }
                        }
                    }
                }
            };

            Directory.CreateDirectory(Path.Combine(AssetsInputDir, "msg/us/"));
            File.Create(Path.Combine(AssetsInputDir, "msg/us/sys.msg")).Using(stream =>
            {
                Msg.Write(stream, new List<Msg.Entry>
                {
                    new Msg.Entry
                    {
                        Data = new byte[] { 1, 2, 3, 0 },
                        Id = 123
                    }
                });
            });
            File.Create(Path.Combine(ModInputDir, "sys.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("- id: 456");
                writer.WriteLine("  en: English");
                writer.WriteLine("  it: Italiano");
                writer.WriteLine("  jp: テスト");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "msg/jp/sys.msg");
            File.OpenRead(Path.Combine(ModOutputDir, "msg/jp/sys.msg")).Using(stream =>
            {
                var msg = Msg.Read(stream);
                Assert.Single(msg);
                Assert.Equal(456, msg[0].Id);
                Assert.Equal("テスト", Encoders.JapaneseSystem.Decode(msg[0].Data).First().Text);
            });

            AssertFileExists(ModOutputDir, "msg/us/sys.msg");
            File.OpenRead(Path.Combine(ModOutputDir, "msg/us/sys.msg")).Using(stream =>
            {
                var msg = Msg.Read(stream);
                Assert.Equal(2, msg.Count);
                Assert.Equal(123, msg[0].Id);
                Assert.Equal(456, msg[1].Id);
                Assert.Equal("English", Encoders.InternationalSystem.Decode(msg[1].Data).First().Text);
            });

            AssertFileExists(ModOutputDir, "msg/it/sys.msg");
            File.OpenRead(Path.Combine(ModOutputDir, "msg/it/sys.msg")).Using(stream =>
            {
                var msg = Msg.Read(stream);
                Assert.Single(msg);
                Assert.Equal(456, msg[0].Id);
                Assert.Equal("Italiano", Encoders.InternationalSystem.Decode(msg[0].Data).First().Text);
            });
        }

        [Fact]
        public void MergeKh2AreaDataScriptTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                {
                    new AssetFile
                    {
                        Name = "map.script",
                        Method = "areadatascript",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "map.txt",
                            }
                        }
                    },
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "map.script")).Using(stream =>
            {
                var compiledProgram = Kh2.Ard.AreaDataScript.Compile("Program 1\nSpawn \"1111\"");
                Kh2.Ard.AreaDataScript.Write(stream, compiledProgram);
            });
            File.Create(Path.Combine(ModInputDir, "map.txt")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("Program 2");
                writer.WriteLine("Spawn \"2222\"");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "map.script");
            File.OpenRead(Path.Combine(ModOutputDir, "map.script")).Using(stream =>
            {
                var scripts = Kh2.Ard.AreaDataScript.Read(stream);
                var decompiled = Kh2.Ard.AreaDataScript.Decompile(scripts);
                decompiled.Contains("Program 1");
                decompiled.Contains("Spawn \"1111\"");
                decompiled.Contains("Program 2");
                decompiled.Contains("Spawn \"2222\"");
            });
        }

        [Fact]
        public void ListReplaceTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                { new AssetFile
                {
                    Name = "fmlv.bin",
                    Method = "binarc",
                    Source = new List<AssetFile>
                    {
                        new AssetFile
                        {
                            Name = "fmlv",
                            Method = "listreplace",
                            Type = "List",
                            Source = new List<AssetFile>
                            {
                                new AssetFile
                                {
                                    Name = "FmlvList.yml",
                                    Type = "fmlv"
                                }
                            }
                        }
                    }
                },
                new AssetFile
                {
                    Name = "lvup.bin",
                    Method = "binarc",
                    Source = new List<AssetFile>
                    {
                        new AssetFile
                        {
                            Name = "lvup",
                            Method = "listreplace",
                            Type = "List",
                            Source = new List<AssetFile>
                            {
                                new AssetFile
                                {
                                    Name = "LvupList.yml",
                                    Type = "lvup"
                                }
                            }
                        }
                    }
                },
                new AssetFile
                {
                    Name = "bons.bin",
                    Method = "binarc",
                    Source = new List<AssetFile>
                    {
                        new AssetFile
                        {
                            Name = "bons",
                            Method = "listreplace",
                            Type = "List",
                            Source = new List<AssetFile>
                            {
                                new AssetFile
                                {
                                    Name = "BonsList.yml",
                                    Type = "bons"
                                }
                            }
                        }
                    }
                },
                new AssetFile
                {
                    Name = "trsr.bin",
                    Method = "binarc",
                    Source = new List<AssetFile>
                    {
                        new AssetFile
                        {
                            Name = "trsr",
                            Method = "listreplace",
                            Type = "List",
                            Source = new List<AssetFile>
                            {
                                new AssetFile
                                {
                                    Name = "TrsrList.yml",
                                    Type = "trsr"
                                }
                            }
                        }
                    }
                },
                new AssetFile
                {
                    Name = "item.bin",
                    Method = "binarc",
                    Source = new List<AssetFile>
                    {
                        new AssetFile
                        {
                            Name = "item",
                            Method = "listreplace",
                            Type = "List",
                            Source = new List<AssetFile>
                            {
                                new AssetFile
                                {
                                    Name = "ItemList.yml",
                                    Type = "item"
                                }
                            }
                        }
                    }
                }
                }
            };


            #region fmlvPatch

            File.Create(Path.Combine(AssetsInputDir, "fmlv.bin")).Using(stream =>
            {
                var fmlvStream = new List<Kh2.Battle.Fmlv>{
                    new Kh2.Battle.Fmlv {
                        Unk0 = 17,
                        AbilityLevel = 0,
                        Ability = 1,
                        Exp = 70,
                        FormId = 1,
                        FormLevel = 1,
                        VanillaForm = Kh2.Battle.Fmlv.FormVanilla.Valor,
                        FinalMixForm = Kh2.Battle.Fmlv.FormFm.Valor
                    }
                };
                Kh2.Battle.Fmlv.Write(stream,fmlvStream);
            });

            var testFmlv = new Kh2.Battle.Fmlv
            {
                Unk0 = 17,
                AbilityLevel = 1,
                Ability = 17,
                Exp = 1,
                FormId = 1,
                FormLevel = 1,
                VanillaForm = Kh2.Battle.Fmlv.FormVanilla.Valor,
                FinalMixForm = Kh2.Battle.Fmlv.FormFm.Valor
            };

            var fmlvDict = new Dictionary<Kh2.Battle.Fmlv.FormFm, List<Kh2.Battle.Fmlv>>();
            fmlvDict.Add(Kh2.Battle.Fmlv.FormFm.Valor, new List<Kh2.Battle.Fmlv>() { testFmlv });

            File.WriteAllText(Path.Combine(ModInputDir, "FmlvList.yml"), serializer.Serialize(fmlvDict));

            #endregion

            #region lvupPatch

            File.Create(Path.Combine(AssetsInputDir, "lvup.bin")).Using(stream =>
            {
                var fmlvStream = new Kh2.Battle.Lvup { 
                    Characters = new List<Kh2.Battle.Lvup.PlayableCharacter>
                    {
                        new Kh2.Battle.Lvup.PlayableCharacter
                        {
                            NumLevels = 99,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level
                                {
                                    Exp = 1000,
                                    Ap = 3,
                                    Strength = 4,
                                    Magic = 5,
                                    Defense = 6,
                                    SwordAbility = 100,
                                    ShieldAbility = 101,
                                    StaffAbility = 102
                                }
                            }
                        }
                    }
                };
                fmlvStream.Write(stream);
            });

            var soraLevel0 = new Kh2.Battle.Lvup.PlayableCharacter.Level
            {
                Exp = 100,
                Ap = 99,
                Strength = 40,
                Magic = 50,
                Defense = 60,
                SwordAbility = 152,
                ShieldAbility = 153,
                StaffAbility = 154
            };

            var testLvup = new Kh2.Battle.Lvup
            {
                Characters = new List<Kh2.Battle.Lvup.PlayableCharacter>
                {
                    new Kh2.Battle.Lvup.PlayableCharacter
                    {
                        NumLevels = 99,
                        Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>
                        {
                            soraLevel0
                        }
                    }
                }
            };


            File.WriteAllText(Path.Combine(ModInputDir, "LvupList.yml"), serializer.Serialize(testLvup));

            #endregion

            #region bonsPatch

            File.Create(Path.Combine(AssetsInputDir, "bons.bin")).Using(stream =>
            {
                var bonsStream = new Kh2.Battle.Bons
                {
                    CharacterId = 0,
                    RewardId = 1,
                    BonusItem1 = 10,
                    BonusItem2 = 20
                };
                Kh2.Battle.Bons.Write(stream, new List<Kh2.Battle.Bons> { bonsStream });
            });

            var testBons = new List<Kh2.Battle.Bons>
            {
                new Kh2.Battle.Bons
                {
                    CharacterId = 0,
                    RewardId = 1,
                    BonusItem1 = 100,
                    BonusItem2 = 200
                }
            };

            File.WriteAllText(Path.Combine(ModInputDir, "BonsList.yml"), serializer.Serialize(testBons));

            #endregion

            #region trsrPatch
            File.Create(Path.Combine(AssetsInputDir, "trsr.bin")).Using(stream =>
            {
                var trsrStream = new Kh2.SystemData.Trsr
                {
                    Id = 120,
                    ItemId = 10
                };
                Kh2.SystemData.Trsr.Write(stream, new List<Kh2.SystemData.Trsr>
                {
                    trsrStream
                });
            });

            var testTrsr = new List<Kh2.SystemData.Trsr>
            {
                new Kh2.SystemData.Trsr
                {
                    Id = 120,
                    ItemId = 100
                }
            };

            File.WriteAllText(Path.Combine(ModInputDir, "TrsrList.yml"), serializer.Serialize(testTrsr));
            #endregion

            #region itemPatch
            File.Create(Path.Combine(AssetsInputDir, "item.bin")).Using(stream =>
            {
                var itemStream = new Kh2.SystemData.Item
                {
                    Items1 = new List<Kh2.SystemData.Item.Entry>
                    {
                        new Kh2.SystemData.Item.Entry
                        {
                            Id = 130,
                            ShopSell = 10
                        }
                    },
                    Items2 = new List<Kh2.SystemData.Item.Stat>
                    {
                        new Kh2.SystemData.Item.Stat
                        {
                            Id = 140,
                            Ability = 10
                        }
                    }
                };
                itemStream.Write(stream);
            });

            var testItem = new Kh2.SystemData.Item
            {
                Items1 = new List<Kh2.SystemData.Item.Entry>
                {
                    new Kh2.SystemData.Item.Entry
                    {
                        Id = 130,
                        ShopSell = 100
                    }
                },
                Items2 = new List<Kh2.SystemData.Item.Stat>
                {
                    new Kh2.SystemData.Item.Stat
                    {
                        Id = 140,
                        Ability = 100
                    }
                }
            };

            File.WriteAllText(Path.Combine(ModInputDir, "ItemList.yml"), serializer.Serialize(testItem));
            


            #endregion

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "fmlv.bin");
            AssertFileExists(ModOutputDir, "lvup.bin");
            AssertFileExists(ModOutputDir, "bons.bin");
            AssertFileExists(ModOutputDir, "trsr.bin");
            AssertFileExists(ModOutputDir, "item.bin");

            File.OpenRead(Path.Combine(ModOutputDir, "fmlv.bin")).Using(stream =>
            {
                var fmlv = Kh2.Battle.Fmlv.Read(stream);
                Assert.True(fmlv[0] == testFmlv);
            });
            File.OpenRead(Path.Combine(ModOutputDir, "lvup.bin")).Using(stream =>
            {
                var lvup = Kh2.Battle.Lvup.Read(stream);
                Assert.True(lvup.Characters[0].Levels[0] == soraLevel0);
            });
            File.OpenRead(Path.Combine(ModOutputDir, "bons.bin")).Using(stream =>
            {
                var bons = Kh2.Battle.Bons.Read(stream);
                Assert.True(bons[0] == testBons[0]);
            });
            File.OpenRead(Path.Combine(ModOutputDir, "trsr.bin")).Using(stream =>
            {
                var trsr = Kh2.SystemData.Trsr.Read(stream);
                Assert.True(trsr[0] == testTrsr[0]);
            });
            File.OpenRead(Path.Combine(ModOutputDir, "item.bin")).Using(stream =>
            {
                var item = Kh2.SystemData.Item.Read(stream);
                Assert.True(item.Items1[0] == testItem.Items1[0]);
                Assert.True(item.Items2[0] == testItem.Items2[0]);
            });

        }

        [Fact]
        public void ProcessMultipleTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                {
                    new AssetFile
                    {
                        Name = "somedir/somefile.bar",
                        Method = "binarc",
                        Multi = new List<Multi>
                        {
                            new Multi { Name = "somedir/another.bar" }
                        },
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "test",
                                Method = "imgd",
                                Type = "imgd",
                                Source = new List<AssetFile>
                                {
                                    new AssetFile
                                    {
                                        Name = "sample.png",
                                        IsSwizzled = false
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Copy("Imaging/res/png/32.png", Path.Combine(ModInputDir, "sample.png"));

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, patch.Assets[0].Name);
            AssertBarFile("test", entry =>
            {
                Assert.True(Imgd.IsValid(entry.Stream));
            }, ModOutputDir, patch.Assets[0].Name);

            AssertFileExists(ModOutputDir, patch.Assets[0].Multi[0].Name);
            AssertBarFile("test", entry =>
            {
                Assert.True(Imgd.IsValid(entry.Stream));
            }, ModOutputDir, patch.Assets[0].Multi[0].Name);
        }

        private static void AssertFileExists(params string[] paths)
        {
            var filePath = Path.Join(paths);
            if (File.Exists(filePath) == false)
                throw new TrueException($"File not found '{filePath}'", false);
        }

        private static void AssertBarFile(string name, Action<Bar.Entry> assertion, params string[] paths)
        {
            var filePath = Path.Join(paths);
            var entries = File.OpenRead(filePath).Using(x =>
            {
                if (!Bar.IsValid(x))
                    throw new TrueException($"Not a valid BinArc", false);
                return Bar.Read(x);
            });

            var entry = entries.SingleOrDefault(x => x.Name == name);
            if (entry == null)
                throw new XunitException($"Entry '{name}' not found");

            assertion(entry);
        }

        private static Stream CreateFile(params string[] paths)
        {
            var filePath = Path.Join(paths);
            var dirPath = Path.GetDirectoryName(filePath);
            Directory.CreateDirectory(dirPath);
            return File.Create(filePath);
        }
    }
}

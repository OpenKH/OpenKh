using OpenKh.Command.Bdxio.Utils;
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
        public void PatchKh2BdscriptTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                {
                    new AssetFile
                    {
                        Name = "aaa",
                        Method = "bdscript",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "test.bdscript",
                            }
                        }
                    },
                }
            };
            File.Create(Path.Combine(ModInputDir, "test.bdscript")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("---");
                writer.WriteLine("WorkSize: 64");
                writer.WriteLine("StackSize: 64");
                writer.WriteLine("TempSize: 64");
                writer.WriteLine("Triggers:");
                writer.WriteLine("- Key: 0");
                writer.WriteLine("  Addr: TR0");
                writer.WriteLine("Name: aaa");
                writer.WriteLine("---");
                writer.WriteLine(" section.text");
                writer.WriteLine("TR0:");
                writer.WriteLine(" ret");
                writer.WriteLine("DUMMY:");
                writer.WriteLine(" ret");
                writer.Flush();
            });
            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "aaa");

            var bdxStream = new MemoryStream(File.ReadAllBytes(Path.Combine(ModOutputDir, "aaa")));
            var decoder = new BdxDecoder(bdxStream);
            var script = BdxDecoder.TextFormatter.Format(decoder);

            var lines = script.Split("\r\n");

            Assert.Equal("WorkSize: 64", lines[1]);
            Assert.Equal("Name: aaa", lines[7]);

        }

        [Fact]
        public void PatchKh2SpawnPointTest()
        {

            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                {
                    new AssetFile
                    {
                        Name = "map.script",
                        Method = "areadataspawn",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "map.yaml",
                            }
                        }
                    },
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "map.script")).Using(stream =>
            {
                var spawnPoint = new List<Kh2.Ard.SpawnPoint>();

                Kh2.Ard.SpawnPoint.Write(stream, spawnPoint);
            });
            File.Create(Path.Combine(ModInputDir, "map.yaml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("- Type: 2");
                writer.WriteLine("  Flag: 1");
                writer.Flush();
            });
            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "map.script");
            var content = File.ReadAllText(Path.Combine(ModOutputDir, "map.script"));
            var scripts = new Deserializer().Deserialize<List< Kh2.Ard.SpawnPoint >> (content);

            Assert.Equal(2, scripts[0].Type);
            Assert.Equal(1, scripts[0].Flag);
           
        }

        
        public void ListPatchTrsrTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata() { 
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "03system.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "trsr",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "TrsrList.yml",
                                        Type = "trsr"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "03system.bar")).Using(stream =>
            {
                var trsrEntry = new List<Kh2.SystemData.Trsr>()
                {
                    new Kh2.SystemData.Trsr
                    {
                        Id = 1,
                        ItemId = 10
                    }
                    };
                using var trsrStream = new MemoryStream();
                Kh2.SystemData.Trsr.Write(trsrStream, trsrEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "trsr",
                        Type = Bar.EntryType.List,
                        Stream = trsrStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "TrsrList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("1:");
                writer.WriteLine("  ItemId: 200");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "03system.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "03system.bar")).Using(stream =>
             {
                 var binarc = Bar.Read(stream);
                 var trsrStream = Kh2.SystemData.Trsr.Read(binarc[0].Stream);
                 Assert.Equal(200, trsrStream[0].ItemId);
             });

        }
        
        [Fact]
        public void ListPatchCmdTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata() { 
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "03system.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "cmd",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "CmdList.yml",
                                        Type = "cmd"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "03system.bar")).Using(stream =>
            {
                var cmdEntry = new List<Kh2.SystemData.Cmd>()
                {
                    new Kh2.SystemData.Cmd
                    {
                        Id = 1,
                        Execute = 3,
                        Argument = 3,
                        SubMenu = 1,
                    }
                    };
                using var cmdStream = new MemoryStream();
                Kh2.SystemData.Cmd.Write(cmdStream, cmdEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "cmd",
                        Type = Bar.EntryType.List,
                        Stream = cmdStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "CmdList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("- Id: 1");
                writer.WriteLine("  Execute: 3");
                writer.WriteLine("  Argument: 3");
                writer.WriteLine("  SubMenu: 1");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "03system.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "03system.bar")).Using(stream =>
             {
                 var binarc = Bar.Read(stream);
                 var cmdStream = Kh2.SystemData.Cmd.Read(binarc[0].Stream);
                 Assert.Equal(1, cmdStream[0].Id);
                 Assert.Equal(3, cmdStream[0].Execute);
                 Assert.Equal(3, cmdStream[0].Argument);
                 Assert.Equal(1, cmdStream[0].SubMenu);
             });

        }
        
        [Fact]
        public void ListPatchItemTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "03system.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "item",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
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

            File.Create(Path.Combine(AssetsInputDir, "03system.bar")).Using(stream =>
            {
                var itemEntry = new List<Kh2.SystemData.Item>()
                {
                    new Kh2.SystemData.Item
                    {
                        Items = new List<Kh2.SystemData.Item.Entry>()
                        {
                            new Kh2.SystemData.Item.Entry()
                            {
                                Id = 1,
                                ShopBuy = 10
                            }
                        },
                        Stats = new List<Kh2.SystemData.Item.Stat>()
                        {
                            new Kh2.SystemData.Item.Stat()
                            {
                                Id = 10,
                                Ability = 15
                            }
                        }
                        
                    }
                    };
                using var itemStream = new MemoryStream();
                itemEntry[0].Write(itemStream);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "item",
                        Type = Bar.EntryType.List,
                        Stream = itemStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "ItemList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("Items:");
                writer.WriteLine("- Id: 1");
                writer.WriteLine("  ShopBuy: 200");
                writer.WriteLine("Stats:");
                writer.WriteLine("- Id: 10");
                writer.WriteLine("  Ability: 150");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "03system.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "03system.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var itemStream = Kh2.SystemData.Item.Read(binarc[0].Stream);
                Assert.Equal(200, itemStream.Items[0].ShopBuy);
                Assert.Equal(150, itemStream.Stats[0].Ability);
            });

        }

        [Fact]
        public void ListPatchSkltTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "03system.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "sklt",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "SkltList.yml",
                                        Type = "sklt"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "03system.bar")).Using(stream =>
            {
                var skltEntry = new List<Kh2.SystemData.Sklt>()
                {
                    new Kh2.SystemData.Sklt
                    {
                        CharacterId = 1,
                        Bone1 = 178,
                        Bone2 = 86
                    }
                };

                using var skltStream = new MemoryStream();
                Kh2.SystemData.Sklt.Write(skltStream, skltEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "sklt",
                        Type = Bar.EntryType.List,
                        Stream = skltStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "SkltList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("- CharacterId: 1");
                writer.WriteLine("  Bone1: 178");
                writer.WriteLine("  Bone2: 86");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "03system.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "03system.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var skltStream = Kh2.SystemData.Sklt.Read(binarc[0].Stream);
                Assert.Equal(1U, skltStream[0].CharacterId);
                Assert.Equal(178, skltStream[0].Bone1);
                Assert.Equal(86, skltStream[0].Bone2);
            });
        }

        [Fact]
        public void ListPatchFmlvTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "00battle.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "fmlv",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "FmlvList.yml",
                                        Type = "fmlv"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "00battle.bar")).Using(stream =>
            {
                var fmlvEntry = new List<Kh2.Battle.Fmlv>()
                {
                    new Kh2.Battle.Fmlv
                    {
                        FormId = 1,
                        FormLevel = 1,
                        Exp = 100,
                        Ability = 200
                    },
                    new Kh2.Battle.Fmlv
                    {
                        FormId = 1,
                        FormLevel = 2,
                        Exp = 100,
                        Ability = 200
                    },
                    new Kh2.Battle.Fmlv
                    {
                        FormId = 2,
                        FormLevel = 1,
                        Exp = 100,
                        Ability = 200
                    },
                };

                using var fmlvStream = new MemoryStream();
                Kh2.Battle.Fmlv.Write(fmlvStream, fmlvEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "fmlv",
                        Type = Bar.EntryType.List,
                        Stream = fmlvStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "FmlvList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                var serializer = new Serializer();
                serializer.Serialize(writer, new Dictionary<string, FmlvDTO[]>
                {
                    ["Valor"] = new[]
                    {
                        new FmlvDTO
                        {
                            FormLevel = 1,
                            Experience = 5,
                            Ability = 127
                        }
                    }
                });
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "00battle.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var fmlv = Kh2.Battle.Fmlv.Read(binarc[0].Stream);

                Assert.Equal(3, fmlv.Count);
                Assert.Equal(1, fmlv[0].FormId);
                Assert.Equal(1, fmlv[0].FormLevel);
                Assert.Equal(5, fmlv[0].Exp);
                Assert.Equal(127, fmlv[0].Ability);

                Assert.Equal(1, fmlv[1].FormId);
                Assert.Equal(2, fmlv[1].FormLevel);

                Assert.Equal(2, fmlv[2].FormId);
                Assert.Equal(1, fmlv[2].FormLevel);
            });
        }

        [Fact]
        public void ListPatchBonsTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "00battle.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "bons",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "BonsList.yml",
                                        Type = "bons"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "00battle.bar")).Using(stream =>
            {
                var bonsEntry = new List<Kh2.Battle.Bons>()
                {
                    new Kh2.Battle.Bons
                    {
                        CharacterId = 1,
                        RewardId = 15,
                        BonusItem1 = 10
                    },
                    new Kh2.Battle.Bons
                    {
                        CharacterId = 2,
                        RewardId = 15,
                        BonusItem1 = 5
                    }
                    };
                using var bonsStream = new MemoryStream();
                Kh2.Battle.Bons.Write(bonsStream, bonsEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "bons",
                        Type = Bar.EntryType.List,
                        Stream = bonsStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "BonsList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("15:");
                writer.WriteLine("  Sora:");
                writer.WriteLine("    BonusItem1: 200");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "00battle.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var bonsStream = Kh2.Battle.Bons.Read(binarc[0].Stream);
                Assert.Equal(200, bonsStream[0].BonusItem1);
                Assert.Equal(5, bonsStream[1].BonusItem1);
            });

        }

        [Fact]
        public void ListPatchLvupTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "00battle.bin",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "lvup",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "LvupList.yml",
                                        Type = "lvup"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "00battle.bin")).Using(stream =>
            {
                var lvupEntry = new Kh2.Battle.Lvup
                {
                    Count = 13,
                    Unknown08 = new byte[0x38],
                    Characters = new List<Kh2.Battle.Lvup.PlayableCharacter>()
                    {
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        },
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        },
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        },
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        },
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        },
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        },
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        },
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        },
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        },
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        },
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        },
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        },
                        new Kh2.Battle.Lvup.PlayableCharacter()
                        {
                            NumLevels = 1,
                            Levels = new List<Kh2.Battle.Lvup.PlayableCharacter.Level>()
                            {
                                new Kh2.Battle.Lvup.PlayableCharacter.Level()
                                {
                                    Exp = 50
                                }
                            }
                        }

                    }
                };
                using var lvupStream = new MemoryStream();
                lvupEntry.Write(lvupStream);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "lvup",
                        Type = Bar.EntryType.List,
                        Stream = lvupStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "LvupList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("Sora:");
                writer.WriteLine("  1:");
                writer.WriteLine("    Exp: 500");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "00battle.bin");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bin")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var lvupStream = Kh2.Battle.Lvup.Read(binarc[0].Stream);
                Assert.Equal(500, lvupStream.Characters[0].Levels[0].Exp);
            });

        }

        void ListPatchAtkpTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "00battle.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "atkp",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "AtkpList.yml",
                                        Type = "atkp"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "00battle.bar")).Using(stream =>
            {
                var atkpEntry = new List<Kh2.Battle.Atkp>()
                {
                    new Kh2.Battle.Atkp
                    {
                        Id = 0,
                        SubId = 3,
                        Type = Kh2.Battle.Atkp.AttackType.PierceArmor,
                        CriticalAdjust = 0,
                        Power = 25,
                        Team = 0,
                        Element = 0,
                        EnemyReaction = 0,
                        EffectOnHit = 2,
                        KnockbackStrength1 = 32767,
                        KnockbackStrength2 = 0,
                        Unknown = 0,
                        Flags = Kh2.Battle.Atkp.AttackFlags.BGHit,
                        RefactSelf = Kh2.Battle.Atkp.Refact.Reflect,
                        RefactOther = Kh2.Battle.Atkp.Refact.Reflect,
                        ReflectedMotion = 0,
                        ReflectHitBack = 0,
                        ReflectAction = 0,
                        ReflectHitSound = 0,
                        ReflectRC = 0,
                        ReflectRange = 0,
                        ReflectAngle = 0,
                        DamageEffect = 0,
                        Switch = 1,
                        Interval = 1,
                        FloorCheck = 1,
                        DriveDrain = 1,
                        RevengeDamage = 1,
                        AttackTrReaction = Kh2.Battle.Atkp.TrReaction.Charge,
                        ComboGroup = 1,
                        RandomEffect = 1,
                        Kind = Kh2.Battle.Atkp.AttackKind.ComboFinisher,
                        HpDrain = 15
                    }
                };

                using var atkpStream = new MemoryStream();
                Kh2.Battle.Atkp.Write(atkpStream, atkpEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "atkp",
                        Type = Bar.EntryType.List,
                        Stream = atkpStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "AtkpList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("- Id: 0");
                writer.WriteLine("  SubId: 3");
                writer.WriteLine("  Type: PierceArmor");
                writer.WriteLine("  CriticalAdjust: 0");
                writer.WriteLine("  Power: 25");
                writer.WriteLine("  Team: 0");
                writer.WriteLine("  Element: 0");
                writer.WriteLine("  EnemyReaction: 0");
                writer.WriteLine("  EffectOnHit: 2");
                writer.WriteLine("  KnockbackStrength1: 32767");
                writer.WriteLine("  KnockbackStrength2: 0");
                writer.WriteLine("  Unknown: 0");
                writer.WriteLine("  Flags: BGHit");
                writer.WriteLine("  RefactSelf: 0");
                writer.WriteLine("  RefactOther: 0");
                writer.WriteLine("  ReflectedMotion: 0");
                writer.WriteLine("  ReflectHitBack: 0");
                writer.WriteLine("  ReflectAction: 0");
                writer.WriteLine("  ReflectHitSound: 0");
                writer.WriteLine("  ReflectRC: 0");
                writer.WriteLine("  ReflectRange: 0");
                writer.WriteLine("  ReflectAngle: 0");
                writer.WriteLine("  DamageEffect: 0");
                writer.WriteLine("  Switch: 1");
                writer.WriteLine("  Interval: 1");
                writer.WriteLine("  FloorCheck: 1");
                writer.WriteLine("  DriveDrain: 1");
                writer.WriteLine("  RevengeDamage: 1");
                writer.WriteLine("  AttackTrReaction: 1");
                writer.WriteLine("  ComboGroup: 1");
                writer.WriteLine("  RandomEffect: 1");
                writer.WriteLine("  Kind: ComboFinisher");
                writer.WriteLine("  HpDrain: 15");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "00battle.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var atkpStream = Kh2.Battle.Atkp.Read(binarc[0].Stream);
                Assert.Equal(0, atkpStream[0].Id);
                Assert.Equal(3, atkpStream[0].SubId);
                Assert.Equal(Kh2.Battle.Atkp.AttackType.PierceArmor, atkpStream[0].Type);
                Assert.Equal(0, atkpStream[0].CriticalAdjust);
                Assert.Equal(25, atkpStream[0].Power);
                Assert.Equal(0, atkpStream[0].Team);
                Assert.Equal(0, atkpStream[0].Element);
                Assert.Equal(0, atkpStream[0].EnemyReaction);
                Assert.Equal(2, atkpStream[0].EffectOnHit);
                Assert.Equal(32767, atkpStream[0].KnockbackStrength1);
                Assert.Equal(0, atkpStream[0].KnockbackStrength2);
                Assert.Equal(0000, atkpStream[0].Unknown);
                Assert.Equal(Kh2.Battle.Atkp.AttackFlags.BGHit, atkpStream[0].Flags);
                Assert.Equal(Kh2.Battle.Atkp.Refact.Reflect, atkpStream[0].RefactSelf);
                Assert.Equal(Kh2.Battle.Atkp.Refact.Reflect, atkpStream[0].RefactOther);
                Assert.Equal(0, atkpStream[0].ReflectedMotion);
                Assert.Equal(0, atkpStream[0].ReflectHitBack);
                Assert.Equal(0, atkpStream[0].ReflectAction);
                Assert.Equal(0, atkpStream[0].ReflectHitSound);
                Assert.Equal(0, atkpStream[0].ReflectRC);
                Assert.Equal(0, atkpStream[0].ReflectRange);
                Assert.Equal(0, atkpStream[0].ReflectAngle);
                Assert.Equal(0, atkpStream[0].DamageEffect);
                Assert.Equal(1, atkpStream[0].Switch);
                Assert.Equal(1, atkpStream[0].Interval);
                Assert.Equal(1, atkpStream[0].FloorCheck);
                Assert.Equal(1, atkpStream[0].DriveDrain);
                Assert.Equal(1, atkpStream[0].RevengeDamage);
                Assert.Equal(Kh2.Battle.Atkp.TrReaction.Charge, atkpStream[0].AttackTrReaction);
                Assert.Equal(1, atkpStream[0].ComboGroup);
                Assert.Equal(1, atkpStream[0].RandomEffect);
                Assert.Equal(Kh2.Battle.Atkp.AttackKind.ComboFinisher, atkpStream[0].Kind);
                Assert.Equal(15, atkpStream[0].HpDrain);
            });
        }

        [Fact]
        public void ListPatchObjEntryTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "00objentry.bin",
                        Method = "listpatch",
                        Type = "List",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "ObjList.yml",
                                Type = "objentry",
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "00objentry.bin")).Using(stream =>
            {
                var objEntry = new List<Kh2.Objentry>()
                {
                    new Kh2.Objentry
                    {
                        ObjectId = 1,
                        ModelName = "M_EX060",
                        AnimationName = "M_EX060.mset"
                    }
                    };
                Kh2.Objentry.Write(stream, objEntry);
            });

            File.Create(Path.Combine(ModInputDir, "ObjList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("1:");
                writer.WriteLine("  ObjectId: 1");
                writer.WriteLine("  ModelName: M_EX100");
                writer.WriteLine("  AnimationName: M_EX100.mset");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "00objentry.bin");

            File.OpenRead(Path.Combine(ModOutputDir, "00objentry.bin")).Using(stream =>
            {
                var objStream = Kh2.Objentry.Read(stream);
                Assert.Equal("M_EX100", objStream[0].ModelName);
                Assert.Equal("M_EX100.mset", objStream[0].AnimationName);
            });

        }

        [Fact]
        public void ListPatchPlrpTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "00battle.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "plrp",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "PlrpList.yml",
                                        Type = "plrp"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "00battle.bar")).Using(stream =>
            {
                var plrpEntry = new List<Kh2.Battle.Plrp>()
                {
                    new Kh2.Battle.Plrp
                    {
                        Id = 7,
                        Character = 1,
                        Ap = 2,
                        Items = new List<ushort>(32),
                        Padding = new byte[52]
                    }
                };

                using var plrpStream = new MemoryStream();
                Kh2.Battle.Plrp.Write(plrpStream, plrpEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "plrp",
                        Type = Bar.EntryType.List,
                        Stream = plrpStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "PlrpList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                var serializer = new Serializer();
                var moddedPlrp = new List<Kh2.Battle.Plrp>{
                    new Kh2.Battle.Plrp
                    {
                        Id = 7,
                        Character = 1,
                        Ap = 200,
                        Items = new List<ushort>(32),
                        Padding = new byte[52]
                    } 
                };
                writer.Write(serializer.Serialize(moddedPlrp));
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "00battle.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var plrp = Kh2.Battle.Plrp.Read(binarc[0].Stream);

                Assert.Equal(200, plrp[0].Ap);
            });
        }

        [Fact]
        public void ListPatchEnmpTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "00battle.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "enmp",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "EnmpList.yml",
                                        Type = "enmp"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "00battle.bar")).Using(stream =>
            {
                var enmpEntry = new List<Kh2.Battle.Enmp>()
                {
                    new Kh2.Battle.Enmp
                    {
                        Id = 7,
                        Level = 1,
                        Health = new short[32],
                        MaxDamage = 1,
                        MinDamage = 1,
                        PhysicalWeakness = 1,
                        FireWeakness = 1,
                        IceWeakness = 1,
                        ThunderWeakness = 1,
                        DarkWeakness = 1,
                        LightWeakness = 1,
                        GeneralWeakness = 1,
                        Experience = 1,
                        Prize = 1,
                        BonusLevel = 1
                    }
                };

                using var enmpStream = new MemoryStream();
                Kh2.Battle.Enmp.Write(enmpStream, enmpEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "enmp",
                        Type = Bar.EntryType.List,
                        Stream = enmpStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "EnmpList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                var serializer = new Serializer();
                var moddedEnmp = new List<Kh2.Battle.Enmp>{
                    new Kh2.Battle.Enmp
                    {
                        Id = 7,
                        Level = 1,
                        Health = new short[32],
                        MaxDamage = 1,
                        MinDamage = 1,
                        PhysicalWeakness = 1,
                        FireWeakness = 1,
                        IceWeakness = 1,
                        ThunderWeakness = 1,
                        DarkWeakness = 1,
                        LightWeakness = 1,
                        GeneralWeakness = 1,
                        Experience = 1,
                        Prize = 1,
                        BonusLevel = 1
                    }
                };
                writer.Write(serializer.Serialize(moddedEnmp));
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "00battle.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var enmp = Kh2.Battle.Enmp.Read(binarc[0].Stream);

                Assert.Equal(1, enmp[0].Level);
            });
        }
        
        [Fact]
        public void ListPatchMagcTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "00battle.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "magc",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "MagcList.yml",
                                        Type = "magc"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "00battle.bar")).Using(stream =>
            {
                var magcEntry = new List<Kh2.Battle.Magc>()
                {
                    new Kh2.Battle.Magc
                    {
                        Id = 7,
                        Level = 3,
                        World = 1,
                        FileName = "magic/FIRE_7.mag"
                    }
                };

                using var magcStream = new MemoryStream();
                Kh2.Battle.Magc.Write(magcStream, magcEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "magc",
                        Type = Bar.EntryType.List,
                        Stream = magcStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "MagcList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                var serializer = new Serializer();
                var moddedMagc = new List<Kh2.Battle.Magc>{
                    new Kh2.Battle.Magc
                    {
                        Id = 7,
                        Level = 3,
                        World = 1,
                        FileName = "magic/FIRE_7.mag"
                    }
                };
                writer.Write(serializer.Serialize(moddedMagc));
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "00battle.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var magc = Kh2.Battle.Magc.Read(binarc[0].Stream);

                Assert.Equal(3, magc[0].Level);
            });
        }

        [Fact]
        public void ListPatchPrztTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "00battle.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "przt",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "PrztList.yml",
                                        Type = "przt"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "00battle.bar")).Using(stream =>
            {
                var prztEntry = new List<Kh2.Battle.Przt>()
                {
                    new Kh2.Battle.Przt
                    {
                        Id = 1,
                        SmallHpOrbs = 0,
                        BigHpOrbs = 1
                    }
                };

                using var prztStream = new MemoryStream();
                Kh2.Battle.Przt.Write(prztStream, prztEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "przt",
                        Type = Bar.EntryType.List,
                        Stream = prztStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "PrztList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                var serializer = new Serializer();
                var moddedPrzt = new List<Kh2.Battle.Przt>{
                    new Kh2.Battle.Przt
                    {
                        Id = 1,
                        SmallHpOrbs = 0,
                        BigHpOrbs = 1
                    }
                };
                writer.Write(serializer.Serialize(moddedPrzt));
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            AssertFileExists(ModOutputDir, "00battle.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var przt = Kh2.Battle.Przt.Read(binarc[0].Stream);

                Assert.Equal(1, przt[0].BigHpOrbs);
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
                Assert.Fail($"File not found '{filePath}'");
        }

        private static void AssertBarFile(string name, Action<Bar.Entry> assertion, params string[] paths)
        {
            var filePath = Path.Join(paths);
            var entries = File.OpenRead(filePath).Using(x =>
            {
                if (!Bar.IsValid(x))
                    Assert.Fail($"Not a valid BinArc");
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

using OpenKh.Bbs;
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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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
                writer.WriteLine(" section .text");
                writer.WriteLine("TR0:");
                writer.WriteLine(" ret");
                writer.WriteLine("DUMMY:");
                writer.WriteLine(" ret");
                writer.Flush();
            });
            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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
            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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
        public void ListPatchShopTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata
            {
                Assets = new List<AssetFile>
                {
                    new AssetFile
                    {
                        Name = "03system.bin",
                        Method = "binarc",
                        Source = new List<AssetFile>
                        {
                            new AssetFile
                            {
                                Name = "shop",
                                Method = "listpatch",
                                Type = "Unknown41",
                                Source = new List<AssetFile>
                                {
                                    new AssetFile
                                    {
                                        Name = "ShopList.yml",
                                        Type = "shop"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "03system.bin")).Using(stream =>
            {
                var shop = new OpenKh.Kh2.SystemData.Shop
                {
                    ShopEntries = new List<OpenKh.Kh2.SystemData.Shop.ShopEntry>
                    {
                        new OpenKh.Kh2.SystemData.Shop.ShopEntry
                        {
                            CommandArgument = 0x0067,
                            UnlockMenuFlag = 0x0029,
                            NameID = 0x8AB8,
                            ShopKeeperEntityID = 0x0539,
                            PosX = 0x008B,
                            PosY = 0x006C,
                            PosZ = -576, //0xFDC0,
                            ExtraInventoryBitMask = 0x81,
                            SoundID = 0x01,
                            InventoryCount = 0x0001,
                            ShopID = 0x00,
                            Unk19 = 0x02,
                            InventoryOffset = 0x0028,
                            Reserved = 0x00
                        }
                    },
                    InventoryEntries = new List<OpenKh.Kh2.SystemData.Shop.InventoryEntry>
                    {
                        new OpenKh.Kh2.SystemData.Shop.InventoryEntry
                        {
                            UnlockEventID = 0xFFFF,
                            ProductCount = 0x0002,
                            ProductOffset = 0x0030,
                            Reserved = 0x00
                        }
                    },
                    ProductEntries = new List<OpenKh.Kh2.SystemData.Shop.ProductEntry>
                    {
                        new OpenKh.Kh2.SystemData.Shop.ProductEntry
                        {
                            ItemID = 0x0094
                        },
                        new OpenKh.Kh2.SystemData.Shop.ProductEntry
                        {
                            ItemID = 0x008B
                        },
                    },
                    ValidProductEntries = new List<OpenKh.Kh2.SystemData.Shop.ProductEntry>
                    {
                        new OpenKh.Kh2.SystemData.Shop.ProductEntry
                        {
                            ItemID = 0x0094
                        },
                        new OpenKh.Kh2.SystemData.Shop.ProductEntry
                        {
                            ItemID = 0x008B
                        },
                        new OpenKh.Kh2.SystemData.Shop.ProductEntry
                        {
                            ItemID = 0x0000
                        },
                        new OpenKh.Kh2.SystemData.Shop.ProductEntry
                        {
                            ItemID = 0x0000
                        },
                        new OpenKh.Kh2.SystemData.Shop.ProductEntry
                        {
                            ItemID = 0x0000
                        },
                        new OpenKh.Kh2.SystemData.Shop.ProductEntry
                        {
                            ItemID = 0x0000
                        }
                    }
                };

                using var shopStream = new MemoryStream();
                OpenKh.Kh2.SystemData.Shop.Write(shopStream, shop);
                Bar.Write(stream, new Bar {
                    new Bar.Entry
                    {
                        Name = "shop",
                        Type = Bar.EntryType.Unknown41,
                        Stream = shopStream
                    }
                });
            });

            var moddedShop = new OpenKh.Kh2.SystemData.Shop.ShopHelper
            {
                ShopEntryHelpers = new List<OpenKh.Kh2.SystemData.Shop.ShopEntryHelper>
                {
                    new OpenKh.Kh2.SystemData.Shop.ShopEntryHelper
                    {
                        CommandArgument = 0x0068,
                        UnlockMenuFlag = 0x002A,
                        NameID = 0x8AB9,
                        ShopKeeperEntityID = 0x0749,
                        PosX = 0x0086,
                        PosY = 0x0096,
                        PosZ = -591, // 0xFDB1,
                        ExtraInventoryBitMask = 0x82,
                        SoundID = 0x01,
                        InventoryCount = 0x0001,
                        ShopID = 0x00,
                        Unk19 = 0x02,
                        InventoryStartIndex = 0
                    }
                },
                InventoryEntryHelpers = new List<OpenKh.Kh2.SystemData.Shop.InventoryEntryHelper>
                {
                    new OpenKh.Kh2.SystemData.Shop.InventoryEntryHelper
                    {
                        InventoryIndex = 0,
                        UnlockEventID = 0xFFFF,
                        ProductCount = 0x0002,
                        ProductStartIndex = 0,
                    }
                },
                ProductEntryHelpers = new List<OpenKh.Kh2.SystemData.Shop.ProductEntryHelper>
                {
                    new OpenKh.Kh2.SystemData.Shop.ProductEntryHelper
                    {
                        ProductIndex = 0,
                        ItemID = 0x0043
                    },
                    new OpenKh.Kh2.SystemData.Shop.ProductEntryHelper
                    {
                        ProductIndex = 1,
                        ItemID = 0x0128
                    }
                },
                ValidProductEntryHelpers = new List<OpenKh.Kh2.SystemData.Shop.ProductEntryHelper>
                {
                    new OpenKh.Kh2.SystemData.Shop.ProductEntryHelper
                    {
                        ProductIndex = 0,
                        ItemID = 0x0043
                    },
                    new OpenKh.Kh2.SystemData.Shop.ProductEntryHelper
                    {
                        ProductIndex = 1,
                        ItemID = 0x0128
                    }
                }
            };

            File.Create(Path.Combine(ModInputDir, "ShopList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);

                var serializer = new Serializer();

                writer.Write(serializer.Serialize(moddedShop));
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "03system.bin");

            File.OpenRead(Path.Combine(ModOutputDir, "03system.bin")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var verifyShop = OpenKh.Kh2.SystemData.Shop.Read(binarc[0].Stream);
                uint inventoryEntriesBaseOffset = (uint)(OpenKh.Kh2.SystemData.Shop.HeaderSize + verifyShop.ShopEntries.Count * OpenKh.Kh2.SystemData.Shop.ShopEntrySize);
                uint productEntriesBaseOffset = (uint)(inventoryEntriesBaseOffset + verifyShop.InventoryEntries.Count * OpenKh.Kh2.SystemData.Shop.InventoryEntrySize);
                foreach (var shopEntryHelper in moddedShop.ShopEntryHelpers)
                {
                    int shopID = shopEntryHelper.ShopID;
                    Assert.Equal(verifyShop.ShopEntries[shopID].CommandArgument, shopEntryHelper.CommandArgument);
                    Assert.Equal(verifyShop.ShopEntries[shopID].UnlockMenuFlag, shopEntryHelper.UnlockMenuFlag);
                    Assert.Equal(verifyShop.ShopEntries[shopID].NameID, shopEntryHelper.NameID);
                    Assert.Equal(verifyShop.ShopEntries[shopID].ShopKeeperEntityID, shopEntryHelper.ShopKeeperEntityID);
                    Assert.Equal(verifyShop.ShopEntries[shopID].PosX, shopEntryHelper.PosX);
                    Assert.Equal(verifyShop.ShopEntries[shopID].PosY, shopEntryHelper.PosY);
                    Assert.Equal(verifyShop.ShopEntries[shopID].PosZ, shopEntryHelper.PosZ);
                    Assert.Equal(verifyShop.ShopEntries[shopID].ExtraInventoryBitMask, shopEntryHelper.ExtraInventoryBitMask);
                    Assert.Equal(verifyShop.ShopEntries[shopID].SoundID, shopEntryHelper.SoundID);
                    Assert.Equal(verifyShop.ShopEntries[shopID].InventoryCount, shopEntryHelper.InventoryCount);
                    Assert.Equal(verifyShop.ShopEntries[shopID].ShopID, shopEntryHelper.ShopID);
                    Assert.Equal(verifyShop.ShopEntries[shopID].Unk19, shopEntryHelper.Unk19);
                    Assert.Equal(verifyShop.ShopEntries[shopID].InventoryOffset, (int)(inventoryEntriesBaseOffset + shopEntryHelper.InventoryStartIndex * OpenKh.Kh2.SystemData.Shop.InventoryEntrySize));
                }
                foreach (var inventoryEntryHelper in moddedShop.InventoryEntryHelpers)
                {
                    Assert.Equal(verifyShop.InventoryEntries[inventoryEntryHelper.InventoryIndex].UnlockEventID, inventoryEntryHelper.UnlockEventID);
                    Assert.Equal(verifyShop.InventoryEntries[inventoryEntryHelper.InventoryIndex].ProductCount, inventoryEntryHelper.ProductCount);
                    Assert.Equal(verifyShop.InventoryEntries[inventoryEntryHelper.InventoryIndex].ProductOffset, (int)(productEntriesBaseOffset + inventoryEntryHelper.ProductStartIndex * OpenKh.Kh2.SystemData.Shop.ProductEntrySize));
                }
                foreach (var productEntryHelper in moddedShop.ProductEntryHelpers)
                {
                    Assert.Equal(verifyShop.ProductEntries[productEntryHelper.ProductIndex].ItemID, productEntryHelper.ItemID);
                }
                foreach (var productEntryHelper in moddedShop.ValidProductEntryHelpers)
                {
                    Assert.Equal(verifyShop.ValidProductEntries[productEntryHelper.ProductIndex].ItemID, productEntryHelper.ItemID);
                }
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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

        [Fact] //Fixed, needed to initialize the BGMSet & Reserved bytes.
        public void ListPatchArifTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
        {
            new AssetFile()
            {
                Name = "03system.bin",
                Method = "binarc",
                Source = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "arif",
                        Method = "listpatch",
                        Type = "List",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "ArifList.yml",
                                Type = "arif"
                            }
                        }
                    }
                }
            }
        }
            };

            using (var stream = File.Create(Path.Combine(AssetsInputDir, "03system.bin")))
            {
                var arifEntry = new Kh2.SystemData.Arif
                {
                    Flags = Kh2.SystemData.Arif.ArifFlags.IsKnownArea,
                    Reverb = 0,
                    SoundEffectBank1 = 13,
                    SoundEffectBank2 = 0,
                    Bgms = Enumerable.Range(0, 8).Select(_ => new Kh2.SystemData.BgmSet { BgmField = 0, BgmBattle = 0 }).ToArray(),
                    Voice = 0,
                    NavigationMapItem = 0,
                    Command = 0,
                    Reserved = new byte[11]
                };

                using (var arifStream = new MemoryStream())
                {
                    Kh2.SystemData.Arif.Write(arifStream, new List<List<Kh2.SystemData.Arif>> { new List<Kh2.SystemData.Arif> { arifEntry } });
                    Bar.Write(stream, new Bar
            {
                new Bar.Entry()
                {
                    Name = "arif",
                    Type = Bar.EntryType.List,
                    Stream = arifStream
                }
            });
                }
            }

            File.Create(Path.Combine(ModInputDir, "ArifList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("EndOfSea:");
                writer.WriteLine("  1:");
                writer.WriteLine("    SoundEffectBank1: 13");
                writer.WriteLine("    Voice: 0");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "03system.bin");

            File.OpenRead(Path.Combine(ModOutputDir, "03system.bin")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var arifList = Kh2.SystemData.Arif.Read(binarc[0].Stream);
                var arifEntry = arifList[0][0];
                Assert.Equal(13, arifEntry.SoundEffectBank1);
                Assert.Equal(0, arifEntry.Voice);
            });
        }

        [Fact]
        public void ListPatchMemtTest()
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
                        Name = "memt",
                        Method = "listpatch",
                        Type = "List",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "MemtList.yml",
                                Type = "memt"
                            }
                        }
                    }
                }
            }
        }
            };

            // Create the 03system.bar file with initial data
            File.Create(Path.Combine(AssetsInputDir, "03system.bar")).Using(stream =>
            {
                var memtEntries = new List<Kh2.SystemData.Memt.EntryFinalMix>()
        {
            new Kh2.SystemData.Memt.EntryFinalMix
            {
                WorldId = 1,
                CheckStoryFlag = 1,
                CheckStoryFlagNegation = 0,
                CheckArea = 0,
                Padding = 0,
                PlayerSize= 0,
                FriendSize = 0,
                Members = new short[18]
            }
        };

                var memberIndices = new Kh2.SystemData.Memt.MemberIndices[7]; // Ensure there are 7 MemberIndices
                for (int i = 0; i < memberIndices.Length; i++)
                {
                    memberIndices[i] = new Kh2.SystemData.Memt.MemberIndices
                    {
                        Player = 0,
                        Friend1 = 0,
                        Friend2 = 0,
                        FriendWorld = 0
                    };
                }

                using var memtStream = new MemoryStream();
                var memt = new Kh2.SystemData.Memt();
                memt.Entries.AddRange(memtEntries.Cast<Kh2.SystemData.Memt.IEntry>());

                Kh2.SystemData.Memt.Write(memtStream, memt);
                memtStream.Seek(0, SeekOrigin.Begin);

                Bar.Write(stream, new Bar()
        {
            new Bar.Entry()
            {
                Name = "memt",
                Type = Bar.EntryType.List,
                Stream = memtStream
            }
        });
            });

            // Create the MemtList.yml patch file
            File.Create(Path.Combine(ModInputDir, "MemtList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("MemtEntries:");
                writer.WriteLine("  - Index: 0");
                writer.WriteLine("    WorldId: 2");
                writer.WriteLine("    CheckStoryFlag: 3");
                writer.WriteLine("    CheckStoryFlagNegation: 4");
                writer.WriteLine("    CheckArea: 5");
                writer.WriteLine("    Padding: 6");
                writer.WriteLine("    PlayerSize: 7");
                writer.WriteLine("    FriendSize: 8");
                writer.WriteLine("    Members: [10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27]");
                writer.WriteLine("MemberIndices:");
                writer.WriteLine("  - Index: 0");
                writer.WriteLine("    Player: 0");
                writer.WriteLine("    Friend1: 0");
                writer.WriteLine("    Friend2: 0");
                writer.WriteLine("    FriendWorld: 0");
                writer.Flush();
            });

            // Apply the patch
            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir);

            // Verify the patched data
            AssertFileExists(ModOutputDir, "03system.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "03system.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var memtStream = binarc[0].Stream;
                memtStream.Seek(0, SeekOrigin.Begin);

                var memt = Kh2.SystemData.Memt.Read(memtStream);

                var memtEntry = memt.Entries.Cast<Kh2.SystemData.Memt.EntryFinalMix>().First();
                Assert.Equal((short)2, memtEntry.WorldId);
                Assert.Equal((short)3, memtEntry.CheckStoryFlag);
                Assert.Equal((short)4, memtEntry.CheckStoryFlagNegation);
                Assert.Equal((short)5, memtEntry.CheckArea);
                Assert.Equal((short)6, memtEntry.Padding);
                Assert.Equal((short)7, memtEntry.PlayerSize);
                Assert.Equal((short)8, memtEntry.FriendSize);
                Assert.Equal(new short[] { 10, 11, 12, 13, 14, 15, 16, 17, 18, 19, 20, 21, 22, 23, 24, 25, 26, 27 }, memtEntry.Members);

                // Check MemberIndices deserialization
                var memberIndices = memt.MemberIndexCollection.First();
                Assert.Equal((byte)0, memberIndices.Player);
                Assert.Equal((byte)0, memberIndices.Friend1);
                Assert.Equal((byte)0, memberIndices.Friend2);
                Assert.Equal((byte)0, memberIndices.FriendWorld);
            });
        }




        [Fact]
        public void ListPatchFmabTest()
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
                                Name = "pref",
                                Method = "binarc",
                                Type = "Binary",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "fmab",
                                        Method = "listpatch",
                                        Type = "list",
                                        Source = new List<AssetFile>()
                                        {
                                            new AssetFile()
                                            {
                                                Name = "FmabList.yml",
                                                Type = "fmab"
                                            }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            };

            File.Create(Path.Combine(AssetsInputDir, "03system.bar")).Using(stream =>
            {
                var FmabEntry = new List<Kh2.SystemData.Fmab>()
                {
                    new Kh2.SystemData.Fmab
                    {
                        HighJumpHeight = 178,
                        AirDodgeHeight = 86
                    }
                };

                using var fmabStream = new MemoryStream();
                Kh2.SystemData.Fmab.Write(fmabStream, FmabEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "fmab",
                        Type = Bar.EntryType.List,
                        Stream = fmabStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "FmabList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("- HighJumpHeight: 178");
                writer.WriteLine("  AirDodgeHeight: 86");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "03system.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "03system.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var fmabStream = Kh2.SystemData.Fmab.Read(binarc[0].Stream);
                Assert.Equal(178, fmabStream[0].HighJumpHeight);
                Assert.Equal(86, fmabStream[0].AirDodgeHeight);
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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "00battle.bin");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bin")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var lvupStream = Kh2.Battle.Lvup.Read(binarc[0].Stream);
                Assert.Equal(500, lvupStream.Characters[0].Levels[0].Exp);
            });

        }

        [Fact]
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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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
        void ListPatchLvpmTest()
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
                                Name = "lvpm",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "LvpmList.yml",
                                        Type = "lvpm"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "00battle.bar")).Using(stream =>
            {
                var lvpmEntry = new List<Kh2.Battle.Lvpm>()
                {
                    new Kh2.Battle.Lvpm
                    {
                        HpMultiplier = 89,
                        Strength = 77,
                        Defense = 88,
                        MaxStrength = 40,
                        MinStrength = 32,
                        Experience = 777
                    }
                };

                using var lvpmStream = new MemoryStream();
                Kh2.Battle.Lvpm.Write(lvpmStream, lvpmEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "lvpm",
                        Type = Bar.EntryType.List,
                        Stream = lvpmStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "LvpmList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);

                var serializer = new Serializer();
                var moddedLvpm = new List<Kh2.Battle.LvpmHelper>{
                    new Kh2.Battle.LvpmHelper
                    {
                        Level = 0,
                        HpMultiplier = 89,
                        Strength = 77,
                        Defense = 88,
                        MaxStrength = 40,
                        MinStrength = 32,
                        Experience = 777
                    }
                };
                writer.Write(serializer.Serialize(moddedLvpm));
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "00battle.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var lvpmStream = Kh2.Battle.Lvpm.Read(binarc[0].Stream, 1);
                var helperList = Kh2.Battle.LvpmHelper.ConvertLvpmListToHelper(lvpmStream);
                Assert.Equal(0, helperList[0].Level);
                Assert.Equal(89, helperList[0].HpMultiplier);
                Assert.Equal(77, helperList[0].Strength);
                Assert.Equal(88, helperList[0].Defense);
                Assert.Equal(40, helperList[0].MaxStrength);
                Assert.Equal(32, helperList[0].MinStrength);
                Assert.Equal(777, helperList[0].Experience);
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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "00battle.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var przt = Kh2.Battle.Przt.Read(binarc[0].Stream);

                Assert.Equal(1, przt[0].BigHpOrbs);
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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "00battle.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var magc = Kh2.Battle.Magc.Read(binarc[0].Stream);

                Assert.Equal(3, magc[0].Level);
            });
        }

        [Fact]
        public void ListPatchLimtTest()
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
                        Name = "Limt",
                        Method = "listpatch",
                        Type = "List",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "LimtList.yml",
                                Type = "limt"
                            }
                        }
                    }
                }
            }
        }
            };

            // Create the initial 00battle.bar file with Limt entry
            File.Create(Path.Combine(AssetsInputDir, "00battle.bar")).Using(stream =>
            {
                var limtEntry = new List<Kh2.Battle.Limt>()
        {
            new Kh2.Battle.Limt
            {
                Id = 0,
                Character = Kh2.Battle.Limt.Characters.Auron,
                Summon = Kh2.Battle.Limt.Characters.Sora,
                Group = 0,
                FileName = "auron.bar",
                SpawnId = 0,
                Command = 82,
                Limit = 204,
                World = 0,
                Padding = new byte[18]
            }
        };

                using var limtStream = new MemoryStream();
                Kh2.Battle.Limt.Write(limtStream, limtEntry);
                limtStream.Position = 0; // Ensure stream position is reset before writing to Bar
                Bar.Write(stream, new Bar() {
            new Bar.Entry()
            {
                Name = "Limt",
                Type = Bar.EntryType.List,
                Stream = limtStream
            }
        });
            });

            // Create the LimtList.yml file using the serializer
            File.Create(Path.Combine(ModInputDir, "LimtList.yml")).Using(stream =>
            {
                using var writer = new StreamWriter(stream);
                var moddedLimt = new List<Kh2.Battle.Limt>
        {
            new Kh2.Battle.Limt
            {
                Id = 0,
                Character = Kh2.Battle.Limt.Characters.Auron,
                Summon = Kh2.Battle.Limt.Characters.Sora,
                Group = 0,
                FileName = "auron.bar",
                SpawnId = 0,
                Command = 82,
                Limit = 204,
                World = 0,
                Padding = new byte[18]
            }
        };
                writer.Write(serializer.Serialize(moddedLimt));
                writer.Flush();
            });

            // Apply the patch
            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            // Verify the output file exists
            AssertFileExists(ModOutputDir, "00battle.bar");

            // Read and validate the output file
            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var limt = Kh2.Battle.Limt.Read(binarc[0].Stream);

                Assert.Equal(0, limt[0].Id);
                Assert.Equal(Kh2.Battle.Limt.Characters.Auron, limt[0].Character);
                Assert.Equal(Kh2.Battle.Limt.Characters.Sora, limt[0].Summon);
                Assert.Equal(0, limt[0].Group);
                Assert.Equal("auron.bar", limt[0].FileName.Trim());
                Assert.Equal(0u, limt[0].SpawnId);
                Assert.Equal(82, limt[0].Command);
                Assert.Equal(204, limt[0].Limit);
                Assert.Equal(0, limt[0].World);
                Assert.Equal(new byte[18], limt[0].Padding);
            });
        }


        [Fact]
        public void ListPatchBtlvTest()
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
                                Name = "btlv",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "BtlvList.yml",
                                        Type = "btlv"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "00battle.bar")).Using(stream =>
            {
                var btlvEntry = new List<Kh2.Battle.Btlv>()
                {
                    new Kh2.Battle.Btlv
                    {
                        Id = 0,
                        ProgressFlag = 3,
                        WorldZZ = 1,
                        Padding = new byte[5],
                    }
                };

                using var btlvStream = new MemoryStream();
                Kh2.Battle.Btlv.Write(btlvStream, btlvEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "btlv",
                        Type = Bar.EntryType.List,
                        Stream = btlvStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "BtlvList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                var serializer = new Serializer();
                var moddedBtlv = new List<Kh2.Battle.Btlv>{
                    new Kh2.Battle.Btlv
                    {
                        Id = 0,
                        ProgressFlag = 3,
                        WorldZZ = 1,
                        Padding = new byte[5],

                    }
                };
                writer.Write(serializer.Serialize(moddedBtlv));
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "00battle.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var btlv = Kh2.Battle.Btlv.Read(binarc[0].Stream);

                Assert.Equal(3, btlv[0].ProgressFlag);
            });
        }

        [Fact]
        public void ListPatchVtblTest()
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
                        Name = "vtbl",
                        Method = "listpatch",
                        Type = "List",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "VtblList.yml",
                                Type = "vtbl"
                            }
                        }
                    }
                }
            }
        }
            };

            File.Create(Path.Combine(AssetsInputDir, "00battle.bar")).Using(stream =>
            {
                var vtblEntry = new List<Kh2.Battle.Vtbl>()
        {
            new Kh2.Battle.Vtbl
            {
                Id = 0,
                CharacterId = 1,
                Priority = 1,
                Reserved = 0,
                Voices = new List<Kh2.Battle.Vtbl.Voice>
                {
                    new Kh2.Battle.Vtbl.Voice { VsbIndex = 0, Weight = 0 }
                }
            }
        };

                using var vtblStream = new MemoryStream();
                Kh2.Battle.Vtbl.Write(vtblStream, vtblEntry);
                Bar.Write(stream, new Bar() {
            new Bar.Entry()
            {
                Name = "vtbl",
                Type = Bar.EntryType.List,
                Stream = vtblStream
            }
        });
            });

            File.Create(Path.Combine(ModInputDir, "VtblList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                var moddedVtbl = new List<Kh2.Battle.Vtbl>{
            new Kh2.Battle.Vtbl
            {
                Id = 0,
                CharacterId = 1,
                Priority = 1,
                Reserved = 0,
                Voices = new List<Kh2.Battle.Vtbl.Voice>
                {
                    new Kh2.Battle.Vtbl.Voice { VsbIndex = 0, Weight = 0 }
                }
            }
        };
                writer.Write(serializer.Serialize(moddedVtbl));
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "00battle.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "00battle.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var vtbl = Kh2.Battle.Vtbl.Read(binarc[0].Stream);

                Assert.Equal(1, vtbl[0].Priority);
            });
        }



        [Fact] //Libretto test.
        public void ListPatchLibrettoTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
        {
            new AssetFile()
            {
                Name = "libretto-ca.bar",
                Method = "binarc",
                Source = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "ca",
                        Method = "listpatch",
                        Type = "List",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "LibrettoList.yml",
                                Type = "libretto"
                            }
                        }
                    }
                }
            }
        }
            };

            using (var stream = File.Create(Path.Combine(AssetsInputDir, "libretto-ca.bar")))
            {
                var librettoEntry = new Kh2.Libretto
                {
                    MagicCode = 0x03, // "LIBR" in ASCII
                    Count = 1,
                    Definitions = new List<Kh2.Libretto.TalkMessageDefinition>
            {
                new Kh2.Libretto.TalkMessageDefinition
                {
                    TalkMessageId = 1,
                    Type = 0,
                    ContentPointer = 8 + 8 // MagicCode (4 bytes) + Count (4 bytes) + Definitions (8 bytes each)
                }
            },
                    Contents = new List<List<Kh2.Libretto.TalkMessageContent>>
            {
                new List<Kh2.Libretto.TalkMessageContent>
                {
                    new Kh2.Libretto.TalkMessageContent { CodeType = 0x0200, Unknown = 0x0200, TextId = 2500 }
                }
            }
                };

                using (var librettoStream = new MemoryStream())
                {
                    Kh2.Libretto.Write(librettoStream, librettoEntry);
                    Bar.Write(stream, new Bar
            {
                new Bar.Entry()
                {
                    Name = "libretto",
                    Type = Bar.EntryType.List,
                    Stream = librettoStream
                }
            });
                }
            }

            File.Create(Path.Combine(ModInputDir, "LibrettoList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("- TalkMessageId: 1");
                writer.WriteLine("  Type: 0");
                writer.WriteLine("  Contents:");
                writer.WriteLine("    - CodeType: 0x0200");
                writer.WriteLine("      Unknown: 0x0200");
                writer.WriteLine("      TextId: 2500");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "libretto-ca.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "libretto-ca.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var librettoList = Kh2.Libretto.Read(binarc[0].Stream);
                var librettoEntry = librettoList.Contents[0][0];
                Assert.Equal(0x0200u, librettoEntry.CodeType);
                Assert.Equal(0x0200u, librettoEntry.Unknown);
                Assert.Equal(2500u, librettoEntry.TextId);
            });
        }

        [Fact]
        public void ListPatchLocalsetTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "07localset.bin",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "loca",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "LocalsetList.yml",
                                        Type = "localset"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "07localset.bin")).Using(stream =>
            {
                var localEntry = new List<Kh2.Localset>()
                {
                    new Kh2.Localset
                    {
                        ProgramId = 1300,
                        MapNumber = 6,
                    }
                };

                using var localStream = new MemoryStream();
                Kh2.Localset.Write(localStream, localEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "loca",
                        Type = Bar.EntryType.List,
                        Stream = localStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "LocalsetList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("- ProgramId: 1300");
                writer.WriteLine("  MapNumber: 6");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "07localset.bin");

            File.OpenRead(Path.Combine(ModOutputDir, "07localset.bin")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var localStream = Kh2.Localset.Read(binarc[0].Stream);
                Assert.Equal(1300u, localStream[0].ProgramId);
                Assert.Equal(6u, localStream[0].MapNumber);
            });
        }

        [Fact]
        public void ListPatchJigsawTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "15jigsaw.bin",
                        Method = "listpatch",
                        Type = "List",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "JigsawList.yml",
                                Type = "jigsaw",
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "15jigsaw.bin")).Using(stream =>
            {
                var jigsawEntry = new List<Kh2.Jigsaw>()
                {
                    new Kh2.Jigsaw
                    {
                        Picture = Kh2.Jigsaw.PictureName.Duality,
                        Part = 2,
                        Text = 1500
                    }
                    };
                Kh2.Jigsaw.Write(stream, jigsawEntry);
            });

            File.Create(Path.Combine(ModInputDir, "JigsawList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("- Picture: Duality");
                writer.WriteLine("  Part: 2");
                writer.WriteLine("  Text: 1500");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "15jigsaw.bin");

            File.OpenRead(Path.Combine(ModOutputDir, "15jigsaw.bin")).Using(stream =>
            {
                var jigsawStream = Kh2.Jigsaw.Read(stream);
                Assert.Equal(2, jigsawStream[0].Part);
            });

        }

        [Fact]
        public void ListPatchPlacesTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "place.bin",
                        Method = "listpatch",
                        Type = "List",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "PlaceList.yml",
                                Type = "place",
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "place.bin")).Using(stream =>
            {
                var placeEntry = new List<Kh2.Places>()
                {
                    new Kh2.Places
                    {
                        MessageId = 100,
                        Padding = 0,
                    }
                    };
                Kh2.Places.Write(stream, placeEntry);
            });

            File.Create(Path.Combine(ModInputDir, "PlaceList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("- MessageId: 100");
                writer.WriteLine("  Padding: 0");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "place.bin");

            File.OpenRead(Path.Combine(ModOutputDir, "place.bin")).Using(stream =>
            {
                var placesStream = Kh2.Places.Read(stream);
                Assert.Equal(100, placesStream[0].MessageId);
            });

        }

        [Fact]
        public void ListPatchSoundInfoTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "12soundinfo.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "zz",
                                Method = "listpatch",
                                Type = "List",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "SoundInfoList.yml",
                                        Type = "soundinfo"
                                    }
                                }
                            }
                        }
                    }
                }
            };
            File.Create(Path.Combine(AssetsInputDir, "12soundinfo.bar")).Using(stream =>
            {
                var soundinfoEntry = new List<Kh2.Soundinfo>()
                {
                    new Kh2.Soundinfo
                    {
                        Reverb = -1,
                        Rate = 1,
                    }
                    };
                Kh2.Soundinfo.Write(stream, soundinfoEntry);
            });

            File.Create(Path.Combine(ModInputDir, "SoundInfoList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("- Reverb: -1");
                writer.WriteLine("  Rate: 1");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "12soundinfo.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "12soundinfo.bar")).Using(stream =>
            {
                var soundinfoStream = Kh2.Soundinfo.Read(stream);
                Assert.Equal(1, soundinfoStream[0].Rate);
            });

        }

        [Fact]
        public void ListPatchMixdataReciTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "mixdata.bar",
                        Method = "binarc",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "reci",
                                Method = "synthpatch",
                                Type = "Synthesis",
                                Source = new List<AssetFile>()
                                {
                                    new AssetFile()
                                    {
                                        Name = "ReciList.yml",
                                        Type = "recipe"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            File.Create(Path.Combine(AssetsInputDir, "mixdata.bar")).Using(stream =>
            {
                var recipeEntry = new List<Kh2.Mixdata.ReciLP>()
                {
                    new Kh2.Mixdata.ReciLP
                    {
                        Id = 1,
                        Unlock = 0,
                        Rank = 0,
                    }
                    };
                using var recipeStream = new MemoryStream();
                Kh2.Mixdata.ReciLP.Write(recipeStream, recipeEntry);
                Bar.Write(stream, new Bar() {
                    new Bar.Entry()
                    {
                        Name = "reci",
                        Type = Bar.EntryType.List,
                        Stream = recipeStream
                    }
                });
            });

            File.Create(Path.Combine(ModInputDir, "ReciList.yml")).Using(stream =>
            {
                var writer = new StreamWriter(stream);
                writer.WriteLine("- Id: 1");
                writer.WriteLine("  Rank: 0");
                writer.Flush();
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, "mixdata.bar");

            File.OpenRead(Path.Combine(ModOutputDir, "mixdata.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var recipeStream = Kh2.Mixdata.ReciLP.Read(binarc[0].Stream);
                Assert.Equal(1, recipeStream[0].Id);
                Assert.Equal(0, recipeStream[0].Rank);
            });

        }

        [Fact]
        public void ListPatchMixdataLeveLPTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
        {
            new AssetFile()
            {
                Name = "mixdata.bar",
                Method = "binarc",
                Source = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "leve",
                        Method = "synthpatch",
                        Type = "Synthesis",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "LeveList.yml",
                                Type = "level"
                            }
                        }
                    }
                }
            }
        }
            };

            // Create the initial mixdata.bar file with LeveLP entry
            File.Create(Path.Combine(AssetsInputDir, "mixdata.bar")).Using(stream =>
            {
                var leveEntry = new List<Kh2.Mixdata.LeveLP>()
        {
            new Kh2.Mixdata.LeveLP
            {
                Title = 1,
                Stat = 2,
                Enable = 1,
                Padding = 0,
                Exp = 100
            }
        };

                using var leveStream = new MemoryStream();
                Kh2.Mixdata.LeveLP.Write(leveStream, leveEntry);
                leveStream.Position = 0; // Ensure stream position is reset before writing to Bar
                Bar.Write(stream, new Bar() {
            new Bar.Entry()
            {
                Name = "leve",
                Type = Bar.EntryType.List,
                Stream = leveStream
            }
        });
            });

            // Create the LeveList.yml file using the serializer
            File.Create(Path.Combine(ModInputDir, "LeveList.yml")).Using(stream =>
            {
                using var writer = new StreamWriter(stream);
                var moddedLeve = new List<Kh2.Mixdata.LeveLP>
        {
            new Kh2.Mixdata.LeveLP
            {
                Title = 1,
                Stat = 2,
                Enable = 1,
                Padding = 0,
                Exp = 100
            }
        };
                writer.Write(serializer.Serialize(moddedLeve));
                writer.Flush();
            });

            // Apply the patch
            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            // Verify the output file exists
            AssertFileExists(ModOutputDir, "mixdata.bar");

            // Read and validate the output file
            File.OpenRead(Path.Combine(ModOutputDir, "mixdata.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var leveStream = Kh2.Mixdata.LeveLP.Read(binarc[0].Stream);

                Assert.Equal(1, leveStream[0].Title);
                Assert.Equal(2, leveStream[0].Stat);
                Assert.Equal(1, leveStream[0].Enable);
                Assert.Equal(0, leveStream[0].Padding);
                Assert.Equal(100, leveStream[0].Exp);
            });
        }

        [Fact]
        public void ListPatchMixdataCondLPTest()
        {
            var patcher = new PatcherProcessor();
            var serializer = new Serializer();
            var patch = new Metadata()
            {
                Assets = new List<AssetFile>()
        {
            new AssetFile()
            {
                Name = "mixdata.bar",
                Method = "binarc",
                Source = new List<AssetFile>()
                {
                    new AssetFile()
                    {
                        Name = "cond",
                        Method = "synthpatch",
                        Type = "Synthesis",
                        Source = new List<AssetFile>()
                        {
                            new AssetFile()
                            {
                                Name = "CondList.yml",
                                Type = "condition"
                            }
                        }
                    }
                }
            }
        }
            };

            // Create the initial mixdata.bar file with CondLP entry
            File.Create(Path.Combine(AssetsInputDir, "mixdata.bar")).Using(stream =>
            {
                var condEntry = new List<Kh2.Mixdata.CondLP>()
        {
            new Kh2.Mixdata.CondLP
            {
                TextId = 1,
                Reward = 100,
                Type = Kh2.Mixdata.CondLP.RewardType.Item,
                MaterialType = 0,
                MaterialRank = 1,
                ItemCollect = Kh2.Mixdata.CondLP.CollectionType.Stack,
                Count = 10,
                ShopUnlock = 5
            }
        };

                using var condStream = new MemoryStream();
                Kh2.Mixdata.CondLP.Write(condStream, condEntry);
                condStream.Position = 0; // Ensure stream position is reset before writing to Bar
                Bar.Write(stream, new Bar() {
            new Bar.Entry()
            {
                Name = "cond",
                Type = Bar.EntryType.List,
                Stream = condStream
            }
        });
            });

            // Create the CondList.yml file using the serializer
            File.Create(Path.Combine(ModInputDir, "CondList.yml")).Using(stream =>
            {
                using var writer = new StreamWriter(stream);
                var moddedCond = new List<Kh2.Mixdata.CondLP>
        {
            new Kh2.Mixdata.CondLP
            {
                TextId = 1,
                Reward = 100,
                Type = Kh2.Mixdata.CondLP.RewardType.Item,
                MaterialType = 0,
                MaterialRank = 1,
                ItemCollect = Kh2.Mixdata.CondLP.CollectionType.Stack,
                Count = 10,
                ShopUnlock = 5
            }
        };
                writer.Write(serializer.Serialize(moddedCond));
                writer.Flush();
            });

            // Apply the patch
            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            // Verify the output file exists
            AssertFileExists(ModOutputDir, "mixdata.bar");

            // Read and validate the output file
            File.OpenRead(Path.Combine(ModOutputDir, "mixdata.bar")).Using(stream =>
            {
                var binarc = Bar.Read(stream);
                var condStream = Kh2.Mixdata.CondLP.Read(binarc[0].Stream);

                Assert.Equal(1, condStream[0].TextId);
                Assert.Equal(100, condStream[0].Reward);
                Assert.Equal(Kh2.Mixdata.CondLP.RewardType.Item, condStream[0].Type);
                Assert.Equal(0, condStream[0].MaterialType);
                Assert.Equal(1, condStream[0].MaterialRank);
                Assert.Equal(Kh2.Mixdata.CondLP.CollectionType.Stack, condStream[0].ItemCollect);
                Assert.Equal(10, condStream[0].Count);
                Assert.Equal(5, condStream[0].ShopUnlock);
            });
        }


        [Fact]
        public void BbsArcCreateArcTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile> {
                    new AssetFile
                    {
                        Name = "somedir/somearc.arc",
                        Method = "bbsarc",
                        Source = new List<AssetFile> {
                            new AssetFile
                            {
                                Name = "newfile",
                                Method = "copy",
                                Source = new List<AssetFile> {
                                    new AssetFile
                                    {
                                        Name = "somedir/somearc/newfile.bin"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            CreateFile(ModInputDir, "somedir/somearc/newfile.bin").Using(x =>
            {
                x.Write(new byte[] { 4, 5, 6, 7 });
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, patch.Assets[0].Name);
            AssertArcFile("newfile", entry =>
            {
                Assert.Equal(4, entry.Data.Length);
                Assert.Equal(new byte[] { 4, 5, 6, 7 }, entry.Data);
            }, ModOutputDir, patch.Assets[0].Name);
        }

        [Fact]
        public void BbsArcAddToArcTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile> {
                    new AssetFile
                    {
                        Name = "somedir/somearc.arc",
                        Method = "bbsarc",
                        Source = new List<AssetFile> {
                            new AssetFile
                            {
                                Name = "newfile",
                                Method = "copy",
                                Source = new List<AssetFile> {
                                    new AssetFile
                                    {
                                        Name = "somedir/somearc/newfile.bin"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            CreateFile(AssetsInputDir, "somedir/somearc.arc").Using(x =>
            {
                Arc.Write(new List<Arc.Entry>
                {
                    new Arc.Entry
                    {
                        Name = "abcd",
                        Data = new byte[] {0, 1, 2, 3 }
                    }
                }, x);
            });

            CreateFile(ModInputDir, "somedir/somearc/newfile.bin").Using(x =>
            {
                x.Write(new byte[] { 4, 5, 6, 7 });
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, patch.Assets[0].Name);
            AssertArcFile("abcd", entry =>
            {
                Assert.Equal(4, entry.Data.Length);
                Assert.Equal(new byte[] { 0, 1, 2, 3 }, entry.Data);
            }, ModOutputDir, patch.Assets[0].Name);
            AssertArcFile("newfile", entry =>
            {
                Assert.Equal(4, entry.Data.Length);
                Assert.Equal(new byte[] { 4, 5, 6, 7 }, entry.Data);
            }, ModOutputDir, patch.Assets[0].Name);
        }

        [Fact]
        public void BbsArcReplaceInArcTest()
        {
            var patcher = new PatcherProcessor();
            var patch = new Metadata
            {
                Assets = new List<AssetFile> {
                    new AssetFile
                    {
                        Name = "somedir/somearc.arc",
                        Method = "bbsarc",
                        Source = new List<AssetFile> {
                            new AssetFile
                            {
                                Name = "abcd",
                                Method = "copy",
                                Source = new List<AssetFile> {
                                    new AssetFile
                                    {
                                        Name = "somedir/somearc/abcd.bin"
                                    }
                                }
                            }
                        }
                    }
                }
            };

            CreateFile(AssetsInputDir, "somedir/somearc.arc").Using(x =>
            {
                Arc.Write(new List<Arc.Entry>
                {
                    new Arc.Entry
                    {
                        Name = "abcd",
                        Data = new byte[] {0, 1, 2, 3}
                    }
                }, x);
            });

            CreateFile(ModInputDir, "somedir/somearc/abcd.bin").Using(x =>
            {
                x.Write(new byte[] { 4, 5, 6, 7 });
            });

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

            AssertFileExists(ModOutputDir, patch.Assets[0].Name);
            AssertArcFile("abcd", entry =>
            {
                Assert.Equal(4, entry.Data.Length);
                Assert.Equal(new byte[] { 4, 5, 6, 7 }, entry.Data);
            }, ModOutputDir, patch.Assets[0].Name);
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

            patcher.Patch(AssetsInputDir, ModOutputDir, patch, ModInputDir, Tests: true);

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

        private static void AssertArcFile(string name, Action<Arc.Entry> assertion, params string[] paths)
        {
            var filePath = Path.Join(paths);
            var entries = File.OpenRead(filePath).Using(x =>
            {
                if (!Arc.IsValid(x))
                    Assert.Fail($"Not a valid Arc");
                return Arc.Read(x);
            });

            var entry = entries.SingleOrDefault(x => x.Name == name);
            if (entry == null)
                throw new XunitException($"Arc Entry '{name}' not found");

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

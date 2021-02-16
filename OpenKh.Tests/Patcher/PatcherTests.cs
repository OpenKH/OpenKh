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

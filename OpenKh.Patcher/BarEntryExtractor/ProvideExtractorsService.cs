using OpenKh.Command.Bdxio.Utils;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using YamlDotNet.Serialization;

namespace OpenKh.Patcher.BarEntryExtractor
{
    public class ProvideExtractorsService
    {
#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously

        private readonly ISerializer _yamlSer = new SerializerBuilder()
            .Build();

        private List<AssetFile> CreateSourceFromArgs(params AssetFile[] assetFiles) => new List<AssetFile>(assetFiles);

        private Extractor CreateMessageExtractor(
            IEnumerable<string> tags,
            Regex sourceFilePattern,
            string language,
            IMessageDecode decoder
        )
        {
            return new Extractor(
                Tags: tags,
                IfApply: (name, type, index) => type == Bar.EntryType.List,
                SourceFileTest: relativePath => sourceFilePattern.IsMatch(relativePath),
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    var msgEntries = Msg.Read(barEntry.Stream.FromBegin());
                    var messages = new List<Dictionary<string, object>>();
                    foreach (var msgEntry in msgEntries)
                    {
                        var readableMessage = MsgSerializer.SerializeText(decoder.Decode(msgEntry.Data));
                        // The format of the dictionary needs to be matched with the patcher.
                        // Check the method: OpenKh.Patcher.PatcherProcessor.PatchKh2Msg
                        messages.Add(
                            new Dictionary<string, object>
                            {
                                { "id", msgEntry.Id },
                                { language, readableMessage }
                            }
                        );
                    }
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            messages
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "sys",
                        Type = "list",
                        Method = "kh2msg",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Language = language,
                            }
                        ),
                    }
                )
            );
        }

        public IEnumerable<Extractor> GetExtractors()
        {
            var extractors = new List<Extractor>();

            extractors.Add(new Extractor(
                Tags: new[] { "trsr", "03system.bin" },
                IfApply: (name, type, index) => name == "trsr" && type == Bar.EntryType.List,
                SourceFileTest: (relativePath) => relativePath == "03system.bin",
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            OpenKh.Kh2.SystemData.Trsr.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "trsr",
                        Type = "list",
                        Method = "listpatch",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Type = "trsr",
                            }
                        ),
                    }
                )
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "item", "03system.bin" },
                IfApply: (name, type, index) => name == "item" && type == Bar.EntryType.List,
                SourceFileTest: (relativePath) => relativePath == "03system.bin",
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            OpenKh.Kh2.SystemData.Item.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "item",
                        Type = "list",
                        Method = "listpatch",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Type = "item",
                            }
                        ),
                    }
                )
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "fmlv", "00battle.bin" },
                IfApply: (name, type, index) => name == "fmlv" && type == Bar.EntryType.List,
                SourceFileTest: (relativePath) => relativePath == "00battle.bin",
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            OpenKh.Kh2.Battle.Fmlv.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "fmlv",
                        Type = "list",
                        Method = "listpatch",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Type = "fmlv",
                            }
                        ),
                    }
                )
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "lvup", "00battle.bin" },
                IfApply: (name, type, index) => name == "lvup" && type == Bar.EntryType.List,
                SourceFileTest: (relativePath) => relativePath == "00battle.bin",
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            OpenKh.Kh2.Battle.Lvup.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "lvup",
                        Type = "list",
                        Method = "listpatch",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Type = "lvup",
                            }
                        ),
                    }
                )
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "bons", "00battle.bin" },
                IfApply: (name, type, index) => name == "bons" && type == Bar.EntryType.List,
                SourceFileTest: (relativePath) => relativePath == "00battle.bin",
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            OpenKh.Kh2.Battle.Bons.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "bons",
                        Type = "list",
                        Method = "listpatch",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Type = "bons",
                            }
                        ),
                    }
                )
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "atkp", "00battle.bin" },
                IfApply: (name, type, index) => name == "atkp" && type == Bar.EntryType.List,
                SourceFileTest: (relativePath) => relativePath == "00battle.bin",
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            OpenKh.Kh2.Battle.Atkp.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "atkp",
                        Type = "list",
                        Method = "listpatch",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Type = "atkp",
                            }
                        ),
                    }
                )
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "plrp", "00battle.bin" },
                IfApply: (name, type, index) => name == "plrp" && type == Bar.EntryType.List,
                SourceFileTest: (relativePath) => relativePath == "00battle.bin",
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            OpenKh.Kh2.Battle.Plrp.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "plrp",
                        Type = "list",
                        Method = "listpatch",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Type = "plrp",
                            }
                        ),
                    }
                )
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "cmd", "03system.bin" },
                IfApply: (name, type, index) => name == "cmd" && type == Bar.EntryType.List,
                SourceFileTest: (relativePath) => relativePath == "03system.bin",
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            OpenKh.Kh2.SystemData.Cmd.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "cmd",
                        Type = "list",
                        Method = "listpatch",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Type = "cmd",
                            }
                        ),
                    }
                )
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "enmp", "00battle.bin" },
                IfApply: (name, type, index) => name == "enmp" && type == Bar.EntryType.List,
                SourceFileTest: (relativePath) => relativePath == "00battle.bin",
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            OpenKh.Kh2.Battle.Enmp.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "enmp",
                        Type = "list",
                        Method = "listpatch",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Type = "enmp",
                            }
                        ),
                    }
                )
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "sklt", "03system.bin" },
                IfApply: (name, type, index) => name == "sklt" && type == Bar.EntryType.List,
                SourceFileTest: (relativePath) => relativePath == "03system.bin",
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            OpenKh.Kh2.SystemData.Sklt.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "sklt",
                        Type = "list",
                        Method = "listpatch",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Type = "sklt",
                            }
                        ),
                    }
                )
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "przt", "00battle.bin" },
                IfApply: (name, type, index) => name == "przt" && type == Bar.EntryType.List,
                SourceFileTest: (relativePath) => relativePath == "00battle.bin",
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            OpenKh.Kh2.Battle.Przt.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "przt",
                        Type = "list",
                        Method = "listpatch",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Type = "przt",
                            }
                        ),
                    }
                )
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "magc", "00battle.bin" },
                IfApply: (name, type, index) => name == "magc" && type == Bar.EntryType.List,
                SourceFileTest: (relativePath) => relativePath == "00battle.bin",
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            OpenKh.Kh2.Battle.Magc.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = "magc",
                        Type = "list",
                        Method = "listpatch",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                                Type = "magc",
                            }
                        ),
                    }
                )
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "model" },
                IfApply: (name, type, index) => type == Bar.EntryType.Model,
                SourceFileTest: (relativePath) => true,
                FileExtension: ".model",
                ExtractAsync: async (barEntry) =>
                {
                    return barEntry.Stream.ReadAllBytes();
                }
            ));

            extractors.Add(new Extractor(
                Tags: new[] { "modeltexture" },
                IfApply: (name, type, index) => type == Bar.EntryType.ModelTexture,
                SourceFileTest: (relativePath) => true,
                FileExtension: ".tim",
                ExtractAsync: async (barEntry) =>
                {
                    return barEntry.Stream.ReadAllBytes();
                }
            ));

            foreach (var lang in "us,fr,gr,it,sp".Split(','))
            {
                extractors.Add(CreateMessageExtractor(
                    tags: new[] { "message", "sys.bar", "InternationalSystem" },
                    sourceFilePattern: new Regex($@"msg/{lang}/sys\.bar"),
                    language: lang,
                    decoder: Encoders.InternationalSystem
                ));

                extractors.Add(CreateMessageExtractor(
                    tags: new[] { "message", "xx.bar", "InternationalSystem" },
                    sourceFilePattern: new Regex($@"msg/{lang}/[a-z]{{2}}\.bar"),
                    language: lang,
                    decoder: Encoders.InternationalSystem
                ));
            }

            extractors.Add(CreateMessageExtractor(
                tags: new[] { "message", "sys.bar", "TurkishSystem" },
                sourceFilePattern: new Regex(@"msg/tr/sys\.bar"),
                language: "tr",
                decoder: Encoders.TurkishSystem
            ));
            extractors.Add(CreateMessageExtractor(
                tags: new[] { "message", "xx.bar", "TurkishSystem" },
                sourceFilePattern: new Regex(@"msg/tr/[a-z]{2}\.bar"),
                language: "tr",
                decoder: Encoders.TurkishSystem
            ));

            extractors.Add(CreateMessageExtractor(
                tags: new[] { "message", "sys.bar", "JapaneseSystem" },
                sourceFilePattern: new Regex(@"msg/jp/sys\.bar"),
                language: "jp",
                decoder: Encoders.JapaneseSystem
            ));
            extractors.Add(CreateMessageExtractor(
                tags: new[] { "message", "xx.bar", "JapaneseEvent" },
                sourceFilePattern: new Regex(@"msg/jp/[a-z]{2}.bar"),
                language: "je",
                decoder: Encoders.JapaneseEvent
            ));


            extractors.Add(new Extractor(
                Tags: new[] { "bdx" },
                IfApply: (name, type, index) => type == Bar.EntryType.Bdx,
                SourceFileTest: (relativePath) => true,
                FileExtension: ".bdscript",
                ExtractAsync: async (barEntry) =>
                {
                    var decoder = new BdxDecoder(
                        read: new MemoryStream(barEntry.Stream.ReadAllBytes(), false),
                        codeRevealer: true,
                        codeRevealerLabeling: true
                    );
                    return Encoding.UTF8.GetBytes(
                        BdxDecoder.TextFormatter.Format(decoder)
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = arg.DestName,
                        Type = "bdx",
                        Method = "bdscript",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                            }
                        ),
                    }
                )
            ));


            extractors.Add(new Extractor(
                Tags: new[] { "areadatascript" },
                IfApply: (name, type, index) => type == Bar.EntryType.AreaDataScript,
                SourceFileTest: (relativePath) => true,
                FileExtension: ".script",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        Kh2.Ard.AreaDataScript.Decompile(
                            Kh2.Ard.AreaDataScript.Read(barEntry.Stream.FromBegin())
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = arg.DestName,
                        Type = "areadatascript",
                        Method = "areadatascript",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                            }
                        ),
                    }
                )
            ));


            extractors.Add(new Extractor(
                Tags: new[] { "areadataspawn", "spawnpoint" },
                IfApply: (name, type, index) => type == Bar.EntryType.AreaDataSpawn,
                SourceFileTest: (relativePath) => true,
                FileExtension: ".yml",
                ExtractAsync: async (barEntry) =>
                {
                    return Encoding.UTF8.GetBytes(
                        _yamlSer.Serialize(
                            Kh2.Ard.SpawnPoint.Read(
                                barEntry.Stream.FromBegin()
                            )
                        )
                    );
                },
                SourceBuilder: arg => CreateSourceFromArgs(
                    new AssetFile
                    {
                        Name = arg.DestName,
                        Type = "areadataspawn",
                        Method = "spawnpoint",
                        Source = CreateSourceFromArgs(
                            new AssetFile
                            {
                                Name = arg.SourceName,
                            }
                        ),
                    }
                )
            ));

            return extractors.AsReadOnly();
        }

#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously
    }
}

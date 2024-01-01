using LibGit2Sharp;
using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.SystemData;
using OpenKh.Patcher;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Models.ViewHelper;
using OpenKh.Tools.ModsManager.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using YamlDotNet.Serialization;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;

namespace OpenKh.Tools.ModsManager.Views
{
    public class YamlGeneratorVM : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;
        public ICommand GenerateCommand { get; set; }
        public ICommand LoadPrefCommand { get; set; }
        public ICommand SavePrefCommand { get; set; }

        private Action<GetDiffService> OnToolSelected { get; set; } = _ => { };
        private Action<string> OnYmlChanged { get; set; } = _ => { };

        #region SelectedPref
        private ConfigurationService.YamlGenPref _selectedPref = null;
        public ConfigurationService.YamlGenPref SelectedPref
        {
            get => _selectedPref;
            set
            {
                _selectedPref = value;
                OnPropertyChanged(nameof(SelectedPref));
            }
        }
        #endregion

        #region PrefLabel
        private string _prefLabel = "";
        public string PrefLabel
        {
            get => _prefLabel;
            set
            {
                _prefLabel = value;
                OnPropertyChanged(nameof(PrefLabel));
            }
        }
        #endregion

        #region Prefs
        private IEnumerable<ConfigurationService.YamlGenPref> _prefs = Array.Empty<ConfigurationService.YamlGenPref>();
        public IEnumerable<ConfigurationService.YamlGenPref> Prefs
        {
            get => _prefs;
            set
            {
                _prefs = value;
                OnPropertyChanged(nameof(Prefs));
            }
        }
        #endregion

        #region ModYmlFilePath 
        private string _modYmlFilePath = "";
        public string ModYmlFilePath
        {
            get => _modYmlFilePath;
            set
            {
                _modYmlFilePath = value;
                OnPropertyChanged(nameof(ModYmlFilePath));
                OnYmlChanged(_modYmlFilePath);
            }
        }
        #endregion 

        #region GeneratingTask
        private Task _generatingTask;
        public Task GeneratingTask
        {
            get => _generatingTask;
            set
            {
                _generatingTask = value;
                OnPropertyChanged(nameof(GeneratingTask));
            }
        }
        #endregion

        #region Tools
        private IEnumerable<GetDiffService> _tools = Enumerable.Empty<GetDiffService>();
        public IEnumerable<GetDiffService> Tools
        {
            get => _tools;
            set
            {
                _tools = value;
                OnPropertyChanged(nameof(Tools));
                SelectedTool = _tools?.FirstOrDefault();
            }
        }
        #endregion

        #region SelectedTool
        private GetDiffService _selectedTool;
        public GetDiffService SelectedTool
        {
            get => _selectedTool;
            set
            {
                _selectedTool = value;
                OnPropertyChanged(nameof(SelectedTool));
                OnToolSelected(_selectedTool);
            }
        }
        #endregion

        #region GameDataPath 
        private string _gameDataPath = "";
        public string GameDataPath
        {
            get => _gameDataPath;
            set
            {
                _gameDataPath = value;
                OnPropertyChanged(nameof(GameDataPath));
                OnGameDataPathChanged(_gameDataPath);
            }
        }
        #endregion 

        public ICommand AppenderCommand { get; set; }

        #region AppenderTask
        private Task _appenderTask;
        public Task AppenderTask
        {
            get => _appenderTask;
            set
            {
                _appenderTask = value;
                OnPropertyChanged(nameof(AppenderTask));
            }
        }
        #endregion

        private Action<string> OnGameDataPathChanged { get; set; } = _ => { };

        private readonly YamlGeneratorService _yamlGeneratorService = new YamlGeneratorService();
        private readonly GetDiffToolsService _getDiffToolsService = new GetDiffToolsService();
        private readonly GetActiveWindowService _getActiveWindowService = new GetActiveWindowService();
        private readonly QueryApplyPatchService _queryApplyPatchService = new QueryApplyPatchService();
        private readonly KeywordsMatcherService _keywordsMatcherService = new KeywordsMatcherService();
        private readonly ISerializer _listSer = new SerializerBuilder()
            .Build();

        public YamlGeneratorVM()
        {
            var extractors = new List<(
                string BarEntryName, 
                string YmlEntryName, 
                Bar.EntryType Type, 
                string FileExtension,
                Func<Bar.Entry, Task<byte[]>> ExtractAsync
            )>();


            extractors.Add((
                "trsr",
                "trsr",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.SystemData.Trsr.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));

            extractors.Add((
                "item",
                "item",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.SystemData.Item.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));

            extractors.Add((
                "fmlv",
                "fmlv",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.Battle.Fmlv.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));

            extractors.Add((
                "lvup",
                "lvup",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.Battle.Lvup.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));

            extractors.Add((
                "bons",
                "bons",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.Battle.Bons.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));

            extractors.Add((
                "atkp",
                "atkp",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.Battle.Atkp.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));

            extractors.Add((
                "plrp",
                "plrp",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.Battle.Plrp.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));

            extractors.Add((
                "plrp",
                "plrp",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.Battle.Plrp.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));

            extractors.Add((
                "cmd",
                "cmd",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.SystemData.Cmd.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));

            extractors.Add((
                "enmp",
                "enmp",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.Battle.Enmp.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));

            extractors.Add((
                "sklt",
                "sklt",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.SystemData.Sklt.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));

            extractors.Add((
                "przt",
                "przt",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.Battle.Przt.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));

            extractors.Add((
                "magc",
                "magc",
                Bar.EntryType.List,
                ".yml",
                async (Bar.Entry barEntry) =>
                {
                    await Task.Yield();
                    return Encoding.UTF8.GetBytes(
                        _listSer.Serialize(
                            OpenKh.Kh2.Battle.Magc.Read(barEntry.Stream.FromBegin())
                        )
                    );
                }
            ));


            GetDiffService diffTool = null;

            async Task ModifyMetadataAsync(
                Func<Metadata, Task> action
            )
            {
                var rawInput = await Task.Run(() => File.Exists(ModYmlFilePath))
                    ? await File.ReadAllBytesAsync(ModYmlFilePath)
                    : null;

                var createNewModYml = rawInput == null;

                var mod = (rawInput != null)
                    ? Metadata.Read(new MemoryStream(rawInput, false))
                    : new Metadata();

                mod.Title ??= Path.GetFileName(Path.GetDirectoryName(ModYmlFilePath));
                mod.OriginalAuthor ??= "You";
                mod.Description ??= $"Generated on {DateTime.UtcNow:R}";
                mod.Assets ??= new List<AssetFile>();

                await action(mod);

                {
                    var temp = new MemoryStream();
                    mod.Write(temp);
                    var rawOutput = temp.ToArray();

                    var rawOutput2 = await diffTool.DiffAsync(rawInput, rawOutput);
                    if (rawOutput2 != null)
                    {
                        if (createNewModYml || await _queryApplyPatchService.QueryAsync())
                        {
                            await File.WriteAllBytesAsync(ModYmlFilePath, rawOutput2);
                            return;
                        }
                        else
                        {
                            return;
                        }
                    }
                    else
                    {
                        return;
                    }
                }
            }

            void DisplayCopy(
                IEnumerable<PrimarySource> primarySourceList,
                Func<PrimarySource, IEnumerable<CopySourceFile>> getCopySourceList,
                Func<IEnumerable<CopySourceFile>, Task> proceedAsync
            )
            {
                var copyWin = new CopySourceFilesWindow();
                copyWin.Owner = _getActiveWindowService.GetActiveWindow();
                copyWin.Closed += (_, __) => copyWin.Owner?.Focus();
                var copyVm = copyWin.VM;
                SimpleAsyncActionCommand<object> proceedCopyCommand;
                copyVm.ProceedCommand = proceedCopyCommand = new SimpleAsyncActionCommand<object>(
                    async _ =>
                    {
                        await proceedAsync(copyVm.CopySourceList);

                        copyWin.Close();
                    },
                    task => copyVm.ProceedTask = task
                )
                {
                    IsEnabled = false,
                };
                copyVm.PropertyChanged += (_, e) =>
                {
                    if (e.PropertyName == null || e.PropertyName == nameof(copyVm.SelectedPrimarySource))
                    {
                        var one = copyVm.SelectedPrimarySource;

                        proceedCopyCommand.IsEnabled = one != null;

                        copyVm.CopySourceList = getCopySourceList(one)
                            .ToArray();
                    }
                };
                copyVm.PrimarySourceList = primarySourceList
                    .ToArray();
                copyWin.Show();
            }

            List<AssetFile> CreateSourceFromArgs(params AssetFile[] assetFiles) => new List<AssetFile>(assetFiles);


            SimpleAsyncActionCommand<object> appenderCommand;

            AppenderCommand = appenderCommand = new SimpleAsyncActionCommand<object>(
                async _ =>
                {
                    SimpleAsyncActionCommand<object> searchCommand;

                    var sourceDir = GameDataPath;
                    var destDir = Path.GetDirectoryName(ModYmlFilePath);

                    var targetWindow = new SelectModTargetFilesWindow();
                    var targetVm = targetWindow.VM;
                    targetVm.SearchCommand = searchCommand = new SimpleAsyncActionCommand<object>(
                        async _ =>
                        {
                            var ifMatch = _keywordsMatcherService.CreateMatcher(targetWindow.VM.SearchKeywords);
                            targetWindow.VM.SearchHits = await Task.Run(
                                () => Directory.EnumerateFiles(sourceDir, "*", SearchOption.AllDirectories)
                                    .Select(
                                        file => (
                                            Path: file,
                                            Relative: Path.GetRelativePath(sourceDir, file)
                                                .Replace('\\', '/')
                                        )
                                    )
                                    .Where(pair => ifMatch(pair.Relative))
                                    .Select(
                                        pair => new SearchHit(
                                            FullPath: pair.Path,
                                            RelativePath: pair.Relative,
                                            Display: pair.Relative
                                        )
                                    )
                                    .ToArray()
                            );
                        }
                    );

                    var selectionIsGood = new BehaviorSubject<bool>(false);

                    var actions = new List<ActionCommand>();

                    SimpleAsyncActionCommand<object> copyMultiCommand;
                    SimpleAsyncActionCommand<object> copyEachCommand;
                    SimpleAsyncActionCommand<object> binArcCommand;

                    var hits = Enumerable.Empty<SearchHit>();

                    actions.Add(
                        new ActionCommand(
                            "Copy multi",
                            copyMultiCommand = new SimpleAsyncActionCommand<object>(
                                async _ =>
                                {
                                    await Task.Yield();

                                    SearchHit selectedHit = null;

                                    DisplayCopy(
                                        primarySourceList: hits
                                            .Select(hit => new PrimarySource(hit.Display)),
                                        getCopySourceList: one =>
                                        {
                                            return hits
                                                .Where(
                                                    hit => hit.Display == one?.Display
                                                )
                                                .Select(
                                                    hit =>
                                                    {
                                                        selectedHit = hit;

                                                        var sourcePath = Path.Combine(sourceDir, hit.RelativePath);
                                                        var destPath = Path.Combine(destDir, hit.RelativePath);
                                                        var exists = File.Exists(destPath);
                                                        return new CopySourceFile(
                                                            one.Display,
                                                            "Copy",
                                                            exists,
                                                            async () =>
                                                            {
                                                                await Task.Run(
                                                                    () => File.Copy(sourcePath, destPath, true)
                                                                );
                                                            }
                                                        )
                                                        {
                                                            DoAction = !exists,
                                                        };
                                                    }
                                                );
                                        },
                                        proceedAsync: async copySourceList =>
                                        {
                                            foreach (var source in copySourceList.Where(it => it.DoAction))
                                            {
                                                await source.AsyncAction();
                                            }

                                            await ModifyMetadataAsync(
                                                async mod =>
                                                {
                                                    await Task.Yield();

                                                    mod.Assets.Add(
                                                        new AssetFile
                                                        {
                                                            Name = selectedHit.RelativePath,
                                                            Multi = new List<Multi>(
                                                                hits
                                                                    .Where(
                                                                        hit => !ReferenceEquals(hit, selectedHit)
                                                                    )
                                                                    .Select(
                                                                        hit => new Multi
                                                                        {
                                                                            Name = hit.RelativePath,
                                                                        }
                                                                    )
                                                                    .ToArray()
                                                            ),
                                                            Method = "copy",
                                                            Source = CreateSourceFromArgs(
                                                                new AssetFile
                                                                {
                                                                    Name = selectedHit.RelativePath,
                                                                }
                                                            ),
                                                        }
                                                    );
                                                }
                                            );
                                        }
                                    );
                                }
                            )
                            {
                                IsEnabled = false,
                            }
                        )
                    );

                    actions.Add(
                        new ActionCommand(
                            "Copy each",
                            copyEachCommand = new SimpleAsyncActionCommand<object>(
                                async _ =>
                                {
                                    await Task.Yield();

                                    var copySourceList = hits
                                        .Select(
                                            hit =>
                                            {
                                                var sourcePath = Path.Combine(sourceDir, hit.RelativePath);
                                                var destPath = Path.Combine(destDir, hit.RelativePath);
                                                var exists = File.Exists(destPath);
                                                return new CopySourceFile(
                                                    hit.RelativePath,
                                                    "Copy",
                                                    exists,
                                                    async () =>
                                                    {
                                                        await Task.Run(
                                                            () => File.Copy(sourcePath, destPath, true)
                                                        );
                                                    }
                                                )
                                                {
                                                    DoAction = !exists,
                                                };
                                            }
                                        )
                                        .ToArray();

                                    DisplayCopy(
                                        primarySourceList: new PrimarySource[] { new PrimarySource("(No selection needed)") },
                                        getCopySourceList: one => copySourceList,
                                        proceedAsync: async copySourceList =>
                                        {
                                            foreach (var source in copySourceList.Where(it => it.DoAction))
                                            {
                                                await source.AsyncAction();
                                            }

                                            await ModifyMetadataAsync(
                                                async mod =>
                                                {
                                                    await Task.Yield();

                                                    mod.Assets.AddRange(
                                                        hits
                                                            .Select(
                                                                hit => new AssetFile
                                                                {
                                                                    Name = hit.RelativePath,
                                                                    Method = "copy",
                                                                    Source = CreateSourceFromArgs(
                                                                        new AssetFile
                                                                        {
                                                                            Name = hit.RelativePath,
                                                                        }
                                                                    ),
                                                                }
                                                            )
                                                    );
                                                }
                                            );
                                        }
                                    );
                                }
                            )
                            {
                                IsEnabled = false,
                            }
                        )
                    );

                    actions.Add(
                        new ActionCommand(
                            "binarc",
                            binArcCommand = new SimpleAsyncActionCommand<object>(
                                async _ =>
                                {
                                    await Task.Yield();

                                    var mixedSourceList = hits
                                        .SelectMany(
                                            hit =>
                                            {
                                                var sourcePath = Path.Combine(sourceDir, hit.RelativePath);

                                                var stream = new MemoryStream(File.ReadAllBytes(sourcePath), false);

                                                var barEntries = Bar.IsValid(stream)
                                                    ? Bar.Read(stream).AsEnumerable()
                                                    : Array.Empty<Bar.Entry>();

                                                return barEntries
                                                    .Select(
                                                        barEntry =>
                                                        {
                                                            var extractor = extractors.FirstOrDefault(
                                                                one => one.BarEntryName == barEntry.Name
                                                                    && one.Type == barEntry.Type
                                                            );

                                                            var destRelative = Path.Combine(
                                                                Path.GetDirectoryName(hit.RelativePath),
                                                                $"{Path.GetFileNameWithoutExtension(hit.RelativePath)}_{barEntry.Name}{extractor.FileExtension}"
                                                            );
                                                            var destPath = Path.Combine(
                                                                destDir,
                                                                destRelative
                                                            );

                                                            var exists = File.Exists(destPath);

                                                            var copySourceFile = new CopySourceFile(
                                                                $"{hit.RelativePath} ({barEntry.Name} {barEntry.Type} {extractor.FileExtension})",
                                                                "Extract",
                                                                exists,
                                                                async () =>
                                                                {
                                                                    await Task.Run(
                                                                        async () =>
                                                                        {
                                                                            Directory.CreateDirectory(Path.GetDirectoryName(destPath));

                                                                            using (var destStream = File.Create(destPath))
                                                                            {
                                                                                if (extractor.ExtractAsync != null)
                                                                                {
                                                                                    await destStream.WriteAsync(
                                                                                        await extractor.ExtractAsync(barEntry)
                                                                                    );
                                                                                }
                                                                                else
                                                                                {
                                                                                    barEntry.Stream.FromBegin().CopyTo(destStream);
                                                                                }
                                                                            }
                                                                        }
                                                                    );
                                                                }
                                                            )
                                                            {
                                                                DoAction = !exists,
                                                            };

                                                            var assetFile = new AssetFile
                                                            {
                                                                Name = hit.RelativePath,
                                                                Method = "binarc",
                                                                Source = CreateSourceFromArgs(
                                                                    new AssetFile
                                                                    {
                                                                        Name = barEntry.Name,
                                                                        Type = barEntry.Type.ToString(),
                                                                        Method = "copy",
                                                                        Source = CreateSourceFromArgs(
                                                                            new AssetFile
                                                                            {
                                                                                Name = destRelative.Replace('\\', '/'),
                                                                            }
                                                                        ),
                                                                    }
                                                                ),
                                                            };

                                                            return (CopySourceFile: copySourceFile, AssetFile: assetFile);
                                                        }
                                                    );
                                            }
                                        );

                                    DisplayCopy(
                                        primarySourceList: new PrimarySource[] { new PrimarySource("(No selection needed)") },
                                        getCopySourceList: one =>
                                        {
                                            return mixedSourceList
                                                .Select(tuple => tuple.CopySourceFile);
                                        },
                                        proceedAsync: async copySourceList =>
                                        {
                                            foreach (var source in copySourceList.Where(it => it.DoAction))
                                            {
                                                try
                                                {
                                                    await source.AsyncAction();
                                                }
                                                catch (Exception ex)
                                                {
                                                    throw new Exception($"Error while processing of: {source.Display}", ex);
                                                }
                                            }

                                            await ModifyMetadataAsync(
                                                async mod =>
                                                {
                                                    await Task.Yield();

                                                    mod.Assets.AddRange(
                                                        mixedSourceList
                                                            .Where(
                                                                tuple => tuple.CopySourceFile.DoAction || tuple.CopySourceFile.DestinationFileExists
                                                            )
                                                            .Select(
                                                                tuple => tuple.AssetFile
                                                            )
                                                    );
                                                }
                                            );
                                        }
                                    );
                                }
                            )
                            {
                                IsEnabled = false,
                            }
                        )
                    );

                    targetVm.Actions = actions;

                    selectionIsGood
                        .ObserveOn(Scheduler.Immediate)
                        .Subscribe(
                            it =>
                            {
                                copyMultiCommand.IsEnabled = it;
                                copyEachCommand.IsEnabled = it;
                                binArcCommand.IsEnabled = it;
                            }
                        );

                    targetVm.OnSearchHitsSelected = them =>
                    {
                        hits = them;
                        selectionIsGood.OnNext(them.Any());
                    };

                    targetWindow.Owner = _getActiveWindowService.GetActiveWindow();
                    targetWindow.Closed += (_, __) => targetWindow.Owner?.Focus();
                    targetWindow.Show();
                },
                task => AppenderTask = task
            );

            SimpleAsyncActionCommand<object> generateCommand;

            GenerateCommand = generateCommand = new SimpleAsyncActionCommand<object>(
                async _ =>
                    {
                        await ModifyMetadataAsync(
                            async mod =>
                            {
                                await _yamlGeneratorService.RefillAssetFilesAsync(
                                assetFiles: mod.Assets,
                                sourceDir: Path.GetDirectoryName(ModYmlFilePath)
                            );
                            }
                        );
                    },
                    task => GeneratingTask = task
                );

            var toolIsGood = new BehaviorSubject<bool>(false);

            OnToolSelected = it =>
            {
                diffTool = it;
                toolIsGood.OnNext(it != null);
            };

            var ymlFilePathIsGood = new BehaviorSubject<bool>(false);

            OnYmlChanged = it =>
            {
                ymlFilePathIsGood.OnNext(it.Length != 0);
            };

            Observable
                .CombineLatest(toolIsGood, ymlFilePathIsGood)
                .ObserveOn(Scheduler.Immediate)
                .Subscribe(array => generateCommand.IsEnabled = array.All(it => it));

            var gameDataPathIsGood = new BehaviorSubject<bool>(false);

            OnGameDataPathChanged = path =>
            {
                try
                {
                    gameDataPathIsGood.OnNext((1 <= path?.Length) && Directory.Exists(path));
                }
                catch
                {
                    // ignore
                }
            };

            Observable
                .CombineLatest(ymlFilePathIsGood, gameDataPathIsGood)
                .ObserveOn(Scheduler.Immediate)
                .Subscribe(array => appenderCommand.IsEnabled = array.All(it => it));

            {
                Tools = _getDiffToolsService.GetDiffServices(".yml")
                    .Append(
                        new GetDiffService
                        {
                            Name = "Use output as is",
                            DiffAsync = async (rawInput, rawOutput) =>
                            {
                                await Task.Yield();
                                return rawOutput;
                            }
                        }
                    )
                    .ToArray();
            }

            void LoadPref(ConfigurationService.YamlGenPref pref)
            {
                GameDataPath = pref.GameDataPath;
                ModYmlFilePath = pref.ModYmlFilePath;
            }

            LoadPrefCommand = new RelayCommand(
                _ =>
                {
                    if (ConfigurationService.YamlGenPrefs.LastOrDefault(it => it.Label == PrefLabel) is ConfigurationService.YamlGenPref pref)
                    {
                        LoadPref(pref);
                    }
                }
            );

            SavePrefCommand = new RelayCommand(
                _ =>
                {
                    if (PrefLabel.Length == 0)
                    {
                        PrefLabel = $"Saved at {DateTime.Now}";
                    }

                    var prefList = ConfigurationService.YamlGenPrefs.ToList();
                    prefList.RemoveAll(
                        pref => pref.Label == PrefLabel
                    );
                    prefList.Add(
                        new ConfigurationService.YamlGenPref
                        {
                            Label = PrefLabel,
                            GameDataPath = GameDataPath,
                            ModYmlFilePath = ModYmlFilePath,
                        }
                    );
                    ConfigurationService.YamlGenPrefs = prefList;

                    var prev = SelectedPref;
                    Prefs = prefList;
                    SelectedPref = prefList.LastOrDefault(it => it.Label == prev?.Label);
                }
            );

            Prefs = ConfigurationService.YamlGenPrefs;

            if (ConfigurationService.YamlGenPrefs.LastOrDefault(it => it.Label == "default") is ConfigurationService.YamlGenPref pref)
            {
                SelectedPref = pref;
                LoadPref(pref);
            }
        }
    }
}
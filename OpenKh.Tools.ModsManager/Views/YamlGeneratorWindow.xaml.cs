using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Patcher;
using OpenKh.Patcher.BarEntryExtractor;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Models.ViewHelper;
using OpenKh.Tools.ModsManager.Services;
using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reactive.Concurrency;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xe.Tools.Wpf.Commands;
using YamlDotNet.Serialization;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for YamlGeneratorWindow.xaml
    /// </summary>
    public partial class YamlGeneratorWindow : Window
    {
        private readonly YamlGeneratorService _yamlGeneratorService = new YamlGeneratorService();
        private readonly GetDiffToolsService _getDiffToolsService = new GetDiffToolsService();
        private readonly GetActiveWindowService _getActiveWindowService = new GetActiveWindowService();
        private readonly QueryApplyPatchService _queryApplyPatchService = new QueryApplyPatchService();
        private readonly KeywordsMatcherService _keywordsMatcherService = new KeywordsMatcherService();
        private readonly ISerializer _listSer = new SerializerBuilder()
            .Build();
        private readonly ProvideExtractorsService _provideExtractorsService = new ProvideExtractorsService();
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        private record MixedSource(CopySourceFile CopySourceFile, AssetFile AssetFile);

        public YamlGeneratorVM VM { get; }

        public YamlGeneratorWindow()
        {
            InitializeComponent();
            DataContext = VM = new YamlGeneratorVM();

            var extractors = _provideExtractorsService
                .GetExtractors()
                .ToImmutableArray();

            GetDiffService diffTool = null;

            async Task ModifyMetadataAsync(
                Func<Metadata, Task> action
            )
            {
                var modYmlFilePath = VM.ModYmlFilePath;

                var rawInput = await Task.Run(() => File.Exists(modYmlFilePath))
                    ? await File.ReadAllBytesAsync(modYmlFilePath)
                    : null;

                var createNewModYml = rawInput == null;

                var mod = (rawInput != null)
                    ? Metadata.Read(new MemoryStream(rawInput, false))
                    : new Metadata();

                mod.Title ??= Path.GetFileName(Path.GetDirectoryName(modYmlFilePath));
                mod.OriginalAuthor ??= "You";
                mod.Description ??= $"Generated on {DateTime.UtcNow:R}";
                mod.Assets ??= new List<AssetFile>();

                await action(mod);

                mod.Assets = MergeAssetSource(mod.Assets.ToArray());

                {
                    var temp = new MemoryStream();
                    mod.Write(temp);
                    var rawOutput = temp.ToArray();

                    var rawOutput2 = await diffTool.DiffAsync(rawInput, rawOutput);
                    if (rawOutput2 != null)
                    {
                        if (createNewModYml || await _queryApplyPatchService.QueryAsync())
                        {
                            await File.WriteAllBytesAsync(modYmlFilePath, rawOutput2);
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
                var copyDisposables = new CompositeDisposable();
                copyWin.Closed += (_, __) =>
                {
                    copyWin.Owner?.Focus();
                    copyDisposables.Dispose();
                };
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

                copyDisposables.Add(
                    Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                        h => (s, e) => h(e),
                        h => copyVm.PropertyChanged += h,
                        h => copyVm.PropertyChanged -= h
                    )
                        .Where(it => it.PropertyName == null || it.PropertyName == nameof(copyVm.SelectedPrimarySource))
                        .Select(it => copyVm.SelectedPrimarySource)
                        .Subscribe(
                            one =>
                            {
                                proceedCopyCommand.IsEnabled = one != null;

                                copyVm.CopySourceList = getCopySourceList(one)
                                    .ToArray();
                            }
                        )
                );

                copyVm.CheckAllCommand = new RelayCommand(
                    _ =>
                    {
                        copyVm.CopySourceList.ToList().ForEach(it => it.DoAction = true);
                    }
                );
                copyVm.UncheckAllCommand = new RelayCommand(
                    _ =>
                    {
                        copyVm.CopySourceList.ToList().ForEach(it => it.DoAction = false);
                    }
                );
                copyVm.PrimarySourceList = primarySourceList
                    .ToArray();
                copyWin.Show();
            }

            List<AssetFile> CreateSourceFromArgs(params AssetFile[] assetFiles) => new List<AssetFile>(assetFiles);


            SimpleAsyncActionCommand<object> appenderCommand;

            VM.AppenderCommand = appenderCommand = new SimpleAsyncActionCommand<object>(
                async _ =>
                {
                    SimpleAsyncActionCommand<object> searchCommand;

                    var sourceDir = VM.GameDataPath;
                    var destDir = Path.GetDirectoryName(VM.ModYmlFilePath);

                    var targetWindow = new SelectModTargetFilesWindow();
                    var targetVm = targetWindow.VM;
                    targetVm.SearchCommand = searchCommand = new SimpleAsyncActionCommand<object>(
                        async _ =>
                        {
                            var ifMatch = _keywordsMatcherService.CreateMatcher(targetVm.SearchKeywords);
                            targetVm.SearchHits = await Task.Run(
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
                        },
                        it => targetVm.SearchingTask = it
                    );

                    var selectionIsGood = new BehaviorSubject<bool>(false);

                    var actions = new List<ActionCommand>();

                    SimpleAsyncActionCommand<object> copyMultiCommand;
                    SimpleAsyncActionCommand<object> copyEachCommand;
                    SimpleAsyncActionCommand<object> binArcCommand;
                    SimpleAsyncActionCommand<object> pathCommand;

                    var hits = Enumerable.Empty<SearchHit>();

                    actions.Add(
                        new ActionCommand(
                            "Copy multi",
                            "Choose one more multiple files. And then select a representative file at the next window. Only the representative file will be copied. The rest files will just refer to representative file by multi field.",
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
                            "Choose one more multiple files. Each file is simply copied. And then each file is mapped correspondingly.",
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
                            "Choose one more multiple files. Expand each file as bar file. And then you can select the required bar entries to be extracted.",
                            binArcCommand = new SimpleAsyncActionCommand<object>(
                                async _ =>
                                {
                                    await Task.Yield();

                                    IEnumerable<MixedSource> mixedSourceList = Array.Empty<MixedSource>();

                                    IEnumerable<MixedSource> UpdateMixedSourceListAndReturn(bool applyExtractors)
                                    {
                                        return mixedSourceList = hits
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
                                                                var extractor = applyExtractors
                                                                    ? extractors.FirstOrDefault(
                                                                        one => one.IfApply(barEntry.Name, barEntry.Type, barEntry.Index)
                                                                            && one.SourceFileTest(hit.RelativePath)
                                                                    )
                                                                    : null;

                                                                var destRelative = Path.Combine(
                                                                    Path.GetDirectoryName(hit.RelativePath),
                                                                    $"{Path.GetFileNameWithoutExtension(hit.RelativePath)}_{barEntry.Name}{extractor?.FileExtension}"
                                                                );
                                                                var destPath = Path.Combine(
                                                                    destDir,
                                                                    destRelative
                                                                );

                                                                var exists = File.Exists(destPath);

                                                                var copySourceFile = new CopySourceFile(
                                                                    $"{hit.RelativePath} ({barEntry.Name} {barEntry.Type} {extractor?.FileExtension})",
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
                                                                                    if (extractor?.ExtractAsync != null)
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
                                                                    Source = (extractor?
                                                                        .SourceBuilder(
                                                                            new SourceBuilderArg(
                                                                                DestName: barEntry.Name,
                                                                                DestType: barEntry.Type.ToString().ToLowerInvariant(),
                                                                                SourceName: destRelative.Replace('\\', '/'),
                                                                                OriginalRelativePath: hit.RelativePath
                                                                            )
                                                                        )?
                                                                        .ToList()
                                                                    )
                                                                        ?? CreateSourceFromArgs(
                                                                            new AssetFile
                                                                            {
                                                                                Name = barEntry.Name,
                                                                                Type = barEntry.Type.ToString().ToLowerInvariant(),
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

                                                                return new MixedSource(copySourceFile, assetFile);
                                                            }
                                                        );
                                                }
                                            )
                                            .ToArray();
                                    }

                                    var applyExtractors = new PrimarySource("Apply built in extractors");
                                    var applyNone = new PrimarySource("None");

                                    DisplayCopy(
                                        primarySourceList: new PrimarySource[] { applyExtractors, applyNone, },
                                        getCopySourceList: one =>
                                        {
                                            return UpdateMixedSourceListAndReturn(
                                                applyExtractors: ReferenceEquals(one, applyExtractors)
                                            )
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

                    actions.Add(
                        new ActionCommand(
                            "path",
                            "Choose one more multiple files. And then, this will display the path list of selected files, as multi field of mod.yml file.",
                            pathCommand = new SimpleAsyncActionCommand<object>(
                                async _ =>
                                {
                                    await Task.Yield();

                                    var noteWin = new NotepadWindow();
                                    var noteVm = noteWin.VM;
                                    noteVm.Text = _listSer.Serialize(
                                        new
                                        {
                                            multi = hits
                                                .Select(hit => hit.RelativePath)
                                                .ToArray(),
                                        }
                                    );
                                    noteWin.Owner = _getActiveWindowService.GetActiveWindow();
                                    noteWin.Closed += (_, __) => noteWin.Owner?.Focus();
                                    noteWin.Show();
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
                                pathCommand.IsEnabled = it;
                            }
                        );

                    var targetDisposables = new CompositeDisposable();

                    targetDisposables.Add(
                        Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                            h => (s, e) => h(e),
                            h => targetVm.PropertyChanged += h,
                            h => targetVm.PropertyChanged -= h
                        )
                            .Where(it => it.PropertyName == null || it.PropertyName == nameof(targetVm.SearchHitSelectedList))
                            .Select(it => targetVm.SearchHitSelectedList)
                            .Subscribe(
                                them =>
                                {
                                    hits = them;
                                    selectionIsGood.OnNext(them.Any());
                                }
                            )
                    );

                    targetWindow.Owner = _getActiveWindowService.GetActiveWindow();
                    targetWindow.Closed += (_, __) =>
                    {
                        targetWindow.Owner?.Focus();
                        targetDisposables.Dispose();
                    };
                    targetWindow.Show();
                },
                task => VM.AppenderTask = task
            );

            SimpleAsyncActionCommand<object> generateCommand;

            VM.GenerateCommand = generateCommand = new SimpleAsyncActionCommand<object>(
                async _ =>
                {
                    await ModifyMetadataAsync(
                        async mod =>
                        {
                            await _yamlGeneratorService.RefillAssetFilesAsync(
                            assetFiles: mod.Assets,
                            sourceDir: Path.GetDirectoryName(VM.ModYmlFilePath)
                        );
                        }
                    );
                },
                    task => VM.GeneratingTask = task
                );

            var toolIsGood = new BehaviorSubject<bool>(false);

            var vmPropertyChanged = Observable.FromEvent<PropertyChangedEventHandler, PropertyChangedEventArgs>(
                h => (s, e) => h(e),
                h => VM.PropertyChanged += h,
                h => VM.PropertyChanged -= h
            );

            _disposable.Add(
                vmPropertyChanged
                    .Where(it => it.PropertyName == null || it.PropertyName == nameof(VM.Tools))
                    .Select(it => VM.Tools.FirstOrDefault())
                    .ObserveOn(Scheduler.Immediate)
                    .Subscribe(
                        it =>
                        {
                            diffTool = it;
                            toolIsGood.OnNext(it != null);
                        }
                    )
            );

            var ymlFilePathIsGood = new BehaviorSubject<bool>(false);

            _disposable.Add(
                vmPropertyChanged
                    .Where(it => it.PropertyName == null || it.PropertyName == nameof(VM.ModYmlFilePath))
                    .Select(it => VM.ModYmlFilePath)
                    .ObserveOn(Scheduler.Immediate)
                    .Subscribe(
                        it =>
                        {
                            ymlFilePathIsGood.OnNext(it.Length != 0);
                        }
                    )
            );

            Observable
                .CombineLatest(toolIsGood, ymlFilePathIsGood)
                .ObserveOn(Scheduler.Immediate)
                .Subscribe(array => generateCommand.IsEnabled = array.All(it => it));

            var gameDataPathIsGood = new BehaviorSubject<bool>(false);

            _disposable.Add(
                vmPropertyChanged
                    .Where(it => it.PropertyName == null || it.PropertyName == nameof(VM.GameDataPath))
                    .Select(it => VM.GameDataPath)
                    .ObserveOn(Scheduler.Immediate)
                    .Subscribe(
                        path =>
                        {
                            try
                            {
                                gameDataPathIsGood.OnNext((1 <= path?.Length) && Directory.Exists(path));
                            }
                            catch
                            {
                                // ignore
                            }
                        }
                    )
            );

            Observable
                .CombineLatest(ymlFilePathIsGood, gameDataPathIsGood)
                .ObserveOn(Scheduler.Immediate)
                .Subscribe(array => appenderCommand.IsEnabled = array.All(it => it));

            {
                VM.Tools = _getDiffToolsService.GetDiffServices(".yml")
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
                VM.GameDataPath = pref.GameDataPath;
                VM.ModYmlFilePath = pref.ModYmlFilePath;
            }

            VM.LoadPrefCommand = new RelayCommand(
                _ =>
                {
                    if (ConfigurationService.YamlGenPrefs.LastOrDefault(it => it.Label == VM.PrefLabel) is ConfigurationService.YamlGenPref pref)
                    {
                        LoadPref(pref);
                    }
                }
            );

            VM.SavePrefCommand = new RelayCommand(
                _ =>
                {
                    if (VM.PrefLabel.Length == 0)
                    {
                        VM.PrefLabel = $"Saved at {DateTime.Now}";
                    }

                    var prefList = ConfigurationService.YamlGenPrefs.ToList();
                    prefList.RemoveAll(
                        pref => pref.Label == VM.PrefLabel
                    );
                    prefList.Add(
                        new ConfigurationService.YamlGenPref
                        {
                            Label = VM.PrefLabel,
                            GameDataPath = VM.GameDataPath,
                            ModYmlFilePath = VM.ModYmlFilePath,
                        }
                    );
                    ConfigurationService.YamlGenPrefs = prefList;

                    var prev = VM.SelectedPref;
                    VM.Prefs = prefList;
                    VM.SelectedPref = prefList.LastOrDefault(it => it.Label == prev?.Label);
                }
            );

            VM.Prefs = ConfigurationService.YamlGenPrefs;

            if (ConfigurationService.YamlGenPrefs.LastOrDefault(it => it.Label == "default") is ConfigurationService.YamlGenPref pref)
            {
                VM.SelectedPref = pref;
                LoadPref(pref);
            }
        }

        private List<AssetFile> MergeAssetSource(IEnumerable<AssetFile> sourceAssets)
        {
            var newAssets = new List<AssetFile>();

            foreach (var source in sourceAssets)
            {
                if (false)
                { }
                else if (source.Method == "copy")
                {
                    newAssets.RemoveAll(it => it.Name == source.Name);
                    newAssets.Add(source);
                }
                else if (source.Method == "binarc")
                {
                    var exists = newAssets.FirstOrDefault(it => it.Name == source.Name && it.Method == source.Method);
                    if (exists == null)
                    {
                        exists = source;
                        newAssets.Add(exists);
                    }

                    exists.Source ??= new List<AssetFile>();

                    foreach (var one in source.Source?.ToArray() ?? Enumerable.Empty<AssetFile>())
                    {
                        exists.Source.RemoveAll(it => it.Name == one.Name && it.Method == one.Method);
                        exists.Source.Add(one);
                    }

                }
            }

            return newAssets;
        }

        protected override void OnClosed(EventArgs e)
        {
            base.OnClosed(e);
            _disposable.Dispose();
        }
    }
}

using LibGit2Sharp;
using OpenKh.Patcher;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Models.ViewHelper;
using OpenKh.Tools.ModsManager.Services;
using System;
using System.Collections.Generic;
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

namespace OpenKh.Tools.ModsManager.Views
{
    public class YamlGeneratorVM : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;
        public ICommand GenerateCommand { get; set; }

        private Action<GetDiffService> OnToolSelected { get; set; } = _ => { };
        private Action<string> OnYmlChanged { get; set; } = _ => { };

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

        public YamlGeneratorVM()
        {
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

            SimpleAsyncActionCommand<object> appenderCommand;

            AppenderCommand = appenderCommand = new SimpleAsyncActionCommand<object>(
                async _ =>
                {
                    SimpleAsyncActionCommand<object> searchCommand;

                    var window = new SelectModTargetFilesWindow();
                    var vm = window.VM;
                    vm.SearchCommand = searchCommand = new SimpleAsyncActionCommand<object>(
                        async _ =>
                        {
                            var sourceDir = GameDataPath;
                            var destDir = Path.GetDirectoryName(ModYmlFilePath);

                            var ifMatch = _keywordsMatcherService.CreateMatcher(window.VM.SearchKeywords);
                            window.VM.SearchHits = await Task.Run(
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
                                            Display: pair.Relative
                                        )
                                    )
                                    .ToArray()
                            );
                        }
                    );

                    var selectionIsGood = new BehaviorSubject<bool>(false);

                    var actions = new List<ActionCommand>();

                    SimpleAsyncActionCommand<object> copyBinCommand;

                    var hits = Enumerable.Empty<SearchHit>();

                    actions.Add(
                        new ActionCommand(
                            "Copy bin",
                            copyBinCommand = new SimpleAsyncActionCommand<object>(
                                async _ =>
                                {

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
                                                        Source = new List<AssetFile>(
                                                            new AssetFile[]
                                                            {
                                                                new AssetFile
                                                                {
                                                                    Name = hit.RelativePath,
                                                                }
                                                            }
                                                        ),
                                                    }
                                                )
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

                    vm.Actions = actions;

                    selectionIsGood
                        .ObserveOn(Scheduler.Immediate)
                        .Subscribe(it => copyBinCommand.IsEnabled = it);

                    vm.OnSearchHitsSelected = them =>
                    {
                        hits = them;
                        selectionIsGood.OnNext(them.Any());
                    };

                    window.Owner = _getActiveWindowService.GetActiveWindow();
                    window.Closed += (_, __) => window.Owner?.Focus();
                    window.Show();
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
        }
    }
}

using LibGit2Sharp;
using OpenKh.Tools.Common.Wpf;
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

        private readonly YamlGeneratorService _yamlGeneratorService = new YamlGeneratorService();
        private readonly GetDiffToolsService _getDiffToolsService = new GetDiffToolsService();
        private readonly GetActiveWindowService _getActiveWindowService = new GetActiveWindowService();
        private readonly QueryApplyPatchService _queryApplyPatchService = new QueryApplyPatchService();

        public YamlGeneratorVM()
        {
            GetDiffService diffTool = null;

            SimpleAsyncActionCommand<object> generateCommand;

            GenerateCommand = generateCommand = new SimpleAsyncActionCommand<object>(
                async _ =>
                {
                    await _yamlGeneratorService.GenerateAsync(
                        ModYmlFilePath,
                        async (rawInput, rawOutput) =>
                        {
                            var diffToolSource = new TaskCompletionSource<GetDiffService>();

                            var rawOutput2 = await diffTool.DiffAsync(rawInput, rawOutput);
                            if (rawOutput2 != null)
                            {
                                if (await _queryApplyPatchService.QueryAsync())
                                {
                                    return rawOutput2;
                                }
                                else
                                {
                                    return null;
                                }
                            }
                            else
                            {
                                return null;
                            }
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
                .Subscribe(
                    list =>
                    {
                        generateCommand.IsEnabled = list.All(it => it);
                    }
                );

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

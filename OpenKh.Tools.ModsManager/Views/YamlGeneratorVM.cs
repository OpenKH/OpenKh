using LibGit2Sharp;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.ModsManager.Views
{
    public class YamlGeneratorVM : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;
        public string ModYmlFilePath { get; set; }
        public ICommand GenerateCommand { get; set; }

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

        private readonly YamlGeneratorService _yamlGeneratorService = new YamlGeneratorService();
        private readonly GetDiffToolsService _getDiffToolsService = new GetDiffToolsService();
        private readonly Func<SelectDiffToolWindow> _newSelectDiffToolWindow = () => new SelectDiffToolWindow();
        private readonly GetActiveWindowService _getActiveWindowService = new GetActiveWindowService();

        public YamlGeneratorVM()
        {
            GenerateCommand = new SimpleAsyncActionCommand<object>(
                async _ =>
                {
                    await _yamlGeneratorService.GenerateAsync(
                        ModYmlFilePath,
                        async (rawInput, rawOutput) =>
                        {
                            var diffTools = _getDiffToolsService.GetDiffServices(".yml")
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

                            var diffToolSource = new TaskCompletionSource<GetDiffService>();

                            var selectDiffToolWindow = _newSelectDiffToolWindow();
                            selectDiffToolWindow.Owner = _getActiveWindowService.GetActiveWindow();
                            selectDiffToolWindow.VM.Tools = diffTools;
                            selectDiffToolWindow.VM.OnSubmit = it =>
                            {
                                diffToolSource.SetResult(it);
                            };
                            selectDiffToolWindow.VM.OnClosed = () =>
                            {
                                diffToolSource.TrySetResult(null);
                            };
                            selectDiffToolWindow.Show();

                            var diffTool = await diffToolSource.Task;
                            if (diffTool != null)
                            {
                                return await diffTool.DiffAsync(rawInput, rawOutput);
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
        }
    }
}

using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.ModsManager.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Xe.Tools;

namespace OpenKh.Tools.ModsManager.Views
{
    public class SelectDiffToolVM : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;
        public ICommand ProceedCommand { get; set; }
        public Action<GetDiffService> OnSubmit { get; set; } = _ => { };
        public Action Close { get; set; } = () => { };
        public Action OnClosed { get; set; } = () => { };

        private Action<GetDiffService> OnToolSelected { get; set; } = _ => { };

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

        #region ProceedingTask
        private Task _proceedingTask;
        public Task ProceedingTask
        {
            get => _proceedingTask;
            set
            {
                _proceedingTask = value;
                OnPropertyChanged(nameof(ProceedingTask));
            }
        }
        #endregion

        public SelectDiffToolVM()
        {
            GetDiffService tool = null;

            SimpleAsyncActionCommand<object> proceedCommand;
            ProceedCommand = proceedCommand = new SimpleAsyncActionCommand<object>(
                async _ =>
                {
                    OnSubmit(tool);
                    Close();
                    await Task.Yield();
                },
                task => ProceedingTask = task
            );

            OnToolSelected = it =>
            {
                tool = it;
                proceedCommand.IsEnabled = it != null;
            };
        }
    }
}

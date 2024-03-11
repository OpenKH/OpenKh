using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.UserControls;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.ModsManager.Views
{
    public class NotepadVM : BaseNotifyPropertyChanged
    {
        public ColorThemeService ColorTheme => ColorThemeService.Instance;
        public ICommand CopyAllCommand { get; set; }
        public ICommand SaveAsCommand { get; set; }

        private readonly GetActiveWindowService _getActiveWindowService = new GetActiveWindowService();

        private string SavedTo { get; set; }

        #region Text
        private string _text = "";
        public string Text
        {
            get => _text;
            set
            {
                _text = value;
                OnPropertyChanged(nameof(Text));
            }
        }
        #endregion

        public NotepadVM()
        {
            CopyAllCommand = new RelayCommand(
                _ =>
                {
                    try
                    {
                        Clipboard.SetText(Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to copy!\n\n" + ex);
                    }
                }
            );

            SaveAsCommand = new RelayCommand(
                _ =>
                {
                    FileDialog.OnSave(
                        path =>
                        {
                            SavedTo = path;

                            try
                            {
                                File.WriteAllText(path, Text);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Failed to save to file!\n\n" + ex);
                            }
                        },
                        new List<FileDialogFilter>()
                            .AddAllFiles(),
                        SavedTo,
                        _getActiveWindowService.GetActiveWindow()
                    );
                }
            );
        }
    }
}

using OpenKh.Tools.ModsManager.Services;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for NotepadWindow.xaml
    /// </summary>
    public partial class NotepadWindow : Window
    {
        private readonly GetActiveWindowService _getActiveWindowService = new GetActiveWindowService();

        public NotepadWindow()
        {
            InitializeComponent();
            DataContext = VM = new NotepadVM();

            VM.CopyAllCommand = new RelayCommand(
                _ =>
                {
                    try
                    {
                        Clipboard.SetText(VM.Text);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("Failed to copy!\n\n" + ex);
                    }
                }
            );

            var saveTo = "";

            VM.SaveAsCommand = new RelayCommand(
                _ =>
                {
                    FileDialog.OnSave(
                        path =>
                        {
                            saveTo = path;

                            try
                            {
                                File.WriteAllText(path, VM.Text);
                            }
                            catch (Exception ex)
                            {
                                MessageBox.Show("Failed to save to file!\n\n" + ex);
                            }
                        },
                        new List<FileDialogFilter>()
                            .AddAllFiles(),
                        saveTo,
                        _getActiveWindowService.GetActiveWindow()
                    );
                }
            );
        }

        public NotepadVM VM { get; }
    }
}

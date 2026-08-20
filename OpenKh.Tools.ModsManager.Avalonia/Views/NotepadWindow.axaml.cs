using Avalonia.Controls;
using OpenKh.Tools.Common.Avalonia;
using System;
using System.Collections.Generic;
using System.IO;
using System.Windows;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;
using FileDialog = Xe.Tools.Wpf.Dialogs.FileDialog;
using FileDialogFilter = Xe.Tools.Wpf.Dialogs.FileDialogFilter;

namespace OpenKh.Tools.ModsManager.Views
{
    public partial class NotepadWindow : DialogWindowBase
    {
        public NotepadWindow()
        {
            InitializeComponent();
            DataContext = VM = new NotepadVM();

            VM.CopyAllCommand = new RelayCommand(
                async _ =>
                {
                    try
                    {
                        await TopLevel.GetTopLevel(this).Clipboard.SetTextAsync(VM.Text);
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
                        this
                    );
                }
            );
        }

        public NotepadVM VM { get; }
    }
}

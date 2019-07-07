using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.LevelUpEditor.ViewModels
{
    public class LvupViewModel : BaseNotifyPropertyChanged
    {
        public CharactersViewModel Characters { get; set; }

        public RelayCommand OpenCommand { get; }
        public RelayCommand SaveCommand { get; }
        public RelayCommand SaveAsCommand { get; }
        public RelayCommand ExitCommand { get; }
        public RelayCommand AboutCommand { get; }

        private Window Window => Application.Current.Windows.OfType<Window>().FirstOrDefault(x => x.IsActive);
        private bool IsFileLoaded;
        private string FileName;
        private Stream stream;

        public LvupViewModel()
        {
            OpenCommand = new RelayCommand(x =>
            {
                var fd = FileDialog.Factory(null, FileDialog.Behavior.Open, ("00battle.bin", "bin"));
                if (fd.ShowDialog() == true)
                {
                    Open(fd.FileName);
                }
            });
            SaveCommand = new RelayCommand(x =>
            {
                try
                {
                    IEnumerable<Bar.Entry> entries;
                    using (var file = File.Open(FileName, FileMode.Open))
                    {
                        entries = Bar.Read(file);
                    }
                    var lvup = entries?.Where(e => e.Name == "lvup").First() ?? null;

                    stream.Position = 0;
                    Lvup.Save(stream, Characters.Items.Select(c => c.Character));

                    lvup.Stream = stream;

                    using (var stream = File.Open(FileName, FileMode.Create))
                    {
                        Bar.Write(stream, entries);
                    }
                    MessageBox.Show("File saved.", string.Empty, MessageBoxButton.OK, MessageBoxImage.Information);
                }
                catch (Exception e)
                {
                    MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }

            }, x => IsFileLoaded && Lvup.CanSave());
            SaveAsCommand = new RelayCommand(x =>
            {
                //var fd = FileDialog.Factory(null, FileDialog.Behavior.Save, )
            });
            ExitCommand = new RelayCommand(x => Window.Close());
            AboutCommand = new RelayCommand(x => new AboutDialog(Assembly.GetExecutingAssembly()).ShowDialog());
        }

        public LvupViewModel(Stream stream)
        {
            Characters = new CharactersViewModel(Lvup.Open(stream));
            OnPropertyChanged(nameof(Characters));
        }

        private void Open(string fileName)
        {
            try
            {
                using (var file = File.Open(fileName, FileMode.Open))
                {
                    var ent = Bar.Read(file,
                        (str, type) => str == "lvup" && type == Bar.EntryType.Binary)
                        .FirstOrDefault();

                    if (ent != null)
                    {
                        FileName = fileName;
                        stream = ent.Stream;
                        Characters = new CharactersViewModel(Lvup.Open(stream));
                        Characters.SelectedItem = Characters.Items[0];
                        OnPropertyChanged(nameof(Characters));

                        IsFileLoaded = true;
                        OnPropertyChanged(nameof(IsFileLoaded));
                    }
                    else
                    {
                        MessageBox.Show("This file does not contain an 'lvup' entry!", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
            catch (Exception e)
            {
                MessageBox.Show(e.Message, "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}

using Microsoft.Win32;
using OpenKh.Kh1;
using OpenKh.Tools.Kh1TextEditor.ViewModels;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace OpenKh.Tools.Kh1TextEditor.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private Kh1Binl _binl;
        private string _binlFileName;
        private TextEntryViewModel _selectedEntry;
        private bool _isDirty;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            EntriesView = CollectionViewSource.GetDefaultView(Entries);
            EntriesView.Filter = FilterEntry;

            var arguments = Environment.GetCommandLineArgs().Skip(1).ToArray();
            _binlFileName = arguments.FirstOrDefault(x =>
                string.Equals(Path.GetExtension(x), ".binl", StringComparison.OrdinalIgnoreCase));
            if (_binlFileName != null)
                LoadFiles();
            else
                UpdateStatus();
        }

        public ObservableCollection<TextEntryViewModel> Entries { get; } = new();
        public ICollectionView EntriesView { get; }

        public TextEntryViewModel SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));
            }
        }

        public bool HasSelection => SelectedEntry != null;

        private string _statusText;
        public string StatusText
        {
            get => _statusText;
            private set
            {
                _statusText = value;
                OnPropertyChanged();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void OpenBinl_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "KH1 remastered messages (*.binl)|*.binl|All files (*.*)|*.*",
                Title = "Open KH1 BINL",
            };
            if (dialog.ShowDialog(this) != true)
                return;

            if (!ConfirmDiscardChanges())
                return;
            _binlFileName = dialog.FileName;
            LoadFiles();
        }

        private void LoadFiles()
        {
            try
            {
                _binl = Kh1Binl.Read(_binlFileName);

                foreach (var oldEntry in Entries)
                    oldEntry.PropertyChanged -= Entry_PropertyChanged;
                Entries.Clear();
                foreach (var entry in _binl.Entries.Select(x => new TextEntryViewModel(x)))
                {
                    entry.PropertyChanged += Entry_PropertyChanged;
                    Entries.Add(entry);
                }

                SelectedEntry = Entries.FirstOrDefault();
                _isDirty = false;
                Title = $"{Path.GetFileName(_binlFileName)} | KH1 Text editor - OpenKH";
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Unable to open file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Save_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            if (_binl == null)
                return;
            SaveFile(_binlFileName);
        }

        private void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (_binl == null)
                return;
            var dialog = new SaveFileDialog
            {
                Filter = "KH1 remastered messages (*.binl)|*.binl|All files (*.*)|*.*",
                FileName = Path.GetFileName(_binlFileName),
                Title = "Save KH1 BINL as",
            };
            if (dialog.ShowDialog(this) == true)
                SaveFile(dialog.FileName);
        }

        private void SaveFile(string fileName)
        {
            try
            {
                foreach (var viewModel in Entries)
                    viewModel.Entry.Text = viewModel.Text;

                using var memory = new MemoryStream();
                _binl.Write(memory);
                File.WriteAllBytes(fileName, memory.ToArray());
                _binlFileName = fileName;
                _isDirty = false;
                Title = $"{Path.GetFileName(_binlFileName)} | KH1 Text editor - OpenKH";
                UpdateStatus();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Unable to save file", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TextEntryViewModel.Text))
            {
                _isDirty = true;
                UpdateStatus();
            }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
            EntriesView.Refresh();

        private bool FilterEntry(object item)
        {
            if (item is not TextEntryViewModel entry || string.IsNullOrWhiteSpace(SearchBox?.Text))
                return true;
            return entry.Text.Contains(SearchBox.Text, StringComparison.CurrentCultureIgnoreCase) ||
                entry.Number.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase) ||
                entry.Offset.Contains(SearchBox.Text, StringComparison.OrdinalIgnoreCase);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                this,
                "KH1 Text editor - OpenKH\n\nEdits remastered EvMsg BINL text using the built-in KH1 encoding.",
                "About KH1 Text editor",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }

        private void Exit_Click(object sender, RoutedEventArgs e) => Close();

        private void Window_Closing(object sender, CancelEventArgs e)
        {
            if (!ConfirmDiscardChanges())
                e.Cancel = true;
        }

        private bool ConfirmDiscardChanges()
        {
            if (!_isDirty)
                return true;
            return MessageBox.Show(
                this,
                "There are unsaved changes. Discard them?",
                "KH1 Text editor",
                MessageBoxButton.YesNo,
                MessageBoxImage.Warning) == MessageBoxResult.Yes;
        }

        private void UpdateStatus()
        {
            var messageName = string.IsNullOrEmpty(_binlFileName) ? "no BINL" : Path.GetFileName(_binlFileName);
            StatusText = $"{messageName} · built-in KH1 encoding · {Entries.Count} text entries" +
                (_isDirty ? " · modified" : string.Empty);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

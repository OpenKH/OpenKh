using Microsoft.Win32;
using OpenKh.Tools.Kh1TextEditor.Models;
using OpenKh.Tools.Kh1TextEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;

namespace OpenKh.Tools.Kh1TextEditor.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private sealed class LoadResult
        {
            public List<LoadedDocument> Documents { get; } = new();
            public List<string> Errors { get; } = new();
        }

        private List<LoadedDocument> _documents = new();
        private List<TextEntryViewModel> _entries = new();
        private ICollectionView _entriesView;
        private TextEntryViewModel _selectedEntry;
        private string _sourcePath;
        private string _statusText;
        private bool _isFolder;
        private bool _isDirty;
        private bool _isBusy;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            SetEntries(new List<TextEntryViewModel>());
            Loaded += MainWindow_Loaded;
        }

        public List<TextEntryViewModel> Entries
        {
            get => _entries;
            private set
            {
                _entries = value;
                OnPropertyChanged();
            }
        }

        public ICollectionView EntriesView
        {
            get => _entriesView;
            private set
            {
                _entriesView = value;
                OnPropertyChanged();
            }
        }

        public TextEntryViewModel SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(HasSelection));
                OnPropertyChanged(nameof(CanEdit));
            }
        }

        public bool HasSelection => SelectedEntry != null;
        public bool CanEdit => HasSelection && !IsBusy;
        public bool CanSaveAs => _documents.Count == 1 && !_isFolder && !IsBusy;

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                Mouse.OverrideCursor = value ? Cursors.Wait : null;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanEdit));
                OnPropertyChanged(nameof(CanSaveAs));
            }
        }

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

        private async void MainWindow_Loaded(object sender, RoutedEventArgs e)
        {
            var argument = Environment.GetCommandLineArgs().Skip(1).FirstOrDefault(x =>
                Directory.Exists(x) || IsSupportedFile(x));
            if (argument != null)
                await LoadPathAsync(argument);
            else
                UpdateStatus();
        }

        private async void OpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "KH1 remastered text (*.binl;*.kmb)|*.binl;*.kmb|BINL files (*.binl)|*.binl|KMB files (*.kmb)|*.kmb|All files (*.*)|*.*",
                Title = "Open KH1 text file",
            };
            if (dialog.ShowDialog(this) == true && ConfirmDiscardChanges())
                await LoadPathAsync(dialog.FileName);
        }

        private async void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Open KH1 remastered folder",
                Multiselect = false,
            };
            if (dialog.ShowDialog(this) == true && ConfirmDiscardChanges())
                await LoadPathAsync(dialog.FolderName);
        }

        private async Task LoadPathAsync(string path)
        {
            try
            {
                IsBusy = true;
                StatusText = Directory.Exists(path)
                    ? "Scanning BINL and KMB files..."
                    : $"Opening {Path.GetFileName(path)}...";

                var loaded = await Task.Run(() =>
                {
                    var documents = LoadDocuments(path);
                    var groups = documents.Documents
                        .SelectMany(x => x.Entries)
                        .GroupBy(x => x.Text, StringComparer.Ordinal)
                        .Select((group, index) => new TextEntryViewModel(index, group))
                        .ToList();
                    return (Documents: documents, Groups: groups);
                });
                var result = loaded.Documents;
                if (result.Documents.Count == 0)
                    throw new InvalidDataException("No readable BINL or KMB files were found.");

                _documents = result.Documents;
                _sourcePath = path;
                _isFolder = Directory.Exists(path);
                SetEntries(loaded.Groups);
                SelectedEntry = Entries.FirstOrDefault();
                _isDirty = false;
                Title = $"{Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))} | KH1 Text editor - OpenKH";
                OnPropertyChanged(nameof(CanSaveAs));
                UpdateStatus();

                if (result.Errors.Count > 0)
                {
                    var details = string.Join(Environment.NewLine, result.Errors.Take(10));
                    if (result.Errors.Count > 10)
                        details += $"{Environment.NewLine}... and {result.Errors.Count - 10} more files.";
                    MessageBox.Show(
                        this,
                        $"{result.Errors.Count} file(s) could not be read:{Environment.NewLine}{Environment.NewLine}{details}",
                        "KH1 Text editor",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Unable to open source", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                UpdateStatus();
            }
        }

        private static LoadResult LoadDocuments(string path)
        {
            var result = new LoadResult();
            var isFolder = Directory.Exists(path);
            var files = isFolder
                ? Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                    .Where(IsSupportedFile)
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                : new[] { path }.AsEnumerable();

            foreach (var fileName in files)
            {
                try
                {
                    var document = LoadedDocument.Read(fileName, path);
                    if (document != null)
                        result.Documents.Add(document);
                }
                catch (Exception ex)
                {
                    if (!isFolder)
                        throw;
                    result.Errors.Add($"{Path.GetRelativePath(path, fileName)}: {ex.Message}");
                }
            }
            return result;
        }

        private async void Save_Executed(object sender, ExecutedRoutedEventArgs e) =>
            await SaveChangesAsync(null);

        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveAs)
                return;

            var document = _documents[0];
            var dialog = new SaveFileDialog
            {
                Filter = string.Equals(document.Format, "KMB", StringComparison.Ordinal)
                    ? "KMB files (*.kmb)|*.kmb|All files (*.*)|*.*"
                    : "BINL files (*.binl)|*.binl|All files (*.*)|*.*",
                FileName = Path.GetFileName(document.FileName),
                Title = "Save KH1 text file as",
            };
            if (dialog.ShowDialog(this) == true)
                await SaveChangesAsync(dialog.FileName);
        }

        private async Task SaveChangesAsync(string saveAsFileName)
        {
            if (_documents.Count == 0 || IsBusy)
                return;

            var modifiedGroups = Entries.Where(x => x.IsModified).ToList();
            if (modifiedGroups.Count == 0 && saveAsFileName == null)
                return;

            try
            {
                IsBusy = true;
                StatusText = "Encoding and validating changes...";
                foreach (var group in modifiedGroups)
                    group.Apply();

                var affectedDocuments = saveAsFileName != null
                    ? _documents
                    : modifiedGroups.SelectMany(x => x.Documents).Distinct().ToList();
                var output = await Task.Run(() => affectedDocuments
                    .Select(x => new { Document = x, Data = x.BuildFile() })
                    .ToList());

                StatusText = "Writing files...";
                await Task.Run(() =>
                {
                    foreach (var item in output)
                    {
                        var target = saveAsFileName ?? item.Document.FileName;
                        LoadedDocument.WriteFile(target, item.Data);
                    }
                });

                foreach (var group in modifiedGroups)
                    group.AcceptChanges();
                _isDirty = false;
                UpdateStatus();

                await LoadPathAsync(saveAsFileName ?? _sourcePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "Unable to save changes", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsBusy = false;
                UpdateStatus();
            }
        }

        private void SetEntries(List<TextEntryViewModel> entries)
        {
            foreach (var oldEntry in Entries)
                oldEntry.PropertyChanged -= Entry_PropertyChanged;
            Entries = entries;
            foreach (var entry in Entries)
                entry.PropertyChanged += Entry_PropertyChanged;

            EntriesView = CollectionViewSource.GetDefaultView(Entries);
            EntriesView.Filter = FilterEntry;
        }

        private void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TextEntryViewModel.Text))
            {
                _isDirty = Entries.Any(x => x.IsModified);
                UpdateStatus();
            }
        }

        private void SearchBox_TextChanged(object sender, System.Windows.Controls.TextChangedEventArgs e) =>
            EntriesView?.Refresh();

        private bool FilterEntry(object item)
        {
            if (item is not TextEntryViewModel entry || string.IsNullOrWhiteSpace(SearchBox?.Text))
                return true;
            var search = SearchBox.Text;
            return entry.Text.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
                entry.Number.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                entry.Formats.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                entry.ContainsLocation(search);
        }

        private static bool IsSupportedFile(string path)
        {
            if (!File.Exists(path))
                return false;
            var extension = Path.GetExtension(path);
            return string.Equals(extension, ".binl", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".kmb", StringComparison.OrdinalIgnoreCase);
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                this,
                "KH1 Text editor - OpenKH\n\nEdits remastered BINL and KMB text. Folder mode groups identical text and updates every occurrence.",
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
            if (IsBusy)
                return;
            if (_documents.Count == 0)
            {
                StatusText = "Open a BINL/KMB file or a remastered folder.";
                return;
            }

            var occurrenceCount = _documents.Sum(x => x.Entries.Count);
            StatusText = $"{_documents.Count:N0} file(s) · {Entries.Count:N0} unique text(s) · {occurrenceCount:N0} occurrence(s)" +
                (_isDirty ? $" · {Entries.Count(x => x.IsModified):N0} modified group(s)" : string.Empty);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

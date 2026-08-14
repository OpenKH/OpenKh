using Microsoft.Win32;
using OpenKh.Tools.Kh1TextEditor.Models;
using OpenKh.Tools.Kh1TextEditor.ViewModels;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;

namespace OpenKh.Tools.Kh1TextEditor.Views
{
    public partial class MainWindow : Window, INotifyPropertyChanged
    {
        private static readonly string[] FormatOrder = { "BINL", "KMB", "BIN", "EVDL", "EV" };

        private sealed class LoadResult
        {
            public List<LoadedDocument> Documents { get; } = new();
            public List<string> Errors { get; } = new();
        }

        private sealed class BuiltDocument
        {
            public LoadedDocument Document { get; init; }
            public byte[] Data { get; init; }
        }

        private List<LoadedDocument> _documents = new();
        private List<TextFormatTabViewModel> _tabs = new();
        private TextFormatTabViewModel _selectedTab;
        private string _sourcePath;
        private string _languageCode;
        private string _statusText;
        private bool _isFolder;
        private bool _isDirty;
        private bool _isBusy;

        public MainWindow()
        {
            InitializeComponent();
            DataContext = this;
            Loaded += MainWindow_Loaded;
        }

        public List<TextFormatTabViewModel> Tabs
        {
            get => _tabs;
            private set
            {
                _tabs = value;
                OnPropertyChanged();
            }
        }

        public TextFormatTabViewModel SelectedTab
        {
            get => _selectedTab;
            set
            {
                _selectedTab = value;
                OnPropertyChanged();
                UpdateStatus();
            }
        }

        public bool CanSaveAs => _documents.Count > 0 && !IsBusy;
        public bool CanChangeLanguage => _isFolder && !IsBusy;
        public string LanguageButtonText => _isFolder
            ? $"File language: {_languageCode ?? "All"}"
            : "File language";

        public bool IsBusy
        {
            get => _isBusy;
            private set
            {
                _isBusy = value;
                Mouse.OverrideCursor = value ? Cursors.Wait : null;
                OnPropertyChanged();
                OnPropertyChanged(nameof(CanSaveAs));
                OnPropertyChanged(nameof(CanChangeLanguage));
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
            if (argument != null && Directory.Exists(argument))
            {
                if (TrySelectLanguage(argument, out var languageCode))
                    await LoadPathAsync(argument, languageCode);
            }
            else if (argument != null)
                await LoadPathAsync(argument, null);
            else
                UpdateStatus();
        }

        private async void OpenFile_Executed(object sender, ExecutedRoutedEventArgs e)
        {
            var dialog = new OpenFileDialog
            {
                Filter = "KH1 remastered text (*.binl;*.kmb;*.bin;*.evdl;*.ev)|*.binl;*.kmb;*.bin;*.evdl;*.ev|" +
                    "BINL files (*.binl)|*.binl|KMB files (*.kmb)|*.kmb|BIN text tables (*.bin)|*.bin|" +
                    "EVDL files (*.evdl)|*.evdl|EV files (*.ev)|*.ev|All files (*.*)|*.*",
                Title = "Open KH1 text file",
            };
            if (dialog.ShowDialog(this) == true && ConfirmDiscardChanges())
                await LoadPathAsync(dialog.FileName, null);
        }

        private async void OpenFolder_Click(object sender, RoutedEventArgs e)
        {
            var dialog = new OpenFolderDialog
            {
                Title = "Open KH1 remastered folder",
                Multiselect = false,
            };
            if (dialog.ShowDialog(this) == true && ConfirmDiscardChanges() &&
                TrySelectLanguage(dialog.FolderName, out var languageCode))
                await LoadPathAsync(dialog.FolderName, languageCode);
        }

        private async Task LoadPathAsync(string path, string languageCode)
        {
            try
            {
                IsBusy = true;
                StatusText = Directory.Exists(path)
                    ? "Scanning KH1 text files..."
                    : $"Opening {Path.GetFileName(path)}...";

                var previousFormat = SelectedTab?.Format;
                var loaded = await Task.Run(() =>
                {
                    var result = LoadDocuments(path, languageCode);
                    var tabs = result.Documents
                        .GroupBy(x => x.Category, StringComparer.Ordinal)
                        .OrderBy(x => Array.IndexOf(FormatOrder, x.Key))
                        .Select(formatGroup =>
                        {
                            var groups = formatGroup
                                .SelectMany(x => x.Entries)
                                .GroupBy(x => x.Text, StringComparer.Ordinal)
                                .Select((group, index) => new TextEntryViewModel(index, group))
                                .ToList();
                            return new TextFormatTabViewModel(formatGroup.Key, groups);
                        })
                        .ToList();
                    return (Result: result, Tabs: tabs);
                });

                if (loaded.Result.Documents.Count == 0)
                    throw new InvalidDataException("No readable KH1 text files were found.");

                _documents = loaded.Result.Documents;
                _sourcePath = path;
                _isFolder = Directory.Exists(path);
                _languageCode = _isFolder ? languageCode : null;
                SetTabs(loaded.Tabs);
                SelectedTab = Tabs.FirstOrDefault(x => x.Format == previousFormat) ?? Tabs.FirstOrDefault();
                _isDirty = false;

                var languageTitle = _languageCode == null ? string.Empty : $" [{_languageCode}]";
                Title = $"{Path.GetFileName(path.TrimEnd(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar))}" +
                    $"{languageTitle} | KH1 Text editor - OpenKH";
                OnPropertyChanged(nameof(CanSaveAs));
                OnPropertyChanged(nameof(CanChangeLanguage));
                OnPropertyChanged(nameof(LanguageButtonText));
                UpdateStatus();

                if (loaded.Result.Errors.Count > 0)
                {
                    var details = string.Join(Environment.NewLine, loaded.Result.Errors.Take(10));
                    if (loaded.Result.Errors.Count > 10)
                        details += $"{Environment.NewLine}... and {loaded.Result.Errors.Count - 10} more files.";
                    MessageBox.Show(
                        this,
                        $"{loaded.Result.Errors.Count} file(s) could not be read:{Environment.NewLine}{Environment.NewLine}{details}",
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

        private static LoadResult LoadDocuments(string path, string languageCode)
        {
            var result = new LoadResult();
            var isFolder = Directory.Exists(path);
            var files = isFolder
                ? Directory.EnumerateFiles(path, "*", SearchOption.AllDirectories)
                    .Where(IsSupportedFile)
                    .Where(x => MatchesLanguage(x, languageCode))
                    .OrderBy(x => x, StringComparer.OrdinalIgnoreCase)
                : new[] { path }.AsEnumerable();

            foreach (var fileName in files)
            {
                try
                {
                    var document = LoadedDocument.Read(fileName, path);
                    if (document != null && document.Entries.Count > 0)
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

        private async void ChangeLanguage_Click(object sender, RoutedEventArgs e)
        {
            if (!CanChangeLanguage || !ConfirmDiscardChanges())
                return;
            if (TrySelectLanguage(_sourcePath, out var languageCode))
                await LoadPathAsync(_sourcePath, languageCode);
        }

        private async void SaveAs_Click(object sender, RoutedEventArgs e)
        {
            if (!CanSaveAs)
                return;

            var dialog = new SaveFileDialog();
            if (_isFolder)
            {
                var language = _languageCode == null ? string.Empty : $"-{_languageCode}";
                dialog.Filter = "ZIP archives (*.zip)|*.zip";
                dialog.FileName = $"{Path.GetFileName(_sourcePath.TrimEnd(Path.DirectorySeparatorChar))}{language}-text.zip";
                dialog.Title = "Export modified KH1 text files";
            }
            else
            {
                var document = _documents[0];
                var extension = Path.GetExtension(document.FileName);
                dialog.Filter = $"{document.Category} files (*{extension})|*{extension}|All files (*.*)|*.*";
                dialog.FileName = Path.GetFileName(document.FileName);
                dialog.Title = "Save KH1 text file as";
            }

            if (dialog.ShowDialog(this) == true)
                await SaveChangesAsync(dialog.FileName);
        }

        private async Task SaveChangesAsync(string saveAsFileName)
        {
            if (_documents.Count == 0 || IsBusy)
                return;

            var modifiedGroups = AllEntries().Where(x => x.IsModified).ToList();
            var exportingZip = _isFolder && saveAsFileName != null;
            if (modifiedGroups.Count == 0)
            {
                if (exportingZip)
                {
                    MessageBox.Show(this, "There are no modified files to export.", "KH1 Text editor",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                }
                if (_isFolder || saveAsFileName == null)
                    return;
            }

            try
            {
                IsBusy = true;
                StatusText = "Encoding and validating changes...";
                foreach (var group in modifiedGroups)
                    group.Apply();

                var affectedDocuments = modifiedGroups
                    .SelectMany(x => x.Documents)
                    .Distinct()
                    .ToList();
                if (!_isFolder && saveAsFileName != null)
                    affectedDocuments = _documents;

                var output = await Task.Run(() => affectedDocuments
                    .Select(x => new BuiltDocument { Document = x, Data = x.BuildFile() })
                    .ToList());

                if (exportingZip)
                {
                    StatusText = "Creating ZIP archive...";
                    await Task.Run(() => WriteZipFile(saveAsFileName, output));
                    UpdateStatus();
                    MessageBox.Show(this, $"Created {saveAsFileName}", "KH1 Text editor",
                        MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

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
                await LoadPathAsync(saveAsFileName ?? _sourcePath, saveAsFileName == null ? _languageCode : null);
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

        private static void WriteZipFile(string fileName, IReadOnlyList<BuiltDocument> documents)
        {
            var directory = Path.GetDirectoryName(Path.GetFullPath(fileName));
            Directory.CreateDirectory(directory);
            var temporaryFile = Path.Combine(directory, $".{Path.GetFileName(fileName)}.{Guid.NewGuid():N}.tmp");
            try
            {
                using (var file = File.Create(temporaryFile))
                using (var archive = new ZipArchive(file, ZipArchiveMode.Create))
                {
                    foreach (var item in documents.OrderBy(x => x.Document.RelativePath, StringComparer.OrdinalIgnoreCase))
                    {
                        var entryName = item.Document.RelativePath.Replace('\\', '/');
                        if (Path.IsPathRooted(entryName) || entryName.Split('/').Any(x => x == ".."))
                            throw new InvalidDataException($"Unsafe ZIP path: {entryName}");
                        var entry = archive.CreateEntry(entryName, CompressionLevel.Optimal);
                        using var output = entry.Open();
                        output.Write(item.Data);
                    }
                }
                File.Move(temporaryFile, fileName, true);
            }
            finally
            {
                if (File.Exists(temporaryFile))
                    File.Delete(temporaryFile);
            }
        }

        private void SetTabs(List<TextFormatTabViewModel> tabs)
        {
            foreach (var oldEntry in AllEntries())
                oldEntry.PropertyChanged -= Entry_PropertyChanged;
            Tabs = tabs;
            foreach (var entry in AllEntries())
                entry.PropertyChanged += Entry_PropertyChanged;
        }

        private IEnumerable<TextEntryViewModel> AllEntries() => Tabs.SelectMany(x => x.Entries);

        private void Entry_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(TextEntryViewModel.Text))
            {
                _isDirty = AllEntries().Any(x => x.IsModified);
                UpdateStatus();
            }
        }

        private static bool IsSupportedFile(string path)
        {
            if (!File.Exists(path))
                return false;
            var extension = Path.GetExtension(path);
            return string.Equals(extension, ".binl", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".kmb", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".evdl", StringComparison.OrdinalIgnoreCase) ||
                string.Equals(extension, ".ev", StringComparison.OrdinalIgnoreCase) ||
                (string.Equals(extension, ".bin", StringComparison.OrdinalIgnoreCase) &&
                    LoadedDocument.IsTextBinFile(path));
        }

        private static bool MatchesLanguage(string path, string languageCode)
        {
            var name = Path.GetFileName(path);
            var hasPrefix = name.Length > 3 && name[2] == '_' &&
                char.IsLetter(name[0]) && char.IsLetter(name[1]);
            if (!hasPrefix)
                return true;

            var prefix = name.Substring(0, 2);
            if (prefix.Equals("FM", StringComparison.OrdinalIgnoreCase))
                return false;
            return languageCode == null || prefix.Equals(languageCode, StringComparison.OrdinalIgnoreCase);
        }

        private bool TrySelectLanguage(string folderName, out string languageCode)
        {
            var languages = Directory.EnumerateFiles(folderName, "*", SearchOption.AllDirectories)
                .Where(IsSupportedFile)
                .Select(Path.GetFileName)
                .Where(x => x.Length > 3 && x[2] == '_')
                .Select(x => x.Substring(0, 2).ToUpperInvariant())
                .Where(x => x != "FM")
                .Distinct()
                .ToList();
            var dialog = new LanguageSelectionWindow(languages, _languageCode ?? "US")
            {
                Owner = this,
            };
            if (dialog.ShowDialog() != true)
            {
                languageCode = null;
                return false;
            }

            languageCode = dialog.SelectedLanguage;
            return true;
        }

        private void About_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show(
                this,
                "KH1 Text editor - OpenKH\n\nEdits remastered BINL, KMB, BIN, EVDL and EV text. " +
                    "Folder mode groups identical text within each file type.",
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
                StatusText = "Open a KH1 remastered text file or folder.";
                return;
            }

            var occurrenceCount = _documents.Sum(x => x.Entries.Count);
            var uniqueCount = Tabs.Sum(x => x.Entries.Count);
            var language = _languageCode == null ? string.Empty : $" · language {_languageCode}";
            var active = SelectedTab == null ? string.Empty : $" · {SelectedTab.Format} tab";
            StatusText = $"{_documents.Count:N0} file(s){language} · {uniqueCount:N0} unique text(s)" +
                $" · {occurrenceCount:N0} occurrence(s){active}" +
                (_isDirty ? $" · {AllEntries().Count(x => x.IsModified):N0} modified group(s)" : string.Empty);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

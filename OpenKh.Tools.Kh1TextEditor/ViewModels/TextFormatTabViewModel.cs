using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows.Data;

namespace OpenKh.Tools.Kh1TextEditor.ViewModels
{
    public sealed class TextFormatTabViewModel : INotifyPropertyChanged
    {
        private TextEntryViewModel _selectedEntry;
        private string _searchText;

        public TextFormatTabViewModel(string format, List<TextEntryViewModel> entries)
        {
            Format = format;
            Entries = entries;
            EntriesView = CollectionViewSource.GetDefaultView(Entries);
            EntriesView.Filter = FilterEntry;
            SelectedEntry = Entries.FirstOrDefault();
        }

        public string Format { get; }
        public string Header => $"{Format} ({Entries.Count:N0})";
        public List<TextEntryViewModel> Entries { get; }
        public ICollectionView EntriesView { get; }

        public TextEntryViewModel SelectedEntry
        {
            get => _selectedEntry;
            set
            {
                _selectedEntry = value;
                OnPropertyChanged();
            }
        }

        public string SearchText
        {
            get => _searchText;
            set
            {
                if (string.Equals(_searchText, value, StringComparison.Ordinal))
                    return;
                _searchText = value;
                OnPropertyChanged();
                EntriesView.Refresh();
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private bool FilterEntry(object item)
        {
            if (item is not TextEntryViewModel entry || string.IsNullOrWhiteSpace(SearchText))
                return true;
            return entry.Text.Contains(SearchText, StringComparison.CurrentCultureIgnoreCase) ||
                entry.Number.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                entry.Formats.Contains(SearchText, StringComparison.OrdinalIgnoreCase) ||
                entry.ContainsLocation(SearchText);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

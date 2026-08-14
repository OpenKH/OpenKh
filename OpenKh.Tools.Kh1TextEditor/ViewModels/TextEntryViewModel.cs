using OpenKh.Tools.Kh1TextEditor.Models;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OpenKh.Tools.Kh1TextEditor.ViewModels
{
    public sealed class TextEntryViewModel : INotifyPropertyChanged
    {
        private readonly IReadOnlyList<TextOccurrence> _occurrences;
        private string _locations;
        private string _originalText;
        private string _text;

        internal TextEntryViewModel(int index, IGrouping<string, TextOccurrence> group)
        {
            Index = index;
            _occurrences = group.ToList();
            _text = group.Key;
            _originalText = group.Key;

        }

        public int Index { get; }
        public string Number => $"#{Index + 1:D4}";
        public string OriginalText => _originalText;
        public bool IsModified => !string.Equals(_originalText, Text, StringComparison.Ordinal);
        public string Preview => Text.Replace("\r", string.Empty).Replace("\n", " ↵ ");
        public int OccurrenceCount => _occurrences.Count;
        public int FileCount => _occurrences.Select(x => x.Document).Distinct().Count();
        public string Occurrences => OccurrenceCount == 1
            ? "1 occurrence"
            : $"{OccurrenceCount} occurrences in {FileCount} file(s)";
        public string Formats => string.Join("/", _occurrences.Select(x => x.Document.Format).Distinct());
        public string Locations => _locations ??= BuildLocations();
        internal IEnumerable<LoadedDocument> Documents => _occurrences.Select(x => x.Document).Distinct();
        internal bool ContainsLocation(string search) => _occurrences.Any(x =>
            x.Document.RelativePath.Contains(search, StringComparison.CurrentCultureIgnoreCase));

        public string Text
        {
            get => _text;
            set
            {
                if (string.Equals(_text, value, StringComparison.Ordinal))
                    return;
                _text = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(Preview));
                OnPropertyChanged(nameof(IsModified));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        internal void Apply()
        {
            foreach (var occurrence in _occurrences)
                occurrence.SetText(Text);
        }

        internal void AcceptChanges()
        {
            _originalText = Text;
            OnPropertyChanged(nameof(OriginalText));
            OnPropertyChanged(nameof(IsModified));
        }

        private string BuildLocations()
        {
            var locations = _occurrences.Take(200).Select(x => x.Location).ToList();
            if (_occurrences.Count > locations.Count)
                locations.Add($"... and {_occurrences.Count - locations.Count} more occurrences");
            return string.Join(Environment.NewLine, locations);
        }

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

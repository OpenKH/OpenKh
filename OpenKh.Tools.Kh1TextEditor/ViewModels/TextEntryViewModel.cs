using OpenKh.Kh1;
using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace OpenKh.Tools.Kh1TextEditor.ViewModels
{
    public sealed class TextEntryViewModel : INotifyPropertyChanged
    {
        private string _text;

        public TextEntryViewModel(Kh1Binl.TextEntry entry)
        {
            Entry = entry;
            _text = entry.Text;
            OriginalText = entry.Text;
        }

        public Kh1Binl.TextEntry Entry { get; }
        public int Index => Entry.Index;
        public string Number => $"#{Index + 1:D3}";
        public string Offset => $"0x{Entry.Offset:X6}";
        public string OriginalText { get; }
        public bool IsModified => !string.Equals(OriginalText, Text, StringComparison.Ordinal);
        public string Preview => Text.Replace("\r", string.Empty).Replace("\n", " ↵ ");

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

        private void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}

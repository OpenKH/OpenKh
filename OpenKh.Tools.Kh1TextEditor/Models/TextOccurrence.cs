using System;

namespace OpenKh.Tools.Kh1TextEditor.Models
{
    internal sealed class TextOccurrence
    {
        private readonly Func<string> _getText;
        private readonly Action<string> _setText;

        public TextOccurrence(
            LoadedDocument document,
            int index,
            int offset,
            Func<string> getText,
            Action<string> setText)
        {
            Document = document;
            Index = index;
            Offset = offset;
            _getText = getText;
            _setText = setText;
        }

        public LoadedDocument Document { get; }
        public int Index { get; }
        public int Offset { get; }
        public string Text => _getText();
        public string Location => $"{Document.RelativePath}  #{Index + 1:D3}  0x{Offset:X6}";

        public void SetText(string text) => _setText(text);
    }
}

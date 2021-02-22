using OpenKh.Kh2;
using OpenKh.Tools.BarTool.Views;

using ReactiveUI;

namespace OpenKh.Tools.BarTool.Models
{
    public class EntryModel : ReactiveObject
    {
        Bar.Entry _entry;

        string _tag;
        Bar.EntryType _type;
        bool _duplicate;

        public Bar.Entry Entry
        {
            get => _entry;
        }

        public string Name 
        { 
            get => string.Format("{0} [{1}] {2}", _tag, Helpers.GetSuggestedExtension(_type), _duplicate ? "D" : ""); 
        }

        public string Tag 
        { 
            get => _tag;
            set
            {
                _entry.Name = value;
                this.RaiseAndSetIfChanged(ref _tag, value, nameof(Name));
                MainWindow.Instance.IsSaved = false;
            }
        }

        public Bar.EntryType Type
        {
            get => _type;
            set
            {
                _entry.Type = value;
                this.RaiseAndSetIfChanged(ref _type, value, nameof(Name));
                MainWindow.Instance.IsSaved = false;
            }
        }

        public bool Duplicate
        {
            get => _duplicate;
        }

        public EntryModel(Bar.Entry Input)
        {
            _entry = Input;

            _tag = _entry.Name;
            _type = _entry.Type;
            _duplicate = _entry.Duplicate;
        }
    }
}

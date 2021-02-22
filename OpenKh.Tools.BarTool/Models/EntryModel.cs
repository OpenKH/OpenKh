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
            get => string.Format("{0} [{1}]{2}", _tag, Helpers.GetSuggestedExtension(_type).ToUpper(), _duplicate ? " - D" : ""); 
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

        public string Size
        {
            get
            {
                string _unit = "B";
                int _factor;

                if (Entry.Stream.Length >= 1024 * 1024)
                {
                    _unit = "MiB";
                    _factor = 1024 * 1024;
                }

                else if (Entry.Stream.Length >= 1024)
                {
                    _unit = "KiB";
                    _factor = 1024;
                }

                else
                {
                    return $"{Entry.Stream.Length} {_unit}";
                }

                return $"{(float)Entry.Stream.Length / _factor:0.00} {_unit}";
            }
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

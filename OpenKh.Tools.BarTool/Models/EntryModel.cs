using OpenKh.Kh2;
using OpenKh.Tools.BarTool.Views;
using OpenKh.Tools.BarTool.Interfaces;

using ReactiveUI;
using System.Text;

namespace OpenKh.Tools.BarTool.Models
{
    public class EntryModel : ReactiveObject
    {
        Bar.Entry _entry;
        IViewSettings _viewSettings;

        string _tag;
        Bar.EntryType _type;
        bool _duplicate;

        private readonly string[] MotionsetMode = new string[] { "BW", "B_", "__", "_W" };

        public Bar.Entry Entry
        {
            get => _entry;
        }

        public string Name 
        {
            get
            {
                var _builder = new StringBuilder(0x30);

                if (_viewSettings.ShowSlotNumber || _viewSettings.ShowMovesetName)
                {
                    var _index = _viewSettings.GetSlotIndex(this);

                    if (_viewSettings.ShowSlotNumber)
                        _builder.Append($"{_index:D02} ");

                    if (_viewSettings.ShowMovesetName)
                    {
                        var _motionID = _index;

                        if (_viewSettings.IsPlayer)
                        {
                            _motionID /= 4;
                            _builder.Append($"{MotionsetMode[_index & 3]} ");
                        }

                        _builder.Append($"{(MotionSet.MotionName)_motionID} {Tag}");
                        return _builder.ToString();
                    }
                }

                _builder.Append(string.Format("{0} [{1}]{2}", Tag, Helpers.GetSuggestedExtension(Type).ToUpper(), Duplicate ? " - D" : ""));
                return _builder.ToString();
            }
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

        public void Invalidate() => this.RaisePropertyChanged((nameof(Name)));


        public EntryModel(Bar.Entry Input, IViewSettings View)
        {
            _entry = Input;
            _viewSettings = View;

            _tag = _entry.Name;
            _type = _entry.Type;
            _duplicate = _entry.Duplicate;
        }
    }
}

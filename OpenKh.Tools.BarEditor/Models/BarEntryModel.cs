using OpenKh.Kh2;
using OpenKh.Tools.BarEditor.Interfaces;
using System.ComponentModel.DataAnnotations;
using System.Text;
using Xe.Tools;
using static OpenKh.Kh2.Bar;

namespace OpenKh.Tools.BarEditor.Models
{
    public class BarEntryModel : BaseNotifyPropertyChanged
    {
        private readonly string[] MotionsetMode = new string[]
        {
            "BW", "B_", "__", "_W"
        };
        private readonly IViewSettings _viewSettings;

		public BarEntryModel(Entry entry, IViewSettings viewSettings)
		{
            Entry = entry;
            _viewSettings = viewSettings;
		}

		public Entry Entry { get; }

        public EntryType Type
		{
			get => Entry.Type;
			set
			{
				Entry.Type = value;
				OnPropertyChanged(nameof(DisplayName));
			}
		}

        public string DisplayName
        {
            get
            {
                var sb = new StringBuilder(0x30);
                if (_viewSettings.ShowSlotNumber || _viewSettings.ShowMovesetName)
                {
                    var slotIndex = _viewSettings.GetSlotIndex(this);

                    if (_viewSettings.ShowSlotNumber)
                        sb.Append($"{slotIndex:D02} ");

                    if (_viewSettings.ShowMovesetName)
                    {
                        var motionId = slotIndex;
                        if (_viewSettings.IsPlayer)
                        {
                            motionId /= 4;
                            sb.Append($"{MotionsetMode[slotIndex & 3]} ");
                        }

                        sb.Append($"{(MotionSet.MotionName)motionId} {Tag}");
                        return sb.ToString();
                    }
                }

                sb.Append($"{Tag} {Index} {Type}");
                return sb.ToString();
            }
        }

        public short Index
		{
			get => (short)Entry.Index;
			set
			{
				Entry.Index = value;
				OnPropertyChanged(nameof(DisplayName));
			}
		}

		[Required]
		[StringLength(4, MinimumLength = 4, ErrorMessage = "Must be 4 characters long.")]
		public string Tag
		{
			get => Entry.Name;
			set
			{
				Entry.Name = value;
				OnPropertyChanged(nameof(DisplayName));
			}
		}

		public string Size
		{
			get
			{
				string unit = "byte";
				int divisor;

				if (Entry.Stream.Length >= 1024 * 1024)
				{
					unit = "MB";
					divisor = 1024 * 1024;
				}
				else if (Entry.Stream.Length >= 1024)
				{
					unit = "KB";
					divisor = 1024;
				}
				else
				{
					return $"{Entry.Stream.Length} {unit}";
				}

				return $"{(float)Entry.Stream.Length / divisor:0.00} {unit}";
			}
		}

        internal void InvalidateTag() => OnPropertyChanged(nameof(DisplayName));
    }
}

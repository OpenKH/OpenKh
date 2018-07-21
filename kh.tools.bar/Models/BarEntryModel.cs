using System.ComponentModel.DataAnnotations;
using Xe.Tools.Wpf;
using static kh.kh2.Bar;

namespace kh.tools.bar.Models
{
    public class BarEntryModel : BaseNotifyPropertyChanged
    {
		public BarEntryModel(Entry entry)
		{
			Entry = entry;
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

		public string DisplayName =>
			$"{Name} {Index} {Type}";

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
		public string Name
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
				int divisor = 1;

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

				return $"{((float)Entry.Stream.Length / divisor).ToString("0.00")} {unit}";
			}
		}
	}
}

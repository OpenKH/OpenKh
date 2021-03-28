using OpenKh.Bbs;
using System.Globalization;
using System.Linq;
using Xe.Tools;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class CharacterViewModel : BaseNotifyPropertyChanged
    {
        private readonly FontCharacterInfo _info;

        public CharacterViewModel(FontCharacterInfo info)
        {
            _info = info;
        }

        public string Id
        {
            get => $"{(_info.Id >> 8) & 0xff:X02} {_info.Id & 0xff:X02}";
            set
            {
                var digits = value
                    .Where(x => !char.IsWhiteSpace(x))
                    .Select(x => int.Parse($"{x}", NumberStyles.HexNumber))
                    .ToArray();
                var actualValue = digits[0] << 12 |
                    digits[1] << 8 |
                    digits[2] << 4 |
                    digits[3];

                _info.Id = (ushort)actualValue;
                OnPropertyChanged(nameof(Title));
            }
        }

        public ushort PositionX
        {
            get => _info.PositionX;
            set => _info.PositionX = value;
        }

        public ushort PositionY
        {
            get => _info.PositionY;
            set => _info.PositionY = value;
        }

        public byte Palette
        {
            get => _info.Palette;
            set => _info.Palette = value;
        }

        public byte Width
        {
            get => _info.Width;
            set => _info.Width = value;
        }

        public string Title => Id;
    }
}

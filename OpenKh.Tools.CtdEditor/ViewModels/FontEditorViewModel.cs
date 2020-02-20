using OpenKh.Bbs;
using OpenKh.Imaging;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Xe.Tools;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class FontEditorViewModel : BaseNotifyPropertyChanged
    {
        private readonly FontsArc _fonts;
        private CharacterViewModel[] _characters;
        private FontEntryViewModel _selectedFont;
        private CharacterViewModel _selectedCharacter;

        public FontEditorViewModel(FontsArc fonts)
        {
            _fonts = fonts;
            Fonts = new[]
            {
                new FontEntryViewModel
                {
                    Name = "Cmd",
                    Font1 = fonts.FontCmd,
                    Font2 = fonts.FontCmd2,
                    Info = fonts.FontCmdInfo
                },
                new FontEntryViewModel
                {
                    Name = "Help",
                    Font1 = fonts.FontHelp,
                    Font2 = fonts.FontHelp2,
                    Info = fonts.FontHelpInfo
                },
                new FontEntryViewModel
                {
                    Name = "Menu",
                    Font1 = fonts.FontMenu,
                    Font2 = fonts.FontMenu2,
                    Info = fonts.FontMenuInfo
                },
                new FontEntryViewModel
                {
                    Name = "Mes",
                    Font1 = fonts.FontMes,
                    Font2 = fonts.FontMes2,
                    Info = fonts.FontMesInfo
                },
                new FontEntryViewModel
                {
                    Name = "Numeral",
                    Font1 = fonts.FontNumeral,
                    Font2 = fonts.FontNumeral2,
                    Info = fonts.FontNumeralInfo
                },
            };
        }

        public FontEntryViewModel[] Fonts { get; }
        public FontEntryViewModel SelectedFont
        {
            get => _selectedFont;
            set
            {
                _selectedFont = value;
                _characters = _selectedFont.Info.Select(x => new CharacterViewModel(x)).ToArray();
                OnPropertyChanged(nameof(IsFontSeleted));
                OnPropertyChanged(nameof(Characters));
            }
        }
        public bool IsFontSeleted => SelectedFont != null;

        public IEnumerable<CharacterViewModel> Characters => _characters;
        public CharacterViewModel SelectedCharacter
        {
            get => _selectedCharacter;
            set
            {
                _selectedCharacter = value;
                OnPropertyChanged(nameof(SelectedCharacter));
                OnPropertyChanged(nameof(IsCharacterSelected));
            }
        }
        public bool IsCharacterSelected => SelectedCharacter != null;
    }

    public class FontEntryViewModel
    {
        public string Name { get; set; }
        public IImageRead Font1 { get; set; }
        public IImageRead Font2 { get; set; }
        public FontCharacterInfo[] Info { get; set; }
    }

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

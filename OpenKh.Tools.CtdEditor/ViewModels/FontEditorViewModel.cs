using OpenKh.Bbs;
using OpenKh.Tools.Common;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xe.Drawing;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.CtdEditor.ViewModels
{
    public class FontEditorViewModel : BaseNotifyPropertyChanged
    {
        private readonly FontsArc _fonts;
        private CharacterViewModel[] _characters;
        private FontEntryViewModel _selectedFont;
        private CharacterViewModel _selectedCharacter;
        private ISurface _surface1;
        private ISurface _surface2;
        private bool orderCharacters;

        public FontEditorViewModel(FontsArc fonts)
        {
            _fonts = fonts;
            Fonts = new[]
            {
                new FontEntryViewModel(fonts.FontCmd),
                new FontEntryViewModel(fonts.FontHelp),
                new FontEntryViewModel(fonts.FontMenu),
                new FontEntryViewModel(fonts.FontMes),
                new FontEntryViewModel(fonts.FontNumeral)
            };

            DrawingContext = new DrawingDirect3D();
            DrawBegin = new RelayCommand(_ =>
            {
                if (_selectedFont?.Font1 == null ||
                    SelectedCharacter == null)
                    return;

                if (_surface1 == null)
                    CreateFontSurfaces();

                if (_surface1 == null)
                    return;

                DrawingContext.Clear(Color.Black);
                PrintCharacter(SelectedCharacter);
                DrawingContext.Flush();
            });
        }

        public FontEntryViewModel[] Fonts { get; }
        public FontEntryViewModel SelectedFont
        {
            get => _selectedFont;
            set
            {
                _selectedFont = value;
                _characters = _selectedFont.CharactersInfo.Select(x => new CharacterViewModel(x)).ToArray();

                DestroyFontSurfaces();
                OnPropertyChanged(nameof(IsFontSeleted));
                OnPropertyChanged(nameof(Characters));
            }
        }
        public bool IsFontSeleted => SelectedFont != null;

        public IEnumerable<CharacterViewModel> Characters =>
            OrderCharacters ? _characters.OrderBy(x => x.Id) : _characters as IEnumerable<CharacterViewModel>;
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

        public bool OrderCharacters
        {
            get => orderCharacters;
            set
            {
                orderCharacters = value;
                OnPropertyChanged(nameof(Characters));
            }
        }

        public IDrawing DrawingContext { get; }
        public RelayCommand DrawBegin { get; }

        private void CreateFontSurfaces()
        {
            _surface1 = DrawingContext.CreateSurface(_selectedFont.Font1);
            _surface2 = DrawingContext.CreateSurface(_selectedFont.Font2);
        }

        private void DestroyFontSurfaces()
        {
            _surface1?.Dispose();
            _surface2?.Dispose();

            _surface1 = null;
            _surface2 = null;
        }

        private void PrintCharacter(CharacterViewModel characterViewModel)
        {
            ISurface surface;

            switch (characterViewModel.Palette)
            {
                case 0:
                    surface = _surface1;
                    break;
                case 1:
                    surface = _surface2;
                    break;
                default:
                    return;
            }

            var src = new Rectangle()
            {
                X = characterViewModel.PositionX,
                Y = characterViewModel.PositionY,
                Width = characterViewModel.Width,
                Height = SelectedFont.Info.CharacterHeight,
            };
            var dst = new Rectangle()
            {
                X = 0,
                Y = 0,
                Width = src.Width * 2,
                Height = src.Height * 2
            };

            DrawingContext.DrawSurface(surface, src, dst);
        }
    }
}

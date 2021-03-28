using OpenKh.Bbs;
using OpenKh.Engine.Renders;
using OpenKh.Tools.Common.Rendering;
using System;
using System.Collections.Generic;
using System.DirectoryServices;
using System.Drawing;
using System.Linq;
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
        private ISpriteTexture _surface1;
        private ISpriteTexture _surface2;
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

            DrawingContext = new SpriteDrawingDirect3D();
            DrawBegin = new RelayCommand(_ =>
            {
                if (_selectedFont?.Font1 == null ||
                    SelectedCharacter == null)
                    return;

                if (_surface1 == null)
                    CreateFontSurfaces();

                if (_surface1 == null)
                    return;

                DrawingContext.Clear(new ColorF(0.0f, 0.0f, 0.0f, 1.0f));
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
            (OrderCharacters ? _characters?.OrderBy(x => x.Id) : _characters as IEnumerable<CharacterViewModel>)
            ?? Array.Empty<CharacterViewModel>();

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

        public ISpriteDrawing DrawingContext { get; }
        public RelayCommand DrawBegin { get; }

        private void CreateFontSurfaces()
        {
            _surface1 = DrawingContext.CreateSpriteTexture(_selectedFont.Font1);
            _surface2 = DrawingContext.CreateSpriteTexture(_selectedFont.Font2);
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
            ISpriteTexture surface;

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

            DrawingContext.AppendSprite(new SpriteDrawingContext()
                .SpriteTexture(surface)
                .Source(characterViewModel.PositionX, characterViewModel.PositionY, characterViewModel.Width, SelectedFont.Info.CharacterHeight)
                .MatchSourceSize()
                .ScaleSize(2));
        }
    }
}

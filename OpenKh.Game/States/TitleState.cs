using OpenKh.Engine;
using OpenKh.Engine.Renderers;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.Linq;
using Xe.Drawing;

namespace OpenKh.Game.States
{
    public class TitleState : IState
    {
        private const int MainMenuNewGameOption = 0;
        private const int MainMenuLoadOption = 1;
        private const int MainMenuTheaterOption = 2;
        private const int MainMenuBackOption = 3;
        private const int MainMenuMaxOptionCount = 4;

        private class TitleLayout
        {
            public int FadeIn { get; set; }
            public int Copyright { get; set; }
            public int Intro { get; set; }
            public int IntroSkip { get; set; }
            public int NewGame { get; set; }
            public int MenuOptionNewGame { get; set; }
            public int MenuOptionLoad { get; set; }
            public int MenuOptionTheater { get; set; }
            public int MenuOptionBack { get; set; }
            public bool HasTheater { get; set; }
            public bool HasBack { get; set; }
        }

        private static readonly TitleLayout VanillaTitleLayout = new TitleLayout
        {
            Copyright = 8,
            Intro = 11,
            IntroSkip = 12,
            NewGame = 13,
            MenuOptionNewGame = 0,
            MenuOptionLoad = 1,
            MenuOptionTheater = 2,
            MenuOptionBack = 3,
            HasTheater = false,
            HasBack = false,
        };

        private static readonly TitleLayout FinalMixTitleLayout = new TitleLayout
        {
            FadeIn = 17,
            Copyright = 8,
            Intro = 12,
            IntroSkip = 14,
            NewGame = 10, // but also 15?
            MenuOptionNewGame = 0,
            MenuOptionLoad = 1,
            MenuOptionTheater = 4,
            HasTheater = false,
            HasBack = false,
        };

        private static readonly TitleLayout FinalMixTheaterTitleLayout = new TitleLayout
        {
            FadeIn = 17,
            Copyright = 8,
            Intro = 13,
            IntroSkip = 14,
            NewGame = 11, // but also 16?
            MenuOptionNewGame = 2,
            MenuOptionLoad = 3,
            MenuOptionTheater = 4,
            HasTheater = true,
            HasBack = false,
        };

        private static readonly TitleLayout ReMixTitleLayout = new TitleLayout
        {
            FadeIn = 17,
            Copyright = 8,
            Intro = 12,
            IntroSkip = 14,
            NewGame = 10, // but also 15?
            MenuOptionNewGame = 0,
            MenuOptionLoad = 1,
            MenuOptionTheater = 4,
            MenuOptionBack = 18,
            HasTheater = false,
            HasBack = true,
        };

        private static readonly TitleLayout ReMixTheaterTitleLayout = new TitleLayout
        {
            FadeIn = 17,
            Copyright = 8,
            Intro = 13,
            IntroSkip = 14,
            NewGame = 11, // but also 15?
            MenuOptionNewGame = 2,
            MenuOptionLoad = 3,
            MenuOptionTheater = 4,
            MenuOptionBack = 19,
            HasTheater = false,
            HasBack = true,
        };

        private Kernel _kernel;
        private ArchiveManager _archiveManager;
        private InputManager _inputManager;
        private IStateChange _stateChange;
        private MonoDrawing drawing;
        private LayoutRenderer layoutRendererFg;
        private LayoutRenderer layoutRendererBg;
        private LayoutRenderer layoutRendererTheater;
        private Dictionary<string, IEnumerable<ISurface>> cachedSurfaces;

        private TitleLayout _titleLayout;
        private bool _isTheaterModeUnlocked;
        private int _optionSelected;
        private bool _isInTheaterMenu;

        public void Initialize(StateInitDesc initDesc)
        {
            _kernel = initDesc.Kernel;
            _archiveManager = initDesc.ArchiveManager;
            _inputManager = initDesc.InputManager;
            _stateChange = initDesc.StateChange;

            drawing = new MonoDrawing(initDesc.GraphicsDevice.GraphicsDevice);
            cachedSurfaces = new Dictionary<string, IEnumerable<ISurface>>();

            _archiveManager.LoadArchive($"menu/{_kernel.Region}/title.2ld");
            _archiveManager.LoadArchive($"menu/{_kernel.Region}/save.2ld");

            _isTheaterModeUnlocked = true;
            if (_kernel.RegionId == Constants.RegionFinalMix)
            {
                if (_isTheaterModeUnlocked)
                    _titleLayout = FinalMixTheaterTitleLayout;
                else
                    _titleLayout = FinalMixTitleLayout;
            }
            else
                _titleLayout = VanillaTitleLayout;

            layoutRendererBg = CreateLayoutRenderer("titl");
            layoutRendererFg = CreateLayoutRenderer("titl");
            layoutRendererBg.SelectedSequenceGroupIndex = _titleLayout.IntroSkip;

            if (_titleLayout.HasTheater)
                layoutRendererTheater = CreateLayoutRenderer("even");

            SetOption(0);
        }

        public void Destroy()
        {
            throw new System.NotImplementedException();
        }

        public void Update(DeltaTimes deltaTimes)
        {
            CheckTitlLoop(layoutRendererBg);
            CheckTitlLoop(layoutRendererFg);
            
            if (_isInTheaterMenu)
            {
                switch (layoutRendererTheater.SelectedSequenceGroupIndex)
                {
                    case 0:
                        layoutRendererTheater.FrameIndex++;
                        if (layoutRendererTheater.FrameIndex > 32)
                        {
                            layoutRendererTheater.FrameIndex = 0;
                            layoutRendererTheater.SelectedSequenceGroupIndex = 1;
                        }
                        break;
                    case 1:
                        break;
                    case 2:
                        layoutRendererTheater.FrameIndex++;
                        if (layoutRendererTheater.FrameIndex > 32)
                            _isInTheaterMenu = false;
                        break;
                }
            }

            if (_isInTheaterMenu == false)
                ProcessInputMainMenu();
            else
                ProcessInputTheaterMenu();
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            layoutRendererBg.Draw();
            layoutRendererFg.Draw();

            if (_isInTheaterMenu)
                layoutRendererTheater.Draw();
        }

        private void ProcessInputMainMenu()
        {
            var currentOption = _optionSelected;
            if (_inputManager.IsUp)
                currentOption--;
            else if (_inputManager.IsDown)
                currentOption++;
            else if (_inputManager.IsCircle || _inputManager.IsCross)
            {
                switch (currentOption)
                {
                    case MainMenuNewGameOption:
                        _stateChange.State = 1;
                        break;
                    case MainMenuLoadOption:
                        _stateChange.State = 1;
                        break;
                    case MainMenuTheaterOption:
                        _isInTheaterMenu = true;
                        layoutRendererTheater.FrameIndex = 0;
                        layoutRendererTheater.SelectedSequenceGroupIndex = 0;
                        break;
                }
            }

            if (currentOption == MainMenuTheaterOption && !_titleLayout.HasTheater)
                currentOption++;
            if (currentOption == MainMenuBackOption && !_titleLayout.HasBack)
                currentOption++;
            if (currentOption >= MainMenuMaxOptionCount)
                currentOption = 0;
            if (currentOption < 0)
            {
                if (_titleLayout.HasBack)
                    currentOption = MainMenuBackOption;
                else if (_titleLayout.HasTheater)
                    currentOption = MainMenuTheaterOption;
                else
                    currentOption = MainMenuLoadOption;
            }

            if (currentOption != _optionSelected)
                SetOption(currentOption);
        }

        private void ProcessInputTheaterMenu()
        {
            if (_inputManager.IsCross)
            {
                layoutRendererTheater.FrameIndex = 0;
                layoutRendererTheater.SelectedSequenceGroupIndex = 2;
            }
        }

        private void SetOption(int option)
        {
            _optionSelected = option;

            switch (_optionSelected)
            {
                case MainMenuNewGameOption:
                    layoutRendererFg.SelectedSequenceGroupIndex = _titleLayout.MenuOptionNewGame;
                    break;
                case MainMenuLoadOption:
                    layoutRendererFg.SelectedSequenceGroupIndex = _titleLayout.MenuOptionLoad;
                    break;
                case MainMenuTheaterOption:
                    layoutRendererFg.SelectedSequenceGroupIndex = _titleLayout.MenuOptionTheater;
                    break;
                case MainMenuBackOption:
                    layoutRendererFg.SelectedSequenceGroupIndex = _titleLayout.MenuOptionBack;
                    break;
            }
        }

        private void CheckTitlLoop(LayoutRenderer layout)
        {
            var currentSequenceGroupIndex = layout.SelectedSequenceGroupIndex;

            if (currentSequenceGroupIndex == _titleLayout.MenuOptionNewGame ||
                currentSequenceGroupIndex == _titleLayout.MenuOptionLoad ||
                currentSequenceGroupIndex == _titleLayout.MenuOptionTheater ||
                currentSequenceGroupIndex == _titleLayout.MenuOptionBack)
            {
                if (layout.FrameIndex > 178)
                    layout.FrameIndex = 70;
                layout.FrameIndex++;
            }
            else if (currentSequenceGroupIndex == _titleLayout.IntroSkip)
            {
                if (layout.FrameIndex < 119)
                    layout.FrameIndex++;
            }
        }

        private LayoutRenderer CreateLayoutRenderer(string resourceName) => CreateLayoutRenderer(resourceName, resourceName);

        private LayoutRenderer CreateLayoutRenderer(string layoutResourceName, string imagesResourceName)
        {
            var layout = _archiveManager.Get<Layout>(layoutResourceName);
            if (!cachedSurfaces.TryGetValue(imagesResourceName, out var images))
                images = cachedSurfaces[imagesResourceName] = _archiveManager.Get<Imgz>(imagesResourceName)
                    ?.Images?.Select(x => drawing.CreateSurface(x));

            return new LayoutRenderer(layout, drawing, images);
        }

        public void DebugUpdate(IDebug debug)
        {
        }

        public void DebugDraw(IDebug debug)
        {
            debug.Println("TITLE SCREEN");
        }
    }
}

using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.Shaders;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.States
{
    public class TitleState : IState
    {
        private const int MainMenuNewGameOption = 0;
        private const int MainMenuLoadOption = 1;
        private const int MainMenuTheaterOption = 2;
        private const int MainMenuBackOption = 3;
        private const int MainMenuMaxOptionCount = 4;

        private const int MainMenuSequence = 0;
        private const int NewGameTitle = 22;
        private const int NewGameWindow = 28;
        private const int NewGameOption = 15;

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
            NewGame = 11, // but also 16?
            MenuOptionNewGame = 2,
            MenuOptionLoad = 3,
            MenuOptionTheater = 4,
            MenuOptionBack = 19,
            HasTheater = true,
            HasBack = true,
        };

        private class AnimatedSequenceRenderer
        {
            private readonly SequenceRenderer _renderer;
            private readonly int _animStart;
            private readonly int _animLoop;
            private readonly int _animEnd;
            private int _anim;
            private int _frame;
            private bool _isRunning;
            public bool IsEnd { get; set; }

            public AnimatedSequenceRenderer(SequenceRenderer renderer, int anim) :
                this(renderer, anim, anim + 1, anim + 2)
            { }

            public AnimatedSequenceRenderer(SequenceRenderer renderer,
                int animStart, int animLoop, int animEnd)
            {
                _renderer = renderer;
                _animStart = animStart;
                _animLoop = animLoop;
                _animEnd = animEnd;
                IsEnd = true;
            }

            public void Update(double deltaTime)
            {
                _frame++;
            }

            public void Draw(float x, float y)
            {
                if (IsEnd)
                    return;

                if (!_renderer.Draw(_anim, _frame, x, y))
                {
                    if (_isRunning)
                    {
                        _anim = _animLoop;
                        _frame = 0;
                    }
                    else
                        IsEnd = true;
                }
            }

            public void Begin()
            {
                _anim = _animStart;
                _isRunning = true;
                IsEnd = false;
                _frame = 0;
            }

            public void Skip()
            {
                if (_isRunning)
                {
                    if (_anim == _animStart)
                    {
                        _anim = _animLoop;
                        _frame = 0;
                    }
                }
                else
                    IsEnd = true;
            }

            public void End()
            {
                _anim = _animEnd;
                _isRunning = false;
                IsEnd = false;
                _frame = 0;
            }
        }

        private const int DifficultyCount = 4;
        private static readonly ushort[] DifficultyTitle = new ushort[DifficultyCount]
        {
            0x4331, 0x4332, 0x4333, 0x4e33
        };
        private static readonly ushort[] DifficultyDescription = new ushort[DifficultyCount]
        {
            0x4334, 0x4335, 0x4336, 0x4e34
        };

        private Kernel _kernel;
        private ArchiveManager _archiveManager;
        private InputManager _inputManager;
        private IStateChange _stateChange;
        private KingdomShader _shader;
        private MonoSpriteDrawing drawing;
        private LayoutRenderer layoutRendererFg;
        private LayoutRenderer layoutRendererBg;
        private LayoutRenderer layoutRendererTheater;
        private SequenceRenderer _sequenceRendererMenu;
        private Kh2MessageRenderer _messageRenderer;
        private DrawContext _messageDrawContext;
        private Layout _titleLayout;
        private Layout _theaterLayout;
        private Dictionary<string, IEnumerable<ISpriteTexture>> cachedSurfaces = new Dictionary<string, IEnumerable<ISpriteTexture>>();
        private Dictionary<ushort, byte[]> _cachedText = new Dictionary<ushort, byte[]>();

        private TitleLayout _titleLayoutDesc;
        private int _optionSelected;

        private bool _isInNewGameMenu;
        private int _subMenuOptionSelected;

        private bool _isTheaterModeUnlocked;
        private bool _isInTheaterMenu;

        private AnimatedSequenceRenderer _animMenuBg;
        private AnimatedSequenceRenderer _animMenuWindow;
        private AnimatedSequenceRenderer _animMenuOption1;
        private AnimatedSequenceRenderer _animMenuOption2;
        private AnimatedSequenceRenderer _animMenuOption3;
        private AnimatedSequenceRenderer _animMenuOption4;
        private AnimatedSequenceRenderer _animMenuOptionSelected;

        private bool IsIntro
        {
            get
            {
                var currentSequence = layoutRendererBg.SelectedSequenceGroupIndex;
                return currentSequence == _titleLayoutDesc.Copyright ||
                    currentSequence == _titleLayoutDesc.Intro;
            }
        }

        private bool IsNewGameStarting =>
            layoutRendererBg.SelectedSequenceGroupIndex == _titleLayoutDesc.NewGame;

        public void Initialize(StateInitDesc initDesc)
        {
            _kernel = initDesc.Kernel;
            _archiveManager = initDesc.ArchiveManager;
            _inputManager = initDesc.InputManager;
            _stateChange = initDesc.StateChange;

            var viewport = initDesc.GraphicsDevice.GraphicsDevice.Viewport;
            _shader = new KingdomShader(initDesc.ContentManager);
            drawing = new MonoSpriteDrawing(initDesc.GraphicsDevice.GraphicsDevice, _shader);
            drawing.SetProjection(
                viewport.Width,
                viewport.Height,
                Global.ResolutionWidth,
                Global.ResolutionHeight,
                1.0f);

            if (_kernel.IsReMix)
                _archiveManager.LoadArchive($"menu/{_kernel.Region}/titlejf.2ld");
            _archiveManager.LoadArchive($"menu/{_kernel.Region}/title.2ld");
            _archiveManager.LoadArchive($"menu/{_kernel.Region}/save.2ld");

            _isTheaterModeUnlocked = false;
            if (_kernel.IsReMix)
            {
                if (_isTheaterModeUnlocked)
                    _titleLayoutDesc = ReMixTheaterTitleLayout;
                else
                    _titleLayoutDesc = ReMixTitleLayout;
            }
            else if (_kernel.RegionId == Constants.RegionFinalMix)
            {
                if (_isTheaterModeUnlocked)
                    _titleLayoutDesc = FinalMixTheaterTitleLayout;
                else
                    _titleLayoutDesc = FinalMixTitleLayout;
            }
            else
                _titleLayoutDesc = VanillaTitleLayout;

            var messageContext = _kernel.SystemMessageContext;
            _messageRenderer = new Kh2MessageRenderer(drawing, messageContext);
            _messageDrawContext = new DrawContext();

            IEnumerable<ISpriteTexture> images;
            (_titleLayout, images) = GetLayoutResources("titl", "titl");

            layoutRendererBg = new LayoutRenderer(_titleLayout, drawing, images);
            layoutRendererFg = new LayoutRenderer(_titleLayout, drawing, images);
            layoutRendererBg.SelectedSequenceGroupIndex = _titleLayoutDesc.Copyright;
            _sequenceRendererMenu = new SequenceRenderer(_titleLayout.SequenceItems[MainMenuSequence], drawing, images.First());

            Log.Info($"Theater={_titleLayoutDesc.HasTheater}");
            if (_titleLayoutDesc.HasTheater)
            {
                (_theaterLayout, images) = GetLayoutResources("even", "even");
                layoutRendererTheater = new LayoutRenderer(_theaterLayout, drawing, images);
            }

            _animMenuBg = new AnimatedSequenceRenderer(_sequenceRendererMenu, NewGameTitle);
            _animMenuWindow = new AnimatedSequenceRenderer(_sequenceRendererMenu, NewGameWindow);
            _animMenuOption1 = new AnimatedSequenceRenderer(_sequenceRendererMenu, NewGameOption);
            _animMenuOption2 = new AnimatedSequenceRenderer(_sequenceRendererMenu, NewGameOption);
            _animMenuOption3 = new AnimatedSequenceRenderer(_sequenceRendererMenu, NewGameOption);
            _animMenuOption4 = new AnimatedSequenceRenderer(_sequenceRendererMenu, NewGameOption);
            _animMenuOptionSelected = new AnimatedSequenceRenderer(_sequenceRendererMenu, 15, 14, 14);

            SetOption(0);
        }

        public void Destroy()
        {
            throw new NotImplementedException();
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

            if (IsIntro)
            {
                if (_inputManager.IsCross || _inputManager.IsCircle)
                    SkipIntro();
            }
            else if (IsNewGameStarting)
            {

            }
            else
            {
                if (_isInNewGameMenu)
                    ProcessNewGameMenu();
                else if (_isInTheaterMenu)
                    ProcessInputTheaterMenu();
                else
                    ProcessInputMainMenu();
            }

            var deltaTime = deltaTimes.DeltaTime;
            _animMenuBg.Update(deltaTime);
            _animMenuWindow.Update(deltaTime);
            _animMenuOption1.Update(deltaTime);
            _animMenuOption2.Update(deltaTime);
            _animMenuOption3.Update(deltaTime);
            _animMenuOption4.Update(deltaTime);
            _animMenuOptionSelected.Update(deltaTime);
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            layoutRendererBg.Draw();

            if (!(IsIntro || IsNewGameStarting))
                layoutRendererFg.Draw();

            if (_isInTheaterMenu)
                layoutRendererTheater.Draw();
            
            if (_isInNewGameMenu) DrawNewGameMenu();

            drawing.Flush();
        }

        private void DrawNewGameMenu()
        {
            const int OptionY = 120;
            const int OptionHDistance = 30;

            if (_animMenuBg.IsEnd)
                _isInNewGameMenu = false;

            _animMenuBg.Draw(0, 0);
            Print(0x432e, 0, 32);
            _animMenuWindow.Draw(0, 0);
            _sequenceRendererMenu.Draw(25, 0, 64, 180);
            Print(0x4330, 0, 82);

            for (var i = 0; i < DifficultyCount; i++)
            {
                _animMenuOption1.Draw(256, OptionY + OptionHDistance * i);
                Print(DifficultyTitle[i], 0, OptionY + OptionHDistance * i);
            }

            _animMenuOptionSelected.Draw(256, OptionY + OptionHDistance * _subMenuOptionSelected);
            Print(DifficultyDescription[_subMenuOptionSelected], 0, 256);
        }

        private void ProcessInputMainMenu()
        {
            var currentOption = _optionSelected;
            if (_inputManager.IsUp)
            {
                currentOption--;
                if (currentOption < 0)
                    currentOption = MainMenuBackOption;

                if (currentOption == MainMenuBackOption && !_titleLayoutDesc.HasBack)
                    currentOption = MainMenuTheaterOption;
                if (currentOption == MainMenuTheaterOption && !_titleLayoutDesc.HasTheater)
                    currentOption = MainMenuLoadOption;
            }
            else if (_inputManager.IsDown)
            {
                currentOption++;
                if (currentOption == MainMenuTheaterOption && !_titleLayoutDesc.HasTheater)
                    currentOption++;
                if (currentOption == MainMenuBackOption && !_titleLayoutDesc.HasBack)
                    currentOption++;
                if (currentOption >= MainMenuMaxOptionCount)
                    currentOption = 0;
            }
            else if (_inputManager.IsCircle)
            {
                switch (currentOption)
                {
                    case MainMenuNewGameOption:
                        _animMenuBg.Begin();
                        _animMenuWindow.Begin();
                        _animMenuOption1.Begin();
                        _animMenuOption2.Begin();
                        _animMenuOption3.Begin();
                        _animMenuOption4.Begin();
                        _animMenuOptionSelected.Begin();
                        _isInNewGameMenu = true;
                        _subMenuOptionSelected = 1;
                        break;
                    case MainMenuLoadOption:
                        SetStateToGameplay();
                        break;
                    case MainMenuTheaterOption:
                        _isInTheaterMenu = true;
                        layoutRendererTheater.FrameIndex = 0;
                        layoutRendererTheater.SelectedSequenceGroupIndex = 0;
                        break;
                }
            }


            if (currentOption != _optionSelected)
                SetOption(currentOption);
        }

        private void ProcessNewGameMenu()
        {
            bool isOptionChanged = false;
            if (_inputManager.IsCross)
            {
                _animMenuBg.End();
                _animMenuWindow.End();
                _animMenuOption1.End();
                _animMenuOption2.End();
                _animMenuOption3.End();
                _animMenuOption4.End();
                _animMenuOptionSelected.End();
            }
            else if (_inputManager.IsCircle)
            {
                _animMenuBg.End();
                _animMenuWindow.End();
                _animMenuOption1.End();
                _animMenuOption2.End();
                _animMenuOption3.End();
                _animMenuOption4.End();
                _animMenuOptionSelected.End();
                _isInNewGameMenu = false;

                layoutRendererBg.SelectedSequenceGroupIndex = _titleLayoutDesc.NewGame;
                layoutRendererBg.FrameIndex = 0;
            }
            else if (_inputManager.IsDebugUp)
            {
                _subMenuOptionSelected--;
                if (_subMenuOptionSelected < 0)
                    _subMenuOptionSelected = DifficultyCount - 1;
                isOptionChanged = true;
            }
            else if (_inputManager.IsDebugDown)
            {
                _subMenuOptionSelected = (_subMenuOptionSelected + 1) % DifficultyCount;
                isOptionChanged = true;
            }

            if (isOptionChanged)
            {
                _animMenuWindow.Skip();
                _animMenuOption1.Skip();
                _animMenuOption2.Skip();
                _animMenuOption3.Skip();
                _animMenuOption4.Skip();
                _animMenuOptionSelected.Skip();
            }
        }

        private void ProcessInputTheaterMenu()
        {
            if (_inputManager.IsCross)
            {
                layoutRendererTheater.FrameIndex = 0;
                layoutRendererTheater.SelectedSequenceGroupIndex = 2;
            }
        }

        private void SkipIntro()
        {
            layoutRendererBg.SelectedSequenceGroupIndex = _titleLayoutDesc.IntroSkip;
            layoutRendererBg.FrameIndex = 0;
        }

        private void SetStateToGameplay()
        {
            _stateChange.State = 1;
        }

        private void SetOption(int option)
        {
            Log.Info($"TitleOption={option} prev={_optionSelected}");
            _optionSelected = option;

            switch (_optionSelected)
            {
                case MainMenuNewGameOption:
                    layoutRendererFg.SelectedSequenceGroupIndex = _titleLayoutDesc.MenuOptionNewGame;
                    break;
                case MainMenuLoadOption:
                    layoutRendererFg.SelectedSequenceGroupIndex = _titleLayoutDesc.MenuOptionLoad;
                    break;
                case MainMenuTheaterOption:
                    layoutRendererFg.SelectedSequenceGroupIndex = _titleLayoutDesc.MenuOptionTheater;
                    break;
                case MainMenuBackOption:
                    layoutRendererFg.SelectedSequenceGroupIndex = _titleLayoutDesc.MenuOptionBack;
                    break;
            }

            layoutRendererFg.FrameIndex = 0;
        }

        private void CheckTitlLoop(LayoutRenderer layout)
        {
            layout.FrameIndex++;
            var currentSequenceGroupIndex = layout.SelectedSequenceGroupIndex;
            if (currentSequenceGroupIndex == _titleLayoutDesc.Copyright)
            {
                if (layout.IsLastFrame)
                {
                    layout.SelectedSequenceGroupIndex = _titleLayoutDesc.Intro;
                    layout.FrameIndex = 0;
                }
            }
            else if (currentSequenceGroupIndex == _titleLayoutDesc.Intro)
            {
                if (layout.FrameIndex >= 480)
                {
                    layout.SelectedSequenceGroupIndex = _titleLayoutDesc.IntroSkip;
                    layout.FrameIndex = 0;
                }
            }
            else if (currentSequenceGroupIndex == _titleLayoutDesc.NewGame)
            {
                if (layout.IsLastFrame)
                    SetStateToGameplay();
            }
            else
                layout.FrameIndex++;
        }

        private (Layout layout, IEnumerable<ISpriteTexture> textures) GetLayoutResources(string layoutResourceName, string imagesResourceName)
        {
            var layout = _archiveManager.Get<Layout>(layoutResourceName);
            if (!cachedSurfaces.TryGetValue(imagesResourceName, out var images))
                images = cachedSurfaces[imagesResourceName] = _archiveManager.Get<Imgz>(imagesResourceName)
                    ?.Images?.Select(x => drawing.CreateSpriteTexture(x)).ToList();

            return (layout, images);
        }

        private void Print(ushort messageId, float x, float y)
        {
            if (!_cachedText.TryGetValue(messageId, out var data))
                _cachedText[messageId] = data = _kernel.MessageProvider.GetMessage(messageId);

            _messageDrawContext.Reset();
            _messageDrawContext.x = x;
            _messageDrawContext.y = y;
            _messageDrawContext.Scale = 0.8f;
            _messageRenderer.Draw(_messageDrawContext, data);
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

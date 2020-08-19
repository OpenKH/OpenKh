using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using static OpenKh.Game.States.Title.Constants;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using OpenKh.Engine.MonoGame;

namespace OpenKh.Game.States.Title
{
    public class TitleState : IState, ITitleMainMenu
    {
        private enum TitleSubMenu
        {
            NewGame,
        }

        private const int MainMenuNewGameOption = 0;
        private const int MainMenuLoadOption = 1;
        private const int MainMenuTheaterOption = 2;
        private const int MainMenuBackOption = 3;
        private const int MainMenuMaxOptionCount = 4;

        private ArchiveManager _archiveManager;
        private IStateChange _stateChange;
        private KingdomShader _shader;
        private MonoSpriteDrawing drawing;
        private LayoutRenderer layoutRendererFg;
        private LayoutRenderer layoutRendererBg;
        private LayoutRenderer layoutRendererTheater;
        private AnimatedSequenceFactory _animatedSequenceFactory;
        private Kh2MessageRenderer _messageRenderer;
        private Layout _titleLayout;
        private Layout _theaterLayout;
        private Dictionary<string, IEnumerable<ISpriteTexture>> cachedSurfaces = new Dictionary<string, IEnumerable<ISpriteTexture>>();
        private Dictionary<ushort, byte[]> _cachedText = new Dictionary<ushort, byte[]>();

        private TitleLayout _titleLayoutDesc;
        private int _optionSelected;

        private ITitleSubMenu _subMenu;

        private bool _isTheaterModeUnlocked;
        private bool _isInTheaterMenu;

        public Kernel Kernel { get; private set; }
        public InputManager InputManager { get; private set; }
        public IMessageRenderer MessageRenderer => _messageRenderer;

        public MainMenuState State
        {
            set
            {
                switch (value)
                {
                    case MainMenuState.StartNewGame:
                        StartNewGame();
                        break;
                }
            }
        }

        private TitleSubMenu SubMenu
        {
            set
            {
                switch (value)
                {
                    case TitleSubMenu.NewGame:
                        _subMenu = new NewGameMenu(_animatedSequenceFactory, this);
                        break;
                    default:
                        Log.Warn($"Submenu {value.ToString()} not implemented.");
                        break;
                }

                _subMenu?.Invoke();
            }
        }

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
            Kernel = initDesc.Kernel;
            _archiveManager = initDesc.ArchiveManager;
            InputManager = initDesc.InputManager;
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

            if (Kernel.IsReMix)
                _archiveManager.LoadArchive($"menu/{Kernel.Region}/titlejf.2ld");
            _archiveManager.LoadArchive($"menu/{Kernel.Region}/title.2ld");
            _archiveManager.LoadArchive($"menu/{Kernel.Region}/save.2ld");

            _isTheaterModeUnlocked = false;
            if (Kernel.IsReMix)
            {
                if (_isTheaterModeUnlocked)
                    _titleLayoutDesc = ReMixTheaterTitleLayout;
                else
                    _titleLayoutDesc = ReMixTitleLayout;
            }
            else if (Kernel.RegionId == Kh2.Constants.RegionFinalMix)
            {
                if (_isTheaterModeUnlocked)
                    _titleLayoutDesc = FinalMixTheaterTitleLayout;
                else
                    _titleLayoutDesc = FinalMixTitleLayout;
            }
            else
                _titleLayoutDesc = VanillaTitleLayout;

            var messageContext = Kernel.SystemMessageContext;
            _messageRenderer = new Kh2MessageRenderer(drawing, messageContext);

            IEnumerable<ISpriteTexture> images;
            (_titleLayout, images) = GetLayoutResources("titl", "titl");

            layoutRendererBg = new LayoutRenderer(_titleLayout, drawing, images);
            layoutRendererFg = new LayoutRenderer(_titleLayout, drawing, images);
            layoutRendererBg.SelectedSequenceGroupIndex = _titleLayoutDesc.Copyright;

            _animatedSequenceFactory = new AnimatedSequenceFactory(
                drawing,
                initDesc.Kernel.MessageProvider,
                _messageRenderer,
                initDesc.Kernel.SystemMessageContext.Encoder,
                _titleLayout.SequenceItems[0],
                images.First());

            Log.Info($"Theater={_titleLayoutDesc.HasTheater}");
            if (_titleLayoutDesc.HasTheater)
            {
                (_theaterLayout, images) = GetLayoutResources("even", "even");
                layoutRendererTheater = new LayoutRenderer(_theaterLayout, drawing, images);
            }

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

            if (_subMenu?.IsOpen == true)
                _subMenu.Update(deltaTimes.DeltaTime);
            else
            {
                if (IsIntro)
                {
                    if (InputManager.IsCross || InputManager.IsCircle)
                        SkipIntro();
                }
                else if (IsNewGameStarting)
                {
                    // block any input
                }
                else if (_isInTheaterMenu)
                    ProcessInputTheaterMenu();
                else
                    ProcessInputMainMenu();
            }
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            layoutRendererBg.Draw();

            if (!(IsIntro || IsNewGameStarting))
                layoutRendererFg.Draw();

            if (_isInTheaterMenu)
                layoutRendererTheater.Draw();

            else if (_subMenu?.IsOpen == true)
                _subMenu.Draw();

            drawing.Flush();
        }

        private void ProcessInputMainMenu()
        {
            var currentOption = _optionSelected;
            if (InputManager.IsUp)
            {
                currentOption--;
                if (currentOption < 0)
                    currentOption = MainMenuBackOption;

                if (currentOption == MainMenuBackOption && !_titleLayoutDesc.HasBack)
                    currentOption = MainMenuTheaterOption;
                if (currentOption == MainMenuTheaterOption && !_titleLayoutDesc.HasTheater)
                    currentOption = MainMenuLoadOption;
            }
            else if (InputManager.IsDown)
            {
                currentOption++;
                if (currentOption == MainMenuTheaterOption && !_titleLayoutDesc.HasTheater)
                    currentOption++;
                if (currentOption == MainMenuBackOption && !_titleLayoutDesc.HasBack)
                    currentOption++;
                if (currentOption >= MainMenuMaxOptionCount)
                    currentOption = 0;
            }
            else if (InputManager.IsCircle)
            {
                switch (currentOption)
                {
                    case MainMenuNewGameOption:
                        SubMenu = TitleSubMenu.NewGame;
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

        private void ProcessInputTheaterMenu()
        {
            if (InputManager.IsCross)
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

        private void StartNewGame()
        {
            layoutRendererBg.SelectedSequenceGroupIndex = _titleLayoutDesc.NewGame;
            layoutRendererBg.FrameIndex = 0;
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

        public byte[] GetMessage(ushort messageId)
        {
            if (!_cachedText.TryGetValue(messageId, out var data))
                _cachedText[messageId] = data = Kernel.MessageProvider.GetMessage(messageId);

            return data;
        }

        public void Print(ushort messageId, float left, float top,
            uint color, TextAlignment alignment)
        {
            var data = GetMessage(messageId);

            var x = alignment switch
            {
                TextAlignment.Left => 0,
                TextAlignment.Center => -(GetTextWidth(data) / 2),
                TextAlignment.Right => -GetTextWidth(data),
                _ => 0,
            };
            _messageRenderer.Draw(new DrawContext
            {
                xStart = left + x,
                x = left + x,
                y = top,
                Scale = 0.8f,
                Color = new ColorF(
                    ((color >> 16) & 0xff) / 255.0f,
                    ((color >> 8) & 0xff) / 255.0f,
                    ((color >> 0) & 0xff) / 255.0f,
                    ((color >> 24) & 0xff) / 255.0f)
            }, data);
        }

        private float GetTextWidth(byte[] text)
        {
            var ctx = new DrawContext
            {
                IgnoreDraw = true,
                Scale = 0.8f
            };
            _messageRenderer.Draw(ctx, text);

            return (float)ctx.Width;
        }

        public void DebugUpdate(IDebug debug)
        {
        }

        public void DebugDraw(IDebug debug)
        {
        }
    }
}

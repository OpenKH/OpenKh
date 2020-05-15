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
        private const int BackgroundScreen = 14;
        private const int Menu1 = 0;
        private const int Menu2 = 2;
        private const int Menu1OptionsCount = 2;
        private const int Menu2OptionsCount = 3;
        private const int MainMenuNewGameOption = 0;
        private const int MainMenuLoadOption = 1;
        private const int MainMenuTheaterOption = 2;

        private Kernel _kernel;
        private ArchiveManager _archiveManager;
        private InputManager _inputManager;
        private IStateChange _stateChange;
        private MonoDrawing drawing;
        private LayoutRenderer layoutRendererFg;
        private LayoutRenderer layoutRendererBg;
        private LayoutRenderer layoutRendererTheater;
        private Dictionary<string, IEnumerable<ISurface>> cachedSurfaces;
        
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

            layoutRendererBg = CreateLayoutRenderer("titl");
            layoutRendererBg.SelectedSequenceGroupIndex = BackgroundScreen;

            layoutRendererFg = CreateLayoutRenderer("titl");

            // it will not work for non-FM versions
            if (_kernel.RegionId == Constants.RegionFinalMix)
            {
                _isTheaterModeUnlocked = true;
                layoutRendererTheater = CreateLayoutRenderer("even");
            }
            else
            {
                _isTheaterModeUnlocked = false;
            }

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
            {
                currentOption--;
                if (currentOption < 0)
                    currentOption = GetMaxOptionsCount() - 1;
            }
            else if (_inputManager.IsDown)
            {
                currentOption++;
                if (currentOption >= GetMaxOptionsCount())
                    currentOption = 0;
            }
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

        private int GetMaxOptionsCount() => _isTheaterModeUnlocked ? Menu2OptionsCount : Menu1OptionsCount;

        private void SetOption(int option)
        {
            _optionSelected = option;
            layoutRendererFg.SelectedSequenceGroupIndex = _optionSelected + (_isTheaterModeUnlocked ? Menu2 : Menu1);
        }

        private void CheckTitlLoop(LayoutRenderer layout)
        {
            switch (layout.SelectedSequenceGroupIndex)
            {
                case Menu1:
                case Menu1 + 1:
                case Menu2:
                case Menu2 + 1:
                case Menu2 + 2:
                    if (layout.FrameIndex > 178)
                        layout.FrameIndex = 70;
                    layout.FrameIndex++;
                    break;
                case BackgroundScreen:
                    if (layout.FrameIndex < 119)
                        layout.FrameIndex++;
                    break;
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

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenKh.Engine;
using OpenKh.Engine.Renderers;
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
        private const bool TheaterModeUnlocked = true;
        private const int MainMenuNewGameOption = 0;
        private const int MainMenuLoadOption = 1;
        private const int MainMenuTheaterOption = 2;
        private ArchiveManager archiveManager;
        private InputManager inputManager;
        private MonoDrawing drawing;
        private LayoutRenderer layoutRendererFg;
        private LayoutRenderer layoutRendererBg;
        private LayoutRenderer layoutRendererTheater;
        private Dictionary<string, IEnumerable<ISurface>> cachedSurfaces;
        private int optionSelected;
        private bool isInTheaterMenu;

        public void Initialize(StateInitDesc initDesc)
        {
            archiveManager = initDesc.ArchiveManager;
            inputManager = initDesc.InputManager;
            drawing = new MonoDrawing(initDesc.GraphicsDevice.GraphicsDevice);
            cachedSurfaces = new Dictionary<string, IEnumerable<ISurface>>();

            archiveManager.LoadArchive("menu/fm/title.2ld");
            archiveManager.LoadArchive("menu/fm/save.2ld");

            layoutRendererBg = CreateLayoutRenderer("titl");
            layoutRendererBg.SelectedSequenceGroupIndex = BackgroundScreen;

            layoutRendererFg = CreateLayoutRenderer("titl");
            SetOption(0);

            layoutRendererTheater = CreateLayoutRenderer("even");
        }

        public void Destroy()
        {
            throw new System.NotImplementedException();
        }

        public void Update(DeltaTimes deltaTimes)
        {
            CheckTitlLoop(layoutRendererBg);
            CheckTitlLoop(layoutRendererFg);
            
            if (isInTheaterMenu)
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
                            isInTheaterMenu = false;
                        break;
                }
            }

            if (isInTheaterMenu == false)
                ProcessInputMainMenu();
            else
                ProcessInputTheaterMenu();
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            layoutRendererBg.Draw();
            layoutRendererFg.Draw();

            if (isInTheaterMenu)
                layoutRendererTheater.Draw();
        }

        private void ProcessInputMainMenu()
        {
            var currentOption = optionSelected;
            if (inputManager.IsUp)
            {
                currentOption--;
                if (currentOption < 0)
                    currentOption = GetMaxOptionsCount() - 1;
            }
            else if (inputManager.IsDown)
            {
                currentOption++;
                if (currentOption >= GetMaxOptionsCount())
                    currentOption = 0;
            }
            else if (inputManager.IsCircle)
            {
                switch (currentOption)
                {
                    case MainMenuNewGameOption:
                        break;
                    case MainMenuLoadOption:
                        break;
                    case MainMenuTheaterOption:
                        isInTheaterMenu = true;
                        layoutRendererTheater.FrameIndex = 0;
                        layoutRendererTheater.SelectedSequenceGroupIndex = 0;
                        break;
                }
            }

            if (currentOption != optionSelected)
                SetOption(currentOption);
        }

        private void ProcessInputTheaterMenu()
        {
            if (inputManager.IsCross)
            {
                layoutRendererTheater.FrameIndex = 0;
                layoutRendererTheater.SelectedSequenceGroupIndex = 2;
            }
        }

        private int GetMaxOptionsCount() => TheaterModeUnlocked ? Menu2OptionsCount : Menu1OptionsCount;

        private void SetOption(int option)
        {
            optionSelected = option;
            layoutRendererFg.SelectedSequenceGroupIndex = optionSelected + (TheaterModeUnlocked ? Menu2 : Menu1);
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
            var layout = archiveManager.Get<Layout>(layoutResourceName);
            if (!cachedSurfaces.TryGetValue(imagesResourceName, out var images))
                images = cachedSurfaces[imagesResourceName] = archiveManager.Get<Imgz>(imagesResourceName)
                    ?.Images?.Select(x => drawing.CreateSurface(x));

            return new LayoutRenderer(layout, drawing, images);
        }
    }
}

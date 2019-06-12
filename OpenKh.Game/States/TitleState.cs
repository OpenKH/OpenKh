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

        private ArchiveManager archiveManager;
        private InputManager inputManager;
        private MonoDrawing drawing;
        private Layout titleLayout;
        private IEnumerable<ISurface> titleImages;
        private LayoutRenderer layoutRendererFg;
        private LayoutRenderer layoutRendererBg;
        private int optionSelected;

        public void Initialize(StateInitDesc initDesc)
        {
            archiveManager = initDesc.ArchiveManager;
            inputManager = initDesc.InputManager;
            drawing = new MonoDrawing(initDesc.GraphicsDevice.GraphicsDevice);

            archiveManager.LoadArchive("menu/fm/title.2ld");
            titleLayout = archiveManager.Get<Layout>("titl");
            titleImages = archiveManager.Get<Imgz>("titl")?.Images?.Select(x => drawing.CreateSurface(x));

            layoutRendererBg = new LayoutRenderer(titleLayout, drawing, titleImages);
            layoutRendererBg.SelectedSequenceGroupIndex = BackgroundScreen;

            layoutRendererFg = new LayoutRenderer(titleLayout, drawing, titleImages);
            layoutRendererFg.SelectedSequenceGroupIndex = TheaterModeUnlocked ? Menu2 : Menu1;
        }

        public void Destroy()
        {
            throw new System.NotImplementedException();
        }

        public void Update(DeltaTimes deltaTimes)
        {
            CheckLoop(layoutRendererBg);
            CheckLoop(layoutRendererFg);
            
            var pad = GamePad.GetState(PlayerIndex.One);
            var keyboard = Keyboard.GetState();

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

            if (currentOption != optionSelected)
                SetOption(currentOption);
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            layoutRendererBg.Draw();
            layoutRendererFg.Draw();
        }

        private int GetMaxOptionsCount() => TheaterModeUnlocked ? Menu2OptionsCount : Menu1OptionsCount;

        private void SetOption(int option)
        {
            optionSelected = option;
            layoutRendererFg.SelectedSequenceGroupIndex = optionSelected + (TheaterModeUnlocked ? Menu2 : Menu1);
        }

        private void CheckLoop(LayoutRenderer layout)
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
    }
}

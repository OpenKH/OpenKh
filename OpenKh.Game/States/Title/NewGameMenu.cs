using OpenKh.Engine.Renderers;
using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.States.Title
{
    internal class NewGameMenu : ITitleSubMenu
    {
        private const int NewGameTitle = 22;
        private const int NewGameWindow = 28;
        private const int NewGameOption = 15;

        private const int DifficultyCount = 4;
        private static readonly ushort[] DifficultyTitle = new ushort[DifficultyCount]
        {
            0x4331, 0x4332, 0x4333, 0x4e33
        };
        private static readonly ushort[] DifficultyDescription = new ushort[DifficultyCount]
        {
            0x4334, 0x4335, 0x4336, 0x4e34
        };

        private readonly ITitleMainMenu _mainMenu;
        private readonly SequenceRenderer _seqRenderer;
        private readonly AnimatedSequenceRenderer _animMenuBg;
        private readonly AnimatedSequenceRenderer _animMenuWindow;
        private readonly AnimatedSequenceRenderer _animMenuOption1;
        private readonly AnimatedSequenceRenderer _animMenuOption2;
        private readonly AnimatedSequenceRenderer _animMenuOption3;
        private readonly AnimatedSequenceRenderer _animMenuOption4;
        private readonly AnimatedSequenceRenderer _animMenuOptionSelected;

        private int _difficultyOption;
        private MainMenuState _stateToSet;

        public bool IsOpen { get; private set; }

        public NewGameMenu(SequenceRenderer sequenceRendererMenu, ITitleMainMenu mainMenu)
        {
            _mainMenu = mainMenu;
            _seqRenderer = sequenceRendererMenu;
            _animMenuBg = new AnimatedSequenceRenderer(_seqRenderer, NewGameTitle);
            _animMenuWindow = new AnimatedSequenceRenderer(_seqRenderer, NewGameWindow);
            _animMenuOption1 = new AnimatedSequenceRenderer(_seqRenderer, NewGameOption);
            _animMenuOption2 = new AnimatedSequenceRenderer(_seqRenderer, NewGameOption);
            _animMenuOption3 = new AnimatedSequenceRenderer(_seqRenderer, NewGameOption);
            _animMenuOption4 = new AnimatedSequenceRenderer(_seqRenderer, NewGameOption);
            _animMenuOptionSelected = new AnimatedSequenceRenderer(_seqRenderer, 15, 14, 14);
        }

        public void Invoke()
        {
            IsOpen = true;
            _difficultyOption = 1;

            _animMenuBg.Begin();
            _animMenuWindow.Begin();
            _animMenuOption1.Begin();
            _animMenuOption2.Begin();
            _animMenuOption3.Begin();
            _animMenuOption4.Begin();
            _animMenuOptionSelected.Begin();
        }

        public void Update(double deltaTime)
        {
            _animMenuBg.Update(deltaTime);
            _animMenuWindow.Update(deltaTime);
            _animMenuOption1.Update(deltaTime);
            _animMenuOption2.Update(deltaTime);
            _animMenuOption3.Update(deltaTime);
            _animMenuOption4.Update(deltaTime);
            _animMenuOptionSelected.Update(deltaTime);

            var inputManager = _mainMenu.InputManager;
            InputNewGameMenu(inputManager);

            if (_animMenuBg.IsEnd)
            {
                _mainMenu.State = _stateToSet;
                IsOpen = false;
            }
        }

        public void Draw()
        {
            DrawNewGameMenu();
        }

        private void InputNewGameMenu(InputManager inputManager)
        {
            bool isOptionChanged = false;
            if (inputManager.IsCross)
            {
                _animMenuBg.End();
                _animMenuWindow.End();
                _animMenuOption1.End();
                _animMenuOption2.End();
                _animMenuOption3.End();
                _animMenuOption4.End();
                _animMenuOptionSelected.End();
                _stateToSet = MainMenuState.Running;
            }
            else if (inputManager.IsCircle)
            {
                _animMenuBg.End();
                _animMenuWindow.End();
                _animMenuOption1.End();
                _animMenuOption2.End();
                _animMenuOption3.End();
                _animMenuOption4.End();
                _animMenuOptionSelected.End();
                _stateToSet = MainMenuState.StartNewGame;
            }
            else if (inputManager.IsMenuUp)
            {
                _difficultyOption--;
                if (_difficultyOption < 0)
                    _difficultyOption = DifficultyCount - 1;
                isOptionChanged = true;
            }
            else if (inputManager.IsMenuDown)
            {
                _difficultyOption = (_difficultyOption + 1) % DifficultyCount;
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

        private void DrawNewGameMenu()
        {
            const int OptionY = 120;
            const int OptionHDistance = 30;

            _animMenuBg.Draw(0, 0);
            _mainMenu.Print(0x432e, 0, 32, 512, TextAlignment.Center);
            _animMenuWindow.Draw(0, 0);
            _seqRenderer.Draw(25, 0, 64, 180);
            _mainMenu.Print(0x4330, 0, 82, 512, TextAlignment.Center);

            for (var i = 0; i < DifficultyCount; i++)
            {
                _animMenuOption1.Draw(256, OptionY + OptionHDistance * i);
                if (i == _difficultyOption)
                    _animMenuOptionSelected.Draw(256, OptionY + OptionHDistance * i);

                _mainMenu.Print(DifficultyTitle[i], 0, OptionY + OptionHDistance * i, 512, TextAlignment.Center);
            }

            _mainMenu.Print(DifficultyDescription[_difficultyOption], 0, 256, 512, TextAlignment.Center);
        }
    }
}

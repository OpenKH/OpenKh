using OpenKh.Engine.Renderers;
using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.States.Title
{
    internal class NewGameMenu : ITitleSubMenu
    {
        private const int NewGameTitle = 22;
        private const int NewGameWindow = 28;
        private const int NewGameOption = 15;

        private static readonly ushort[] DifficultyTitle = new ushort[]
        {
            0x4331, 0x4332, 0x4333, 0x4e33
        };
        private static readonly ushort[] DifficultyDescription = new ushort[]
        {
            0x4334, 0x4335, 0x4336, 0x4e34
        };

        private readonly ITitleMainMenu _mainMenu;
        private readonly AnimatedSequenceFactory _animSeqFactory;
        private readonly IAnimatedSequence _animMenuBg;
        private readonly IAnimatedSequence _animMenuWindow;
        private readonly IAnimatedSequence _animMenuTitle;
        private readonly IAnimatedSequence _animMenuOption1;
        private readonly IAnimatedSequence _animMenuOption2;
        private readonly IAnimatedSequence _animMenuOption3;
        private readonly IAnimatedSequence _animMenuOption4;
        private readonly IAnimatedSequence _animMenuOptionSelected;

        private int _difficultyCount;
        private int _difficultyOption;
        private MainMenuState _stateToSet;

        public bool IsOpen { get; private set; }

        public NewGameMenu(AnimatedSequenceFactory sequenceFactory, ITitleMainMenu mainMenu)
        {
            _difficultyCount = mainMenu.Kernel.IsFinalMix ? 4 : 3;

            _mainMenu = mainMenu;
            _animSeqFactory = sequenceFactory;

            _animMenuBg = sequenceFactory.FromAnimatedIndex(NewGameTitle);
            _animMenuWindow = sequenceFactory.FromAnimatedIndex(NewGameWindow);
            _animMenuTitle = sequenceFactory.FromStaticIndex(25);
            _animMenuOption1 = sequenceFactory.FromAnimatedIndex(NewGameOption);
            _animMenuOption2 = sequenceFactory.FromAnimatedIndex(NewGameOption);
            _animMenuOption3 = sequenceFactory.FromAnimatedIndex(NewGameOption);
            _animMenuOption4 = sequenceFactory.FromAnimatedIndex(NewGameOption);

            _animMenuOptionSelected = sequenceFactory.FromAnimatedIndex(15, 14, 14);
            //_animMenuOptionSelected.AddChild(new AnimatedSequenceRenderer(_seqRenderer, 7));
            //_animMenuOptionSelected.AddChild(new AnimatedSequenceRenderer(_seqRenderer, 10), true);
            //_animMenuOptionSelected.ChildPositionX = -110;
        }

        public void Invoke()
        {
            IsOpen = true;
            _difficultyOption = 1;

            _animMenuBg.Begin();
            _animMenuBg.TextAnchor = TextAnchor.Center;
            _animMenuBg.SetMessage(0x432e);

            _animMenuWindow.Begin();

            _animMenuTitle.Begin();
            _animMenuTitle.TextAnchor = TextAnchor.BottomCenter;
            _animMenuTitle.SetMessage(0x4330);

            _animMenuOption1.Begin();
            _animMenuOption1.TextAnchor = TextAnchor.BottomCenter;
            _animMenuOption1.SetMessage(DifficultyTitle[0]);

            _animMenuOption2.Begin();
            _animMenuOption2.TextAnchor = TextAnchor.BottomCenter;
            _animMenuOption2.SetMessage(DifficultyTitle[1]);

            _animMenuOption3.Begin();
            _animMenuOption3.TextAnchor = TextAnchor.BottomCenter;
            _animMenuOption3.SetMessage(DifficultyTitle[2]);

            _animMenuOption4.Begin();
            _animMenuOption4.TextAnchor = TextAnchor.BottomCenter;
            _animMenuOption4.SetMessage(DifficultyTitle[3]);

            _animMenuOptionSelected.Begin();
        }

        public void Update(double deltaTime)
        {
            _animMenuBg.Update(deltaTime);
            _animMenuWindow.Update(deltaTime);
            _animMenuTitle.Update(deltaTime);
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
                _animMenuTitle.End();
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
                _animMenuTitle.End();
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
                    _difficultyOption = _difficultyCount - 1;
                isOptionChanged = true;
            }
            else if (inputManager.IsMenuDown)
            {
                _difficultyOption = (_difficultyOption + 1) % _difficultyCount;
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
            const int OptionHDistance = 30;
            var subTitleBgY = _mainMenu.Kernel.IsFinalMix ? 180 : 188;
            var optionY = _mainMenu.Kernel.IsFinalMix ? 120 : 144;

            _animMenuBg.Draw(0, 0);

            _animMenuWindow.TextAnchor = TextAnchor.BottomCenter;
            _animMenuWindow.SetMessage(DifficultyDescription[_difficultyOption]);
            _animMenuWindow.Draw(0, 0);

            _animMenuTitle.Draw(256, subTitleBgY);

            _animMenuOption1.Draw(256, optionY + OptionHDistance * 0);
            _animMenuOption2.Draw(256, optionY + OptionHDistance * 1);
            _animMenuOption3.Draw(256, optionY + OptionHDistance * 2);
            if (_difficultyCount >= 4)
                _animMenuOption4.Draw(256, optionY + OptionHDistance * 3);

            _animMenuOptionSelected.TextAnchor = TextAnchor.BottomCenter;
            _animMenuOptionSelected.SetMessage(DifficultyTitle[_difficultyOption]);
            _animMenuOptionSelected.Draw(256, optionY + OptionHDistance * _difficultyOption);
        }
    }
}

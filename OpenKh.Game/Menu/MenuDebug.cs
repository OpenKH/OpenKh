using OpenKh.Engine.Renderers;
using OpenKh.Game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.Menu
{
    public class DebugMenuEntry
    {
        public string Name { get; }
        public Action Action { get; }

        public DebugMenuEntry(string name, Action action)
        {
            Name = name;
            Action = action;
        }
    }

    public class MenuDebug : MenuBase
    {
        public static string Name = "DEBUG";
        private readonly DebugMenuEntry[] Entries;
        private IAnimatedSequence _menuSeq;
        private int _selectedOption;

        public int SelectedOption
        {
            get => _selectedOption;
            set
            {
                value = value < 0 ? Entries.Length - 1 : value % Entries.Length;
                if (_selectedOption != value)
                {
                    _selectedOption = value;
                    _menuSeq = InitializeMenu(true);
                    _menuSeq.Begin();
                }
            }
        }

        public override ushort MenuNameId => 0;
        protected override bool IsEnd => _menuSeq.IsEnd;

        public MenuDebug(IMenuManager menuManager) : base(menuManager)
        {
            Entries = new DebugMenuEntry[]
            {
                new DebugMenuEntry("TITLE SCREEN", ReturnToTitleScreen),
                new DebugMenuEntry(MenuDebugChangePlace.Name, OpenChangePlaceMenu),
            };
            _menuSeq = InitializeMenu(false);
        }

        private void ReturnToTitleScreen()
        {
            MenuManager.GameContext.LoadTitleScreen();
            MenuManager.CloseAllMenu();
        }

        private void OpenChangePlaceMenu() =>
            Push(new MenuDebugChangePlace(MenuManager));

        private void OpenResolutionMenu()
        {
        }

        private IAnimatedSequence InitializeMenu(bool skipIntro)
        {
            return SequenceFactory.Create(Enumerable.Range(0, Entries.Length)
                .Select(i => new AnimatedSequenceDesc
                {
                    SequenceIndexStart = skipIntro ? -1 : 133,
                    SequenceIndexLoop = SelectedOption == i ? 132 : 134,
                    SequenceIndexEnd = 135,
                    StackIndex = i,
                    StackWidth = 0,
                    StackHeight = AnimatedSequenceDesc.DefaultStacking,
                    Flags = AnimationFlags.TextTranslateX |
                        AnimationFlags.ChildStackHorizontally,
                    MessageText = Entries[i].Name,
                    Children = SelectedOption == i ? new List<AnimatedSequenceDesc>
                    {
                        new AnimatedSequenceDesc
                        {
                            SequenceIndexLoop = 25,
                            TextAnchor = TextAnchor.BottomLeft,
                            Flags = AnimationFlags.NoChildTranslationX,
                        },
                        new AnimatedSequenceDesc
                        {
                            SequenceIndexStart = skipIntro ? -1 : 27,
                            SequenceIndexLoop = 28,
                            SequenceIndexEnd = 29,
                        }
                    } : new List<AnimatedSequenceDesc>()
                }));
        }

        protected override void ProcessInput(InputManager inputManager)
        {
            if (inputManager.IsMenuUp)
                SelectedOption--;
            else if (inputManager.IsMenuDown)
                SelectedOption++;
            else if (inputManager.IsCircle)
                Entries[SelectedOption].Action();
            else
                base.ProcessInput(inputManager);
        }

        public override void Open()
        {
            _menuSeq.Begin();
            base.Open();
        }

        public override void Close()
        {
            _menuSeq.End();
            base.Close();
        }

        protected override void MyUpdate(double deltaTime) =>
            _menuSeq.Update(deltaTime);

        protected override void MyDraw() =>
            _menuSeq.Draw(0, 0);

        public override string ToString() => Name;
    }
}

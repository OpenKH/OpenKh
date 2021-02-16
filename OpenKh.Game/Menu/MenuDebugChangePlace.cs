using OpenKh.Engine.Renderers;
using OpenKh.Game.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.Menu
{
    public class MenuDebugChangePlace : MenuBase
    {
        const int MaxOptionPerColumn = 10;
        private const int WorldCount = Kh2.Constants.WorldCount;
        private static readonly ushort[] WorldMsgIds = Enumerable
            .Range(0, WorldCount)
            .Select(i => (ushort)(0x8211 + i))
            .ToArray();

        public static string Name = "CHANGE PLACE";
        private IAnimatedSequence _menuSeq;
        private int _selectedOption;

        public int SelectedOption
        {
            get => _selectedOption;
            set
            {
                value = value < 0 ? WorldCount - 1 : value % WorldCount;
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

        public MenuDebugChangePlace(IMenuManager menuManager) : base(menuManager)
        {
            _menuSeq = InitializeMenu(false);
        }

        private IAnimatedSequence InitializeMenu(bool skipIntro)
        {
            return SequenceFactory.Create(Enumerable.Range(0, WorldMsgIds.Length)
                .Select(i => new AnimatedSequenceDesc
                {
                    X = i / MaxOptionPerColumn * 140,
                    SequenceIndexStart = skipIntro ? -1 : 133,
                    SequenceIndexLoop = SelectedOption == i ? 132 : 134,
                    SequenceIndexEnd = 135,
                    StackIndex = i % MaxOptionPerColumn,
                    StackWidth = 0,
                    StackHeight = AnimatedSequenceDesc.DefaultStacking,
                    Flags = AnimationFlags.TextTranslateX |
                        AnimationFlags.ChildStackHorizontally,
                    MessageId = WorldMsgIds[i],
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
            if (inputManager.IsMenuLeft)
                SelectedOption -= MaxOptionPerColumn;
            if (inputManager.IsMenuRight)
                SelectedOption += MaxOptionPerColumn;
            else if (inputManager.IsCircle)
                Push(new MenuDebugSelectPlace(
                    MenuManager, SelectedOption, WorldMsgIds[SelectedOption]));
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

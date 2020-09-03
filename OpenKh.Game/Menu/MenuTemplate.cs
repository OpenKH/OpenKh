using OpenKh.Engine.Renderers;
using OpenKh.Game.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.Menu
{
    public class MenuTemplate : MenuBase
    {
        private const int OptionCount = 3;
        private readonly IMenuManager _menuManager;
        private readonly int _stackIndex;
        private IAnimatedSequence _menuSeq;
        private int _selectedOption;

        public int SelectedOption
        {
            get => _selectedOption;
            set
            {
                value = value < 0 ? OptionCount - 1 : value % OptionCount;
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

        public MenuTemplate(IMenuManager menuManager, int stackIndex) : base(menuManager)
        {
            _menuManager = menuManager;
            _stackIndex = stackIndex;
            _menuSeq = InitializeMenu(false);
        }

        private IAnimatedSequence InitializeMenu(bool skipIntro)
        {
            return SequenceFactory.Create(Enumerable.Range(0, OptionCount)
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
                    MessageText = $"TEMPLATE {_stackIndex}-{i}",
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
            {
                Push(new MenuTemplate(_menuManager, _stackIndex + 1));
            }
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

        public override string ToString() => "TEMPLATE";
    }
}

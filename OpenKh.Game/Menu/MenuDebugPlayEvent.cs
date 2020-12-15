using OpenKh.Engine.Renderers;
using OpenKh.Game.Infrastructure;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.Menu
{
    public class MenuDebugPlayEvent : MenuBase
    {
        const int MaxOptionPerColumn = 10;
        public static string Name = "PLAY EVENT";
        private IAnimatedSequence _menuSeq;
        private int _selectedOption;

        private IList<string> Events => MenuManager.GameContext.Field.Events;
        private int OptionCount => Events.Count;

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

        public MenuDebugPlayEvent(IMenuManager menuManager) :
            base(menuManager)
        {
            _menuSeq = InitializeMenu(false);
        }

        private IAnimatedSequence InitializeMenu(bool skipIntro)
        {
            return SequenceFactory.Create(Events
                .Select((name, i) => new AnimatedSequenceDesc
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
                    MessageText = name.ToUpper(),
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
                MenuManager.GameContext.Field.PlayEvent(Events[SelectedOption]);
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

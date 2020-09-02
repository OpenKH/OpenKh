using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.Menu
{
    public class MenuTemplate : MenuBase
    {
        private readonly int _stackIndex;
        private IAnimatedSequence _menuSeq;

        public int SelectedIndex { get; set; }
        protected override bool IsEnd => _menuSeq.IsEnd;

        public MenuTemplate(
            AnimatedSequenceFactory animatedSequenceFactory,
            InputManager inputManager,
            int stackIndex) : base(animatedSequenceFactory, inputManager)
        {
            _stackIndex = stackIndex;
            _menuSeq = InitializeMenu(false);
        }

        private IAnimatedSequence InitializeMenu(bool skipIntro)
        {
            return SequenceFactory.Create(new AnimatedSequenceDesc
            {
                SequenceIndexStart = skipIntro ? -1 : 133,
                SequenceIndexLoop = 134,
                SequenceIndexEnd = 135,
                Flags = AnimationFlags.TextTranslateX |
                    AnimationFlags.ChildStackHorizontally,
                MessageText = $"Template {_stackIndex}"
            });
        }

        protected override void ProcessInput(InputManager inputManager)
        {
            if (inputManager.IsMenuUp)
                SelectedIndex--;
            else if (inputManager.IsMenuDown)
                SelectedIndex++;
            else if (inputManager.IsCircle)
            {
                Push(new MenuTemplate(
                    SequenceFactory, InputManager, _stackIndex + 1));
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
    }
}

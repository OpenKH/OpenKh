using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.Menu
{
    public class MenuTemplate : IMenu
    {
        private readonly AnimatedSequenceFactory _animSeqFactory;
        private readonly InputManager _inputManager;
        private readonly int _stackIndex;
        private bool _isClosing;
        private IMenu _subMenu;
        private IAnimatedSequence _menuSeq;

        public bool IsClosed { get; private set; }
        public int SelectedOption { get; set; }

        public MenuTemplate(
            AnimatedSequenceFactory animatedSequenceFactory,
            InputManager inputManager,
            int stackIndex)
        {
            _animSeqFactory = animatedSequenceFactory;
            _inputManager = inputManager;
            _stackIndex = stackIndex;
            _menuSeq = InitializeMenu();
        }

        private IAnimatedSequence InitializeMenu()
        {
            return _animSeqFactory.Create(new AnimatedSequenceDesc
            {
                SequenceIndexStart = 133,
                SequenceIndexLoop = 134,
                SequenceIndexEnd = 135,
                MessageText = $"Template {_stackIndex}"
            });
        }

        private void ProcessInput(InputManager inputManager)
        {
            if (_isClosing)
                return;

            if (inputManager.IsMenuUp)
                SelectedOption--;
            else if (inputManager.IsMenuDown)
                SelectedOption++;
            else if (inputManager.IsCircle)
            {
                Push(new MenuTemplate(
                    _animSeqFactory, _inputManager, _stackIndex + 1));
            }
            else if (inputManager.IsCross)
                Close();
        }

        public void Open()
        {
            _menuSeq.Begin();
        }

        public void Close()
        {
            _isClosing = true;
            _menuSeq.End();
        }

        public void Push(IMenu subMenu)
        {
            _subMenu = subMenu;
            _subMenu.Open();
        }

        public void Update(double deltaTime)
        {
            IsClosed = _menuSeq.IsEnd;

            if (_subMenu == null)
            {
                _menuSeq.Update(deltaTime);
                ProcessInput(_inputManager);
            }
            else
            {
                _subMenu.Update(deltaTime);
                if (_subMenu.IsClosed)
                {
                    _subMenu = null;
                    Open();
                }
            }
        }

        public void Draw()
        {
            if (_subMenu == null)
                _menuSeq.Draw(0, 0);
            else
                _subMenu.Draw();
        }
    }
}

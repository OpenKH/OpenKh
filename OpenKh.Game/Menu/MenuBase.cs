using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.Menu
{
    public abstract class MenuBase : IMenu
    {
        private bool _isClosing;
        private IMenu _subMenu;

        protected AnimatedSequenceFactory SequenceFactory { get; }
        protected InputManager InputManager { get; }
        abstract protected bool IsEnd { get; }
        public bool IsClosed { get; private set; }

        public MenuBase(
            AnimatedSequenceFactory animatedSequenceFactory,
            InputManager inputManager)
        {
            SequenceFactory = animatedSequenceFactory;
            InputManager = inputManager;
        }

        virtual protected void ProcessInput(InputManager inputManager)
        {
            if (inputManager.IsCross)
                Close();
        }

        virtual public void Open()
        {
            _isClosing = false;
        }

        virtual public void Close()
        {
            _isClosing = true;
        }

        virtual public void Push(IMenu subMenu)
        {
            _subMenu = subMenu;
            _subMenu.Open();
            Close();
        }

        public void Update(double deltaTime)
        {
            if (!IsEnd)
                MyUpdate(deltaTime);

            if (_subMenu == null)
            {
                if (!_isClosing)
                    ProcessInput(InputManager);

                IsClosed = IsEnd;
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
            if (_subMenu == null || !IsEnd)
                MyDraw();
            else
                _subMenu.Draw();
        }

        abstract protected void MyUpdate(double deltaTime);
        abstract protected void MyDraw();
    }
}

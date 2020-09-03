using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.Menu
{
    public abstract class MenuBase : IMenu
    {
        private bool _isClosing;
        private IMenu _subMenu;

        protected IMenuManager MenuManager { get; }
        protected AnimatedSequenceFactory SequenceFactory => MenuManager.SequenceFactory;
        protected InputManager InputManager => MenuManager.InputManager;
        abstract protected bool IsEnd { get; }
        abstract public ushort MenuNameId { get; }
        public bool IsClosed { get; private set; }

        public MenuBase(IMenuManager menuManager)
        {
            MenuManager = menuManager;
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
            _subMenu?.Close();
        }

        virtual public void Push(IMenu subMenu)
        {
            Close();
            _subMenu = subMenu;
            _subMenu.Open();

            if (subMenu.MenuNameId == 0)
                MenuManager.PushSubMenuDescription(subMenu.ToString());
            else
                MenuManager.PushSubMenuDescription(subMenu.MenuNameId);
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
                    MenuManager.PopSubMenuDescription();
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

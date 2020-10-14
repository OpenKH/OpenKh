using OpenKh.Research.Kh2AnimTest.Infrastructure;

namespace OpenKh.Research.Kh2AnimTest.Menu
{
    public interface IMenuManager
    {
        IGameContext GameContext { get; }
        AnimatedSequenceFactory SequenceFactory { get; }
        InputManager InputManager { get; }

        void PushSubMenuDescription(ushort messageId);
        void PushSubMenuDescription(string message);
        void PopSubMenuDescription();
        void SetElementDescription(ushort messageId);
        void CloseAllMenu();
    }
}

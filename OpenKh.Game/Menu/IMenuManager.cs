using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.Menu
{
    public interface IMenuManager
    {
        AnimatedSequenceFactory SequenceFactory { get; }
        InputManager InputManager { get; }

        void PushSubMenuDescription(ushort messageId);
        void PushSubMenuDescription(string message);
        void PopSubMenuDescription();
        void SetElementDescription(ushort messageId);
    }
}

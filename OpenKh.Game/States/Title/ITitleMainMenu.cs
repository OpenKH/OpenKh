using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.States.Title
{
    public enum MainMenuState
    {
        Running,
        StartNewGame
    }

    interface ITitleMainMenu
    {
        InputManager InputManager { get; }
        MainMenuState State { set; }

        void Print(ushort messageId, float x, float y);
    }
}

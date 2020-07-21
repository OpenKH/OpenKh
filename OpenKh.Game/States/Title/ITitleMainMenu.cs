using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.States.Title
{
    public enum MainMenuState
    {
        Running,
        StartNewGame
    }

    public enum TextAlignment
    {
        Left,
        Center,
        Right
    }

    interface ITitleMainMenu
    {
        InputManager InputManager { get; }
        MainMenuState State { set; }

        void Print(ushort messageId, float left, float top, float right = 0,
            TextAlignment alignment = TextAlignment.Left);
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace OpenKh.Game.Infrastructure
{
    public class InputManager
    {
        private GamePadState pad;
        private KeyboardState keyboard;
        private KeyboardState prevKeyboard;

        public bool IsExit => pad.Buttons.Back == ButtonState.Pressed || keyboard.IsKeyDown(Keys.Escape);
        public bool IsDown => keyboard.IsKeyDown(Keys.Down) && !prevKeyboard.IsKeyDown(Keys.Down);
        public bool IsUp => keyboard.IsKeyDown(Keys.Up) && !prevKeyboard.IsKeyDown(Keys.Up);
        public bool IsCircle => keyboard.IsKeyDown(Keys.K) && !prevKeyboard.IsKeyDown(Keys.K);
        public bool IsCross => keyboard.IsKeyDown(Keys.L) && !prevKeyboard.IsKeyDown(Keys.L);

        public void Update()
        {
            pad = GamePad.GetState(PlayerIndex.One);
            prevKeyboard = keyboard;
            keyboard = Keyboard.GetState();
        }
    }
}

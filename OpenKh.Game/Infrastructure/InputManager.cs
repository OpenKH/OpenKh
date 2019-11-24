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
        public bool IsUp => Up && !prevKeyboard.IsKeyDown(Keys.Up);
        public bool IsDown => Down && !prevKeyboard.IsKeyDown(Keys.Down);
        public bool IsLeft => Left && !prevKeyboard.IsKeyDown(Keys.Left);
        public bool IsRight => Right && !prevKeyboard.IsKeyDown(Keys.Right);
        public bool IsCircle => keyboard.IsKeyDown(Keys.K) && !prevKeyboard.IsKeyDown(Keys.K);
        public bool IsCross => keyboard.IsKeyDown(Keys.L) && !prevKeyboard.IsKeyDown(Keys.L);

        public bool Up => keyboard.IsKeyDown(Keys.Up);
        public bool Down => keyboard.IsKeyDown(Keys.Down);
        public bool Left => keyboard.IsKeyDown(Keys.Left);
        public bool Right => keyboard.IsKeyDown(Keys.Right);
        public bool A => keyboard.IsKeyDown(Keys.A);
        public bool D => keyboard.IsKeyDown(Keys.D);
        public bool S => keyboard.IsKeyDown(Keys.S);
        public bool W => keyboard.IsKeyDown(Keys.W);

        public void Update()
        {
            pad = GamePad.GetState(PlayerIndex.One);
            prevKeyboard = keyboard;
            keyboard = Keyboard.GetState();
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System.Linq;

namespace OpenKh.Game.Infrastructure.Input
{
    public class KeyboardInput : IInputDevice
    {
        private KeyboardState keyboard;
        private KeyboardState prevKeyboard;
        private RepeatableKeyboard repeatableKeyboard = new RepeatableKeyboard();

        public bool IsUp => Up && !prevKeyboard.IsKeyDown(Keys.Up);
        public bool IsDown => Down && !prevKeyboard.IsKeyDown(Keys.Down);
        public bool IsLeft => Left && !prevKeyboard.IsKeyDown(Keys.Left);
        public bool IsRight => Right && !prevKeyboard.IsKeyDown(Keys.Right);
        public bool IsW => W && !prevKeyboard.IsKeyDown(Keys.W);
        public bool IsS => S && !prevKeyboard.IsKeyDown(Keys.S);
        public bool IsA => A && !prevKeyboard.IsKeyDown(Keys.A);
        public bool IsD => D && !prevKeyboard.IsKeyDown(Keys.D);

        public bool IsCircle => keyboard.IsKeyDown(Keys.K) && !prevKeyboard.IsKeyDown(Keys.K);
        public bool IsCross => keyboard.IsKeyDown(Keys.L) && !prevKeyboard.IsKeyDown(Keys.L);

        public bool IsDebug => keyboard.IsKeyDown(Keys.Tab) && !prevKeyboard.IsKeyDown(Keys.Tab);
        public bool IsShift => keyboard.IsKeyDown(Keys.LeftShift) /*&& !prevKeyboard.IsKeyDown(Keys.LeftShift)*/;
        public bool IsExit => keyboard.IsKeyDown(Keys.Escape) && !prevKeyboard.IsKeyDown(Keys.Escape);

        public bool Up => keyboard.IsKeyDown(Keys.Up);
        public bool Down => keyboard.IsKeyDown(Keys.Down);
        public bool Left => keyboard.IsKeyDown(Keys.Left);
        public bool Right => keyboard.IsKeyDown(Keys.Right);
        public bool W => keyboard.IsKeyDown(Keys.W);
        public bool S => keyboard.IsKeyDown(Keys.S);
        public bool A => keyboard.IsKeyDown(Keys.A);
        public bool D => keyboard.IsKeyDown(Keys.D);

        public bool IsDebugUp => repeatableKeyboard.IsKeyRepeat(Keys.Up);
        public bool IsDebugDown => repeatableKeyboard.IsKeyRepeat(Keys.Down);
        public bool IsDebugLeft => repeatableKeyboard.IsKeyRepeat(Keys.Left);
        public bool IsDebugRight => repeatableKeyboard.IsKeyRepeat(Keys.Right);

        public void Update(GameTime gameTime)
        {
            prevKeyboard = keyboard;
            keyboard = Keyboard.GetState();

            var pressedKeys = prevKeyboard.GetPressedKeys();
            var pressingKeys = keyboard.GetPressedKeys();

            foreach (var keyDown in pressingKeys.Except(pressedKeys))
            {
                repeatableKeyboard.PressKey(keyDown);
            }

            foreach (var keyUp in pressedKeys.Except(pressingKeys))
            {
                repeatableKeyboard.ReleaseKey(keyUp);
            }

            var seconds = (float)gameTime.ElapsedGameTime.TotalSeconds;

            foreach (var keepDown in pressedKeys.Intersect(pressingKeys))
            {
                repeatableKeyboard.UpdateKey(keepDown, seconds);
            }
        }
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

namespace OpenKh.Game.Infrastructure.Input
{
    public class KeyboardInput : IInputDevice
    {
        private KeyboardState keyboard;
        private KeyboardState prevKeyboard;
        private RepeatableKeyboard repeatableKeyboard = new RepeatableKeyboard();

        public bool IsDPadUp => DPadUp && !prevKeyboard.IsKeyDown(Keys.Up);
        public bool IsDPadDown => DPadDown && !prevKeyboard.IsKeyDown(Keys.Down);
        public bool IsDPadLeft => DPadLeft && !prevKeyboard.IsKeyDown(Keys.Left);
        public bool IsDPadRight => DPadRight && !prevKeyboard.IsKeyDown(Keys.Right);

        public bool IsLeftStickUp => LeftStickUp && !prevKeyboard.IsKeyDown(Keys.W);
        public bool IsLeftStickDown => LeftStickDown && !prevKeyboard.IsKeyDown(Keys.S);
        public bool IsLeftStickLeft => LeftStickLeft && !prevKeyboard.IsKeyDown(Keys.A);
        public bool IsLeftStickRight => LeftStickRight && !prevKeyboard.IsKeyDown(Keys.D);

        public bool IsRightStickUp => RightStickUp && !prevKeyboard.IsKeyDown(Keys.Up);
        public bool IsRightStickDown => RightStickDown && !prevKeyboard.IsKeyDown(Keys.Down);
        public bool IsRightStickLeft => RightStickLeft && !prevKeyboard.IsKeyDown(Keys.Left);
        public bool IsRightStickRight => RightStickDown && !prevKeyboard.IsKeyDown(Keys.Right);

        public bool IsCircle => keyboard.IsKeyDown(Keys.K) && !prevKeyboard.IsKeyDown(Keys.K);
        public bool IsCross => keyboard.IsKeyDown(Keys.L) && !prevKeyboard.IsKeyDown(Keys.L);
        public bool IsSquare => throw new NotImplementedException();
        public bool IsTriangle => throw new NotImplementedException();

        public bool IsStart => keyboard.IsKeyDown(Keys.Enter) && !prevKeyboard.IsKeyDown(Keys.Enter);
        public bool IsSelect => throw new NotImplementedException();
        public bool IsHome => throw new NotImplementedException();

        public bool IsLeftShoulder => throw new NotImplementedException();
        public bool IsRightShoulder => RightShoulder && !prevKeyboard.IsKeyDown(Keys.Tab);

        public bool IsLeftTrigger => throw new NotImplementedException();
        public bool IsRightTrigger => throw new NotImplementedException();

        public bool IsLeftStickButton => throw new NotImplementedException();
        public bool IsRightStickButton => throw new NotImplementedException();

        public bool IsDebug => IsRightShoulder;
        public bool IsShift => LeftShoulder;
        public bool IsExit => keyboard.IsKeyDown(Keys.Escape);

        public bool IsRepetableUp => repeatableKeyboard.IsKeyRepeat(Keys.Up);
        public bool IsRepetableDown => repeatableKeyboard.IsKeyRepeat(Keys.Down);
        public bool IsRepetableLeft => repeatableKeyboard.IsKeyRepeat(Keys.Left);
        public bool IsRepetableRight => repeatableKeyboard.IsKeyRepeat(Keys.Right);

        public bool DPadUp => keyboard.IsKeyDown(Keys.Up);
        public bool DPadDown => keyboard.IsKeyDown(Keys.Down);
        public bool DPadLeft => keyboard.IsKeyDown(Keys.Left);
        public bool DPadRight => keyboard.IsKeyDown(Keys.Right);

        public bool LeftStickUp => keyboard.IsKeyDown(Keys.W);
        public bool LeftStickDown => keyboard.IsKeyDown(Keys.S);
        public bool LeftStickLeft => keyboard.IsKeyDown(Keys.A);
        public bool LeftStickRight => keyboard.IsKeyDown(Keys.D);

        public bool RightStickUp => keyboard.IsKeyDown(Keys.Up);
        public bool RightStickDown => keyboard.IsKeyDown(Keys.Down);
        public bool RightStickLeft => keyboard.IsKeyDown(Keys.Left);
        public bool RightStickRight => keyboard.IsKeyDown(Keys.Right);

        public bool LeftShoulder => keyboard.IsKeyDown(Keys.LeftShift);
        public bool RightShoulder => keyboard.IsKeyDown(Keys.Tab);

        public bool LeftTrigger => keyboard.IsKeyDown(Keys.U);
        public bool RightTrigger => keyboard.IsKeyDown(Keys.I);

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

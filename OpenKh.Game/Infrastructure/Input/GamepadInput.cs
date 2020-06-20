using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace OpenKh.Game.Infrastructure.Input
{
    public class GamepadInput : IInputDevice
    {
        private const float leftThumbDeadZone = 0.1f;
        private const float rightThumbDeadZone = 0.1f;

        private GamePadState pad;
        private GamePadState prevPad;

        public bool IsUp => Up && ExcludeAxesDeadZone(prevPad.ThumbSticks.Right.Y, rightThumbDeadZone) == 0.0f;
        public bool IsDown => Down && ExcludeAxesDeadZone(prevPad.ThumbSticks.Right.Y, rightThumbDeadZone) == 0.0f;
        public bool IsLeft => Left && ExcludeAxesDeadZone(prevPad.ThumbSticks.Right.X, rightThumbDeadZone) == 0.0f;
        public bool IsRight => Right && ExcludeAxesDeadZone(prevPad.ThumbSticks.Right.X, rightThumbDeadZone) == 0.0f;
        public bool IsW => W && ExcludeAxesDeadZone(prevPad.ThumbSticks.Left.Y, leftThumbDeadZone) == 0.0f;
        public bool IsS => S && ExcludeAxesDeadZone(prevPad.ThumbSticks.Left.Y, leftThumbDeadZone) == 0.0f;
        public bool IsA => A && ExcludeAxesDeadZone(prevPad.ThumbSticks.Left.X, leftThumbDeadZone) == 0.0f; 
        public bool IsD => D && ExcludeAxesDeadZone(prevPad.ThumbSticks.Left.X, leftThumbDeadZone) == 0.0f;

        public bool IsCircle => pad.Buttons.B == ButtonState.Pressed && prevPad.Buttons.B != ButtonState.Pressed;
        public bool IsCross => pad.Buttons.A == ButtonState.Pressed && prevPad.Buttons.A != ButtonState.Pressed;

        public bool IsDebug => pad.Buttons.RightShoulder == ButtonState.Pressed && prevPad.Buttons.RightShoulder != ButtonState.Pressed;
        public bool IsShift => pad.Buttons.LeftShoulder == ButtonState.Pressed /*&& prevPad.Buttons.LeftShoulder != ButtonState.Pressed*/;
        public bool IsExit => pad.Buttons.Back == ButtonState.Pressed && prevPad.Buttons.Back != ButtonState.Pressed;

        public bool Up => ExcludeAxesDeadZone(pad.ThumbSticks.Right.Y, rightThumbDeadZone) > 0.0f;
        public bool Down => ExcludeAxesDeadZone(pad.ThumbSticks.Right.Y, rightThumbDeadZone) < 0.0f;
        public bool Left => ExcludeAxesDeadZone(pad.ThumbSticks.Right.X, rightThumbDeadZone) < 0.0f;
        public bool Right => ExcludeAxesDeadZone(pad.ThumbSticks.Right.X, rightThumbDeadZone) > 0.0f;
        public bool W => ExcludeAxesDeadZone(pad.ThumbSticks.Left.Y, leftThumbDeadZone) > 0.0f;
        public bool S => ExcludeAxesDeadZone(pad.ThumbSticks.Left.Y, leftThumbDeadZone) < 0.0f;
        public bool A => ExcludeAxesDeadZone(pad.ThumbSticks.Left.X, leftThumbDeadZone) < 0.0f;
        public bool D => ExcludeAxesDeadZone(pad.ThumbSticks.Left.X, leftThumbDeadZone) > 0.0f;

        public bool IsDebugUp => pad.DPad.Up == ButtonState.Pressed && prevPad.DPad.Up != ButtonState.Pressed;
        public bool IsDebugDown => pad.DPad.Down == ButtonState.Pressed && prevPad.DPad.Down != ButtonState.Pressed;
        public bool IsDebugLeft => pad.DPad.Left == ButtonState.Pressed && prevPad.DPad.Left != ButtonState.Pressed;
        public bool IsDebugRight => pad.DPad.Right == ButtonState.Pressed && prevPad.DPad.Right != ButtonState.Pressed;

        public void Update(GameTime gameTime)
        {
            prevPad = pad;
            pad = GamePad.GetState(PlayerIndex.One);
        }

        private float ExcludeAxesDeadZone(float input, float deadzone)
        {
            if (Math.Abs(input) < deadzone)
                return 0.0f;
            else return input;
        }

        private Vector2 ExcludeRadialDeadZone(Vector2 input, float deadzone)
        {
            if (input.Length() < deadzone)
                return Vector2.Zero;
            else return input;
        }

        private Vector2 ExcludeScaledRadialDeadZone(Vector2 input, float deadzone)
        {
            if (input.Length() < deadzone)
                return Vector2.Zero;
            else
            {
                input.Normalize();
                return input * ((input.Length() - deadzone) / (1 - deadzone));
            }
        }

    }
}

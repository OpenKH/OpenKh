using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;

namespace OpenKh.Game.Infrastructure.Input
{
    public class GamepadInput : IInputDevice
    {
        private const float leftThumbDeadZone = 0.1f;
        private const float rightThumbDeadZone = 0.1f;
        private const float triggersDeadZone = 0.1f;

        private GamePadState pad;
        private GamePadState prevPad;

        public bool IsDPadUp => DPadUp && prevPad.DPad.Up != ButtonState.Pressed;
        public bool IsDPadDown => DPadDown && prevPad.DPad.Down != ButtonState.Pressed;
        public bool IsDPadLeft => DPadLeft && prevPad.DPad.Left != ButtonState.Pressed;
        public bool IsDPadRight => DPadRight && prevPad.DPad.Right != ButtonState.Pressed;

        public bool IsLeftStickUp => LeftStickUp && ExcludeAxesDeadZone(prevPad.ThumbSticks.Left.Y, leftThumbDeadZone) == 0.0f;
        public bool IsLeftStickDown => LeftStickDown && ExcludeAxesDeadZone(prevPad.ThumbSticks.Left.Y, leftThumbDeadZone) == 0.0f;
        public bool IsLeftStickLeft => LeftStickLeft && ExcludeAxesDeadZone(prevPad.ThumbSticks.Left.X, leftThumbDeadZone) == 0.0f;
        public bool IsLeftStickRight => LeftStickRight && ExcludeAxesDeadZone(prevPad.ThumbSticks.Left.X, leftThumbDeadZone) == 0.0f;

        public bool IsRightStickUp => RightStickUp && ExcludeAxesDeadZone(prevPad.ThumbSticks.Right.Y, rightThumbDeadZone) == 0.0f;
        public bool IsRightStickDown => RightStickDown && ExcludeAxesDeadZone(prevPad.ThumbSticks.Right.Y, rightThumbDeadZone) == 0.0f;
        public bool IsRightStickLeft => RightStickLeft && ExcludeAxesDeadZone(prevPad.ThumbSticks.Right.X, rightThumbDeadZone) == 0.0f;
        public bool IsRightStickRight => RightStickDown && ExcludeAxesDeadZone(prevPad.ThumbSticks.Right.X, rightThumbDeadZone) == 0.0f;

        public bool IsCircle => pad.Buttons.B == ButtonState.Pressed && prevPad.Buttons.B != ButtonState.Pressed;
        public bool IsCross => pad.Buttons.A == ButtonState.Pressed && prevPad.Buttons.A != ButtonState.Pressed;
        public bool IsSquare => pad.Buttons.X == ButtonState.Pressed && prevPad.Buttons.X != ButtonState.Pressed;
        public bool IsTriangle => pad.Buttons.Y == ButtonState.Pressed && prevPad.Buttons.Y != ButtonState.Pressed;

        public bool IsStart => pad.Buttons.Start == ButtonState.Pressed && prevPad.Buttons.Start != ButtonState.Pressed;
        public bool IsSelect => pad.Buttons.Back == ButtonState.Pressed && prevPad.Buttons.Back != ButtonState.Pressed;
        public bool IsHome => pad.Buttons.BigButton == ButtonState.Pressed && prevPad.Buttons.BigButton != ButtonState.Pressed;

        public bool IsLeftShoulder => LeftShoulder && prevPad.Buttons.LeftShoulder != ButtonState.Pressed;
        public bool IsRightShoulder => RightShoulder && prevPad.Buttons.RightShoulder != ButtonState.Pressed;

        public bool IsLeftTrigger => LeftTrigger && ExcludeAxesDeadZone(prevPad.Triggers.Left, triggersDeadZone) == 0.0f;
        public bool IsRightTrigger => RightTrigger && ExcludeAxesDeadZone(prevPad.Triggers.Right, triggersDeadZone) == 0.0f;

        public bool IsLeftStickButton => pad.Buttons.LeftStick == ButtonState.Pressed && prevPad.Buttons.LeftStick != ButtonState.Pressed;
        public bool IsRightStickButton => pad.Buttons.RightStick == ButtonState.Pressed && prevPad.Buttons.RightStick != ButtonState.Pressed;

        public bool IsDebug => IsRightShoulder;
        public bool IsShift => LeftShoulder;
        public bool IsExit => IsSelect;

        public bool IsRepetableUp => IsDPadUp;
        public bool IsRepetableDown => IsDPadDown;
        public bool IsRepetableLeft => IsDPadLeft;
        public bool IsRepetableRight => IsDPadRight;

        public bool DPadUp => pad.DPad.Up == ButtonState.Pressed;
        public bool DPadDown => pad.DPad.Down == ButtonState.Pressed;
        public bool DPadLeft => pad.DPad.Left == ButtonState.Pressed;
        public bool DPadRight => pad.DPad.Right == ButtonState.Pressed;

        public bool LeftStickUp => ExcludeAxesDeadZone(pad.ThumbSticks.Left.Y, leftThumbDeadZone) > 0.0f;
        public bool LeftStickDown => ExcludeAxesDeadZone(pad.ThumbSticks.Left.Y, leftThumbDeadZone) < 0.0f;
        public bool LeftStickLeft => ExcludeAxesDeadZone(pad.ThumbSticks.Left.X, leftThumbDeadZone) < 0.0f;
        public bool LeftStickRight => ExcludeAxesDeadZone(pad.ThumbSticks.Left.X, leftThumbDeadZone) > 0.0f;

        public bool RightStickUp => ExcludeAxesDeadZone(pad.ThumbSticks.Right.Y, rightThumbDeadZone) > 0.0f;
        public bool RightStickDown => ExcludeAxesDeadZone(pad.ThumbSticks.Right.Y, rightThumbDeadZone) < 0.0f;
        public bool RightStickLeft => ExcludeAxesDeadZone(pad.ThumbSticks.Right.X, rightThumbDeadZone) < 0.0f;
        public bool RightStickRight => ExcludeAxesDeadZone(pad.ThumbSticks.Right.X, rightThumbDeadZone) > 0.0f;

        public bool LeftShoulder => pad.Buttons.LeftShoulder == ButtonState.Pressed;
        public bool RightShoulder => pad.Buttons.RightShoulder == ButtonState.Pressed;

        public bool LeftTrigger => ExcludeAxesDeadZone(pad.Triggers.Left, triggersDeadZone) > 0.0f;
        public bool RightTrigger => ExcludeAxesDeadZone(pad.Triggers.Right, triggersDeadZone) > 0.0f;

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

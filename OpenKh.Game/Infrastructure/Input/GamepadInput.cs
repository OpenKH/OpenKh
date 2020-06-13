using Microsoft.Xna.Framework.Input;

namespace OpenKh.Game.Infrastructure.Input
{
    public class GamepadInput : IInputDevice
    {
        private GamePadState pad;
        private GamePadState prevPad;

        public bool IsUp => Up && prevPad.ThumbSticks.Right.Y == 0.0f;
        public bool IsDown => Down && prevPad.ThumbSticks.Right.Y == 0.0f;
        public bool IsLeft => Left && prevPad.ThumbSticks.Right.X == 0.0f;
        public bool IsRight => Right && prevPad.ThumbSticks.Right.X == 0.0f;
        public bool IsW => W && prevPad.ThumbSticks.Left.Y == 0.0f;
        public bool IsA => A && prevPad.ThumbSticks.Left.Y == 0.0f;
        public bool IsS => S && prevPad.ThumbSticks.Left.X == 0.0f;
        public bool IsD => D && prevPad.ThumbSticks.Left.X == 0.0f;

        public bool IsCircle => pad.Buttons.B == ButtonState.Pressed && prevPad.Buttons.B != ButtonState.Pressed;
        public bool IsCross => pad.Buttons.A == ButtonState.Pressed && prevPad.Buttons.A != ButtonState.Pressed;

        public bool IsDebug => pad.Buttons.RightShoulder == ButtonState.Pressed && prevPad.Buttons.RightShoulder != ButtonState.Pressed;
        public bool IsShift => pad.Buttons.LeftShoulder == ButtonState.Pressed /*&& prevPad.Buttons.LeftShoulder != ButtonState.Pressed*/;
        public bool IsExit => pad.Buttons.Back == ButtonState.Pressed && prevPad.Buttons.Back != ButtonState.Pressed;

        public bool Up => pad.ThumbSticks.Right.Y > 0.0f;
        public bool Down => pad.ThumbSticks.Right.Y < 0.0f;
        public bool Left => pad.ThumbSticks.Right.X < 0.0f;
        public bool Right => pad.ThumbSticks.Right.X > 0.0f;
        public bool W => pad.ThumbSticks.Left.Y > 0.0f;
        public bool A => pad.ThumbSticks.Left.X < 0.0f; 
        public bool S => pad.ThumbSticks.Left.Y < 0.0f;
        public bool D => pad.ThumbSticks.Left.X > 0.0f;

        public bool IsDebugUp => pad.DPad.Up == ButtonState.Pressed && prevPad.DPad.Up != ButtonState.Pressed;
        public bool IsDebugDown => pad.DPad.Down == ButtonState.Pressed && prevPad.DPad.Down != ButtonState.Pressed;
        public bool IsDebugLeft => pad.DPad.Left == ButtonState.Pressed && prevPad.DPad.Left != ButtonState.Pressed;
        public bool IsDebugRight => pad.DPad.Right == ButtonState.Pressed && prevPad.DPad.Right != ButtonState.Pressed;

        public void Update()
        {
            prevPad = pad;
            pad = GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
        }
    }
}

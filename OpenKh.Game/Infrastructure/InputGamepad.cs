using Microsoft.Xna.Framework.Input;
using OpenKh.Engine.Input;
using System;
using System.Numerics;

namespace OpenKh.Game.Infrastructure
{
    public class InputGamepad : IInputDevice
    {
        private const float AxisDeadZone = 0.1f;
        private const float TriggerDeadZone = 0.8f;

        public Vector3 AnalogLeft { get; private set; }
        public Vector3 AnalogRight { get; private set; }
        public bool Up { get; private set; }
        public bool Down { get; private set; }
        public bool Left { get; private set; }
        public bool Right { get; private set; }
        public bool FaceDown { get; private set; }
        public bool FaceRight { get; private set; }
        public bool FaceLeft { get; private set; }
        public bool FaceUp { get; private set; }
        public bool SpecialLeft { get; private set; }
        public bool SpecialRight { get; private set; }
        public bool L1 { get; private set; }
        public bool L2 { get; private set; }
        public bool L3 { get; private set; }
        public bool R1 { get; private set; }
        public bool R2 { get; private set; }
        public bool R3 { get; private set; }
        public bool Confirm => false;
        public bool Cancel => false;

        public void Update()
        {
            var state = GamePad.GetState(Microsoft.Xna.Framework.PlayerIndex.One);
            Up = state.DPad.Up == ButtonState.Pressed;
            Down = state.DPad.Down == ButtonState.Pressed;
            Left = state.DPad.Left == ButtonState.Pressed;
            Right = state.DPad.Right == ButtonState.Pressed;
            FaceDown = state.Buttons.A == ButtonState.Pressed;
            FaceRight = state.Buttons.B == ButtonState.Pressed;
            FaceLeft = state.Buttons.X == ButtonState.Pressed;
            FaceUp = state.Buttons.Y == ButtonState.Pressed;
            SpecialLeft = state.Buttons.Back == ButtonState.Pressed;
            SpecialRight = state.Buttons.Start == ButtonState.Pressed;
            L1 = state.Buttons.LeftShoulder == ButtonState.Pressed;
            L3 = state.Buttons.LeftStick == ButtonState.Pressed;
            R1 = state.Buttons.RightStick == ButtonState.Pressed;
            R3 = state.Buttons.RightStick == ButtonState.Pressed;

            AnalogLeft = new Vector3(
                ExcludeAxisDeadZone(state.ThumbSticks.Left.X),
                ExcludeAxisDeadZone(state.ThumbSticks.Left.Y),
                ExcludeAxisDeadZone(state.Triggers.Left));
            AnalogRight = new Vector3(
                ExcludeAxisDeadZone(state.ThumbSticks.Right.X),
                ExcludeAxisDeadZone(state.ThumbSticks.Right.Y),
                ExcludeAxisDeadZone(state.Triggers.Right));

            L2 = ExcludeTriggerDeadZone(AnalogLeft.Z);
            R2 = ExcludeTriggerDeadZone(AnalogRight.Z);
        }

        private static float ExcludeAxisDeadZone(float value) =>
            Math.Abs(value) < AxisDeadZone ? 0 : value;

        private static bool ExcludeTriggerDeadZone(float value) =>
            Math.Abs(value) >= TriggerDeadZone;
    }
}

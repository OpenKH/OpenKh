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
        public bool Cross { get; private set; }
        public bool Circle { get; private set; }
        public bool Square { get; private set; }
        public bool Triangle { get; private set; }
        public bool Select { get; private set; }
        public bool Start { get; private set; }
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
            Cross = state.Buttons.A == ButtonState.Pressed;
            Circle = state.Buttons.B == ButtonState.Pressed;
            Square = state.Buttons.X == ButtonState.Pressed;
            Triangle = state.Buttons.Y == ButtonState.Pressed;
            Select = state.Buttons.Back == ButtonState.Pressed;
            Start = state.Buttons.Start == ButtonState.Pressed;
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

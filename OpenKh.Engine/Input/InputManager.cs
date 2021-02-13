using System;
using System.Numerics;

namespace OpenKh.Engine.Input
{
    public class InputManager : IInput
    {
        private enum Button
        {
            Up,
            Down,
            Left,
            Right,
            Cross,
            Circle,
            Square,
            Triangle,
            Select,
            Start,
            L1,
            L2,
            L3,
            R1,
            R2,
            R3,
            Confirm,
            Cancel,
        }

        private class Buttons : IInputButtons
        {
            public uint Raw;

            public bool Up => (Raw & (1 << (int)Button.Up)) != 0;
            public bool Down => (Raw & (1 << (int)Button.Down)) != 0;
            public bool Left => (Raw & (1 << (int)Button.Left)) != 0;
            public bool Right => (Raw & (1 << (int)Button.Right)) != 0;
            public bool Cross => (Raw & (1 << (int)Button.Cross)) != 0;
            public bool Circle => (Raw & (1 << (int)Button.Circle)) != 0;
            public bool Square => (Raw & (1 << (int)Button.Square)) != 0;
            public bool Triangle => (Raw & (1 << (int)Button.Triangle)) != 0;
            public bool Select => (Raw & (1 << (int)Button.Select)) != 0;
            public bool Start => (Raw & (1 << (int)Button.Start)) != 0;
            public bool L1 => (Raw & (1 << (int)Button.L1)) != 0;
            public bool L2 => (Raw & (1 << (int)Button.L2)) != 0;
            public bool L3 => (Raw & (1 << (int)Button.L3)) != 0;
            public bool R1 => (Raw & (1 << (int)Button.R1)) != 0;
            public bool R2 => (Raw & (1 << (int)Button.R2)) != 0;
            public bool R3 => (Raw & (1 << (int)Button.R3)) != 0;
            public bool Confirm => (Raw & (1 << 16)) != 0;
            public bool Cancel => (Raw & (1 << 17)) != 0;

            public void MakeConfirmCancel(Button confirmMask, Button cancelMask)
            {
                Raw &= ~(1U << (int)Button.Confirm);
                Raw &= ~(1U << (int)Button.Cancel);

                if ((Raw & (1 << (int)confirmMask)) != 0)
                    Raw |= (1U << (int)Button.Confirm);
                if ((Raw & (1 << (int)cancelMask)) != 0)
                    Raw |= (1U << (int)Button.Cancel);

            }
        }

        private const float ContinuousRepeatTime = 0.05f;
        private const float MinimumRepeatTime = 1f / 3f - ContinuousRepeatTime;

        private readonly Button _buttonConfirmMask;
        private readonly Button _buttonCancelMask;
        private readonly IInputDevice[] _devices;
        private readonly Buttons _pressed = new Buttons();
        private readonly Buttons _released = new Buttons();
        private readonly Buttons _triggered = new Buttons();
        private readonly Buttons _repeated = new Buttons();
        private float[] _repeatTimers = new float[sizeof(uint) * 8];

        public IInputButtons Pressed => _pressed;
        public IInputButtons Released => _released;
        public IInputButtons Triggered => _triggered;
        public IInputButtons Repeated => _repeated;
        public Vector3 AxisLeft { get; private set; }
        public Vector3 AxisRight { get; private set; }

        public InputManager(bool japaneseStyleButtons, params IInputDevice[] devices)
        {
            _buttonConfirmMask = japaneseStyleButtons ? Button.Circle : Button.Cross;
            _buttonCancelMask = japaneseStyleButtons ? Button.Cross : Button.Circle;
            _devices = devices;
        }

        public void Update(double deltaTime)
        {
            foreach (var device in _devices)
                device.Update();

            var previouslyPressed = _pressed.Raw;
            var pressedNow = 0U;
            AxisLeft = Vector3.Zero;
            AxisRight = Vector3.Zero;
            foreach (var device in _devices)
            {
                if (device.Up)
                    pressedNow |= 1 << (int)Button.Up;
                if (device.Down)
                    pressedNow |= 1 << (int)Button.Down;
                if (device.Left)
                    pressedNow |= 1 << (int)Button.Left;
                if (device.Right)
                    pressedNow |= 1 << (int)Button.Right;
                if (device.Cross)
                    pressedNow |= 1 << (int)Button.Cross;
                if (device.Circle)
                    pressedNow |= 1 << (int)Button.Circle;
                if (device.Square)
                    pressedNow |= 1 << (int)Button.Square;
                if (device.Triangle)
                    pressedNow |= 1 << (int)Button.Triangle;
                if (device.Select)
                    pressedNow |= 1 << (int)Button.Select;
                if (device.Start)
                    pressedNow |= 1 << (int)Button.Start;
                if (device.L1)
                    pressedNow |= 1 << (int)Button.L1;
                if (device.L2)
                    pressedNow |= 1 << (int)Button.L2;
                if (device.L3)
                    pressedNow |= 1 << (int)Button.L3;
                if (device.R1)
                    pressedNow |= 1 << (int)Button.R1;
                if (device.R2)
                    pressedNow |= 1 << (int)Button.R2;
                if (device.R3)
                    pressedNow |= 1 << (int)Button.R3;

                if (device.AnalogLeft != Vector3.Zero)
                    AxisLeft = device.AnalogLeft;
                if (device.AnalogRight != Vector3.Zero)
                    AxisRight = device.AnalogRight;
            }

            _pressed.Raw = pressedNow;
            _released.Raw = (pressedNow ^ previouslyPressed) & ~pressedNow;
            _triggered.Raw = (pressedNow ^ previouslyPressed) & pressedNow;

            _repeated.Raw = 0;
            for (var i = 0; i < _repeatTimers.Length; i++)
            {
                var flag = 1U << i;
                var isPressed = (_pressed.Raw & flag) != 0;
                if (isPressed)
                {
                    _repeatTimers[i] += (float)deltaTime;
                    if (_repeatTimers[i] >= MinimumRepeatTime)
                    {
                        var continuousRepeatTimer = _repeatTimers[i] - MinimumRepeatTime;
                        if (continuousRepeatTimer >= ContinuousRepeatTime)
                        {
                            continuousRepeatTimer %= ContinuousRepeatTime;
                            _repeated.Raw |= flag;
                        }

                        _repeatTimers[i] = MinimumRepeatTime + continuousRepeatTimer;
                    }
                    else if (isPressed)
                    {
                        _repeated.Raw |= _triggered.Raw & flag;
                    }
                }
                else
                    _repeatTimers[i] = 0f;
            }

            _pressed.MakeConfirmCancel(_buttonConfirmMask, _buttonCancelMask);
            _released.MakeConfirmCancel(_buttonConfirmMask, _buttonCancelMask);
            _triggered.MakeConfirmCancel(_buttonConfirmMask, _buttonCancelMask);
            _repeated.MakeConfirmCancel(_buttonConfirmMask, _buttonCancelMask);
        }
    }
}

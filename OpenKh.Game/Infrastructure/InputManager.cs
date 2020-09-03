using Microsoft.Xna.Framework;
using OpenKh.Game.Infrastructure.Input;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.Infrastructure
{
    public class InputManager
    {
        private List<IInputDevice> _devices;

        public bool IsDebug => _devices.Any(x => x.IsDebug);
        public bool IsShift => _devices.Any(x => x.IsShift);
        public bool IsMenuRight => _devices.Any(x => x.IsRepetableRight);
        public bool IsMenuLeft => _devices.Any(x => x.IsRepetableLeft);
        public bool IsMenuUp => _devices.Any(x => x.IsRepetableUp);
        public bool IsMenuDown => _devices.Any(x => x.IsRepetableDown);

        public bool IsExit => _devices.Any(x => x.IsExit);
        public bool IsUp => _devices.Any(x => x.IsDPadUp);
        public bool IsDown => _devices.Any(x => x.IsDPadDown);
        public bool IsLeft => _devices.Any(x => x.IsDPadLeft);
        public bool IsRight => _devices.Any(x => x.IsDPadRight);
        public bool IsCircle => _devices.Any(x => x.IsCircle);
        public bool IsCross => _devices.Any(x => x.IsCross);
        public bool IsSquare => _devices.Any(x => x.IsSquare);
        public bool IsTriangle => _devices.Any(x => x.IsTriangle);
        public bool IsStart => _devices.Any(x => x.IsStart);
        public bool IsSelect => _devices.Any(x => x.IsSelect);

        public bool Up => _devices.Any(x => x.RightStickUp);
        public bool Down => _devices.Any(x => x.RightStickDown);
        public bool Left => _devices.Any(x => x.RightStickLeft);
        public bool Right => _devices.Any(x => x.RightStickRight);
        public bool W => _devices.Any(x => x.LeftStickUp);
        public bool S => _devices.Any(x => x.LeftStickDown);
        public bool A => _devices.Any(x => x.LeftStickLeft);
        public bool D => _devices.Any(x => x.LeftStickRight);
        public bool RightTrigger => _devices.Any(x => x.RightTrigger);

        public InputManager()
        {
            _devices = new List<IInputDevice>()
            {
                new KeyboardInput(),
                new GamepadInput(),
            };
        }

        public void Update(GameTime gameTime)
        {
            foreach (var device in _devices)
                device.Update(gameTime);
        }
    }
}

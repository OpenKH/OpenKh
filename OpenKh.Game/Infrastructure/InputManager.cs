using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Linq;

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
        public bool IsDebugRight => _devices.Any(x => x.IsDebugRight);
        public bool IsDebugLeft => _devices.Any(x => x.IsDebugLeft);
        public bool IsDebugUp => _devices.Any(x => x.IsDebugUp);
        public bool IsDebugDown => _devices.Any(x => x.IsDebugDown);

        public bool IsExit => _devices.Any(x => x.IsExit);
        public bool IsUp => _devices.Any(x => x.IsUp);
        public bool IsDown => _devices.Any(x => x.IsDown);
        public bool IsLeft => _devices.Any(x => x.IsLeft);
        public bool IsRight => _devices.Any(x => x.IsRight);
        public bool IsCircle => _devices.Any(x => x.IsCircle);
        public bool IsCross => _devices.Any(x => x.IsCross);

        public bool Up => _devices.Any(x => x.Up);
        public bool Down => _devices.Any(x => x.Down);
        public bool Left => _devices.Any(x => x.Left);
        public bool Right => _devices.Any(x => x.Right);
        public bool W => _devices.Any(x => x.W);
        public bool S =>_devices.Any(x => x.S);
        public bool A =>_devices.Any(x => x.A);
        public bool D =>_devices.Any(x => x.D);

        public InputManager()
        {
            _devices = new List<IInputDevice>()
            {
                new KeyboardInput(),
                new GamepadInput(),
            };
        }

        public void Update(GameTime gameTime) => _devices.ForEach(device => device.Update(gameTime));
    }
}

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenKh.Game.Infrastructure
{
    class RepeatableKeyboard
    {
        public bool IsKeyPressed(Keys key) => GetKey(key).IsPressed;
        public void PressKey(Keys key) => KeyStatePool[(int)key] = new KeyState { IsPressed = true };
        public void ReleaseKey(Keys key) => KeyStatePool[(int)key] = new KeyState { IsPressed = false };
        public void UpdateKey(Keys key, float seconds) => KeyStatePool[(int)key].Timer += seconds;

        public bool IsKeyRepeat(Keys key)
        {
            if (!IsKeyPressed(key))
                return false;
            var keyState = KeyStatePool[(int)key];
            var timer = keyState.Timer;
            if (timer < MinimumRepeatTime)
                return false;
            timer -= MinimumRepeatTime;
            if (timer < keyState.PressCount * ContinuousRepeatTime)
                return false;
            keyState.PressCount++;
            KeyStatePool[(int)key] = keyState;
            return true;
        }

        const float MinimumRepeatTime = 1.0f; // seconds
        const float ContinuousRepeatTime = 0.25f; // every 250ms

        private const int MaxKeyCount = 256;

        private KeyState[] KeyStatePool = new KeyState[MaxKeyCount];
        private KeyState GetKey(Keys key) => KeyStatePool[(int)key];

        private struct KeyState
        {
            public bool IsPressed;
            public float Timer;
            public int PressCount;
        }
    }
}

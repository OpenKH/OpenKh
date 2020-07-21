using Microsoft.Xna.Framework.Input;

namespace OpenKh.Game.Infrastructure.Input
{
    class RepeatableKeyboard
    {
        public bool IsKeyPressed(Keys key) => GetKey(key).IsPressed;
        public void PressKey(Keys key) => KeyStatePool[(int)key] = new KeyState { IsPressed = true, KeyDownFirstChance = true };
        public void ReleaseKey(Keys key) => KeyStatePool[(int)key] = new KeyState { IsPressed = false };
        public void UpdateKey(Keys key, float seconds)
        {
            var state = KeyStatePool[(int)key];

            state.Timer += seconds;
            state.KeyDownFirstChance = false;

            KeyStatePool[(int)key] = state;
        }

        public bool IsKeyRepeat(Keys key)
        {
            var state = KeyStatePool[(int)key];
            if (!state.IsPressed)
                return false;
            if (state.KeyDownFirstChance)
                return true;
            var timer = state.Timer;
            if (timer < MinimumRepeatTime)
                return false;
            timer -= MinimumRepeatTime;
            if (timer < state.PressCount * ContinuousRepeatTime)
                return false;
            state.PressCount++;
            KeyStatePool[(int)key] = state;
            return true;
        }

        const float MinimumRepeatTime = 1.0f; // seconds
        const float ContinuousRepeatTime = 0.05f; // every 50ms

        private const int MaxKeyCount = 256;

        private KeyState[] KeyStatePool = new KeyState[MaxKeyCount];
        private KeyState GetKey(Keys key) => KeyStatePool[(int)key];

        private struct KeyState
        {
            public bool IsPressed;
            public bool KeyDownFirstChance;
            public float Timer;
            public int PressCount;
        }
    }
}

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
        private bool isPressing = false;
        private Keys lastPressed;
        private double nextRepeatInMillis = 0;
        private int keyboardDelay;
        private int keyboardSpeed;
        private bool blockRepeats = false;

        [DllImport("user32.dll", SetLastError = true)]
        [return: MarshalAs(UnmanagedType.Bool)]
        static extern bool SystemParametersInfo(int uiAction, int uiParam, out int pvParam, int fWinIni);

        /// <summary>
        /// Retrieves the keyboard repeat-delay setting, which is a value in the range from 0 (approximately 250 ms delay) through 3 (approximately 1 second delay). The actual delay associated with each value may vary depending on the hardware. The pvParam parameter must point to an integer variable that receives the setting.
        /// </summary>
        /// <remarks>
        /// first:
        /// 0 = 250 ms
        /// 1 = 500 ms
        /// 2 = 750 ms
        /// 3 = 1000 ms
        /// </remarks>
        const int SPI_GETKEYBOARDDELAY = 0x16;

        /// <summary>
        /// Retrieves the keyboard repeat-speed setting, which is a value in the range from 0 (approximately 2.5 repetitions per second) through 31 (approximately 30 repetitions per second). The actual repeat rates are hardware-dependent and may vary from a linear scale by as much as 20%. The pvParam parameter must point to a DWORD variable that receives the setting.
        /// </summary>
        /// <remarks>
        /// repeat:
        /// 0 = 400 ms
        /// ...
        /// 31 = 33 ms
        /// </remarks>
        const int SPI_GETKEYBOARDSPEED = 0x0a;


        public RepeatableKeyboard()
        {
            {
                int value;
                SystemParametersInfo(SPI_GETKEYBOARDDELAY, 0, out value, 0);
                keyboardDelay = 250 + 250 * value;
            }
            {
                int value;
                SystemParametersInfo(SPI_GETKEYBOARDSPEED, 0, out value, 0);
                keyboardSpeed = 400 - 12 * value;
            }
        }

        public void UnblockRepeats()
        {
            blockRepeats = false;
        }

        public bool IsKeyPressed(Keys key)
        {
            var pressed = key == lastPressed && isPressing && !blockRepeats;
            if (pressed)
            {
                blockRepeats = true;
            }
            return pressed;
        }

        public void UpdateWith(KeyboardState keyboard, GameTime gameTime)
        {
            isPressing = false;

            var downKeys = keyboard.GetPressedKeys();
            if (downKeys.Length == 1)
            {
                var downKey = downKeys[0];

                if (lastPressed == downKeys[0])
                {
                    nextRepeatInMillis -= gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (nextRepeatInMillis <= 0)
                    {
                        isPressing = true;

                        nextRepeatInMillis = keyboardSpeed;
                    }
                }
                else
                {
                    isPressing = true;
                    lastPressed = downKeys[0];
                    nextRepeatInMillis = keyboardDelay;
                    blockRepeats = false;
                }
            }
            else
            {
                lastPressed = Keys.None;
            }
        }
    }
}

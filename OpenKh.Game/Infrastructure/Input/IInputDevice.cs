using Microsoft.Xna.Framework;

namespace OpenKh.Game.Infrastructure.Input
{
    public interface IInputDevice
    {
        #region Button Press
        bool IsDPadUp { get; }
        bool IsDPadDown { get; }
        bool IsDPadLeft { get; }
        bool IsDPadRight { get; }

        bool IsLeftStickUp { get; }
        bool IsLeftStickDown { get; }
        bool IsLeftStickLeft { get; }
        bool IsLeftStickRight { get; }

        bool IsRightStickUp { get; }
        bool IsRightStickDown { get; }
        bool IsRightStickLeft { get; }
        bool IsRightStickRight { get; }

        bool IsCircle { get; }
        bool IsCross { get; }
        bool IsSquare { get; }
        bool IsTriangle { get; }

        bool IsStart { get; }
        bool IsSelect { get; }
        bool IsHome { get; }

        bool IsLeftShoulder { get; }
        bool IsRightShoulder { get; }

        bool IsLeftTrigger { get; }
        bool IsRightTrigger { get; }

        bool IsLeftStickButton { get; }
        bool IsRightStickButton { get; }

        bool IsDebug { get; }
        bool IsShift { get; }
        bool IsExit { get; }

        bool IsRepetableUp { get; }
        bool IsRepetableDown { get; }
        bool IsRepetableLeft { get; }
        bool IsRepetableRight { get; }

        #endregion

        #region Button Hold
        bool DPadUp { get; }
        bool DPadDown { get; }
        bool DPadLeft { get; }
        bool DPadRight { get; }

        bool LeftStickUp { get; }
        bool LeftStickDown { get; }
        bool LeftStickLeft { get; }
        bool LeftStickRight { get; }

        bool RightStickUp { get; }
        bool RightStickDown { get; }
        bool RightStickLeft { get; }
        bool RightStickRight { get; }

        bool LeftShoulder { get; }
        bool RightShoulder { get; }

        bool LeftTrigger { get; }
        bool RightTrigger { get; }

        #endregion

        void Update(GameTime gameTime);
    }
}

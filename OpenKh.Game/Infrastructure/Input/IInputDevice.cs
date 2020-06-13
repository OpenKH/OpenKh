namespace OpenKh.Game.Infrastructure.Input
{
    public interface IInputDevice
    {
        bool IsUp { get; }
        bool IsDown { get; }
        bool IsLeft { get; }
        bool IsRight { get; }

        bool IsW { get; }
        bool IsA { get; }
        bool IsS { get; }
        bool IsD { get; }

        bool IsCircle { get; }
        bool IsCross { get; }

        bool IsDebug { get; }
        bool IsShift { get; }
        bool IsExit { get; }

        bool Up { get; }
        bool Down { get; }
        bool Left { get; }
        bool Right { get; }

        bool W { get; }
        bool A { get; }
        bool S { get; }
        bool D { get; }

        bool IsDebugUp { get; }
        bool IsDebugDown { get; }
        bool IsDebugLeft { get; }
        bool IsDebugRight { get; }

        void Update(GameTime gameTime);
    }
}

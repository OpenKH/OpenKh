using System.Numerics;

namespace OpenKh.Engine.Input
{
    public interface IInputDevice : IInputButtons
    {
        Vector3 AnalogLeft { get; }
        Vector3 AnalogRight { get; }

        void Update();
    }
}

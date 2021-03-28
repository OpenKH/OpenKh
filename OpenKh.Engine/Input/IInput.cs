using System.Numerics;

namespace OpenKh.Engine.Input
{
    public interface IInput
    {
        IInputButtons Pressed { get; }
        IInputButtons Released { get; }
        IInputButtons Repeated { get; }
        IInputButtons Triggered { get; }
        Vector3 AxisLeft { get; }
        Vector3 AxisRight { get; }

        void Update(double deltaTime);
    }
}

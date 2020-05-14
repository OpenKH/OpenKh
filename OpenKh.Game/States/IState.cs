using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;

namespace OpenKh.Game.States
{
    public interface IState : IDebugConsumer
    {
        void Initialize(StateInitDesc initDesc);
        void Destroy();

        void Update(DeltaTimes deltaTimes);
        void Draw(DeltaTimes deltaTimes);
    }
}

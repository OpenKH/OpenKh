using OpenKh.Research.Kh2AnimTest.Debugging;
using OpenKh.Research.Kh2AnimTest.Infrastructure;

namespace OpenKh.Research.Kh2AnimTest.States
{
    public interface IState : IDebugConsumer
    {
        void Initialize(StateInitDesc initDesc);
        void Destroy();

        void Update(DeltaTimes deltaTimes);
        void Draw(DeltaTimes deltaTimes);
    }
}

using OpenKh.Research.Kh2AnimTest.Infrastructure;

namespace OpenKh.Research.Kh2AnimTest
{
    public interface IGameContext
    {
        public Kernel Kernel { get; }

        void LoadTitleScreen();
        void LoadPlace(int worldId, int placeId, int spawnIndex);
    }
}

using OpenKh.Game.Infrastructure;

namespace OpenKh.Game
{
    public interface IGameContext
    {
        public Kernel Kernel { get; }

        void LoadTitleScreen();
        void LoadPlace(int worldId, int placeId, int spawnIndex);
    }
}

using Microsoft.Xna.Framework;

namespace OpenKh.Game.Infrastructure
{
    public class StateInitDesc
    {
        public IDataContent DataContent { get; set; }
        public ArchiveManager ArchiveManager { get; set; }
        public GraphicsDeviceManager GraphicsDevice { get; set; }
    }
}

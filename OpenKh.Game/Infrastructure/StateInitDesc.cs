using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using OpenKh.Game.States;

namespace OpenKh.Game.Infrastructure
{
    public class StateInitDesc
    {
        public IDataContent DataContent { get; set; }
        public ArchiveManager ArchiveManager { get; set; }
        public Kernel Kernel { get; set; }
        public InputManager InputManager { get; set; }
        public ContentManager ContentManager { get; set; }
        public GraphicsDeviceManager GraphicsDevice { get; set; }
        public IStateChange StateChange { get; set; }
    }
}

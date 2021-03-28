using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using OpenKh.Engine.Input;
using OpenKh.Game.States;
using System.Collections.Generic;

namespace OpenKh.Game.Infrastructure
{
    public class StateInitDesc
    {
        public IDataContent DataContent { get; set; }
        public ArchiveManager ArchiveManager { get; set; }
        public Kernel Kernel { get; set; }
        public IInput Input { get; set; }
        public ContentManager ContentManager { get; set; }
        public GraphicsDeviceManager GraphicsDevice { get; set; }
        public IStateChange StateChange { get; set; }
        public Dictionary<string, string> StateSettings { get; set; }
    }
}

using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;

namespace OpenKh.Engine.MonoGame
{
    public class MeshGroup
    {
        public KingdomTexture[] Textures { get; set; }
        public List<MeshDesc> MeshDescriptors { get; set; }
    }
}

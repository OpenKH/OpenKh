using System.Collections.Generic;

namespace OpenKh.Engine.MonoGame
{
    public class MeshGroup : IMonoGameModel
    {
        public IKingdomTexture[] Textures { get; set; }
        public List<MeshDesc> MeshDescriptors { get; set; }
    }
}

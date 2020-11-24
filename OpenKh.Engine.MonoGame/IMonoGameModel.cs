using OpenKh.Engine.Parsers;
using System.Collections.Generic;

namespace OpenKh.Engine.MonoGame
{
    public interface IMonoGameModel
    {
        List<MeshDesc> MeshDescriptors { get; }

        IKingdomTexture[] Textures { get; }
    }
}

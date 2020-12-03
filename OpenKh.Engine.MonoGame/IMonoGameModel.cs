using OpenKh.Engine.Parsers;
using System.Collections.Generic;

namespace OpenKh.Engine.MonoGame
{
    public interface IMonoGameModel
    {
        List<MeshDescriptor> MeshDescriptors { get; }

        IKingdomTexture[] Textures { get; }
    }
}

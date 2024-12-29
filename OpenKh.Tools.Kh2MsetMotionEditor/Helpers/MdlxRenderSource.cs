using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetMotionEditor.Usecases;
using System.Collections.Generic;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public record MdlxRenderSource(
        Mdlx Model,
        List<ModelTexture.Texture> Textures,
        GetMutatedMeshDescriptorsUsecase GetMutatedMeshDescriptors
    )
    {
        public bool IsVisible { get; set; } = true;
    }
}

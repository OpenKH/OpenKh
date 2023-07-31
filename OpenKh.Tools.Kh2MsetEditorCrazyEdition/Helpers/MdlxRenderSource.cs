using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetEditorCrazyEdition.Usecases;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers
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

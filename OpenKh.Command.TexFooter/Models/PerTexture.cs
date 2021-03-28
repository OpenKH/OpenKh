using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Command.TexFooter.Models
{
    public class PerTexture
    {
        public Dictionary<string, TextureFooterDataIMEx> Textures { get; set; } = new Dictionary<string, TextureFooterDataIMEx>();
    }
}

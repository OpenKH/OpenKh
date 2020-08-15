using Microsoft.VisualBasic.CompilerServices;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Command.MapGen.Models
{
    public class MaterialDef
    {
        /// <summary>
        /// model's material name matching by simple pattern
        /// </summary>
        public string name { get; set; }

        /// <summary>
        /// do not render
        /// </summary>
        public bool ignore { get; set; }

        /// <summary>
        /// texture from png file
        /// </summary>
        public string fromFile { get; set; }

        /// <summary>
        /// use if reuseImd: Path.ChangeExtension(fromFile3, ".imd")
        /// </summary>
        public string fromFile2 { get; set; }

        /// <summary>
        /// texture from png file (diffuse map with texture file source)
        /// </summary>
        public string fromFile3 { get; set; }

        /// <summary>
        /// Have not collision
        /// </summary>
        public bool noclip { get; set; } = false;

        /// <summary>
        /// Have not rendered
        /// </summary>
        public bool nodraw { get; set; } = false;

        /// <summary>
        /// surfaceFlags if has collision
        /// </summary>
        public int surfaceFlags { get; set; } = 0x3f1;

        /// <summary>
        /// Intensity: PS2'S 128 is Windows's 255.
        /// </summary>
        public byte? maxColorIntensity { get; set; }

        /// <summary>
        /// Intensity: PS2'S 128 is Windows's 255.
        /// </summary>
        public byte? maxAlpha { get; set; }

        /// <summary>
        /// options for imgtool if needed
        /// </summary>
        public string imgtoolOptions { get; set; }

        public TextureOptions textureOptions { get; set; } = new TextureOptions();

        public short? transparentFlag { get; set; }

        public static MaterialDef CreateFallbackFor(string name) =>
            new MaterialDef
            {
                name = name,
            };
    }
}

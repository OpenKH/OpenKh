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
        /// Generate camera clip
        /// </summary>
        public bool cameraClip { get; set; } = false;

        /// <summary>
        /// Generate light clip
        /// </summary>
        public bool lightClip { get; set; } = false;

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

        public int? uvscIndex { get; set; }

        /// <summary>
        /// Ground value in collision. Such as 30, 25, 8, 2, 1, or 0
        /// </summary>
        public byte ground { get; set; }

        /// <summary>
        /// FloorLevel value in collision
        /// </summary>
        public byte floorLevel { get; set; }

        /// <summary>
        /// Group value for Collision. Used with MapVisibility to turn on/off map meshes and collision.
        /// </summary>
        public byte group { get; set; }
        
        /// <summary>
        /// Collision.Attributes for camera collision. Still unknown. Such as 0x000003F0
        /// </summary>
        public int cameraFlags { get; set; }

        /// <summary>
        /// Collision.Attributes for light collision. Still unknown. Such as 0x000803F1
        /// </summary>
        public int lightFlags { get; set; }

        /// <summary>
        /// Drop no shadow?
        /// </summary>
        public bool noShadow { get; set; }

        /// <summary>
        /// Additive alpha blending?
        /// </summary>
        public bool alphaAdd { get; set; }

        /// <summary>
        /// Subtractive alpha blending?
        /// </summary>
        public bool alphaSubtract { get; set; }

        /// <summary>
        /// Enable normal vector
        /// </summary>
        public bool normal { get; set; } = false;

        public static MaterialDef CreateFallbackFor(string name) =>
            new MaterialDef
            {
                name = name,
            };
    }
}

using OpenKh.Engine.MonoGame;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetEditorCrazyEdition.Helpers
{
    public class LoadedModel
    {
        public Bar? MdlxEntries { get; set; }
        public Bar? MsetEntries { get; set; }
        public Bar? AnbEntries { get; set; }
        public List<MdlxRenderSource> MdlxRenderableList { get; set; } = new List<MdlxRenderSource>();
        public List<MotionDisplay> MotionList { get; set; } = new List<MotionDisplay>();
        public int SelectedMotionIndex { get; set; } = -1;
        public Motion? MotionData { get; set; }
        public float FrameTime { get; set; }
        public Func<float, Matrix4x4[]>? PoseProvider { get; set; }

        /// <summary>
        /// Needed for Kh2AnimEmu
        /// </summary>
        public byte[]? MdlxBytes { get; set; }

        public Dictionary<ModelTexture.Texture, KingdomTexture> KingdomTextureCache { get; set; } = new Dictionary<ModelTexture.Texture, KingdomTexture>();

        /// <summary>
        /// From anb
        /// </summary>
        public float FramePerSecond { get; set; } = 0;
        /// <summary>
        /// From anb
        /// </summary>
        public float FrameEnd { get; set; } = 0;
    }
}

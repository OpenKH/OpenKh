using OpenKh.Engine.MonoGame;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MsetMotionEditor.Models.BoneDictSpec;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Helpers
{
    public class LoadedModel
    {
        public Bar? MdlxEntries { get; set; }
        public Bar? MsetEntries { get; set; }
        public Bar? AnbEntries { get; set; }
        public List<MdlxRenderSource> MdlxRenderableList { get; set; } = new List<MdlxRenderSource>();
        public List<MotionDisplay> MotionList { get; set; } = new List<MotionDisplay>();
        public int SelectedMotionIndex { get; set; } = -1;
        public InterpolatedMotion? MotionData { get; set; }
        public float FrameTime { get; set; }
        public Func<float, FkIkMatrices>? PoseProvider { get; set; }

        /// <summary>
        /// Needed for Kh2AnimEmu
        /// </summary>
        public byte[]? MdlxBytes { get; set; }

        public Dictionary<ModelTexture.Texture, KingdomTexture> KingdomTextureCache { get; set; } = new Dictionary<ModelTexture.Texture, KingdomTexture>();

        /// <summary>
        /// Local data, from anb
        /// </summary>
        public float FramePerSecond { get; set; } = 0;
        /// <summary>
        /// Local data, from anb
        /// </summary>
        public float FrameEnd { get; set; } = 0;
        /// <summary>
        /// Local data, from anb
        /// </summary>
        public float FrameLoop { get; set; }

        public OneTimeOn OpenMotionPlayerOnce { get; set; } = new OneTimeOn(false);

        public List<JointDescription> FKJointDescriptions { get; set; } = new List<JointDescription>();
        public List<JointDescription> IKJointDescriptions { get; set; } = new List<JointDescription>();
        public AgeManager JointDescriptionsAge { get; set; } = new AgeManager();

        public Func<IEnumerable<BoneElement>>? GetActiveFkBoneViews { get; set; }
        public Func<IEnumerable<BoneElement>>? GetActiveIkBoneViews { get; set; }

        public int SelectedJointIndex { get; set; } = -1;
        public AgeManager SelectedJointIndexAge { get; set; } = new AgeManager();

        public Dictionary<Bar.Entry, InterpolatedMotion> InterpolatedMotionCache { get; set; } = new Dictionary<Bar.Entry, InterpolatedMotion>();

        /// <summary>
        /// Bump if MotionData has been modified
        /// </summary>
        public OneTimeOn SendBackMotionData { get; set; } = new OneTimeOn(false);

        /// <summary>
        /// FK bones, read only, you cannot edit
        /// </summary>
        public List<Mdlx.Bone> InternalFkBones { get; set; } = new List<Mdlx.Bone>();

        public AgeManager Kh2PresetsAge { get; set; } = new AgeManager();

        public string? PreferredMotionExportXlsx { get; set; }
        public string? MdlxFile { get; set; }
        public string? MsetFile { get; set; }
        public string? AnbFile { get; set; }
        public int SelectFCurveKey { get; set; } = -1;
        public int SelectFCurvesFoward { get; set; } = -1;
        public int SelectFCurvesInverse { get; set; } = -1;

        public AgeManager StopAnimPlayer { get; set; } = new AgeManager();
    }
}

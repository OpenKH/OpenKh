using OpenKh.Kh2;
using System.Collections.Generic;
using static OpenKh.Kh2.Bar;

namespace OpenKh.Tools.BarEditor
{
    public class Helpers
    {
        private static readonly string DefaultExtension = "bin";

        private static readonly Dictionary<EntryType, string> SuggestedExtensions =
            new Dictionary<Bar.EntryType, string>
            {
                [EntryType.Dummy] = "dummy",
                [EntryType.Binary] = "bin",
                [EntryType.List] = "list",
                [EntryType.Btx] = "btx",
                [EntryType.Model] = "model",
                [EntryType.MeshOcclusion] = "doct",
                [EntryType.MapCollision] = "coct",
                [EntryType.ModelTexture] = "modeltexture",
                [EntryType.Dpx] = "dpx",
                [EntryType.Motion] = "motion",
                [EntryType.Texture] = "texture",
                [EntryType.CameraCollision] = "coctcamera",
                [EntryType.SpawnPoint] = "spawnpoint",
                [EntryType.SpawnScript] = "spawnscript",
                [EntryType.MapColorDiffuse] = "colordiffuse",
                [EntryType.LightData] = "coctlight",
                [EntryType.MotionTriggers] = "triggers",
                [EntryType.Anb] = "anb",
                [EntryType.Pax] = "pax",
                [EntryType.MapCollision2] = "coctmap",
                [EntryType.AnimationLimit] = "animlimit",
                [EntryType.BobDescriptor] = "bobdesc",
                [EntryType.AnimationLoader] = "animload",
                [EntryType.ModelCollision] = "coctmodel",
                [EntryType.Imgd] = "imd",
                [EntryType.Seqd] = "sqd",
                [EntryType.Layout] = "lad",
                [EntryType.Imgz] = "imz",
                [EntryType.AnimationMap] = "mapanim",
                [EntryType.Seb] = "seb",
                [EntryType.Wd] = "wd",
                [EntryType.Unknown33] = "unk33",
                [EntryType.IopVoice] = "iopvoice",
                [EntryType.RawBitmap] = "rgb",
                [EntryType.MemoryCard] = "memcard",
                [EntryType.WrappedCollisionData] = "coctwrapped",
                [EntryType.Unknown39] = "unk39",
                [EntryType.Unknown40] = "unk40",
                [EntryType.Unknown41] = "unk41",
                [EntryType.Minigame] = "minigame",
                [EntryType.JimiData] = "jimidata",
                [EntryType.Progress] = "progress",
                [EntryType.Synthesis] = "synthesis",
                [EntryType.BarUnknown] = "bar",
                [EntryType.Vibration] = "vibration",
                [EntryType.Vag] = "vag",

            };

        public static string GetSuggestedExtension(Bar.EntryType type) =>
            SuggestedExtensions.TryGetValue(type, out var ext) ? ext : DefaultExtension;
    }
}

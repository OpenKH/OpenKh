using System.Collections.Generic;

using OpenKh.Kh2;
using static OpenKh.Kh2.Bar;

namespace OpenKh.Tools.BarTool
{
    public class Helpers
    {
        private static readonly string DefaultExtension = "bin";

        private static readonly Dictionary<EntryType, string> SuggestedExtensions =
            new Dictionary<Bar.EntryType, string>
            {
                [EntryType.DUMMY] = "dummy",
                [EntryType.Binary] = "bin",
                [EntryType.List] = "list",
                [EntryType.BDX] = "bdx",
                [EntryType.Model] = "model",
                [EntryType.DrawOctalTree] = "ocd",
                [EntryType.CollisionOctalTree] = "occ",
                [EntryType.ModelTexture] = "texture",
                [EntryType.DPX] = "dpx",
                [EntryType.Motion] = "motion",
                [EntryType.PS2Image] = "tm2",
                [EntryType.CameraOctalTree] = "och",
                [EntryType.AreaDataSpawn] = "spawn",
                [EntryType.AreaDataScript] = "script",
                [EntryType.FogColor] = "fog",
                [EntryType.ColorOctalTree] = "ocl",
                [EntryType.MotionTriggers] = "triggers",
                [EntryType.AnimationBinary] = "anb",
                [EntryType.PAX] = "pax",
                [EntryType.MapCollision2] = "owa",
                [EntryType.Motionset] = "mset",
                [EntryType.BgObjPlacement] = "bop",
                [EntryType.Event] = "event",
                [EntryType.ModelCollision] = "coctmodel",
                [EntryType.ImageData] = "imd",
                [EntryType.SequenceData] = "sqd",
                [EntryType.LayoutData] = "lad",
                [EntryType.ImageZip] = "imz",
                [EntryType.AnimationMap] = "mapanim",
                [EntryType.SeBlock] = "seb",
                [EntryType.InstrumentData] = "wd",
                [EntryType.UNK33] = "unk33",
                [EntryType.IopVoice] = "iopvoice",
                [EntryType.RawBitmap] = "rgb",
                [EntryType.MemoryCard] = "memcard",
                [EntryType.WrappedCollisionData] = "coctwrapped",
                [EntryType.UNK39] = "unk39",
                [EntryType.UNK40] = "unk40",
                [EntryType.UNK41] = "unk41",
                [EntryType.Minigame] = "minigame",
                [EntryType.JiminyData] = "jimminy",
                [EntryType.Progress] = "progress",
                [EntryType.Synthesis] = "synthesis",
                [EntryType.BarUnknown] = "bar",
                [EntryType.Vibration] = "vibration",
                [EntryType.SonyADPCM] = "vag",
            };

        public static string GetSuggestedExtension(Bar.EntryType type) =>
            SuggestedExtensions.TryGetValue(type, out var ext) ? ext : DefaultExtension;
    }
}

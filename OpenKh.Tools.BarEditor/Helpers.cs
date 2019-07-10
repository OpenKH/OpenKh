using OpenKh.Kh2;
using static OpenKh.Kh2.Bar;
using System.Collections.Generic;

namespace OpenKh.Tools.BarEditor
{
    public class Helpers
    {
        private static readonly string DefaultExtension = "bin";

        private static readonly Dictionary<EntryType, string> SuggestedExtensions =
            new Dictionary<Bar.EntryType, string>
            {
                [EntryType.Binary] = "bin",
                [EntryType.Ai] = "ai",
                [EntryType.Tim2] = "tm2",
                [EntryType.SpawnPoint] = "pspwn",
                [EntryType.SpawnScript] = "sspwn",
                [EntryType.Bar] = "bar",
                [EntryType.Pax] = "pax",
                [EntryType.AnimationLoader] = "al",
                [EntryType.Imgd] = "imd",
                [EntryType.Seqd] = "2dd",
                [EntryType.Layout] = "2ld",
                [EntryType.Imgz] = "imz",
                [EntryType.Seb] = "seb",
                [EntryType.Wd] = "wd",
                [EntryType.RawBitmap] = "rgb",
                [EntryType.Vibration] = "vibration",
                [EntryType.Vag] = "vag",

            };

        public static string GetSuggestedExtension(Bar.EntryType type) =>
            SuggestedExtensions.TryGetValue(type, out var ext) ? ext : DefaultExtension;
    }
}

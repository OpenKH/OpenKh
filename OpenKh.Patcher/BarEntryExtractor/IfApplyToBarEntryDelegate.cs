using OpenKh.Kh2;

namespace OpenKh.Patcher.BarEntryExtractor
{
    public delegate bool IfApplyToBarEntryDelegate(string EntryName, Bar.EntryType EntryType, int EntryIndex);
}

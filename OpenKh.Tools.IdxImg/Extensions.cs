using OpenKh.Kh2;

namespace OpenKh.Tools.IdxImg
{
    public static class Extensions
    {
        public static string GetFullName(this Idx.Entry entry) =>
            IdxName.Lookup(entry) ?? $"@{entry.Hash32:X08}_{entry.Hash16}";
    }
}

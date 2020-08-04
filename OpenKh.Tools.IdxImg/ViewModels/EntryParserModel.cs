using OpenKh.Kh2;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    internal class EntryParserModel
    {
        internal EntryParserModel(Idx.Entry entry)
        {
            Entry = entry;
            Path = IdxName.Lookup(entry) ?? $"@{entry.Hash32:X08}_{entry.Hash16:X04}";
            SplitPath = Path.Split('/');
        }

        public Idx.Entry Entry { get; }
        public string Path { get; }
        public string[] SplitPath { get; }
        public string Name => SplitPath[^1];
        public bool IsIdx => IsLeaf(0) &&
            System.IO.Path.GetExtension(Name).ToLower() == ".idx";

        public bool IsLeaf(int index) => SplitPath.Length == index + 1;

        public static IEnumerable<EntryViewModel> GetEntries(List<EntryParserModel> entries, int depth)
        {
            var dirs = entries
                .Where(x => !x.IsLeaf(depth))
                .GroupBy(x => x.SplitPath[depth])
                .Select(x => new FolderViewModel(x.Key, depth + 1, x));
            var files =
                entries
                .Where(x => x.IsLeaf(depth) && depth == 0 && !x.IsIdx)
                .Select(x => new FileViewModel(x));
            
            var tree = dirs.Cast<EntryViewModel>().Concat(files);
            if (depth == 0)
                tree = entries
                    .Where(x => x.IsIdx)
                    .Select(x => new IdxViewModel(x.Name, x.Entry))
                    .Cast<EntryViewModel>()
                    .Concat(tree);

            return tree;
        }
    }
}

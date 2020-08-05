using OpenKh.Kh2;
using OpenKh.Tools.IdxImg.Interfaces;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.IdxImg.ViewModels
{
    internal class EntryParserModel
    {
        internal EntryParserModel(Idx.Entry entry)
        {
            Entry = entry;
            Path = entry.GetFullName();
            SplitPath = Path.Split('/');
        }

        public Idx.Entry Entry { get; }
        public string Path { get; }
        public string[] SplitPath { get; }
        public string Name => SplitPath[^1];
        public bool IsIdx => IsLeaf(0) &&
            System.IO.Path.GetExtension(Name).ToLower() == ".idx";

        public bool IsLeaf(int index) => SplitPath.Length == index + 1;

        public static IEnumerable<EntryViewModel> GetEntries(
            List<EntryParserModel> entries, int depth, IIdxManager idxManager)
        {
            var dirs = entries
                .Where(x => !x.IsLeaf(depth))
                .GroupBy(x => x.SplitPath[depth])
                .Select(x => new FolderViewModel(x.Key, depth + 1, x, idxManager));
            var files =
                entries
                .Where(x => x.IsLeaf(depth) && !x.IsIdx)
                .Select(x => new FileViewModel(x, idxManager));
            
            var tree = dirs.Cast<EntryViewModel>().Concat(files);
            if (depth == 0)
                tree = entries
                    .Where(x => x.IsIdx)
                    .Select(x => new IdxViewModel(x.Name, x.Entry, idxManager))
                    .Cast<EntryViewModel>()
                    .Concat(tree);

            return tree;
        }

        public static IEnumerable<EntryViewModel> GetChildren(
            List<Idx.Entry> idxEntries, IIdxManager idxManager)
        {
            var entries = idxEntries
                .Select(x => new EntryParserModel(x))
                .OrderBy(x => x.Path)
                .ToList();

            return GetEntries(entries, 0, idxManager);
        }
    }
}

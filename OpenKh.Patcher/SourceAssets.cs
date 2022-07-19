using OpenKh.Common;
using OpenKh.Egs;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.IO;

namespace OpenKh.Patcher
{
    public interface ISourceAssets : IDisposable
    {
        bool Exists(string path);
        Stream OpenRead(string path);
    }

    public class PlainSourceAssets : ISourceAssets
    {
        private string _basePath;

        public PlainSourceAssets(string baseDirectory)
        {
            _basePath = baseDirectory;
        }

        private string GetFullPath(string path) => Path.Combine(_basePath, path);
        public bool Exists(string path) => File.Exists(GetFullPath(path));
        public Stream OpenRead(string path) => File.OpenRead(GetFullPath(path));
        public void Dispose() { }
    }

    public class EpicGamesSourceAssets : ISourceAssets
    {
        internal record HedEntry(string FileName, Hed.Entry Entry, string PkgFilePath);

        private static readonly string[] Kh2Pkgs = new string[]
        {
            "kh2_first",
            "kh2_second",
            "kh2_third",
            "kh2_fourth",
            "kh2_fifth",
            "kh2_sixth",
        };
        private string _basePath;
        private readonly Dictionary<string, HedEntry> _entries = new();
        private readonly Dictionary<string, FileStream> _pkgStreams = new();

        public EpicGamesSourceAssets(string baseDirectory)
        {
            _basePath = Path.Combine(baseDirectory, "Image", "en");
            Kh2Pkgs.AsParallel().ForAll(baseFileName =>
            {
                var hedFilePath = Path.Combine(_basePath, $"{baseFileName}.hed");
                if (!File.Exists(hedFilePath))
                    return;

                using var hedStream = new FileStream(hedFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
                var foundEntries = Hed.Read(hedStream)
                    .Select(entry =>
                    {
                        var hash = Egs.Helpers.ToString(entry.MD5);
                        if (EgsTools.Names.TryGetValue(hash, out var fileName))
                            return (fileName, entry);
                        return (null, null);
                    })
                    .ToList();

                lock (_entries)
                {
                    foreach (var entry in foundEntries)
                        _entries[entry.fileName] = new HedEntry(
                            entry.fileName,
                            entry.entry,
                            baseFileName
                        );
                }
            });

            _pkgStreams = Kh2Pkgs.ToDictionary(x => x, x => File.OpenRead(Path.Combine(_basePath, $"{x}.pkg")));
        }

        private string NormalizePath(string path) => path;
        public bool Exists(string path) => _entries.ContainsKey(NormalizePath(path));
        public Stream OpenRead(string path)
        {
            var entry = _entries[NormalizePath(path)];
            lock (_pkgStreams[entry.PkgFilePath])
            {
                var hdAsset = new EgsHdAsset(_pkgStreams[entry.PkgFilePath].SetPosition(entry.Entry.Offset));
                return new MemoryStream(hdAsset.OriginalData);
            }
        }
        public void Dispose()
        {
            foreach (var stream in _pkgStreams)
                stream.Value.Dispose();
        }
    }
}

using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Egs
{
    public class EgsHdAsset
    {
        private class Header
        {
            [Data] public int DecompressedLength { get; set; }
            [Data] public int RemasteredAssetCount { get; set; }
            [Data] public int CompressedLength { get; set; }
            [Data] public int Unknown0c { get; set; }
        }

        private class RemasteredEntry
        {
            [Data(Count = 0x20)] public string Name { get; set; }
            [Data] public int Offset { get; set; }
            [Data] public int Unknown24 { get; set; }
            [Data] public int CompressedLength { get; set; }
            [Data] public int DecompressedLength { get; set; }
        }

        private readonly Stream _stream;
        private readonly Header _header;
        private readonly byte[] _seed;
        private readonly long _baseOffset;
        private readonly long _dataOffset;
        private readonly Dictionary<string, RemasteredEntry> _entries;

        public static string[] MareNames = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "resources/mare.txt"));
        public static string[] SettingMenuNames = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "resources/settingmenu.txt"));
        public static string[] TheaterNames = File.ReadAllLines(Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "resources/theater.txt"));

        public EgsHdAsset(Stream stream)
        {
            _stream = stream;
            _baseOffset = stream.Position;
            _seed = stream.ReadBytes(0x10);
            _header = BinaryMapping.ReadObject<Header>(new MemoryStream(_seed));
            var entries = Enumerable
                .Range(0, _header.RemasteredAssetCount)
                .Select(_ => BinaryMapping.ReadObject<RemasteredEntry>(stream))
                .ToList();
            _entries = entries.ToDictionary(x => x.Name, x => x);
            Assets = entries.Select(x => x.Name).ToArray();
            _dataOffset = stream.Position;
        }

        public string[] Assets { get; }

        public byte[] ReadAsset(string assetName)
        {
            var entry = _entries[assetName];
            return null;
        }

        public byte[] ReadData()
        {
            const int PassCount = 10;
            var dataLength = _header.CompressedLength >= 0 ? _header.CompressedLength : _header.DecompressedLength;
            var key = EgsEncryption.GenerateKey(_seed, PassCount);
            var data = _stream.SetPosition(_dataOffset).ReadBytes(dataLength);
            for (var i = 0; i < Math.Min(dataLength, 0x100); i += 0x10)
                EgsEncryption.DecryptChunk(key, data, i, PassCount);

            if (_header.CompressedLength >= 0)
            {
                using var compressedStream = new MemoryStream(data);
                using var deflate = new DeflateStream(compressedStream.SetPosition(2), CompressionMode.Decompress);

                var decompressedData = new byte[_header.DecompressedLength];
                deflate.Read(decompressedData);

                return decompressedData;
            }

            return data;
        }
    }
}

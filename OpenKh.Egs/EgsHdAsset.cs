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
        public class Header
        {
            [Data] public int DecompressedLength { get; set; }
            [Data] public int RemasteredAssetCount { get; set; }
            // Original data's compressed length => -2: no compression and encryption, -1: no compression, > 0: compressed size
            [Data] public int CompressedLength { get; set; }
            [Data] public int CreationDate { get; set; }
        }

        public class RemasteredEntry
        {
            [Data(Count = 0x20)] public string Name { get; set; }
            // The offset is relative to: Original asset's header size + all remastered asset's header size + original asset's decompressed data length
            [Data] public int Offset { get; set; }
            [Data] public int Unknown24 { get; set; }
            [Data] public int DecompressedLength { get; set; }
            [Data] public int CompressedLength { get; set; }
        }

        private static readonly string ResourcePath = Path.Combine(Path.GetDirectoryName(AppContext.BaseDirectory), "resources");
        public static string[] DddNames = File.ReadAllLines(Path.Combine(ResourcePath, "ddd.txt"));
        public static string[] MareNames = File.ReadAllLines(Path.Combine(ResourcePath, "mare.txt"));
        public static string[] SettingMenuNames = File.ReadAllLines(Path.Combine(ResourcePath, "settingmenu.txt"));
        public static string[] TheaterNames = File.ReadAllLines(Path.Combine(ResourcePath, "theater.txt"));
        public static string[] RecomNames = File.ReadAllLines(Path.Combine(ResourcePath, "recom.txt"));
        public static string[] BbsNames = File.ReadAllLines(Path.Combine(ResourcePath, "bbs.txt"));
        public static string[] Kh1AdditionalNames = File.ReadAllLines(Path.Combine(ResourcePath, "kh1pc.txt"));
        public static string[] Launcher28Names = File.ReadAllLines(Path.Combine(ResourcePath, "launcher28.txt"));

        private const int PASS_COUNT = 10;

        private readonly Stream _stream;
        private readonly Header _header;
        private readonly byte[] _key;
        private readonly byte[] _seed;
        private readonly long _baseOffset;
        private readonly long _dataOffset;
        private readonly Dictionary<string, RemasteredEntry> _entries;
        private byte[] _originalData;
        private byte[] _originalRawData;
        private readonly Dictionary<string, byte[]> _remasteredAssetsData = new Dictionary<string, byte[]>();
        private readonly Dictionary<string, byte[]> _remasteredAssetsRawData = new Dictionary<string, byte[]>();

        public string[] Assets { get; }
        public byte[] Seed => _seed;
        public Header OriginalAssetHeader => _header;
        public Dictionary<string, RemasteredEntry> RemasteredAssetHeaders => _entries;
        public byte[] OriginalData => _originalData;
        public byte[] OriginalRawData => _originalRawData;
        public Dictionary<string, byte[]> RemasteredAssetsDecompressedData => _remasteredAssetsData;
        public Dictionary<string, byte[]> RemasteredAssetsCompressedData => _remasteredAssetsRawData;

        public EgsHdAsset(Stream stream)
        {
            _stream = stream;
            _baseOffset = stream.Position;

            _seed = stream.ReadBytes(0x10);
            _key = EgsEncryption.GenerateKey(_seed, PASS_COUNT);

            _header = BinaryMapping.ReadObject<Header>(new MemoryStream(_seed));

            var entries = Enumerable
                .Range(0, _header.RemasteredAssetCount)
                .Select(_ => BinaryMapping.ReadObject<RemasteredEntry>(stream))
                .ToList();

            _entries = entries.ToDictionary(x => x.Name, x => x);
            _dataOffset = stream.Position;

            Assets = entries.Select(x => x.Name).ToArray();

            ReadData();

            foreach (var remasteredAssetName in Assets)
            {
                ReadRemasteredAsset(remasteredAssetName);
            }

            stream.SetPosition(_dataOffset);
        }

        private byte[] ReadRemasteredAsset(string assetName)
        {
            var header = _entries[assetName];
            var dataLength = header.CompressedLength >= 0 ? header.CompressedLength : header.DecompressedLength;

            if (dataLength % 16 != 0)
                dataLength += 16 - (dataLength % 16);

            var data = _stream.AlignPosition(0x10).ReadBytes(dataLength);

            _remasteredAssetsRawData.Add(assetName, data.ToArray());

            if (header.CompressedLength > -2)
            {
                for (var i = 0; i < Math.Min(dataLength, 0x100); i += 0x10)
                    EgsEncryption.DecryptChunk(_key, data, i, PASS_COUNT);
            }

            if (header.CompressedLength > -1)
            {
                using var compressedStream = new MemoryStream(data);
                using var deflate = new DeflateStream(compressedStream.SetPosition(2), CompressionMode.Decompress);

                var decompressedData = new byte[header.DecompressedLength];
                deflate.Read(decompressedData);

                data = decompressedData;
            }

            _remasteredAssetsData.Add(assetName, data.ToArray());

            return data;
        }

        private byte[] ReadData()
        {
            var dataLength = _header.CompressedLength >= 0 ? _header.CompressedLength : _header.DecompressedLength;
            var data = _stream.SetPosition(_dataOffset).ReadBytes(dataLength);

            _originalRawData = data.ToArray();

            if (_header.CompressedLength > -2)
            {
                for (var i = 0; i < Math.Min(dataLength, 0x100); i += 0x10)
                    EgsEncryption.DecryptChunk(_key, data, i, PASS_COUNT);
            }

            if (_header.CompressedLength > -1)
            {
                using var compressedStream = new MemoryStream(data);
                using var deflate = new DeflateStream(compressedStream.SetPosition(2), CompressionMode.Decompress);

                var decompressedData = new byte[_header.DecompressedLength];
                deflate.Read(decompressedData);

                data = decompressedData;
            }

            _originalData = data.ToArray();

            return data;
        }
    }
}

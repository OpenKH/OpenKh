using OpenKh.Common;
using OpenKh.Imaging;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Recom
{
    public class Rtm
    {
        private interface IHeaderEntry
        {
            string Name { get; set; }
            uint Offset { get; set; }
            uint Length { get; set; }
            ushort Flags { get; set; }
            uint Reserved1 { get; set; }
            ushort Reserved2 { get; set; }
        }

        private class FinalHeaderEntry : IHeaderEntry
        {
            [Data] public uint Offset { get; set; }
            [Data(Count = 16)] public string Name { get; set; }
            [Data] public uint Reserved1 { get; set; }
            [Data] public ushort Reserved2 { get; set; }
            [Data] public ushort Flags { get; set; }
            [Data] public uint Length { get; set; }
        }

        private class BetaHeaderEntry : IHeaderEntry
        {
            [Data(Count = 16)] public string Name { get; set; }
            [Data] public uint Offset { get; set; }
            [Data] public ushort Flags { get; set; }
            [Data] public ushort Reserved2 { get; set; }
            [Data] public uint Reserved1 { get; set; }
            [Data] public uint Length { get; set; }
        }

        public string Name { get; set; }
        public List<Tm2> Textures { get; set; }

        private static IEnumerable<IHeaderEntry> ReadHeader<THeaderEntry>(Stream stream)
            where THeaderEntry : class, IHeaderEntry
        {
            while (true)
            {
                var entry = BinaryMapping.ReadObject<THeaderEntry>(stream);
                if (entry.Name.Length == 0)
                    yield break;
                yield return entry;
            }
        }

        public static List<Rtm> Read(Stream stream)
        {
            stream.MustReadAndSeek();
            var basePosition = stream.Position;
            var textureEntries = ReadHeader<FinalHeaderEntry>(stream).ToList();
            if (textureEntries.Count > 0)
            {
                if (textureEntries[0].Reserved1 != 0 ||
                    textureEntries[0].Reserved2 != 0 ||
                    textureEntries[0].Flags != 0x0102)
                    textureEntries = ReadHeader<BetaHeaderEntry>(stream.SetPosition(basePosition)).ToList();
            }

            return textureEntries
                .Select(x =>
                {
                    stream.SetPosition(basePosition + (x.Offset & 0x7FFFFFFFU));
                    return new Rtm
                    {
                        Name = x.Name,
                        Textures = Tm2.Read(stream).ToList()
                    };
                })
                .ToList();
        }

        public static void Write(Stream stream, ICollection<Rtm> textures)
        {
            stream.MustWriteAndSeek();

            var headerEntries = new List<FinalHeaderEntry>();
            var basePosition = stream.Position;
            stream.Position += textures.Count * 0x20 + 0x20;
            foreach (var texture in textures)
            {
                var prevPosition = stream.Position;
                Tm2.Write(stream, texture.Textures);
                headerEntries.Add(new FinalHeaderEntry
                {
                    Name = texture.Name,
                    Flags = 0x0102,
                    Length = (uint)(stream.Position - prevPosition - basePosition) | 0x80000000U,
                    Offset = (uint)(prevPosition - basePosition)
                });
            }
            headerEntries.Add(new FinalHeaderEntry
            {
                Name = string.Empty
            });

            var finalPosition = stream.Position;

            stream.SetPosition(basePosition);
            foreach (var entry in headerEntries)
                BinaryMapping.WriteObject(stream, entry);

            stream.SetPosition(finalPosition);
        }
    }
}

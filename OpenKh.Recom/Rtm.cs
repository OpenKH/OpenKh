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
        private class HeaderEntry
        {
            [Data(Count = 20)] public string Name { get; set; }
            [Data] public uint Flags { get; set; }
            [Data] public uint Length { get; set; }
            [Data] public uint NextTextureOffset { get; set; }
        }

        public string Name { get; set; }
        public List<Tm2> Textures { get; set; }

        public static List<Rtm> Read(Stream stream)
        {
            stream.MustReadAndSeek();
            var basePosition = stream.Position;
            var headerLength = stream.ReadUInt32();
            var textureEntries = new List<HeaderEntry>(16);
            var canContinue = headerLength >= 0x40;
            while (canContinue)
            {
                var headerEntry = BinaryMapping.ReadObject<HeaderEntry>(stream);
                textureEntries.Add(headerEntry);
                canContinue = headerEntry.NextTextureOffset > 0;
            }

            var nextTextureOffset = headerLength;
            return textureEntries
                .Select(x =>
                {
                    stream.SetPosition(basePosition + nextTextureOffset);
                    nextTextureOffset = x.NextTextureOffset & 0x7FFFFFFFU;
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

            var headerSize = textures.Count * 0x20 + 0x20;
            var headerEntries = new List<HeaderEntry>();
            var basePosition = stream.Position;
            stream.Position += headerSize;

            foreach (var texture in textures)
            {
                var prevPosition = stream.Position;
                Tm2.Write(stream, texture.Textures);

                headerEntries.Add(new HeaderEntry
                {
                    Name = texture.Name,
                    Flags = 0x1020000,
                    Length = (uint)(stream.Position - prevPosition - basePosition) | 0x80000000U,
                    NextTextureOffset = (uint)(stream.Position - basePosition)
                });
            }

            if (headerEntries.Count > 0)
                headerEntries[^1].NextTextureOffset = 0;

            var finalPosition = stream.Position;

            stream.SetPosition(basePosition);
            stream.Write(headerSize);
            foreach (var entry in headerEntries)
                BinaryMapping.WriteObject(stream, entry);

            stream.SetPosition(finalPosition);
        }
    }
}

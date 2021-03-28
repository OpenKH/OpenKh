using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class FontCharacterInfo
    {
        [Data] public ushort Id { get; set; }
        [Data] public ushort PositionX { get; set; }
        [Data] public ushort PositionY { get; set; }
        [Data] public byte Palette { get; set; }
        [Data] public byte Width { get; set; }

        public static FontCharacterInfo[] Read(Stream stream)
        {
            var characterCount = (int)stream.Length / 8;
            var charactersInfo = new FontCharacterInfo[characterCount];
            stream.Position = 0;

            for (var i = 0; i < characterCount; i++)
                charactersInfo[i] = BinaryMapping.ReadObject<FontCharacterInfo>(stream);

            return charactersInfo;
        }

        public static void Write(Stream stream, IEnumerable<FontCharacterInfo> charactersInfo)
        {
            stream.Position = 0;
            foreach (var info in charactersInfo)
                BinaryMapping.WriteObject(stream, info);

            stream.SetLength(stream.Position);
        }
    }
}

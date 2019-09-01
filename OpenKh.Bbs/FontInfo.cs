using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class FontInfo
    {
        [Data] public short CharacterCount { get; set; }
        [Data] public short ImageWidth { get; set; }
        [Data] public short MaxImageHeight { get; set; }
        [Data] public byte CharacterWidth { get; set; }
        [Data] public byte CharacterHeight { get; set; }

        public static FontInfo Read(Stream stream) =>
            BinaryMapping.ReadObject<FontInfo>(stream);

        public void Write(Stream stream) =>
            BinaryMapping.WriteObject(stream, this);
    }
}

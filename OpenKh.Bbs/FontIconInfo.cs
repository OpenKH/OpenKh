using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Bbs
{
    public class FontIconInfo
    {
        public ushort Key { get; set; }
        public byte Left { get; set; }
        public byte Top { get; set; }
        public byte Right { get; set; }
        public byte Bottom { get; set; }

        private class MetaIcon
        {
            [Data] public ushort Key { get; set; }
            [Data] public byte Left { get; set; }
            [Data] public byte Top { get; set; }
            [Data] public byte Right { get; set; }
            [Data] public byte Bottom { get; set; }
            [Data] public short RESERVED { get; set; }
        }

        public static IEnumerable<FontIconInfo> Read(Stream stream)
        {
            var count = stream.ReadInt32();
            return Enumerable.Range(0, count)
                .Select(_ => BinaryMapping.ReadObject<MetaIcon>(stream))
                .Select(x => new FontIconInfo
                {
                    Key = x.Key,
                    Left = x.Left,
                    Top = x.Top,
                    Right = x.Right,
                    Bottom = x.Bottom
                })
                .ToArray();
        }

        public static void Write(Stream stream, IEnumerable<FontIconInfo> fontIconsInfo)
        {
            var myFontIconsInfo = fontIconsInfo.ToArray();
            stream.Write(myFontIconsInfo.Length);
            foreach (var info in myFontIconsInfo)
                BinaryMapping.WriteObject(stream, new MetaIcon
                {
                    Key = info.Key,
                    Left = info.Left,
                    Top = info.Top,
                    Right = info.Right,
                    Bottom = info.Bottom,
                    RESERVED = 0
                });
        }
    }
}

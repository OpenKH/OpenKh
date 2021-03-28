using OpenKh.Imaging;
using OpenKh.Kh2.SystemData;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Kh2.Contextes
{
    public class FontContext
    {
        private IImageRead imageSystem;
        private IImageRead imageSystem2;
        private IImageRead imageEvent;
        private IImageRead imageEvent2;
        private IImageRead imageIcon;
        private byte[] spacingSystem;
        private byte[] spacingEvent;
        private byte[] spacingIcon;

        public IImageRead ImageSystem { get => imageSystem; set => imageSystem = value; }
        public IImageRead ImageSystem2 { get => imageSystem2; set => imageSystem2 = value; }
        public IImageRead ImageEvent { get => imageEvent; set => imageEvent = value; }
        public IImageRead ImageEvent2 { get => imageEvent2; set => imageEvent2 = value; }
        public IImageRead ImageIcon { get => imageIcon; set => imageIcon = value; }

        public byte[] SpacingSystem { get => spacingSystem; set => spacingSystem = value; }
        public byte[] SpacingEvent { get => spacingEvent; set => spacingEvent = value; }
        public byte[] SpacingIcon { get => spacingIcon; set => spacingIcon = value; }

        public Dictionary<int, Ftst.Entry> palette;

        public void Read(IEnumerable<Bar.Entry> entries)
        {
            foreach (var entry in entries)
            {
                switch (entry.Name)
                {
                    case "sys":
                        ReadFont(entry, ref imageSystem, ref imageSystem2, ref spacingSystem);
                        break;
                    case "evt":
                        ReadFont(entry, ref imageEvent, ref imageEvent2, ref spacingEvent);
                        break;
                    case "icon":
                        ReadIcon(entry, ref imageIcon, ref spacingIcon, 256, 160);
                        break;
                    case "ftst":
                        if (entry.Type == Bar.EntryType.List)
                            palette = Ftst.Read(entry.Stream).ToDictionary(x => x.Id, x => x);
                        break;
                }
            }
        }

        private void ReadFont(Bar.Entry entry, ref IImageRead image1, ref IImageRead image2, ref byte[] spacing)
        {
            switch (entry.Type)
            {
                case Bar.EntryType.List:
                    spacing = ReadSpacing(entry);
                    break;
                case Bar.EntryType.RawBitmap:
                    entry.Stream.Position = 0;
                    image1 = ReadImagePalette1(entry);
                    entry.Stream.Position = 0;
                    image2 = ReadImagePalette2(entry);
                    break;
            }
        }

        private void ReadIcon(Bar.Entry entry, ref IImageRead image, ref byte[] spacing, int width, int height)
        {
            switch (entry.Type)
            {
                case Bar.EntryType.List:
                    spacing = ReadSpacing(entry);
                    break;
                case Bar.EntryType.RawBitmap:
                    entry.Stream.Position = 0;
                    image = ReadImage8bit(entry, width, height);
                    break;
            }
        }

        private static byte[] ReadSpacing(Bar.Entry entry)
        {
            return new BinaryReader(entry.Stream).ReadBytes((int)entry.Stream.Length);
        }

        private static IImageRead ReadImage8bit(Bar.Entry entry, int width, int height)
        {
            return RawBitmap.Read8bit(entry.Stream, width, height);
        }

        private static IImageRead ReadImagePalette1(Bar.Entry entry)
        {
            DeductImageSize((int)entry.Stream.Length, out var width, out var height);
            return RawBitmap.Read4bitPalette1(entry.Stream, width, height);
        }

        private static IImageRead ReadImagePalette2(Bar.Entry entry)
        {
            DeductImageSize((int)entry.Stream.Length, out var width, out var height);
            return RawBitmap.Read4bitPalette2(entry.Stream, width, height);
        }

        private static bool DeductImageSize(int rawLength, out int width, out int height)
        {
            if (rawLength < 128 * 1024)
            {
                width = 512;
                height = 256;
            }
            else if (rawLength < 256 * 1024)
            {
                width = 512;
                height = 512;
            }
            else if (rawLength < 512 * 1024)
            {
                width = 512;
                height = 1024;
            }
            else
            {
                width = 0;
                height = 0;
                return false;
            }

            return true;
        }
    }
}

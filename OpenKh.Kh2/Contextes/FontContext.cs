using OpenKh.Imaging;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Kh2.Contextes
{
    public class FontContext
    {
        private IImageRead imageSystem;
        private IImageRead imageEvent;
        private IImageRead imageIcon;
        private byte[] spacingSystem;
        private byte[] spacingEvent;
        private byte[] spacingIcon;

        public IImageRead ImageSystem { get => imageSystem; set => imageSystem = value; }
        public IImageRead ImageEvent { get => imageEvent; set => imageEvent = value; }
        public IImageRead ImageIcon { get => imageIcon; set => imageIcon = value; }

        public byte[] SpacingSystem { get => spacingSystem; set => spacingSystem = value; }
        public byte[] SpacingEvent { get => spacingEvent; set => spacingEvent = value; }
        public byte[] SpacingIcon { get => spacingIcon; set => spacingIcon = value; }

        public void Read(IEnumerable<Bar.Entry> entries)
        {
            foreach (var entry in entries)
            {
                switch (entry.Name)
                {
                    case "sys":
                        Read(entry, ref imageSystem, ref spacingSystem, false);
                        break;
                    case "evt":
                        Read(entry, ref imageEvent, ref spacingEvent, false);
                        break;
                    case "icon":
                        Read(entry, ref imageIcon, ref spacingIcon, 256, 160, true);
                        break;
                }
            }
        }

        private void Read(Bar.Entry entry, ref IImageRead image, ref byte[] spacing, bool is8bit)
        {
            switch (entry.Type)
            {
                case Bar.EntryType.Msg:
                    spacing = ReadSpacing(entry);
                    break;
                case Bar.EntryType.RawBitmap:
                    image = ReadImage(entry, is8bit);
                    break;
            }
        }

        private void Read(Bar.Entry entry, ref IImageRead image, ref byte[] spacing, int width, int height, bool is8bit)
        {
            switch (entry.Type)
            {
                case Bar.EntryType.Msg:
                    spacing = ReadSpacing(entry);
                    break;
                case Bar.EntryType.RawBitmap:
                    image = ReadImage(entry, width, height, is8bit);
                    break;
            }
        }

        private static byte[] ReadSpacing(Bar.Entry entry)
        {
            return new BinaryReader(entry.Stream).ReadBytes((int)entry.Stream.Length);
        }

        private static IImageRead ReadImage(Bar.Entry entry, bool is8bit)
        {
            DeductImageSize((int)entry.Stream.Length, out var width, out var height);
            return ReadImage(entry, width, height, is8bit);
        }

        private static IImageRead ReadImage(Bar.Entry entry, int width, int height, bool is8bit)
        {
            return RawBitmap.Read(entry.Stream, width, height, is8bit);
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

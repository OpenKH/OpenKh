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
                        Read(entry, ref imageSystem, ref spacingSystem, 512, 256, false);
                        break;
                    case "evt":
                        Read(entry, ref imageEvent, ref spacingEvent, 512, 512, false);
                        break;
                    case "icon":
                        Read(entry, ref imageIcon, ref spacingIcon, 256, 160, true);
                        break;
                }
            }
        }

        private void Read(Bar.Entry entry, ref IImageRead image, ref byte[] spacing, int width, int height, bool is8bit)
        {
            switch (entry.Type)
            {
                case Bar.EntryType.Msg:
                    spacing = new BinaryReader(entry.Stream).ReadBytes((int)entry.Stream.Length);
                    break;
                case Bar.EntryType.RawBitmap:
                    image = RawBitmap.Read(entry.Stream, width, height, is8bit);
                    break;
            }
        }
    }
}

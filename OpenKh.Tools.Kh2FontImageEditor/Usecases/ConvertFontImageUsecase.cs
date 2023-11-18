using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.FontEditing;
using OpenKh.Tools.Kh2FontImageEditor.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2FontImageEditor.Usecases
{
    public class ConvertFontImageUsecase
    {
        public IEnumerable<Bar.Entry> Encode(FontImageData fontImageData)
        {
            var newBar = new List<Bar.Entry>();

            if (fontImageData.ImageIcon is IImageRead icon1)
            {
                var font = FontIconBitmap.WriteIconBitmap(icon1);
                newBar.Add(new Bar.Entry { Name = "icon", Type = Bar.EntryType.RawBitmap, Stream = new MemoryStream(font), });
            }

            if (true
                && fontImageData.ImageEvent is IImageRead evt1
                && fontImageData.ImageEvent2 is IImageRead evt2
            )
            {
                var font = FontImageBitmap.WriteFont(evt1, evt2);
                newBar.Add(new Bar.Entry { Name = "evt", Type = Bar.EntryType.RawBitmap, Stream = new MemoryStream(font), });
            }

            if (true
                && fontImageData.ImageSystem is IImageRead sys1
                && fontImageData.ImageSystem2 is IImageRead sys2
            )
            {
                var font = FontImageBitmap.WriteFont(sys1, sys2);
                newBar.Add(new Bar.Entry { Name = "sys", Type = Bar.EntryType.RawBitmap, Stream = new MemoryStream(font), });
            }

            return newBar.AsReadOnly();
        }

        public FontImageData Decode(IEnumerable<Bar.Entry> fontImageBar)
        {
            IImageRead? sys1 = null;
            IImageRead? sys2 = null;

            if (fontImageBar.SingleOrDefault(it => it.Name == "sys" && it.Type == Bar.EntryType.RawBitmap) is Bar.Entry sys)
            {
                var body = sys.Stream.FromBegin().ReadBytes();
                sys1 = new FontImageBitmap(body, true);
                sys2 = new FontImageBitmap(body, false);
            }

            IImageRead? evt1 = null;
            IImageRead? evt2 = null;

            if (fontImageBar.SingleOrDefault(it => it.Name == "evt" && it.Type == Bar.EntryType.RawBitmap) is Bar.Entry evt)
            {
                var body = evt.Stream.FromBegin().ReadBytes();
                evt1 = new FontImageBitmap(body, true);
                evt2 = new FontImageBitmap(body, false);
            }

            IImageRead? icon1 = null;

            if (fontImageBar.SingleOrDefault(it => it.Name == "icon" && it.Type == Bar.EntryType.RawBitmap) is Bar.Entry icon)
            {
                icon1 = RawBitmap.Read8bit(icon.Stream.FromBegin(), 256, 160);
            }

            return new FontImageData(sys1, sys2, evt1, evt2, icon1);
        }
    }
}

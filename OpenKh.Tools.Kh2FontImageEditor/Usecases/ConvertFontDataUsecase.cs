using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2FontImageEditor.Helpers;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2FontImageEditor.Usecases
{
    public class ConvertFontDataUsecase
    {
        public FontInfoData Decode(IEnumerable<Bar.Entry> fontInfoBar)
        {
            byte[]? sys = null;
            byte[]? evt = null;
            byte[]? icon = null;

            if (fontInfoBar.SingleOrDefault(it => it.Name == "sys" && it.Type == Bar.EntryType.List) is Bar.Entry sysEntry)
            {
                sys = sysEntry.Stream.FromBegin().ReadBytes();
            }

            if (fontInfoBar.SingleOrDefault(it => it.Name == "evt" && it.Type == Bar.EntryType.List) is Bar.Entry evtEntry)
            {
                evt = evtEntry.Stream.FromBegin().ReadBytes();
            }

            if (fontInfoBar.SingleOrDefault(it => it.Name == "icon" && it.Type == Bar.EntryType.List) is Bar.Entry iconEntry)
            {
                icon = iconEntry.Stream.FromBegin().ReadBytes();
            }

            return new FontInfoData(sys, evt, icon);
        }

        public IEnumerable<Bar.Entry> Encode(FontInfoData fontInfoData)
        {
            var newBar = new List<Bar.Entry>();

            if (fontInfoData.System is byte[] sys)
            {
                newBar.Add(new Bar.Entry { Name = "sys", Type = Bar.EntryType.List, Stream = new MemoryStream(sys) });
            }

            if (fontInfoData.Event is byte[] evt)
            {
                newBar.Add(new Bar.Entry { Name = "evt", Type = Bar.EntryType.List, Stream = new MemoryStream(evt) });
            }

            if (fontInfoData.Icon is byte[] icon)
            {
                newBar.Add(new Bar.Entry { Name = "icon", Type = Bar.EntryType.List, Stream = new MemoryStream(icon) });
            }

            return newBar.AsReadOnly();
        }
    }
}

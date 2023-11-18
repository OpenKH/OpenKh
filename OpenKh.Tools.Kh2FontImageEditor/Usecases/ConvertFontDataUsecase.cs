using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2FontImageEditor.Helpers;
using System;
using System.Collections.Generic;
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
    }
}

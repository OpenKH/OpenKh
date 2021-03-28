using OpenKh.Command.TexFooter.Interfaces;
using OpenKh.Imaging;
using OpenKh.Kh2.TextureFooter;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKh.Command.TexFooter.Models
{
    public class TextureFooterDataIMEx
    {
        public List<UvScroll> UvscList { get; set; }
        public List<TextureAnimationIMEx> TextureAnimationList { get; set; }
        public byte[] UnkFooter { get; set; }
        public bool ShouldEmitDMYAtFirst { get; set; }
        public bool ShouldEmitKN5 { get; set; }

        public TextureFooterDataIMEx()
        {

        }

        public TextureFooterDataIMEx(TextureFooterData footerData)
        {
            UvscList = footerData.UvscList.ToList();
            TextureAnimationList = footerData.TextureAnimationList.Select(it => new TextureAnimationIMEx(it)).ToList();
            UnkFooter = footerData.UnkFooter;
            ShouldEmitDMYAtFirst = footerData.ShouldEmitDMYAtFirst;
            ShouldEmitKN5 = footerData.ShouldEmitKN5;
        }

        public TextureFooterData ConvertBackTo(Func<string, ISpriteImageSource> imageLoader, TextureFooterData back)
        {
            back.UvscList.Clear();
            back.UvscList.AddRange(UvscList);
            back.TextureAnimationList.Clear();
            back.TextureAnimationList.AddRange(
                TextureAnimationList
                    .Select(texa => texa.ConvertBack(imageLoader))
            );
            back.UnkFooter = UnkFooter;
            back.ShouldEmitDMYAtFirst = ShouldEmitDMYAtFirst;
            back.ShouldEmitKN5 = ShouldEmitKN5;
            return back;
        }
    }
}

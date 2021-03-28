using OpenKh.Kh2.TextureFooter;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Text;
using YamlDotNet.Core;
using YamlDotNet.Serialization;
using OpenKh.Imaging;
using OpenKh.Command.TexFooter.Interfaces;

namespace OpenKh.Command.TexFooter.Models
{
    public class TextureAnimationIMEx
    {
        public ushort Unk1 { get; set; }
        public ushort TextureIndex { get; set; }
        public ushort FrameStride { get; set; }
        public ushort BaseSlotIndex { get; set; }

        public string SpriteImageFile { get; set; }
        public ushort NumSpritesInImageData { get; set; }
        public ushort UOffsetInBaseImage { get; set; }
        public ushort VOffsetInBaseImage { get; set; }

        public int DefaultAnimationIndex { get; set; }

        public short[] SlotTable { get; set; }
        public TextureFrameGroup[] FrameGroupList { get; set; }

        internal TextureAnimation _source;

        public TextureAnimationIMEx()
        {

        }

        public TextureAnimationIMEx(TextureAnimation src)
        {
            _source = src;

            Unk1 = src.Unk1;
            TextureIndex = src.TextureIndex;
            FrameStride = src.FrameStride;
            BaseSlotIndex = src.BaseSlotIndex;
            NumSpritesInImageData = src.NumSpritesInImageData;
            UOffsetInBaseImage = src.UOffsetInBaseImage;
            VOffsetInBaseImage = src.VOffsetInBaseImage;
            DefaultAnimationIndex = src.DefaultAnimationIndex;
            SlotTable = src.SlotTable;
            FrameGroupList = src.FrameGroupList;
        }

        public TextureAnimation ConvertBack(Func<string, ISpriteImageSource> imageLoader)
        {
            var spriteImage = imageLoader(SpriteImageFile);
            var bitsPerPixel = spriteImage.BitsPerPixel;

            var back = new TextureAnimation
            {
                Unk1 = Unk1,
                TextureIndex = TextureIndex,
                FrameStride = FrameStride,
                BitsPerPixel = Convert.ToUInt16(bitsPerPixel),
                BaseSlotIndex = BaseSlotIndex,
                MaximumSlotIndex = Convert.ToUInt16(BaseSlotIndex + SlotTable.Length - 1),

                NumAnimations = Convert.ToUInt16(FrameGroupList.Length),
                NumSpritesInImageData = NumSpritesInImageData,
                UOffsetInBaseImage = UOffsetInBaseImage,
                VOffsetInBaseImage = VOffsetInBaseImage,
                SpriteWidth = Convert.ToUInt16(spriteImage.Size.Width),
                SpriteHeight = Convert.ToUInt16(spriteImage.Size.Height / NumSpritesInImageData),

                OffsetSlotTable = 0,

                OffsetAnimationTable = 0,

                OffsetSpriteImage = 0,
                DefaultAnimationIndex = DefaultAnimationIndex,

                SlotTable = SlotTable,
                FrameGroupList = FrameGroupList,
                SpriteImage = spriteImage.Data,
            };

            return back;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.TextureFooter
{
    public class TextureAnimation
    {
        [Data] public ushort Unk1 { get; set; }
        [Data] public ushort TextureIndex { get; set; }
        [Data] public ushort FrameStride { get; set; }
        [Data] public ushort BitsPerPixel { get; set; }
        [Data] public ushort BaseSlotIndex { get; set; }
        [Data] public ushort MaximumSlotIndex { get; set; }

        /// <remarks>This field may be updated by serialization.</remarks>
        [Data] public ushort NumAnimations { get; set; }
        [Data] public ushort NumSpritesInImageData { get; set; }
        [Data] public ushort UOffsetInBaseImage { get; set; }
        [Data] public ushort VOffsetInBaseImage { get; set; }
        [Data] public ushort SpriteWidth { get; set; }
        [Data] public ushort SpriteHeight { get; set; }

        /// <remarks>This field may be updated by serialization.</remarks>
        [Data] public int OffsetSlotTable { get; set; }

        /// <remarks>This field may be updated by serialization.</remarks>
        [Data] public int OffsetAnimationTable { get; set; }

        /// <remarks>This field may be updated by serialization.</remarks>
        [Data] public int OffsetSpriteImage { get; set; }
        [Data] public int DefaultAnimationIndex { get; set; }

        public short[] SlotTable { get; set; } = new short[0];
        public TextureFrameGroup[] FrameGroupList { get; set; } = new TextureFrameGroup[0];
        public byte[] SpriteImage { get; set; } = new byte[0];
        public int SpriteStride => (BitsPerPixel == 4) ? (SpriteWidth + 1) / 2
            : (BitsPerPixel == 8) ? SpriteWidth
            : throw new NotSupportedException();
    }
}

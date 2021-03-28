using OpenKh.Common.Utils;
using System;
using System.Collections.Generic;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.TextureFooter
{
    public class TextureFrame
    {
        [Data] public ushort Data { get; set; }
        [Data] public ushort MinimumLength { get; set; }
        [Data] public ushort MaximumLength { get; set; }
        [Data] public ushort SpriteImageIndex { get; set; }

        public TextureFrameControl FrameControl
        {
            get => (TextureFrameControl)(Data & 15);
            set => Data = (ushort)BitsUtil.Int.SetBits(Data, 0, 4, (int)value);
        }

        public override string ToString() => $"{FrameControl}({FrameIndexDelta}), {MinimumLength}~{MaximumLength}, Sprite:{SpriteImageIndex}";

        /// <summary>
        /// Available if JumpFrame is set. NextFrameIndex is CurrentFrameIndex + FrameIndexDelta.
        /// </summary>
        public int FrameIndexDelta
        {
            get => BitsUtil.Int.SignExtend(Data, 4, 11);
            set => Data = (ushort)BitsUtil.Int.SetBits(Data, 4, 12, value);
        }

        public const int SizeInBytes = 8;
    }
}

using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.TextureFooter
{
    public class TextureFooterData
    {
        public List<UvScroll> UvscList { get; } = new List<UvScroll>();
        public List<TextureAnimation> TextureAnimationList { get; } = new List<TextureAnimation>();
        public byte[] UnkFooter { get; set; }
        public bool ShouldEmitDMYAtFirst { get; set; }
        public bool ShouldEmitKN5 { get; set; }

        public override string ToString() => $"{UvscList.Count} UVSC, {TextureAnimationList.Count} TEXA";

        public TextureFooterData()
        {

        }

        public TextureFooterData(Stream stream)
        {
            var shouldEmitDMYAtFirst = false;
            var index = -1;

            while (stream.Position < stream.Length)
            {
                ++index;

                var tag = Encoding.ASCII.GetString(stream.ReadBytes(4));
                if (tag == "_KN5")
                {
                    ShouldEmitDMYAtFirst = (index == 1) && shouldEmitDMYAtFirst;
                    ShouldEmitKN5 = true;

                    UnkFooter = stream.ReadBytes();
                    break;
                }

                var length = stream.ReadInt32();
                var nextTag = stream.Position + length;

                var subStreamPos = Convert.ToInt32(stream.Position);

                if (tag == "UVSC")
                {
                    UvscList.Add(BinaryMapping.ReadObject<UvScroll>(stream));
                }
                else if (tag == "TEXA")
                {
                    var header = BinaryMapping.ReadObject<TextureAnimation>(stream);

                    stream.Position = subStreamPos + header.OffsetSlotTable;
                    header.SlotTable = Enumerable.Range(0, 1 + header.MaximumSlotIndex - header.BaseSlotIndex)
                        .Select(_ => stream.ReadInt16())
                        .ToArray();

                    stream.Position = subStreamPos + header.OffsetAnimationTable;
                    var frameGroupOffsetList = Enumerable.Range(0, header.NumAnimations)
                        .Select(idx => stream.ReadUInt32())
                        .ToArray();

                    header.FrameGroupList = frameGroupOffsetList
                        .Select(
                            firstPosition =>
                            {
                                stream.Position = subStreamPos + firstPosition;
                                var indexedFrameList = new Dictionary<int, TextureFrame>();
                                var idx = 0;
                                while (true)
                                {
                                    if (indexedFrameList.ContainsKey(idx))
                                    {
                                        break;
                                    }

                                    var frame = BinaryMapping.ReadObject<TextureFrame>(stream);
                                    indexedFrameList[idx] = frame;
                                    if (frame.FrameControl == TextureFrameControl.Jump || frame.FrameControl == TextureFrameControl.Stop)
                                    {
                                        idx += frame.FrameIndexDelta;
                                        stream.Seek(TextureFrame.SizeInBytes * (-1 + frame.FrameIndexDelta), SeekOrigin.Current);
                                    }
                                    else
                                    {
                                        idx++;
                                    }
                                }
                                return new TextureFrameGroup
                                {
                                    IndexedFrameList = indexedFrameList,
                                };
                            }
                        )
                        .ToArray();

                    header.SpriteImage = stream
                        .SetPosition(subStreamPos + header.OffsetSpriteImage)
                        .ReadBytes(header.SpriteStride * header.SpriteHeight * header.NumSpritesInImageData);

                    TextureAnimationList.Add(header);
                }
                else if (tag == "_DMY")
                {
                    shouldEmitDMYAtFirst |= (index == 0) ? true : false;
                }

                stream.Position = nextTag;
            }
        }

        public static TextureFooterData Read(Stream stream) =>
            new TextureFooterData(stream);

        /// <summary>
        /// Terminator mark
        /// </summary>
        private static byte[] _KN5 = Encoding.ASCII.GetBytes("_KN5");

        /// <summary>
        /// Dummy
        /// </summary>
        private static byte[] _DMY = Encoding.ASCII.GetBytes("_DMY");

        private static byte[] UVSC = Encoding.ASCII.GetBytes("UVSC");

        private static byte[] TEXA = Encoding.ASCII.GetBytes("TEXA");

        public void Write(Stream stream)
        {
            if (ShouldEmitDMYAtFirst)
            {
                // Some map files has style "_DMY" "_KN5". This is for that consistency.
                stream.Write(_DMY);
                stream.Write(0);
            }

            foreach (var item in TextureAnimationList)
            {
                var texaStream = new MemoryStream();

                {
                    item.NumAnimations = Convert.ToUInt16(item.FrameGroupList.Length);
                    var animationTable = new int[item.NumAnimations];

                    for (var pass = 1; pass <= 2; pass++)
                    {
                        texaStream.Position = 0;
                        BinaryMapping.WriteObject(texaStream, item);

                        item.OffsetSlotTable = Convert.ToInt32(texaStream.Position);
                        item.SlotTable.ToList().ForEach(texaStream.Write);

                        texaStream.AlignPosition(4);

                        item.OffsetAnimationTable = Convert.ToInt32(texaStream.Position);
                        texaStream.Write(animationTable);

                        foreach (var (group, groupIndex) in item.FrameGroupList.Select((group, groupIndex) => (group, groupIndex)))
                        {
                            animationTable[groupIndex] = Convert.ToInt32(texaStream.Position);

                            if (group.IndexedFrameList.Any())
                            {
                                int minIdx = group.IndexedFrameList.Keys.Min();
                                int maxIdx = group.IndexedFrameList.Keys.Max();

                                for (; minIdx <= maxIdx; minIdx++)
                                {
                                    group.IndexedFrameList.TryGetValue(minIdx, out TextureFrame frame);
                                    frame = frame ?? new TextureFrame();

                                    BinaryMapping.WriteObject(texaStream, frame);
                                }
                            }
                        }

                        texaStream.AlignPosition(16);
                        item.OffsetSpriteImage = Convert.ToInt32(texaStream.Position);

                        texaStream.Write(item.SpriteImage);
                        texaStream.AlignPosition(16);
                    }
                }

                stream.Write(_DMY);
                stream.Write(0);

                stream.Write(TEXA);
                stream.Write(Convert.ToUInt32(texaStream.Length));

                texaStream.Position = 0;
                texaStream.CopyTo(stream);
            }

            if (UvscList.Any())
            {
                // Before first "UVSC", "_DMY" has to be placed. This is for that consistency.

                stream.Write(_DMY);
                stream.Write(0);

                foreach (var item in UvscList)
                {
                    stream.Write(UVSC);
                    stream.Write(12);
                    BinaryMapping.WriteObject(stream, item);
                }
            }
            else if (TextureAnimationList.Any())
            {
                // Between "TEXA" and "_KN5", "_DMY" has to be placed. This is for that consistency.

                stream.Write(_DMY);
                stream.Write(0);
            }

            if (ShouldEmitKN5)
            {
                stream.Write(_KN5);
            }

            if (UnkFooter != null)
            {
                stream.Write(UnkFooter);
            }
        }
    }
}

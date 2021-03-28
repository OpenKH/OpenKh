using OpenKh.Common;
using OpenKh.Imaging;

using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Xe.BinaryMapper;
using System.Linq;
using System.Text;
using OpenKh.Common.Utils;

namespace OpenKh.Bbs
{
    public class Pam
    {
        private const uint MagicCode = 0x4D4150;

        public class Header
        {
            [Data] public uint MagicCode { get; set; }
            [Data] public uint AnimationCount { get; set; }
            [Data(Count = 6)] public byte[] Padding { get; set; }
            [Data] public ushort Version { get; set; }
        }

        public class AnimationEntry
        {
            [Data] public uint AnimationOffset { get; set; }
            [Data(Count = 12)] public string AnimationName { get; set; }
        }

        public class AnimationHeader
        {
            [Data] public ushort Flag { get; set; }
            [Data] public byte Framerate { get; set; }
            [Data] public byte InterpFrameCount { get; set; }
            [Data] public ushort LoopFrame { get; set; }
            [Data] public byte BoneCount { get; set; }
            [Data] public byte Padding { get; set; }
            [Data] public ushort FrameCount { get; set; }
            [Data] public ushort ReturnFrame { get; set; }
        }

        public struct AnimationFlag
        {
            [Data] public bool TranslationX { get; set; }
            [Data] public bool TranslationY { get; set; }
            [Data] public bool TranslationZ { get; set; }
            [Data] public bool RotationX { get; set; }
            [Data] public bool RotationY { get; set; }
            [Data] public bool RotationZ { get; set; }
            [Data] public bool ScaleX { get; set; }
            [Data] public bool ScaleY { get; set; }
            [Data] public bool ScaleZ { get; set; }
        }

        public static AnimationFlag GetAnimFlags(ushort Flag)
        {
            AnimationFlag flag = new AnimationFlag();

            flag.TranslationX = BitsUtil.Int.GetBit(Flag, 0);
            flag.TranslationY = BitsUtil.Int.GetBit(Flag, 1);
            flag.TranslationZ = BitsUtil.Int.GetBit(Flag, 2);
            flag.RotationX = BitsUtil.Int.GetBit(Flag, 3);
            flag.RotationY = BitsUtil.Int.GetBit(Flag, 4);
            flag.RotationZ = BitsUtil.Int.GetBit(Flag, 5);
            flag.ScaleX = BitsUtil.Int.GetBit(Flag, 6);
            flag.ScaleY = BitsUtil.Int.GetBit(Flag, 7);
            flag.ScaleZ = BitsUtil.Int.GetBit(Flag, 8);

            return flag;
        }

        public class ChannelHeader
        {
            [Data] public float MaxValue { get; set; }
            [Data] public float MinValue { get; set; }
            [Data] public byte KeyframeCount_8bits { get; set; }
            [Data] public ushort KeyframeCount_16bits { get; set; }
        }

        public class KeyframeEntry
        {
            [Data] public byte FrameID_8bits { get; set; }
            [Data] public ushort FrameID_16bits { get; set; }
            [Data] public ushort Value { get; set; }
        }

        public class AnimationData
        {
            [Data] public ChannelHeader Header { get; set; }
            [Data] public List<KeyframeEntry> Keyframes { get; set; }
        }

        public class BoneChannel
        {
            [Data] public AnimationData TranslationX { get; set; }
            [Data] public AnimationData TranslationY { get; set; }
            [Data] public AnimationData TranslationZ { get; set; }
            [Data] public AnimationData RotationX { get; set; }
            [Data] public AnimationData RotationY { get; set; }
            [Data] public AnimationData RotationZ { get; set; }
            [Data] public AnimationData ScaleX { get; set; }
            [Data] public AnimationData ScaleY { get; set; }
            [Data] public AnimationData ScaleZ { get; set; }
        }

        public class AnimationInfo
        {
            [Data] public List<ushort> ChannelFlags { get; set; }
            [Data] public AnimationEntry AnimEntry { get; set; }
            [Data] public AnimationHeader AnimHeader { get; set; }
            [Data] public List<BoneChannel> BoneChannels { get; set; }
        }


        public Header header = new Header();
        public List<AnimationInfo> animList = new List<AnimationInfo>();

        public static Pam Read(Stream stream)
        {
            Pam pam = new Pam();

            pam.header = BinaryMapping.ReadObject<Header>(stream);
            pam.animList = new List<AnimationInfo>();

            for (int i = 0; i < pam.header.AnimationCount; i++)
            {
                pam.animList.Add(new AnimationInfo());
                pam.animList[i].AnimEntry = BinaryMapping.ReadObject<AnimationEntry>(stream);
            }

            // Get all anims in PAM pack.
            for (int j = 0; j < pam.animList.Count; j++)
            {
                stream.Seek(pam.animList[j].AnimEntry.AnimationOffset, SeekOrigin.Begin);
                pam.animList[j].AnimHeader = BinaryMapping.ReadObject<AnimationHeader>(stream);

                byte BoneNum = pam.animList[j].AnimHeader.BoneCount;
                ushort frameNum = pam.animList[j].AnimHeader.FrameCount;

                pam.animList[j].ChannelFlags = new List<ushort>();

                // Channel Flags
                for (int k = 0; k < BoneNum; k++)
                {
                    pam.animList[j].ChannelFlags.Add(stream.ReadUInt16());
                }

                pam.animList[j].BoneChannels = new List<BoneChannel>();

                // Channel Header & Data
                for (int l = 0; l < BoneNum; l++)
                {
                    BoneChannel boneChannel = new BoneChannel();
                    AnimationFlag flg = GetAnimFlags(pam.animList[j].ChannelFlags[l]);
                    ushort frameCnt = pam.animList[j].AnimHeader.FrameCount;

                    /** TRANSLATION **/

                    if (flg.TranslationX)
                    {
                        ushort keyframeCnt = 0;
                        boneChannel.TranslationX = new AnimationData();
                        boneChannel.TranslationX.Header = new ChannelHeader();
                        boneChannel.TranslationX.Header.MaxValue = stream.ReadFloat();
                        boneChannel.TranslationX.Header.MinValue = stream.ReadFloat();
                        if (frameCnt > 255)
                        {
                            keyframeCnt = boneChannel.TranslationX.Header.KeyframeCount_16bits = stream.ReadUInt16();
                        }
                        else
                        {
                            keyframeCnt = boneChannel.TranslationX.Header.KeyframeCount_8bits = (byte)stream.ReadByte();
                        }

                        if (keyframeCnt != 1)
                        {
                            boneChannel.TranslationX.Keyframes = new List<KeyframeEntry>();
                            for (ushort z = 0; z < keyframeCnt; z++)
                            {
                                KeyframeEntry ent = new KeyframeEntry();
                                if (keyframeCnt == frameCnt)
                                {
                                    ent.FrameID_16bits = z;
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.TranslationX.Keyframes.Add(ent);
                                }
                                else
                                {
                                    if (frameCnt > 255)
                                    {
                                        ent.FrameID_16bits = stream.ReadUInt16();
                                    }
                                    else
                                    {
                                        ent.FrameID_8bits = (byte)stream.ReadByte();
                                    }
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.TranslationX.Keyframes.Add(ent);
                                }
                            }
                        }
                    }

                    if (flg.TranslationY)
                    {
                        ushort keyframeCnt = 0;
                        boneChannel.TranslationY = new AnimationData();
                        boneChannel.TranslationY.Header = new ChannelHeader();
                        boneChannel.TranslationY.Header.MaxValue = stream.ReadFloat();
                        boneChannel.TranslationY.Header.MinValue = stream.ReadFloat();
                        if (frameCnt > 255)
                        {
                            keyframeCnt = boneChannel.TranslationY.Header.KeyframeCount_16bits = stream.ReadUInt16();
                        }
                        else
                        {
                            keyframeCnt = boneChannel.TranslationY.Header.KeyframeCount_8bits = (byte)stream.ReadByte();
                        }

                        if (keyframeCnt != 1)
                        {
                            boneChannel.TranslationY.Keyframes = new List<KeyframeEntry>();
                            for (ushort z = 0; z < keyframeCnt; z++)
                            {
                                KeyframeEntry ent = new KeyframeEntry();

                                if (keyframeCnt == frameCnt)
                                {
                                    ent.FrameID_16bits = z;
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.TranslationY.Keyframes.Add(ent);
                                }
                                else
                                {
                                    if (frameCnt > 255)
                                    {
                                        ent.FrameID_16bits = stream.ReadUInt16();
                                    }
                                    else
                                    {
                                        ent.FrameID_8bits = (byte)stream.ReadByte();
                                    }
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.TranslationY.Keyframes.Add(ent);
                                }
                            }
                        }
                    }

                    if (flg.TranslationZ)
                    {
                        ushort keyframeCnt = 0;
                        boneChannel.TranslationZ = new AnimationData();
                        boneChannel.TranslationZ.Header = new ChannelHeader();
                        boneChannel.TranslationZ.Header.MaxValue = stream.ReadFloat();
                        boneChannel.TranslationZ.Header.MinValue = stream.ReadFloat();
                        if (frameCnt > 255)
                        {
                            keyframeCnt = boneChannel.TranslationZ.Header.KeyframeCount_16bits = stream.ReadUInt16();
                        }
                        else
                        {
                            keyframeCnt = boneChannel.TranslationZ.Header.KeyframeCount_8bits = (byte)stream.ReadByte();
                        }

                        if (keyframeCnt != 1)
                        {
                            boneChannel.TranslationZ.Keyframes = new List<KeyframeEntry>();
                            for (ushort z = 0; z < keyframeCnt; z++)
                            {
                                KeyframeEntry ent = new KeyframeEntry();
                                if (keyframeCnt == frameCnt)
                                {
                                    ent.FrameID_16bits = z;
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.TranslationZ.Keyframes.Add(ent);
                                }
                                else
                                {
                                    if (frameCnt > 255)
                                    {
                                        ent.FrameID_16bits = stream.ReadUInt16();
                                    }
                                    else
                                    {
                                        ent.FrameID_8bits = (byte)stream.ReadByte();
                                    }
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.TranslationZ.Keyframes.Add(ent);
                                }
                            }
                        }
                    }

                    /** ROTATION **/

                    if (flg.RotationX)
                    {
                        ushort keyframeCnt = 0;
                        boneChannel.RotationX = new AnimationData();
                        boneChannel.RotationX.Header = new ChannelHeader();
                        boneChannel.RotationX.Header.MaxValue = stream.ReadFloat();
                        boneChannel.RotationX.Header.MinValue = stream.ReadFloat();
                        if (frameCnt > 255)
                        {
                            keyframeCnt = boneChannel.RotationX.Header.KeyframeCount_16bits = stream.ReadUInt16();
                        }
                        else
                        {
                            keyframeCnt = boneChannel.RotationX.Header.KeyframeCount_8bits = (byte)stream.ReadByte();
                        }

                        if (keyframeCnt != 1)
                        {
                            boneChannel.RotationX.Keyframes = new List<KeyframeEntry>();
                            for (ushort z = 0; z < keyframeCnt; z++)
                            {
                                KeyframeEntry ent = new KeyframeEntry();
                                if (keyframeCnt == frameCnt)
                                {
                                    ent.FrameID_16bits = z;
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.RotationX.Keyframes.Add(ent);
                                }
                                else
                                {
                                    if (frameCnt > 255)
                                    {
                                        ent.FrameID_16bits = stream.ReadUInt16();
                                    }
                                    else
                                    {
                                        ent.FrameID_8bits = (byte)stream.ReadByte();
                                    }
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.RotationX.Keyframes.Add(ent);
                                }
                            }
                        }
                    }

                    if (flg.RotationY)
                    {
                        ushort keyframeCnt = 0;
                        boneChannel.RotationY = new AnimationData();
                        boneChannel.RotationY.Header = new ChannelHeader();
                        boneChannel.RotationY.Header.MaxValue = stream.ReadFloat();
                        boneChannel.RotationY.Header.MinValue = stream.ReadFloat();
                        if (frameCnt > 255)
                        {
                            keyframeCnt = boneChannel.RotationY.Header.KeyframeCount_16bits = stream.ReadUInt16();
                        }
                        else
                        {
                            keyframeCnt = boneChannel.RotationY.Header.KeyframeCount_8bits = (byte)stream.ReadByte();
                        }

                        if (keyframeCnt != 1)
                        {
                            boneChannel.RotationY.Keyframes = new List<KeyframeEntry>();
                            for (ushort z = 0; z < keyframeCnt; z++)
                            {
                                KeyframeEntry ent = new KeyframeEntry();
                                if (keyframeCnt == frameCnt)
                                {
                                    ent.FrameID_16bits = z;
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.RotationY.Keyframes.Add(ent);
                                }
                                else
                                {
                                    if (frameCnt > 255)
                                    {
                                        ent.FrameID_16bits = stream.ReadUInt16();
                                    }
                                    else
                                    {
                                        ent.FrameID_8bits = (byte)stream.ReadByte();
                                    }
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.RotationY.Keyframes.Add(ent);
                                }
                            }
                        }
                    }

                    if (flg.RotationZ)
                    {
                        ushort keyframeCnt = 0;
                        boneChannel.RotationZ = new AnimationData();
                        boneChannel.RotationZ.Header = new ChannelHeader();
                        boneChannel.RotationZ.Header.MaxValue = stream.ReadFloat();
                        boneChannel.RotationZ.Header.MinValue = stream.ReadFloat();
                        if (frameCnt > 255)
                        {
                            keyframeCnt = boneChannel.RotationZ.Header.KeyframeCount_16bits = stream.ReadUInt16();
                        }
                        else
                        {
                            keyframeCnt = boneChannel.RotationZ.Header.KeyframeCount_8bits = (byte)stream.ReadByte();
                        }

                        if (keyframeCnt != 1)
                        {
                            boneChannel.RotationZ.Keyframes = new List<KeyframeEntry>();
                            for (ushort z = 0; z < keyframeCnt; z++)
                            {
                                KeyframeEntry ent = new KeyframeEntry();
                                if (keyframeCnt == frameCnt)
                                {
                                    ent.FrameID_16bits = z;
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.RotationZ.Keyframes.Add(ent);
                                }
                                else
                                {
                                    if (frameCnt > 255)
                                    {
                                        ent.FrameID_16bits = stream.ReadUInt16();
                                    }
                                    else
                                    {
                                        ent.FrameID_8bits = (byte)stream.ReadByte();
                                    }
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.RotationZ.Keyframes.Add(ent);
                                }
                            }
                        }
                    }

                    /** SCALE **/

                    if (flg.ScaleX)
                    {
                        ushort keyframeCnt = 0;
                        boneChannel.ScaleX = new AnimationData();
                        boneChannel.ScaleX.Header = new ChannelHeader();
                        boneChannel.ScaleX.Header.MaxValue = stream.ReadFloat();
                        boneChannel.ScaleX.Header.MinValue = stream.ReadFloat();
                        if (frameCnt > 255)
                        {
                            keyframeCnt = boneChannel.ScaleX.Header.KeyframeCount_16bits = stream.ReadUInt16();
                        }
                        else
                        {
                            keyframeCnt = boneChannel.ScaleX.Header.KeyframeCount_8bits = (byte)stream.ReadByte();
                        }

                        if (keyframeCnt != 1)
                        {
                            boneChannel.ScaleX.Keyframes = new List<KeyframeEntry>();
                            for (ushort z = 0; z < keyframeCnt; z++)
                            {
                                KeyframeEntry ent = new KeyframeEntry();
                                if (keyframeCnt == frameCnt)
                                {
                                    ent.FrameID_16bits = z;
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.ScaleX.Keyframes.Add(ent);
                                }
                                else
                                {
                                    if (frameCnt > 255)
                                    {
                                        ent.FrameID_16bits = stream.ReadUInt16();
                                    }
                                    else
                                    {
                                        ent.FrameID_8bits = (byte)stream.ReadByte();
                                    }
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.ScaleX.Keyframes.Add(ent);
                                }
                            }
                        }
                    }

                    if (flg.ScaleY)
                    {
                        ushort keyframeCnt = 0;
                        boneChannel.ScaleY = new AnimationData();
                        boneChannel.ScaleY.Header = new ChannelHeader();
                        boneChannel.ScaleY.Header.MaxValue = stream.ReadFloat();
                        boneChannel.ScaleY.Header.MinValue = stream.ReadFloat();
                        if (frameCnt > 255)
                        {
                            keyframeCnt = boneChannel.ScaleY.Header.KeyframeCount_16bits = stream.ReadUInt16();
                        }
                        else
                        {
                            keyframeCnt = boneChannel.ScaleY.Header.KeyframeCount_8bits = (byte)stream.ReadByte();
                        }

                        if (keyframeCnt != 1)
                        {
                            boneChannel.ScaleY.Keyframes = new List<KeyframeEntry>();
                            for (ushort z = 0; z < keyframeCnt; z++)
                            {
                                KeyframeEntry ent = new KeyframeEntry();
                                if (keyframeCnt == frameCnt)
                                {
                                    ent.FrameID_16bits = z;
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.ScaleY.Keyframes.Add(ent);
                                }
                                else
                                {
                                    if (frameCnt > 255)
                                    {
                                        ent.FrameID_16bits = stream.ReadUInt16();
                                    }
                                    else
                                    {
                                        ent.FrameID_8bits = (byte)stream.ReadByte();
                                    }
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.ScaleY.Keyframes.Add(ent);
                                }
                            }
                        }
                    }

                    if (flg.ScaleZ)
                    {
                        ushort keyframeCnt = 0;
                        boneChannel.ScaleZ = new AnimationData();
                        boneChannel.ScaleZ.Header = new ChannelHeader();
                        boneChannel.ScaleZ.Header.MaxValue = stream.ReadFloat();
                        boneChannel.ScaleZ.Header.MinValue = stream.ReadFloat();
                        if (frameCnt > 255)
                        {
                            keyframeCnt = boneChannel.ScaleZ.Header.KeyframeCount_16bits = stream.ReadUInt16();
                        }
                        else
                        {
                            keyframeCnt = boneChannel.ScaleZ.Header.KeyframeCount_8bits = (byte)stream.ReadByte();
                        }

                        if (keyframeCnt != 1)
                        {
                            boneChannel.ScaleZ.Keyframes = new List<KeyframeEntry>();
                            for (ushort z = 0; z < keyframeCnt; z++)
                            {
                                KeyframeEntry ent = new KeyframeEntry();
                                if (keyframeCnt == frameCnt)
                                {
                                    ent.FrameID_16bits = z;
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.ScaleZ.Keyframes.Add(ent);
                                }
                                else
                                {
                                    if (frameCnt > 255)
                                    {
                                        ent.FrameID_16bits = stream.ReadUInt16();
                                    }
                                    else
                                    {
                                        ent.FrameID_8bits = (byte)stream.ReadByte();
                                    }
                                    ent.Value = stream.ReadUInt16();
                                    boneChannel.ScaleZ.Keyframes.Add(ent);
                                }
                            }
                        }
                    }

                    pam.animList[j].BoneChannels.Add(boneChannel);
                }
            }

            return pam;
        }
    }
}

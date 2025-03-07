using OpenKh.Bbs;
using OpenKh.Command.KHAnimationConverter;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using Xunit;
using Xunit.Sdk;

namespace OpenKh.Tests.Commands
{
    public class KHAnimationConverterTests
    {
        [Theory]
        [MemberData(nameof(GetSource))]
        public void TestConvertPamToANBs(Pam pam)
        {
            List<short> pamChannelsToAnbChannels = new List<short>() { 6, 7, 8, 3, 4, 5, 0, 1, 2 };

            List<AnimationBinary> anbs = KHFormat.PAMtoANBs(pam, 30);

            Pam.Header pamHeader = pam.header;

            int pamAnimCount = 0;
            for (int i = 0; i < pamHeader.AnimationCount; i++)
            {
                Pam.AnimationInfo pamAnim = pam.animList[i];
                Pam.AnimationHeader pamAnimHeader = pamAnim.AnimHeader;
                Pam.AnimationEntry pamAnimEntry = pamAnim.AnimEntry;
                List<ushort> pamBoneChannelFlags = pamAnim.ChannelFlags;

                if (pamAnimEntry.AnimationOffset == 0)
                {
                    continue;
                }

                int pamFrameCount = pamAnimHeader.FrameCount;

                AnimationBinary anb = anbs[i];
                Motion.InterpolatedMotion motionFile = anb.MotionFile;
                List<Motion.Joint> anbJoints = motionFile.Joints;

                Assert.Equal(pamAnimHeader.BoneCount, motionFile.Joints.Count);

                for (int j = 0; j < pamAnimHeader.BoneCount; j++)
                {
                    ushort pamChannelFlags = pamBoneChannelFlags[j];
                    Motion.Joint anbJoint = anbJoints[j];
                    Pam.BoneChannel pamAnimBoneChannel = pamAnim.BoneChannels[j];
                    List<Pam.AnimationData> pamAnimBoneChannelChannels = new List<Pam.AnimationData>() { 
                        pamAnimBoneChannel.ScaleX, pamAnimBoneChannel.ScaleY, pamAnimBoneChannel.ScaleZ,
                        pamAnimBoneChannel.RotationX, pamAnimBoneChannel.RotationY, pamAnimBoneChannel.RotationZ,
                        pamAnimBoneChannel.TranslationX, pamAnimBoneChannel.TranslationY, pamAnimBoneChannel.TranslationZ
                    };
                    int initialPoseInd = 0;
                    int fCurveInd = 0;
                    List<Motion.InitialPose> anbInitialPoses = motionFile.InitialPoses;
                    List<Motion.Key> anbFCurves = motionFile.FCurveKeys;
                    for (int k = 0; k < pamChannelsToAnbChannels.Count; k++)
                    {
                        Pam.AnimationData pamAnimData = pamAnimBoneChannelChannels[k];
                        short anbChannelInd = pamChannelsToAnbChannels[k];
                        Pam.ChannelHeader pamChannelHeader = pamAnimData.Header;
                        int pamKeyframeCount = pamChannelHeader.KeyframeCount_16bits + pamChannelHeader.KeyframeCount_8bits;
                        if (pamKeyframeCount == 1)
                        {
                            Motion.InitialPose initialPose = anbInitialPoses[initialPoseInd];
                            initialPoseInd++;
                            Assert.Equal(expected: j, actual: initialPose.BoneId);
                            Assert.Equal(expected: anbChannelInd, actual: initialPose.Channel);
                        }
                        else
                        {
                            Motion.FCurve fCurve = motionFile.FCurvesForward[fCurveInd];
                            Assert.Equal(expected: j, actual: fCurve.JointId);
                            Assert.Equal(expected: anbChannelInd, fCurve.Channel);
                            Assert.Equal(expected: pamAnimData.Keyframes.Count, actual: fCurve.KeyCount);
                            fCurveInd++;
                        }
                    }
                }
                pamAnimCount++;
            }

            Assert.Equal(pamAnimCount, anbs.Count);

        }

        [Theory]
        [InlineData("../../../res/animconverter/inputs/test-0000.anb")]
        public void TestExportANBs(string inputPath)
        {
            Stream stream = File.OpenRead(inputPath);
            AnimationBinary anb = new AnimationBinary(stream);
            stream.Close();
            KHFormat file = new KHFormat()
            {
                anbs = new List<AnimationBinary>()
                {
                    anb
                }
            };
            string inputName = Path.GetFileName(inputPath);
            string outputName = $"anim-0000.anb";
            string outDir = Path.Join(Path.GetDirectoryName(Path.GetDirectoryName(inputPath)), "outputs");
            string outPath = Path.Join(outDir, outputName);
            KHFormat.ExportFile(file, "anbs", outDir);
            stream = File.OpenRead(outPath);
            AnimationBinary loadedAnb = new AnimationBinary(stream);
            Assert.Equal(expected: anb.MotionFile.InterpolatedMotionHeader.BoneCount, actual: loadedAnb.MotionFile.InterpolatedMotionHeader.BoneCount);
            Assert.Equal(expected: anb.MotionFile.InterpolatedMotionHeader.FrameCount, actual: loadedAnb.MotionFile.InterpolatedMotionHeader.FrameCount);
            Assert.Equal(expected: anb.MotionFile.InterpolatedMotionHeader.FrameData.FramesPerSecond, actual: loadedAnb.MotionFile.InterpolatedMotionHeader.FrameData.FramesPerSecond);
            Assert.Equal(expected: anb.MotionFile.Joints.Count, actual: loadedAnb.MotionFile.Joints.Count);
            Assert.Equal(expected: anb.MotionFile.InitialPoses.Count, actual: loadedAnb.MotionFile.InitialPoses.Count);
            Assert.Equal(expected: anb.MotionFile.FCurvesForward.Count, actual: loadedAnb.MotionFile.FCurvesForward.Count);
            Assert.Equal(expected: anb.MotionFile.FCurveKeys.Count, actual: loadedAnb.MotionFile.FCurveKeys.Count);
            Assert.Equal(expected: anb.MotionFile.KeyTimes.Count, actual: loadedAnb.MotionFile.KeyTimes.Count);
            Assert.Equal(expected: anb.MotionFile.KeyValues.Count, actual: loadedAnb.MotionFile.KeyValues.Count);
            Assert.Equal(expected: anb.MotionTriggerFile.RangeTriggerList.Count, actual: loadedAnb.MotionTriggerFile.RangeTriggerList.Count);
            Assert.Equal(expected: anb.MotionTriggerFile.FrameTriggerList.Count, actual: loadedAnb.MotionTriggerFile.FrameTriggerList.Count);
            stream.Close();
            using var disposer = new FileDisposer(outPath);
        }

        public static IEnumerable<object[]> GetSource()
        {
            List<Pam> pams = CreateSamplePams();
            foreach (var pam in pams)
            {
                yield return new object[] { pam };
            }
        }

        // TODO - Enable writing of Pam files so that we can just read from a file instead of hard coding our data
        public static List<Pam> CreateSamplePams()
        {
            List<List<bool>> dummyMap = new List<List<bool>>() 
            {
                new List<bool>()
                {
                    true,
                    false,
                    false,
                    true,
                }
            };

            List<List<ushort>> flags = new List<List<ushort>>()
            {
                new List<ushort>()
                {
                    0x0000,
                    0x0100,
                    0x0101,
                    0x0000,
                } 
            };
            List<List<byte>> framerates = new List<List<byte>>()
            {
                new List<byte>()
                {
                    0,
                    30,
                    30,
                    0
                }
            };
            List<List<ushort>> framecounts = new List<List<ushort>>()
            {
                new List<ushort>() 
                {
                    0,
                    10,
                    10,
                    0
                }
            };

            // Bone Count for our generated animations
            List<byte> boneCounts = new List<byte>() 
            {
                1
            };

            List<List<List<ushort>>> channelFlags = new List<List<List<ushort>>>()
            {
                new List<List<ushort>>()
                {
                    new List<ushort>()
                    {
                        0x0000
                    },
                    new List<ushort>()
                    {
                        0x01FF
                    },
                    new List<ushort>()
                    {
                        0x1F4
                    },
                    new List<ushort>()
                    {
                        0x0000
                    }
                }
            };

            List<List<List<List<ushort>>>> keyframeCounts = new List<List<List<List<ushort>>>>()
            {
                new List<List<List<ushort>>>()
                {
                    new List<List<ushort>>() 
                    {
                        new List<ushort>() 
                        {
                            0, 0, 0, // Translate
                            0, 0, 0, // Rotate
                            0, 0, 0  // Scale
                        }
                    }
                },
                new List<List<List<ushort>>>()
                {
                    new List<List<ushort>>()
                    {
                        new List<ushort>()
                        {
                            2, 2, 2, // Translate
                            2, 2, 2, // Rotate
                            1, 1, 1  // Scale
                        }
                    }
                },
                new List<List<List<ushort>>>()
                {
                    new List<List<ushort>>()
                    {
                        new List<ushort>()
                        {
                            0, 0, 1, // Translate
                            0, 1, 1, // Rotate
                            1, 1, 1  // Scale
                        }
                    }
                },
                new List<List<List<ushort>>>()
                {
                    new List<List<ushort>>() 
                    {
                        new List<ushort>()
                        {
                            0, 0, 0, // Translate
                            0, 0, 0, // Rotate
                            0, 0, 0  // Scale
                        }
                    }
                }
            };

            List<List<List<List<List<float>>>>> channelBounds = new List<List<List<List<List<float>>>>>()
            {
                new List<List<List<List<float>>>>()
                {
                    new List<List<List<float>>>()
                    {
                        new List<List<float>>() {
                            new List<float>(), new List<float>(), new List<float>(), // Translate
                            new List<float>(), new List<float>(), new List<float>(), // Rotate
                            new List<float>(), new List<float>(), new List<float>()  // Scale
                        }
                    }
                },
                new List<List<List<List<float>>>>()
                {
                    new List<List<List<float>>>()
                    {
                        new List<List<float>>() {
                            new List<float>() { 0f, 1f }, new List<float>() { 0f, 2f }, new List<float>() { 0f, 3f }, // Translate
                            new List<float>() { 0f, 3f }, new List<float>() { 0, 1.5f }, new List<float>() { 0f, 2.72f }, // Rotate
                            new List<float>() { 1f, 1f }, new List<float>() { 1f, 1f }, new List<float>() { 1f, 1f }  // Scale
                        }
                    }
                },
                new List<List<List<List<float>>>>()
                {
                    new List<List<List<float>>>()
                    {
                        new List<List<float>>() {
                            new List<float>(), new List<float>(), new List<float>() { 3f, 3f }, // Translate
                            new List<float>(), new List<float>() { 1.2f, 1.2f }, new List<float>() { 0.234f, 0.234f }, // Rotate
                            new List<float>() { 1f, 1f }, new List<float>() { 1f, 1f }, new List<float>() { 1f, 1f }  // Scale
                        }
                    }
                },
                new List<List<List<List<float>>>>()
                {
                    new List<List<List<float>>>()
                    {
                        new List<List<float>>() {
                            new List<float>(), new List<float>(), new List<float>(), // Translate
                            new List<float>(), new List<float>(), new List<float>(), // Rotate
                            new List<float>(), new List<float>(), new List<float>()  // Scale
                        }
                    }
                }
            };

            List<List<List<List<List<ushort>>>>> keyframeInds = new List<List<List<List<List<ushort>>>>>()
            {
                new List<List<List<List<ushort>>>>()
                {
                    new List<List<List<ushort>>>()
                    {
                        new List<List<ushort>>() {
                            new List<ushort>(), new List<ushort>(), new List<ushort>(), // Translate
                            new List<ushort>(), new List<ushort>(), new List<ushort>(), // Rotate
                            new List<ushort>(), new List<ushort>(), new List<ushort>()  // Scale
                        }
                    },
                    new List<List<List<ushort>>>()
                    {
                        new List<List<ushort>>() {
                            new List<ushort>() { 0, 10 }, new List<ushort>() { 0, 10 }, new List<ushort>() { 0, 10 }, // Translate
                            new List<ushort>() { 0, 10 }, new List<ushort>() { 0, 10 }, new List<ushort>() { 0, 10 }, // Rotate
                            new List<ushort>() { 0 }, new List<ushort>() { 0 }, new List<ushort>() { 0 }  // Scale
                        }
                    },
                    new List<List<List<ushort>>>()
                    {
                        new List<List<ushort>>() {
                            new List<ushort>(), new List<ushort>(), new List<ushort>() { 0 }, // Translate
                            new List<ushort>(), new List<ushort>() { 0 }, new List<ushort>() { 0 }, // Rotate
                            new List<ushort>() { 0 }, new List<ushort>() { 0 }, new List<ushort>() { 0 }  // Scale
                        }
                    },
                    new List<List<List<ushort>>>()
                    {
                        new List<List<ushort>>() {
                            new List<ushort>(), new List<ushort>(), new List<ushort>(), // Translate
                            new List<ushort>(), new List<ushort>(), new List<ushort>(), // Rotate
                            new List<ushort>(), new List<ushort>(), new List<ushort>()  // Scale
                        }
                    }
                }
            };

            List<List<List<List<List<ushort>>>>> keyframeVals = new List<List<List<List<List<ushort>>>>>()
            {
                new List<List<List<List<ushort>>>>()
                {
                    new List<List<List<ushort>>>()
                    {
                        new List<List<ushort>>() {
                            new List<ushort>(), new List<ushort>(), new List<ushort>(), // Translate
                            new List<ushort>(), new List<ushort>(), new List<ushort>(), // Rotate
                            new List<ushort>(), new List<ushort>(), new List<ushort>()  // Scale
                        }
                    },
                    new List<List<List<ushort>>>()
                    {
                        new List<List<ushort>>() {
                            new List<ushort>() { 0, ushort.MaxValue }, new List<ushort>() { 0, ushort.MaxValue }, new List<ushort>() { 0, ushort.MaxValue }, // Translate
                            new List<ushort>() { 0, ushort.MaxValue }, new List<ushort>() { 0, ushort.MaxValue }, new List<ushort>() { 0, ushort.MaxValue }, // Rotate
                            new List<ushort>() { 0 }, new List<ushort>() { 0 }, new List<ushort>() { 0 }  // Scale
                        }
                    },
                    new List<List<List<ushort>>>()
                    {
                        new List<List<ushort>>() {
                            new List<ushort>(), new List<ushort>(), new List<ushort>() { 0 }, // Translate
                            new List<ushort>(), new List<ushort>() { 0 }, new List<ushort>() { 0 }, // Rotate
                            new List<ushort>() { 0 }, new List<ushort>() { 0 }, new List<ushort>() { 0 }  // Scale
                        }
                    },
                    new List<List<List<ushort>>>()
                    {
                        new List<List<ushort>>() {
                            new List<ushort>(), new List<ushort>(), new List<ushort>(), // Translate
                            new List<ushort>(), new List<ushort>(), new List<ushort>(), // Rotate
                            new List<ushort>(), new List<ushort>(), new List<ushort>()  // Scale
                        }
                    }
                }
            };
            int pamCount = boneCounts.Count;
            List<Pam> pams = new List<Pam>();
            for (int a = 0; a < pamCount; a++)
            {
                Pam.Header header = new Pam.Header()
                {
                    AnimationCount = (uint)dummyMap.Count,
                    Version = 1
                };
                List<Pam.AnimationInfo> animList = new List<Pam.AnimationInfo>();
                for (int i = 0; i < header.AnimationCount; i++)
                {
                    Pam.AnimationEntry entry;
                    Pam.AnimationInfo info;
                    if (dummyMap[a][i])
                    {
                        entry = new Pam.AnimationEntry()
                        {
                            AnimationOffset = 0,
                            AnimationName = ""
                        };
                        info = new Pam.AnimationInfo()
                        {
                            AnimEntry = entry,
                        };
                    }
                    else
                    {
                        Pam.AnimationHeader animHeader = new Pam.AnimationHeader()
                        {
                            Flag = flags[a][i],
                            Framerate = framerates[a][i],
                            InterpFrameCount = 0,
                            LoopFrame = framecounts[a][i],
                            BoneCount = boneCounts[a],
                            FrameCount = framecounts[a][i],
                            ReturnFrame = 0,
                        };
                        List<ushort> animChannelFlags = new List<ushort>(channelFlags[a][i]);
                        List<Pam.BoneChannel> animBoneChannels = new List<Pam.BoneChannel>();
                        List<List<ushort>> animKeyframeCounts = keyframeCounts[a][i];
                        for (int j = 0; j < boneCounts[a]; j++)
                        {
                            Pam.AnimationFlag animFlag = Pam.GetAnimFlags(animChannelFlags[j]);
                            List<bool> animFlagList = new List<bool>()
                        {
                            animFlag.TranslationX,
                            animFlag.TranslationY,
                            animFlag.TranslationZ,
                            animFlag.RotationX,
                            animFlag.RotationY,
                            animFlag.RotationZ,
                            animFlag.ScaleX,
                            animFlag.ScaleY,
                            animFlag.ScaleZ
                        };
                            List<ushort> boneKeyframeCounts = animKeyframeCounts[j];
                            Pam.BoneChannel boneChannel = new Pam.BoneChannel();
                            for (int k = 0; k < 9; k++)
                            {
                                ushort keyframesCount = boneKeyframeCounts[k];
                                if (animFlagList[k])
                                {
                                    Pam.ChannelHeader channelHeader = new Pam.ChannelHeader()
                                    {
                                        MaxValue = channelBounds[a][i][j][k][1],
                                        MinValue = channelBounds[a][i][j][k][0],
                                        KeyframeCount_16bits = animHeader.FrameCount > 255 ? keyframesCount : (ushort)0,
                                        KeyframeCount_8bits = animHeader.FrameCount <= 255 ? (byte)keyframesCount : (byte)0,
                                    };
                                    Pam.AnimationData pamAnimData = new Pam.AnimationData()
                                    {
                                        Header = channelHeader
                                    };
                                    if (keyframesCount != 1)
                                    {
                                        List<Pam.KeyframeEntry> animDataKeyframeEntries = new List<Pam.KeyframeEntry>();
                                        for (ushort l = 0; l < keyframesCount; l++)
                                        {
                                            ushort frameInd;
                                            if (keyframesCount == animHeader.FrameCount)
                                            {
                                                frameInd = l;
                                            }
                                            else
                                            {
                                                frameInd = keyframeInds[a][i][j][k][l];
                                            }
                                            ushort value = keyframeVals[a][i][j][k][l];
                                            Pam.KeyframeEntry keyframeEntry = new Pam.KeyframeEntry()
                                            {
                                                FrameID_16bits = animHeader.FrameCount > 255 ? frameInd : (ushort)0,
                                                FrameID_8bits = animHeader.FrameCount <= 255 ? (byte)frameInd : (byte)0,
                                                Value = value,
                                            };
                                            animDataKeyframeEntries.Add(keyframeEntry);
                                        }
                                        pamAnimData.Keyframes = animDataKeyframeEntries;
                                    }
                                    switch (k)
                                    {
                                        case 0:
                                            boneChannel.TranslationX = pamAnimData;
                                            break;
                                        case 1:
                                            boneChannel.TranslationY = pamAnimData;
                                            break;
                                        case 2:
                                            boneChannel.TranslationZ = pamAnimData;
                                            break;
                                        case 3:
                                            boneChannel.RotationX = pamAnimData;
                                            break;
                                        case 4:
                                            boneChannel.RotationY = pamAnimData;
                                            break;
                                        case 5:
                                            boneChannel.RotationZ = pamAnimData;
                                            break;
                                        case 6:
                                            boneChannel.ScaleX = pamAnimData;
                                            break;
                                        case 7:
                                            boneChannel.ScaleY = pamAnimData;
                                            break;
                                        case 8:
                                            boneChannel.ScaleZ = pamAnimData;
                                            break;
                                    }
                                }
                            }
                            animBoneChannels.Add(boneChannel);
                        }
                        entry = new Pam.AnimationEntry()
                        {
                            AnimationOffset = 0,
                            AnimationName = $"{i:000}"
                        };
                        info = new Pam.AnimationInfo()
                        {
                            AnimEntry = entry,
                            AnimHeader = animHeader,
                            ChannelFlags = animChannelFlags,
                            BoneChannels = animBoneChannels
                        };
                    }
                    animList.Add(info);
                }
                Pam pam = new Pam()
                {
                    header = header,
                    animList = animList,
                };
                pams.Add(pam);
            }
            return pams;
        }
    }
}

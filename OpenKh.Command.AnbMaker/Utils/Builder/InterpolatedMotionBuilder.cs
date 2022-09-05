using NLog;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Kh2.Motion;

namespace OpenKh.Command.AnbMaker.Utils.Builder
{
    public class InterpolatedMotionBuilder
    {
        public InterpolatedMotion ipm { get; }

        public InterpolatedMotionBuilder(
            int DurationInTicks,
            float TicksPerSecond,
            int fbxArmatureBoneCount,
            float NodeScaling,
            Func<int, AChannel> getAChannel
        )
        {
            ipm = InterpolatedMotion.CreateEmpty();

            var logger = LogManager.GetCurrentClassLogger();

            var frameCount = DurationInTicks;

            // convert source animation's keyTime to KH2 internal frame rate 60 fps which is called GFR (Global Frame Rate)
            var keyTimeMultiplier = 60 / TicksPerSecond;

            ipm.InterpolatedMotionHeader.BoneCount = Convert.ToInt16(fbxArmatureBoneCount);
            ipm.InterpolatedMotionHeader.TotalBoneCount = Convert.ToInt16(fbxArmatureBoneCount);
            ipm.InterpolatedMotionHeader.FrameCount = (int)(frameCount * keyTimeMultiplier); // in 1/60 seconds
            ipm.InterpolatedMotionHeader.FrameData.FrameStart = 0;
            ipm.InterpolatedMotionHeader.FrameData.FrameEnd = frameCount - 1;
            ipm.InterpolatedMotionHeader.FrameData.FramesPerSecond = TicksPerSecond;
            ipm.InterpolatedMotionHeader.BoundingBox = new BoundingBox
            {
                BoundingBoxMinX = -100,
                BoundingBoxMinY = -100,
                BoundingBoxMinZ = -100,
                BoundingBoxMinW = 1,
                BoundingBoxMaxX = 100,
                BoundingBoxMaxY = 100,
                BoundingBoxMaxZ = 100,
                BoundingBoxMaxW = 1,
            };

            ipm.KeyTangents.Add(0);

            short AddKeyTime(float keyTime)
            {
                var idx = ipm.KeyTimes.IndexOf(keyTime);
                if (idx < 0)
                {
                    idx = ipm.KeyTimes.Count;
                    ipm.KeyTimes.Add(keyTime);
                }
                return (short)Convert.ToUInt16(idx);
            }

            short AddSourceKeyTime(double sourceKeyTime)
            {
                return AddKeyTime(GetLowerPrecisionValue((float)(sourceKeyTime * keyTimeMultiplier)));
            }

            short AddKeyValue(float keyValue)
            {
                var idx = ipm.KeyValues.IndexOf(keyValue);
                if (idx < 0)
                {
                    idx = ipm.KeyValues.Count;
                    ipm.KeyValues.Add(keyValue);
                }
                return (short)Convert.ToUInt16(idx);
            }

            for (int boneIdx = 0; boneIdx < fbxArmatureBoneCount; boneIdx++)
            {
                var hit = getAChannel(boneIdx);
                if (hit != null)
                {
                    float FixScaling(float value)
                    {
                        return ((boneIdx == 0) ? NodeScaling : 1f) * value;
                    }

                    float FixScalingValue(float value) => GetLowerPrecisionValue(FixScaling(value));

                    float FixPos(float value)
                    {
                        return value / ((boneIdx == 0) ? 1 : NodeScaling);
                    }

                    float FixPosValue(float value) => GetLowerPrecisionValue(FixPos(value));

                    var channels = new ChannelProvider[]
                    {
                        new ChannelProvider
                        {
                            type = Channel.SCALE_X,
                            jointFlags = JointFlags.HasScaling,
                            keys = hit.ScaleXKeys,
                            fixValue = FixScalingValue,
                        },
                        new ChannelProvider
                        {
                            type = Channel.SCALE_Y,
                            jointFlags = JointFlags.HasScaling,
                            keys = hit.ScaleYKeys,
                            fixValue = FixScalingValue,
                        },
                        new ChannelProvider
                        {
                            type = Channel.SCALE_Z,
                            jointFlags = JointFlags.HasScaling,
                            keys = hit.ScaleZKeys,
                            fixValue = FixScalingValue,
                        },

                        new ChannelProvider
                        {
                            type = Channel.ROTATATION_X,
                            jointFlags = JointFlags.HasRotation,
                            keys = hit.RotationXKeys,
                            fixValue = it => it,
                        },
                        new ChannelProvider
                        {
                            type = Channel.ROTATATION_Y,
                            jointFlags = JointFlags.HasRotation,
                            keys = hit.RotationYKeys,
                            fixValue = it => it,
                        },
                        new ChannelProvider
                        {
                            type = Channel.ROTATATION_Z,
                            jointFlags = JointFlags.HasRotation,
                            keys = hit.RotationZKeys,
                            fixValue = it => it,
                        },

                        new ChannelProvider
                        {
                            type = Channel.TRANSLATION_X,
                            jointFlags = JointFlags.HasPosition,
                            keys = hit.PositionXKeys,
                            fixValue = FixPosValue,
                        },
                        new ChannelProvider
                        {
                            type = Channel.TRANSLATION_Y,
                            jointFlags = JointFlags.HasPosition,
                            keys = hit.PositionYKeys,
                            fixValue = FixPosValue,
                        },
                        new ChannelProvider
                        {
                            type = Channel.TRANSLATION_Z,
                            jointFlags = JointFlags.HasPosition,
                            keys = hit.PositionZKeys,
                            fixValue = FixPosValue,
                        },
                    };

                    var jointFlag = JointFlags.None;

                    foreach (var channel in channels)
                    {
                        if (channel.keys.Any())
                        {
                            jointFlag |= channel.jointFlags;

                            var numKeys = Convert.ToByte(channel.keys.Count());

                            var lastKeyIdx = ipm.FCurveKeys.Count;

                            ipm.FCurvesForward.Add(
                                new FCurve
                                {
                                    JointId = Convert.ToInt16(boneIdx),
                                    Channel = (byte)(channel.type),
                                    KeyStartId = Convert.ToInt16(lastKeyIdx),
                                    KeyCount = numKeys,
                                }
                            );

                            ipm.FCurveKeys.AddRange(
                                channel.keys
                                    .Select(
                                        key => new Key
                                        {
                                            Type = Interpolation.Hermite,
                                            Time = AddSourceKeyTime(key.Time),
                                            ValueId = AddKeyValue(channel.fixValue(key.Value)),
                                        }
                                    )
                            );
                        }
                    }
                }

                ipm.Joints.Add(
                    new Joint
                    {
                        JointId = Convert.ToInt16(boneIdx),
                        // Setting to 0, if no constraints exist.
                        Flags = 0,
                    }
                );
            }

            // reduce fcurve keys
            {
                var numSames = 0;

                var fCurveKeyReuseTable = new SortedDictionary<string, ushort>();
                var reducedFCurveKeys = new List<Key>();
                foreach (var (fCurve, idx) in ipm.FCurvesForward
                    .Select((fCurve, idx) => (fCurve, idx))
                    .ToArray()
                )
                {
                    var thisKeys = ipm.FCurveKeys
                        .Skip(fCurve.KeyStartId)
                        .Take(fCurve.KeyCount)
                        .ToArray();

                    var sames = thisKeys
                        .Select(key => key.ValueId)
                        .Distinct()
                        .ToArray();

                    if (sames.Length == 1)
                    {
                        numSames++;

                        ipm.InitialPoses.Add(
                            new InitialPose
                            {
                                BoneId = fCurve.JointId,
                                ChannelValue = fCurve.ChannelValue,
                                Value = ipm.KeyValues[sames.Single()],
                            }
                        );

                        ipm.FCurvesForward.Remove(fCurve);
                    }
                    else
                    {
                        var fCurveKeyReuseKey = string.Join(
                            " ",
                            thisKeys
                                .Select(it => $"{it.Type_Time:x4},{it.ValueId:x4},{it.LeftTangentId:x4},{it.RightTangentId:x4}")
                        );

                        if (fCurveKeyReuseTable.TryGetValue(fCurveKeyReuseKey, out ushort foundIdx))
                        {
                            fCurve.KeyStartId = (short)foundIdx;
                        }
                        else
                        {
                            var newIdx = Convert.ToUInt16(reducedFCurveKeys.Count);

                            reducedFCurveKeys.AddRange(thisKeys);

                            fCurve.KeyStartId = (short)newIdx;

                            fCurveKeyReuseTable[fCurveKeyReuseKey] = newIdx;
                        }
                    }
                }

                if (numSames != 0)
                {
                    logger.Debug($"{numSames:#,##0} channels have only single value. They are converted to InitialPoses.");
                }

                logger.Debug($"FCurveKeys count reduced from {ipm.FCurveKeys.Count:#,##0} to {reducedFCurveKeys.Count:#,##0}");

                ipm.FCurveKeys.Clear();
                ipm.FCurveKeys.AddRange(reducedFCurveKeys);
            }

            // sort key time in ascending order
            {
                var newKeyTimes = ipm.KeyTimes
                    .OrderBy(it => it)
                    .ToArray();

                foreach (var fCurveKey in ipm.FCurveKeys)
                {
                    fCurveKey.Time = (short)Convert.ToUInt16(Array.IndexOf(newKeyTimes, ipm.KeyTimes[fCurveKey.Time]));
                }

                ipm.KeyTimes.Clear();
                ipm.KeyTimes.AddRange(newKeyTimes);
            }
        }

        [Flags]
        private enum JointFlags
        {
            None = 0,
            HasPosition = 1,
            HasRotation = 2,
            HasScaling = 4,
        }

        private static float GetLowerPrecisionValue(float value)
        {
            return (float)Math.Round(value, 2);
        }

        private class ChannelProvider
        {
            internal Channel type;
            internal JointFlags jointFlags;
            internal AScalarKey[] keys;
            internal Func<float, float> fixValue;
        }
    }
}

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
        public InterpolatedMotion Ipm { get; }

        public InterpolatedMotionBuilder(
            BasicSourceMotion parm
        )
        {
            Ipm = InterpolatedMotion.CreateEmpty();

            var logger = LogManager.GetCurrentClassLogger();

            var frameCount = parm.DurationInTicks;

            if (parm.TicksPerSecond <= 0)
            {
                throw new Exception("TicksPerSecond must be set!");
            }

            // convert source animation's keyTime to KH2 internal frame rate 60 fps which is called GFR (Global Frame Rate)
            var keyTimeMultiplier = 60 / parm.TicksPerSecond;

            Ipm.InterpolatedMotionHeader.BoneCount = Convert.ToInt16(parm.BoneCount);
            Ipm.InterpolatedMotionHeader.TotalBoneCount = Convert.ToInt16(parm.BoneCount);
            Ipm.InterpolatedMotionHeader.FrameCount = (int)(frameCount * keyTimeMultiplier); // in 1/60 seconds
            Ipm.InterpolatedMotionHeader.FrameData.FrameStart = 0;
            Ipm.InterpolatedMotionHeader.FrameData.FrameEnd = frameCount - 1;
            Ipm.InterpolatedMotionHeader.FrameData.FramesPerSecond = parm.TicksPerSecond;
            Ipm.InterpolatedMotionHeader.BoundingBox = new BoundingBox
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

            Ipm.KeyTangents.Add(0);

            short AddKeyTime(float keyTime)
            {
                var idx = Ipm.KeyTimes.IndexOf(keyTime);
                if (idx < 0)
                {
                    idx = Ipm.KeyTimes.Count;
                    Ipm.KeyTimes.Add(keyTime);
                }
                return (short)Convert.ToUInt16(idx);
            }

            short AddSourceKeyTime(double sourceKeyTime)
            {
                return AddKeyTime(GetLowerPrecisionValue((float)(sourceKeyTime * keyTimeMultiplier)));
            }

            short AddKeyValue(float keyValue)
            {
                var idx = Ipm.KeyValues.IndexOf(keyValue);
                if (idx < 0)
                {
                    idx = Ipm.KeyValues.Count;
                    Ipm.KeyValues.Add(keyValue);
                }
                return (short)Convert.ToUInt16(idx);
            }

            for (int boneIdx = 0; boneIdx < parm.BoneCount; boneIdx++)
            {
                var hit = parm.GetAChannel(boneIdx);
                if (hit != null)
                {
                    float FixScaling(float value)
                    {
                        return ((boneIdx == 0) ? parm.NodeScaling : 1f) * value;
                    }

                    float FixScalingValue(float value) => GetLowerPrecisionValue(FixScaling(value));

                    float FixPos(float value)
                    {
                        return value / ((boneIdx == 0) ? 1 : parm.NodeScaling) * parm.PositionScaling;
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
                        if (channel.keys?.Any() ?? false)
                        {
                            jointFlag |= channel.jointFlags;

                            var numKeys = Convert.ToByte(channel.keys.Count());

                            var lastKeyIdx = Ipm.FCurveKeys.Count;

                            Ipm.FCurvesForward.Add(
                                new FCurve
                                {
                                    JointId = Convert.ToInt16(boneIdx),
                                    Channel = (byte)(channel.type),
                                    KeyStartId = Convert.ToInt16(lastKeyIdx),
                                    KeyCount = numKeys,
                                }
                            );

                            Ipm.FCurveKeys.AddRange(
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

                Ipm.Joints.Add(
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
                foreach (var (fCurve, idx) in Ipm.FCurvesForward
                    .Select((fCurve, idx) => (fCurve, idx))
                    .ToArray()
                )
                {
                    var thisKeys = Ipm.FCurveKeys
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

                        Ipm.InitialPoses.Add(
                            new InitialPose
                            {
                                BoneId = fCurve.JointId,
                                ChannelValue = fCurve.ChannelValue,
                                Value = Ipm.KeyValues[sames.Single()],
                            }
                        );

                        Ipm.FCurvesForward.Remove(fCurve);
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

                logger.Debug($"FCurveKeys count reduced from {Ipm.FCurveKeys.Count:#,##0} to {reducedFCurveKeys.Count:#,##0}");

                Ipm.FCurveKeys.Clear();
                Ipm.FCurveKeys.AddRange(reducedFCurveKeys);
            }

            // sort key time in ascending order
            {
                var newKeyTimes = Ipm.KeyTimes
                    .OrderBy(it => it)
                    .ToArray();

                foreach (var fCurveKey in Ipm.FCurveKeys)
                {
                    fCurveKey.Time = (short)Convert.ToUInt16(Array.IndexOf(newKeyTimes, Ipm.KeyTimes[fCurveKey.Time]));
                }

                Ipm.KeyTimes.Clear();
                Ipm.KeyTimes.AddRange(newKeyTimes);
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

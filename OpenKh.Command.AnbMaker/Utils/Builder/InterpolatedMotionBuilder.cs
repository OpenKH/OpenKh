using NLog;
using OpenKh.Command.AnbMaker.Extensions;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using System.Numerics;
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

            logger.Info($"BoneCount {parm.BoneCount}");
            logger.Debug($"Frame {0} to {frameCount - 1}, framesPerSecond {parm.TicksPerSecond}, consumes {frameCount * 1.0f / parm.TicksPerSecond:0.0} seconds");

            Ipm.InterpolatedMotionHeader.BoneCount = Convert.ToInt16(parm.BoneCount);
            Ipm.InterpolatedMotionHeader.TotalBoneCount = Convert.ToInt16(parm.BoneCount);
            Ipm.InterpolatedMotionHeader.FrameCount = (int)(frameCount * keyTimeMultiplier); // in 1/60 seconds
            Ipm.InterpolatedMotionHeader.FrameData.FrameStart = 0;
            Ipm.InterpolatedMotionHeader.FrameData.FrameEnd = frameCount;
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
                var idx = Ipm.KeyValues.IndexOf(GetLowerPrecisionValue2(keyValue));
                if (idx < 0)
                {
                    idx = Ipm.KeyValues.Count;
                    Ipm.KeyValues.Add(GetLowerPrecisionValue2(keyValue));
                }
                return (short)Convert.ToUInt16(idx);
            }

            var initialPoseDict = new SortedDictionary<InitialPoseKey, float>(
                ComparerOfInitialPoseKey.Instance
            );

            for (int boneIdx = 0; boneIdx < parm.BoneCount; boneIdx++)
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

                var hit = parm.GetAChannel(boneIdx);
                if (hit != null)
                {
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
                            type = Channel.ROTATION_X,
                            jointFlags = JointFlags.HasRotation,
                            keys = hit.RotationXKeys,
                            fixValue = it => it,
                        },
                        new ChannelProvider
                        {
                            type = Channel.ROTATION_Y,
                            jointFlags = JointFlags.HasRotation,
                            keys = hit.RotationYKeys,
                            fixValue = it => it,
                        },
                        new ChannelProvider
                        {
                            type = Channel.ROTATION_Z,
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

                    fixRotations(channels);

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
                                            Type = Interpolation.Linear,
                                            Time = AddSourceKeyTime(key.Time),
                                            ValueId = AddKeyValue(channel.fixValue(key.Value)),
                                        }
                                    )
                            );
                        }
                    }
                }

                {
                    Matrix4x4.Decompose(
                        parm.GetInitialMatrix(boneIdx),
                        out Vector3 scale,
                        out Quaternion quaternion,
                        out Vector3 translation
                    );

                    Vector3 rotation;
                    if (quaternion.IsIdentity)
                    {
                        rotation = Vector3.Zero;
                    }
                    else
                    {
                        rotation = quaternion.ToEulerAngles();
                    }


                    initialPoseDict[new InitialPoseKey(boneIdx, Channel.SCALE_X)] = FixScalingValue(scale.X);
                    initialPoseDict[new InitialPoseKey(boneIdx, Channel.SCALE_Y)] = FixScalingValue(scale.Y);
                    initialPoseDict[new InitialPoseKey(boneIdx, Channel.SCALE_Z)] = FixScalingValue(scale.Z);
                    initialPoseDict[new InitialPoseKey(boneIdx, Channel.ROTATION_X)] = rotation.X;
                    initialPoseDict[new InitialPoseKey(boneIdx, Channel.ROTATION_Y)] = rotation.Y;
                    initialPoseDict[new InitialPoseKey(boneIdx, Channel.ROTATION_Z)] = rotation.Z;
                    initialPoseDict[new InitialPoseKey(boneIdx, Channel.TRANSLATION_X)] = FixPosValue(translation.X);
                    initialPoseDict[new InitialPoseKey(boneIdx, Channel.TRANSLATION_Y)] = FixPosValue(translation.Y);
                    initialPoseDict[new InitialPoseKey(boneIdx, Channel.TRANSLATION_Z)] = FixPosValue(translation.Z);
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

                        initialPoseDict[new InitialPoseKey(fCurve.JointId, fCurve.ChannelValue)] = Ipm.KeyValues[sames.Single()];

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

                foreach (var set in initialPoseDict)
                {
                    Ipm.InitialPoses.Add(
                        new InitialPose
                        {
                            BoneId = Convert.ToInt16(set.Key.BoneIndex),
                            ChannelValue = set.Key.Type,
                            Value = set.Value,
                        }
                    );
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

        private static float GetLowerPrecisionValue2(float value)
        {
            return (float)Math.Round(value, 3);
        }

        private class ChannelProvider
        {
            internal Channel type;
            internal JointFlags jointFlags;
            internal AScalarKey[] keys;
            internal Func<float, float> fixValue;
        }

        private record InitialPoseKey(int BoneIndex, Channel Type);

        private class ComparerOfInitialPoseKey : IComparer<InitialPoseKey>
        {
            public static readonly ComparerOfInitialPoseKey Instance = new ComparerOfInitialPoseKey();

            private static readonly InitialPoseKey _fallback = new InitialPoseKey(-1, Channel.UNKNOWN);

            public int Compare(InitialPoseKey? left, InitialPoseKey? right)
            {
                left = left ?? _fallback;
                right = right ?? _fallback;
                var diff = left.BoneIndex.CompareTo(right.BoneIndex);
                if (diff == 0)
                {
                    diff = left.Type.CompareTo(right.Type);
                }
                return diff;
            }
        }

        // Fix by Some1fromthedark
        private float unwrapAngle(float previous_angle, float current_angle)
        {
            float diff = current_angle - previous_angle;
            if (diff < -Math.PI)
            {
                while (diff < -Math.PI)
                {
                    current_angle += (float)(2 * Math.PI);
                    diff = current_angle - previous_angle;
                }
            }
            else if (diff > Math.PI)
            {
                while (Math.PI < diff)
                {
                    current_angle -= (float)(2 * Math.PI);
                    diff = current_angle - previous_angle;
                }
            }
            return current_angle;
        }

        private void fixRotations(ChannelProvider[] channels)
        {
            foreach (ChannelProvider channel in channels)
            {
                if (channel.type == Channel.ROTATION_X ||
                    channel.type == Channel.ROTATION_Y ||
                    channel.type == Channel.ROTATION_Z)
                {
                    float previousAngle = 0;
                    foreach (var k in channel.keys)
                    {
                        k.Value = unwrapAngle(previousAngle, k.Value);
                        previousAngle = k.Value;
                    }
                }
            }
        }
    }
}

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
                return AddKeyTime((float)(sourceKeyTime * keyTimeMultiplier));
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
                    var jointFlag = JointFlags.None;

                    if (hit.ScalingKeyCount != 0)
                    {
                        jointFlag |= JointFlags.HasScaling;

                        var numKeys = Convert.ToByte(hit.ScalingKeyCount);

                        Key[] xKeys;
                        Key[] yKeys;
                        Key[] zKeys;

                        {
                            var lastKeyIdx = ipm.FCurveKeys.Count;

                            ipm.FCurvesForward.Add(
                                new FCurve
                                {
                                    JointId = Convert.ToInt16(boneIdx),
                                    Channel = (byte)(Channel.SCALE_X),
                                    KeyStartId = Convert.ToInt16(lastKeyIdx),
                                    KeyCount = numKeys,
                                }
                            );

                            ipm.FCurveKeys.AddRange(
                                xKeys = Enumerable.Range(0, numKeys)
                                    .Select(_ => new Key { })
                                    .ToArray()
                            );
                        }

                        {
                            var lastKeyIdx = ipm.FCurveKeys.Count;

                            ipm.FCurvesForward.Add(
                                new FCurve
                                {
                                    JointId = Convert.ToInt16(boneIdx),
                                    Channel = (byte)(Channel.SCALE_Y),
                                    KeyStartId = Convert.ToInt16(lastKeyIdx),
                                    KeyCount = numKeys,
                                }
                            );

                            ipm.FCurveKeys.AddRange(
                                yKeys = Enumerable.Range(0, numKeys)
                                    .Select(_ => new Key { })
                                    .ToArray()
                            );
                        }


                        {
                            var lastKeyIdx = ipm.FCurveKeys.Count;

                            ipm.FCurvesForward.Add(
                                new FCurve
                                {
                                    JointId = Convert.ToInt16(boneIdx),
                                    Channel = (byte)(Channel.SCALE_Z),
                                    KeyStartId = Convert.ToInt16(lastKeyIdx),
                                    KeyCount = numKeys,
                                }
                            );

                            ipm.FCurveKeys.AddRange(
                                zKeys = Enumerable.Range(0, numKeys)
                                    .Select(_ => new Key { })
                                    .ToArray()
                            );
                        }

                        float FixScaling(float value)
                        {
                            return ((boneIdx == 0) ? NodeScaling : 1f) * value;
                        }

                        foreach (var (key, idx) in hit.ScalingKeys.Select((key, idx) => (key, idx)))
                        {
                            var keyTimeIdx = AddSourceKeyTime(key.Time);

                            xKeys[idx].Type = Interpolation.Hermite;
                            xKeys[idx].Time = keyTimeIdx;

                            yKeys[idx].Type = Interpolation.Hermite;
                            yKeys[idx].Time = keyTimeIdx;

                            zKeys[idx].Type = Interpolation.Hermite;
                            zKeys[idx].Time = keyTimeIdx;

                            xKeys[idx].ValueId = AddKeyValue(GetLowerPrecisionValue(FixScaling(key.Value.X)));
                            yKeys[idx].ValueId = AddKeyValue(GetLowerPrecisionValue(FixScaling(key.Value.Y)));
                            zKeys[idx].ValueId = AddKeyValue(GetLowerPrecisionValue(FixScaling(key.Value.Z)));
                        }
                    }

                    if (hit.RotationKeyCount != 0)
                    {
                        jointFlag |= JointFlags.HasRotation;

                        var numKeys = Convert.ToByte(hit.RotationKeyCount);

                        Key[] xKeys;
                        Key[] yKeys;
                        Key[] zKeys;

                        {
                            var lastKeyIdx = ipm.FCurveKeys.Count;

                            ipm.FCurvesForward.Add(
                                new FCurve
                                {
                                    JointId = Convert.ToInt16(boneIdx),
                                    Channel = (byte)(Channel.ROTATATION_X),
                                    KeyStartId = Convert.ToInt16(lastKeyIdx),
                                    KeyCount = numKeys,
                                }
                            );

                            ipm.FCurveKeys.AddRange(
                                xKeys = Enumerable.Range(0, numKeys)
                                    .Select(_ => new Key { })
                                    .ToArray()
                            );
                        }

                        {
                            var lastKeyIdx = ipm.FCurveKeys.Count;

                            ipm.FCurvesForward.Add(
                                new FCurve
                                {
                                    JointId = Convert.ToInt16(boneIdx),
                                    Channel = (byte)(Channel.ROTATATION_Y),
                                    KeyStartId = Convert.ToInt16(lastKeyIdx),
                                    KeyCount = numKeys,
                                }
                            );

                            ipm.FCurveKeys.AddRange(
                                yKeys = Enumerable.Range(0, numKeys)
                                    .Select(_ => new Key { })
                                    .ToArray()
                            );
                        }


                        {
                            var lastKeyIdx = ipm.FCurveKeys.Count;

                            ipm.FCurvesForward.Add(
                                new FCurve
                                {
                                    JointId = Convert.ToInt16(boneIdx),
                                    Channel = (byte)(Channel.ROTATATION_Z),
                                    KeyStartId = Convert.ToInt16(lastKeyIdx),
                                    KeyCount = numKeys,
                                }
                            );

                            ipm.FCurveKeys.AddRange(
                                zKeys = Enumerable.Range(0, numKeys)
                                    .Select(_ => new Key { })
                                    .ToArray()
                            );
                        }

                        foreach (var (key, idx) in hit.RotationKeys.Select((key, idx) => (key, idx)))
                        {
                            var keyTimeIdx = AddSourceKeyTime(key.Time);

                            xKeys[idx].Type = Interpolation.Hermite;
                            xKeys[idx].Time = keyTimeIdx;

                            yKeys[idx].Type = Interpolation.Hermite;
                            yKeys[idx].Time = keyTimeIdx;

                            zKeys[idx].Type = Interpolation.Hermite;
                            zKeys[idx].Time = keyTimeIdx;

                            var angles = ToEulerAngles(key.Value);

                            static float FromEulerAngle(float angle) => angle;

                            xKeys[idx].ValueId = AddKeyValue(FromEulerAngle(angles.X));
                            yKeys[idx].ValueId = AddKeyValue(FromEulerAngle(angles.Y));
                            zKeys[idx].ValueId = AddKeyValue(FromEulerAngle(angles.Z));
                        }
                    }

                    if (hit.PositionKeyCount != 0)
                    {
                        jointFlag |= JointFlags.HasPosition;

                        var numKeys = Convert.ToByte(hit.PositionKeyCount);

                        Key[] xKeys;
                        Key[] yKeys;
                        Key[] zKeys;

                        {
                            var lastKeyIdx = ipm.FCurveKeys.Count;

                            ipm.FCurvesForward.Add(
                                new FCurve
                                {
                                    JointId = Convert.ToInt16(boneIdx),
                                    Channel = (byte)(Channel.TRANSLATION_X),
                                    KeyStartId = Convert.ToInt16(lastKeyIdx),
                                    KeyCount = numKeys,
                                }
                            );

                            ipm.FCurveKeys.AddRange(
                                xKeys = Enumerable.Range(0, numKeys)
                                    .Select(_ => new Key { })
                                    .ToArray()
                            );
                        }

                        {
                            var lastKeyIdx = ipm.FCurveKeys.Count;

                            ipm.FCurvesForward.Add(
                                new FCurve
                                {
                                    JointId = Convert.ToInt16(boneIdx),
                                    Channel = (byte)(Channel.TRANSLATION_Y),
                                    KeyStartId = Convert.ToInt16(lastKeyIdx),
                                    KeyCount = numKeys,
                                }
                            );

                            ipm.FCurveKeys.AddRange(
                                yKeys = Enumerable.Range(0, numKeys)
                                    .Select(_ => new Key { })
                                    .ToArray()
                            );
                        }


                        {
                            var lastKeyIdx = ipm.FCurveKeys.Count;

                            ipm.FCurvesForward.Add(
                                new FCurve
                                {
                                    JointId = Convert.ToInt16(boneIdx),
                                    Channel = (byte)(Channel.TRANSLATION_Z),
                                    KeyStartId = Convert.ToInt16(lastKeyIdx),
                                    KeyCount = numKeys,
                                }
                            );

                            ipm.FCurveKeys.AddRange(
                                zKeys = Enumerable.Range(0, numKeys)
                                    .Select(_ => new Key { })
                                    .ToArray()
                            );
                        }

                        float FixPos(float value)
                        {
                            return value / ((boneIdx == 0) ? 1 : NodeScaling);
                        }

                        foreach (var (key, idx) in hit.PositionKeys.Select((key, idx) => (key, idx)))
                        {
                            var keyTimeIdx = AddSourceKeyTime(key.Time);

                            xKeys[idx].Type = Interpolation.Hermite;
                            xKeys[idx].Time = keyTimeIdx;

                            yKeys[idx].Type = Interpolation.Hermite;
                            yKeys[idx].Time = keyTimeIdx;

                            zKeys[idx].Type = Interpolation.Hermite;
                            zKeys[idx].Time = keyTimeIdx;

                            xKeys[idx].ValueId = AddKeyValue(GetLowerPrecisionValue(FixPos(key.Value.X)));
                            yKeys[idx].ValueId = AddKeyValue(GetLowerPrecisionValue(FixPos(key.Value.Y)));
                            zKeys[idx].ValueId = AddKeyValue(GetLowerPrecisionValue(FixPos(key.Value.Z)));
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
                    var sames = ipm.FCurveKeys
                        .Skip(fCurve.KeyStartId)
                        .Take(fCurve.KeyCount)
                        .Select(key => ipm.KeyValues[key.ValueId])
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
                                Value = sames.Single(),
                            }
                        );

                        ipm.FCurvesForward.Remove(fCurve);
                    }
                    else
                    {
                        var fCurveKeyReuseKey = string.Join(
                            " ",
                            ipm.FCurveKeys.Skip(fCurve.KeyStartId).Take(fCurve.KeyCount)
                                .Select(it => $"{it.Time:x4},{it.ValueId:x4},{it.LeftTangentId:x4},{it.RightTangentId:x4}")
                        );

                        if (fCurveKeyReuseTable.TryGetValue(fCurveKeyReuseKey, out ushort foundIdx))
                        {
                            fCurve.KeyStartId = (short)foundIdx;
                        }
                        else
                        {
                            var newIdx = Convert.ToUInt16(reducedFCurveKeys.Count);

                            reducedFCurveKeys.AddRange(
                                ipm.FCurveKeys
                                    .Skip(fCurve.KeyStartId)
                                    .Take(fCurve.KeyCount)
                            );

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

        /// <summary>
        /// ToEulerAngles
        /// </summary>
        /// <see cref="https://stackoverflow.com/a/70462919"/>
        private static Vector3 ToEulerAngles(Quaternion q)
        {
            Vector3 angles = new();

            // roll / x
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch / y
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }

            // yaw / z
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }
    }
}

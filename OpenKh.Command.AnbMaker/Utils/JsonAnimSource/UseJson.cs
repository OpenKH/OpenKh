using OpenKh.Command.AnbMaker.Extensions;
using OpenKh.Command.AnbMaker.Utils.Builder;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.JsonAnimSource
{
    public class UseJson
    {
        public IEnumerable<InterpolatedMotionBuilder.Parameter> Parameters { get; }

        public UseJson(
            string inputModel,
            string meshName,
            string rootName,
            string animationName,
            float nodeScaling
        )
        {
            var model = JsonSerializer.Deserialize<BRoot>(
                File.ReadAllText(inputModel),
                new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                }
            );

            if (model.Version != "1")
            {
                throw new Exception("Invalid JSON source version");
            }

            bool TestAnimName(string it) => string.IsNullOrEmpty(animationName) ? true : animationName == it;

            Parameters = model.Objects
                .Where(obj => true
                    && obj.Type == "ARMATURE"
                    && obj.Bones != null
                    && obj.AnimationAction != null
                    && TestAnimName(obj.AnimationAction.Name)
                )
                .Select(
                    obj =>
                    {
                        var action = obj.AnimationAction;

                        AScalarKey GetKey(BKeyFrame source, float valueAdd = 0)
                        {
                            return new AScalarKey
                            {
                                Time = source.Time - action.FrameStart,
                                Value = source.Value + valueAdd,
                            };
                        }

                        AScalarKey GetRotationKey(PseudoRotation rotation, float value)
                        {
                            return new AScalarKey
                            {
                                Time = (float)(rotation.time - action.FrameStart),
                                Value = value,
                            };
                        }

                        return new InterpolatedMotionBuilder.Parameter
                        {
                            BoneCount = obj.Bones.Count(),
                            DurationInTicks = (int)(action.FrameEnd - action.FrameStart),
                            TicksPerSecond = NzFloat(obj.Fps, 25),
                            NodeScaling = nodeScaling,
                            GetAChannel = boneIdx =>
                            {
                                var bone = obj.Bones[boneIdx];
                                var group = action.Groups.FirstOrDefault(it => it.Name == bone.Name);

                                var fcurves = group?.Channels ?? new BFCurve[0];

                                BFCurve ProvideFallback(string channelRef, float fallbackValue)
                                {
                                    return new BFCurve
                                    {
                                        ChannelRef = channelRef,
                                        KeyFrames = new BKeyFrame[]
                                        {
                                            new BKeyFrame
                                            {
                                                Time = 0,
                                                Value = fallbackValue,
                                            },
                                            new BKeyFrame
                                            {
                                                Time = action.FrameEnd - action.FrameStart,
                                                Value = fallbackValue
                                            },
                                        }
                                    };
                                }

                                var Qw = (fcurves
                                    .FirstOrDefault(it => it.ChannelRef == "rotation_quaternion.0") ?? ProvideFallback("rotation_quaternion.0", 1))
                                    .KeyFrames;
                                var Qx = (fcurves
                                    .FirstOrDefault(it => it.ChannelRef == "rotation_quaternion.1") ?? ProvideFallback("rotation_quaternion.1", 0))
                                    .KeyFrames;
                                var Qy = (fcurves
                                    .FirstOrDefault(it => it.ChannelRef == "rotation_quaternion.2") ?? ProvideFallback("rotation_quaternion.2", 0))
                                    .KeyFrames;
                                var Qz = (fcurves
                                    .FirstOrDefault(it => it.ChannelRef == "rotation_quaternion.3") ?? ProvideFallback("rotation_quaternion.3", 0))
                                    .KeyFrames;

                                var pseudoRotations = new List<PseudoRotation>();

                                if (new int[] { Qw.Length, Qx.Length, Qy.Length, Qz.Length }.Distinct().Count() == 1)
                                {
                                    pseudoRotations.AddRange(
                                        Enumerable.Range(0, Qw.Length)
                                            .Select(
                                                idx =>
                                                {
                                                    return new PseudoRotation
                                                    {
                                                        time = Qw[idx].Time,
                                                        rotation = (
                                                            Quaternion.Identity
                                                            * new Quaternion(
                                                                w: bone.Rotation[0],
                                                                x: bone.Rotation[1],
                                                                y: bone.Rotation[2],
                                                                z: bone.Rotation[3]
                                                            )
                                                            * new Quaternion(
                                                                x: Qx[idx].Value,
                                                                y: Qy[idx].Value,
                                                                z: Qz[idx].Value,
                                                                w: Qw[idx].Value
                                                            )
                                                        )
                                                            .ToEulerAngles()
                                                    };
                                                }
                                            )
                                    );
                                }

                                if (!pseudoRotations.Any())
                                {
                                    pseudoRotations.Add(
                                        new PseudoRotation
                                        {
                                            time = 0,
                                            rotation = new Quaternion(
                                                w: bone.Rotation[0],
                                                x: bone.Rotation[1],
                                                y: bone.Rotation[2],
                                                z: bone.Rotation[3]
                                            )
                                                .ToEulerAngles(),
                                        }
                                    );
                                }

                                return new AChannel
                                {
                                    ScaleXKeys = (fcurves
                                        .FirstOrDefault(it => it.ChannelRef == "scale.0") ?? ProvideFallback("scale.0", bone.Scale[0]))
                                        .KeyFrames
                                        .Select(it => GetKey(it))
                                        .ToArray(),

                                    ScaleYKeys = (fcurves
                                        .FirstOrDefault(it => it.ChannelRef == "scale.1") ?? ProvideFallback("scale.1", bone.Scale[1]))
                                        .KeyFrames
                                        .Select(it => GetKey(it))
                                        .ToArray(),

                                    ScaleZKeys = (fcurves
                                        .FirstOrDefault(it => it.ChannelRef == "scale.2") ?? ProvideFallback("scale.2", bone.Scale[2]))
                                        .KeyFrames
                                        .Select(it => GetKey(it))
                                        .ToArray(),

                                    RotationXKeys = pseudoRotations
                                        .Select(it => GetRotationKey(it, it.rotation.X))
                                        .ToArray(),

                                    RotationYKeys = pseudoRotations
                                        .Select(it => GetRotationKey(it, it.rotation.Y))
                                        .ToArray(),

                                    RotationZKeys = pseudoRotations
                                        .Select(it => GetRotationKey(it, it.rotation.Z))
                                        .ToArray(),

                                    PositionXKeys = (fcurves
                                        .FirstOrDefault(it => it.ChannelRef == "location.0") ?? ProvideFallback("location.0", bone.Translation[0]))
                                        .KeyFrames
                                        .Select(it => GetKey(it, bone.Translation[0]))
                                        .ToArray(),

                                    PositionYKeys = (fcurves
                                        .FirstOrDefault(it => it.ChannelRef == "location.1") ?? ProvideFallback("location.1", bone.Translation[1]))
                                        .KeyFrames
                                        .Select(it => GetKey(it, bone.Translation[1]))
                                        .ToArray(),

                                    PositionZKeys = (fcurves
                                        .FirstOrDefault(it => it.ChannelRef == "location.2") ?? ProvideFallback("location.2", bone.Translation[2]))
                                        .KeyFrames
                                        .Select(it => GetKey(it, bone.Translation[2]))
                                        .ToArray(),
                                };
                            }
                        };
                    }
                )
                .ToArray();
        }

        private class PseudoRotation
        {
            internal double time;
            internal Vector3 rotation;
        }

        private static float NzFloat(float value, float fallback)
        {
            return (value == 0) ? fallback : value;
        }
    }
}

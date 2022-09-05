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

                        AScalarKey GetKey(BKeyFrame source)
                        {
                            return new AScalarKey
                            {
                                Time = source.Time - action.FrameStart,
                                Value = source.Value,
                            };
                        }

                        AScalarKey GetRotationKey(PseudoRotation rotation, float value)
                        {
                            return new AScalarKey
                            {
                                Time = (float)rotation.time,
                                Value = value,
                            };
                        }

                        return new InterpolatedMotionBuilder.Parameter
                        {
                            BoneCount = obj.Bones.Count(),
                            DurationInTicks = (int)(action.FrameEnd - action.FrameStart),
                            TicksPerSecond = obj.Fps,
                            NodeScaling = nodeScaling,
                            GetAChannel = boneIdx =>
                            {
                                var fcurves = action.Groups[boneIdx].Channels;

                                var Qw = fcurves
                                    .First(it => it.ChannelRef == "rotation_quaternion.0")
                                    .KeyFrames;
                                var Qx = fcurves
                                    .First(it => it.ChannelRef == "rotation_quaternion.1")
                                    .KeyFrames;
                                var Qy = fcurves
                                    .First(it => it.ChannelRef == "rotation_quaternion.2")
                                    .KeyFrames;
                                var Qz = fcurves
                                    .First(it => it.ChannelRef == "rotation_quaternion.3")
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
                                                        rotation = new Quaternion(
                                                            Qx[idx].Value,
                                                            Qy[idx].Value,
                                                            Qz[idx].Value,
                                                            Qw[idx].Value
                                                        )
                                                            .ToEulerAngles()
                                                    };
                                                }
                                            )
                                    );
                                }

                                return new AChannel
                                {
                                    ScaleXKeys = fcurves
                                        .First(it => it.ChannelRef == "scale.0")
                                        .KeyFrames
                                        .Select(it => GetKey(it))
                                        .ToArray(),

                                    ScaleYKeys = fcurves
                                        .First(it => it.ChannelRef == "scale.1")
                                        .KeyFrames
                                        .Select(it => GetKey(it))
                                        .ToArray(),

                                    ScaleZKeys = fcurves
                                        .First(it => it.ChannelRef == "scale.2")
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

                                    PositionXKeys = fcurves
                                        .First(it => it.ChannelRef == "location.0")
                                        .KeyFrames
                                        .Select(it => GetKey(it))
                                        .ToArray(),

                                    PositionYKeys = fcurves
                                        .First(it => it.ChannelRef == "location.1")
                                        .KeyFrames
                                        .Select(it => GetKey(it))
                                        .ToArray(),

                                    PositionZKeys = fcurves
                                        .First(it => it.ChannelRef == "location.2")
                                        .KeyFrames
                                        .Select(it => GetKey(it))
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
    }
}

using OpenKh.Command.AnbMaker.Extensions;
using OpenKh.Command.AnbMaker.Utils.Builder;
using OpenKh.Command.AnbMaker.Utils.Builder.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.AssimpAnimSource
{
    public class UseAssimp
    {
        public IEnumerable<BasicSourceMotion> Parameters { get; }

        public UseAssimp(
            string inputModel,
            string meshName,
            string rootName,
            string animationName,
            float nodeScaling
        )
        {
            var outputList = new List<BasicSourceMotion>();
            Parameters = outputList;

            var assimp = new Assimp.AssimpContext();
            var scene = assimp.ImportFile(inputModel, Assimp.PostProcessSteps.None);

            bool IsMeshNameMatched(string it) =>
                string.IsNullOrEmpty(meshName)
                    ? true
                    : it == meshName;

            bool IsAnimationNameMatched(string it) =>
                string.IsNullOrEmpty(animationName)
                    ? true
                    : it == animationName;

            foreach (var fbxMesh in scene.Meshes.Where(mesh => IsMeshNameMatched(mesh.Name)))
            {
                var fbxArmatureRoot = scene.RootNode.FindNode(rootName ?? "bone000"); //"kh_sk"
                var fbxArmatureNodes = AssimpHelper.FlattenNodes(fbxArmatureRoot);
                var fbxArmatureBoneCount = fbxArmatureNodes.Length;

                foreach (var fbxAnim in scene.Animations.Where(anim => IsAnimationNameMatched(anim.Name)))
                {
                    AScalarKey GetAScalarKey(double time, float value) =>
                        new AScalarKey
                        {
                            Time = (float)time,
                            Value = value,
                        };

                    var output = new BasicSourceMotion
                    {
                        DurationInTicks = (int)fbxAnim.DurationInTicks,
                        TicksPerSecond = (float)fbxAnim.TicksPerSecond,
                        BoneCount = fbxArmatureBoneCount,
                        NodeScaling = nodeScaling,
                        GetAChannel = boneIdx =>
                        {
                            var name = fbxArmatureNodes[boneIdx].ArmatureNode.Name;
                            var hit = fbxAnim.NodeAnimationChannels.FirstOrDefault(it => it.NodeName == name);
                            if (hit != null)
                            {
                                var pseudoRotations = hit.RotationKeys
                                    .Select(
                                        key => new PseudoRotation
                                        {
                                            time = key.Time,
                                            rotation = key.Value.ToDotNetQuaternion().ToEulerAngles()
                                        }
                                    )
                                    .ToArray();

                                return new AChannel
                                {
                                    ScaleXKeys = hit.ScalingKeys
                                        .Select(it => GetAScalarKey(it.Time, it.Value.X))
                                        .ToArray(),

                                    ScaleYKeys = hit.ScalingKeys
                                        .Select(it => GetAScalarKey(it.Time, it.Value.Y))
                                        .ToArray(),

                                    ScaleZKeys = hit.ScalingKeys
                                        .Select(it => GetAScalarKey(it.Time, it.Value.Z))
                                        .ToArray(),

                                    RotationXKeys = pseudoRotations
                                        .Select(it => GetAScalarKey(it.time, it.rotation.X))
                                        .ToArray(),
                                    RotationYKeys = pseudoRotations
                                        .Select(it => GetAScalarKey(it.time, it.rotation.Y))
                                        .ToArray(),
                                    RotationZKeys = pseudoRotations
                                        .Select(it => GetAScalarKey(it.time, it.rotation.Z))
                                        .ToArray(),

                                    PositionXKeys = hit.PositionKeys
                                        .Select(it => GetAScalarKey(it.Time, it.Value.X))
                                        .ToArray(),

                                    PositionYKeys = hit.PositionKeys
                                        .Select(it => GetAScalarKey(it.Time, it.Value.Y))
                                        .ToArray(),

                                    PositionZKeys = hit.PositionKeys
                                        .Select(it => GetAScalarKey(it.Time, it.Value.Z))
                                        .ToArray(),
                                };
                            }

                            return null;
                        },
                        Bones = fbxArmatureNodes
                            .Select(
                                node => new ABone
                                {
                                    NodeName = node.ArmatureNode.Name,
                                    ParentIndex = node.ParentIndex,
                                }
                            )
                            .ToArray(),
                    };

                    outputList.Add(output);
                }
            }
        }

        private class PseudoRotation
        {
            internal double time;
            internal Vector3 rotation;
        }
    }
}

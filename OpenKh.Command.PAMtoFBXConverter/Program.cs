using OpenKh.Engine.Parsers;
using OpenKh.Imaging;

using McMaster.Extensions.CommandLineUtils;
using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.ComponentModel.DataAnnotations;
using OpenKh.Bbs;
using System.Collections.Generic;
using System.Numerics;
using OpenKh.Common.Utils;
using Assimp;

namespace OpenKh.Command.PAMtoFBXConverter
{
    [Command("OpenKh.Command.PAMtoFBXConverter")]
    [VersionOptionFromMember("--version", MemberName = nameof(GetVersion))]
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                CommandLineApplication.Execute<Program>(args);
            }
            catch (FileNotFoundException e)
            {
                Console.WriteLine($"The file {e.FileName} cannot be found. The program will now exit.");
            }
            catch (Exception e)
            {
                Console.WriteLine($"FATAL ERROR: {e.Message}\n{e.StackTrace}");
            }
        }

        private static string GetVersion()
            => typeof(Program).Assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>().InformationalVersion;

        [Required]
        [Argument(0, "PMO File", "The PMO to use as a base for the conversion.")]
        public string PMO_FileName { get; }

        [Required]
        [Argument(1, "PAM File", "The PAM animation set to use for the conversion.")]
        public string PAM_FileName { get; }

        private void OnExecute()
        {
            try
            {
                Convert(PMO_FileName, PAM_FileName);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"ERROR: {ex.Message}");
            }
        }

        private static void Convert(string PMO_Path, string PAM_Path)
        {
            Stream pmoStream = File.OpenRead(PMO_Path);
            Stream pamStream = File.OpenRead(PAM_Path);
            Pmo pmo = Pmo.Read(pmoStream);
            Pam pam = Pam.Read(pamStream);

            Assimp.Scene nScene = GetPMOScene(pmo);
            List<Assimp.Animation> FBXAnims = PAMtoFBXAnim(pmo, pam);
            nScene.Animations.AddRange(FBXAnims);

            pmoStream.Close();
            pamStream.Close();

            using var ctx = new AssimpContext();
            ctx.ExportFile(nScene, "Test.fbx", "fbx");

        }

        private static Assimp.Scene GetPMOScene(Pmo pmo)
        {
            Assimp.Scene scene = new Assimp.Scene();
            scene.RootNode = new Assimp.Node("root");

            // Add materials.
            List<Material> matList = new List<Material>();
            for (int t = 0; t < pmo.header.TextureCount; t++)
            {
                Material mat = new Material();
                mat.Clear();
                mat.Name = pmo.textureInfo[t].TextureName;
                scene.Materials.Add(mat);
            }

            // Add skeleton.
            List<Node> Skeleton = new List<Node>();
            for (int b = 0; b < pmo.skeletonHeader.BoneCount; b++)
            {
                Pmo.BoneData bn = pmo.boneList[b];

                Assimp.Matrix4x4 mtx = new Assimp.Matrix4x4();
                mtx.A1 = bn.Transform.M11;
                mtx.A2 = bn.Transform.M12;
                mtx.A3 = bn.Transform.M13;
                mtx.A4 = bn.Transform.M14;
                mtx.B1 = bn.Transform.M21;
                mtx.B2 = bn.Transform.M22;
                mtx.B3 = bn.Transform.M23;
                mtx.B4 = bn.Transform.M24;
                mtx.C1 = bn.Transform.M31;
                mtx.C2 = bn.Transform.M32;
                mtx.C3 = bn.Transform.M33;
                mtx.C4 = bn.Transform.M34;
                mtx.D1 = bn.Transform.M41;
                mtx.D2 = bn.Transform.M42;
                mtx.D3 = bn.Transform.M43;
                mtx.D4 = bn.Transform.M44;

                Assimp.Matrix4x4 nd_mtx = mtx;
                nd_mtx.Transpose();
                if (bn.ParentBoneIndex == 0xFFFF)
                {

                    Node curNode = new Node(bn.JointName);
                    curNode.Transform = nd_mtx;
                    scene.RootNode.Children.Add(curNode);
                    Skeleton.Add(curNode);
                }
                else
                {
                    Node curNode = new Node(bn.JointName, Skeleton[bn.ParentBoneIndex]);

                    nd_mtx.A4 *= 100.0f;
                    nd_mtx.B4 *= 100.0f;
                    nd_mtx.C4 *= 100.0f;

                    curNode.Transform = nd_mtx;
                    Skeleton.Add(curNode);
                    scene.RootNode.FindNode(Skeleton[bn.ParentBoneIndex].Name).Children.Add(curNode);
                }
            }

            // Add meshes.
            for (int i = 0; i < pmo.Meshes.Count; i++)
            {
                Assimp.Mesh mesh = new Assimp.Mesh($"Mesh{i}", Assimp.PrimitiveType.Triangle);
                Pmo.MeshChunks chunk = pmo.Meshes[i];

                // Add vertices, vertex color and normals.
                for (int j = 0; j < chunk.vertices.Count; j++)
                {
                    mesh.Vertices.Add(new Assimp.Vector3D(
                        chunk.vertices[j].X * pmo.header.ModelScale * 100.0f,
                        chunk.vertices[j].Y * pmo.header.ModelScale * 100.0f,
                        chunk.vertices[j].Z * pmo.header.ModelScale * 100.0f));

                    mesh.VertexColorChannels[0].Add(new Color4D(1.0f, 1.0f, 1.0f, 1.0f));
                    mesh.Normals.Add(new Vector3D());

                }
                mesh.SetIndices(chunk.Indices.ToArray(), 3);
                mesh.MaterialIndex = chunk.SectionInfo.TextureID;
                scene.Meshes.Add(mesh);

                for (int v = 0; v < chunk.vertices.Count; v++)
                {
                    // Build bone influences.
                    for (int z = 0; z < chunk.SectionInfo_opt1.SectionBoneIndices.Length; z++)
                    {
                        if (chunk.SectionInfo_opt1.SectionBoneIndices[z] != 0xFF)
                        {
                            Pmo.BoneData currentBone = new Pmo.BoneData();

                            int currentIndex = chunk.SectionInfo_opt1.SectionBoneIndices[z];
                            currentBone = pmo.boneList[currentIndex];

                            string boneName = currentBone.JointName;
                            Assimp.Matrix4x4 mtx = new Assimp.Matrix4x4();
                            mtx.A1 = currentBone.Transform.M11;
                            mtx.A2 = currentBone.Transform.M12;
                            mtx.A3 = currentBone.Transform.M13;
                            mtx.A4 = currentBone.Transform.M14;
                            mtx.B1 = currentBone.Transform.M21;
                            mtx.B2 = currentBone.Transform.M22;
                            mtx.B3 = currentBone.Transform.M23;
                            mtx.B4 = currentBone.Transform.M24;
                            mtx.C1 = currentBone.Transform.M31;
                            mtx.C2 = currentBone.Transform.M32;
                            mtx.C3 = currentBone.Transform.M33;
                            mtx.C4 = currentBone.Transform.M34;
                            mtx.D1 = currentBone.Transform.M41;
                            mtx.D2 = currentBone.Transform.M42;
                            mtx.D3 = currentBone.Transform.M43;
                            mtx.D4 = currentBone.Transform.M44;
                            Matrix3x3 mtx3 = new Matrix3x3(mtx);

                            mtx.Transpose();

                            mtx.A4 *= 100.0f;
                            mtx.B4 *= 100.0f;
                            mtx.C4 *= 100.0f;


                            mtx3.Transpose();

                            List<VertexWeight> weight = new List<VertexWeight>();

                            VertexWeight vW = new VertexWeight();
                            vW.VertexID = v;

                            float currentWeight = chunk.jointWeights[v].weights[z];

                            switch (chunk.jointWeights[v].coordFormart)
                            {
                                case Pmo.CoordinateFormat.NO_VERTEX:
                                    break;
                                case Pmo.CoordinateFormat.NORMALIZED_8_BITS:
                                    currentWeight *= 127.0f;
                                    currentWeight /= 128.0f;
                                    break;
                                case Pmo.CoordinateFormat.NORMALIZED_16_BITS:
                                    currentWeight *= 32767.0f;
                                    currentWeight /= 32768.0f;
                                    break;
                                case Pmo.CoordinateFormat.FLOAT_32_BITS:
                                    break;
                            }

                            vW.Weight = currentWeight;
                            weight.Add(vW);

                            Bone tempBone = scene.Meshes[i].Bones.Find(x => x.Name == boneName);
                            int boneInd = scene.Meshes[i].Bones.FindIndex(0, x => x.Name == boneName);

                            if (tempBone == null)
                            {
                                Bone bone = new Bone(boneName, mtx3, weight.ToArray());
                                scene.Meshes[i].Bones.Add(bone);
                            }
                            else
                            {
                                scene.Meshes[i].Bones[boneInd].VertexWeights.Add(vW);
                            }
                        }
                    }
                }
            }

            scene.RootNode.MeshIndices.AddRange(Enumerable.Range(0, scene.MeshCount));

            return scene;
        }

        private static List<Assimp.Animation> PAMtoFBXAnim(Pmo pmo, Pam pam)
        {
            List<Assimp.Animation> animationList = new List<Assimp.Animation>();

            for (int i = 0; i < pam.header.AnimationCount; i++)
            {
                Assimp.Animation anim = new Assimp.Animation();
                anim.Name = pam.animList[i].AnimEntry.AnimationName;
                anim.DurationInTicks = pam.animList[i].AnimHeader.FrameCount;
                anim.TicksPerSecond = pam.animList[i].AnimHeader.Framerate;

                //anim.MeshAnimationChannels[0].MeshKeys.Add(new MeshKey(anim.DurationInTicks / 2, new Vector3D(0, 0, 100)));

                for (int b = 0; b < pam.animList[i].AnimHeader.BoneCount; b++)
                {
                    Pam.BoneChannel chann = pam.animList[i].BoneChannels[b];

                    anim.NodeAnimationChannels.Add(new NodeAnimationChannel());
                    anim.NodeAnimationChannels[b].NodeName = pmo.boneList[b].JointName;
                    ChannelData dat = GetChannelKeyframes(chann, anim.DurationInTicks);

                    //AddTranslationAtKeyframe(anim.NodeAnimationChannels[b].PositionKeys, 0, new Vector3D(), anim.TicksPerSecond);
                    // Position
                    /*if(dat.transData != null)
                    {
                        foreach (ChannelTranslationData trans in dat.transData)
                        {
                            AddTranslationAtKeyframe(anim.NodeAnimationChannels[b].PositionKeys, trans.keyframeID, trans.Translation, anim.TicksPerSecond);
                        }
                    }*/

                    /*if (dat.rotData != null)
                    {
                        // Rotation
                        foreach (ChannelRotationData trans in dat.rotData)
                        {
                            AddRotationAtKeyframe(anim.NodeAnimationChannels[b].RotationKeys, trans.keyframeID, trans.Rotation, anim.TicksPerSecond);
                        }
                    }*/

                }

                animationList.Add(anim);
            }

            return animationList;
        }

        private static void AddTranslationAtKeyframe(List<VectorKey> Keys, int keyframe, Vector3D value, double framerate = 30.0)
        {
            double frametick = 1.0 / framerate;
            double keyframeTime = frametick * keyframe;
            Keys.Add(new VectorKey(keyframeTime, value));
        }

        private static void AddRotationAtKeyframe(List<QuaternionKey> Keys, int keyframe, Assimp.Quaternion value, double framerate = 30.0)
        {
            double frametick = 1.0 / framerate;
            double keyframeTime = frametick * keyframe;
            Keys.Add(new QuaternionKey(keyframeTime, value));
        }

        public class ChannelTranslationData
        {
            public int keyframeID = 0;
            public Vector3D Translation = new Vector3D();
        }

        public class ChannelRotationData
        {
            public int keyframeID = 0;
            public Assimp.Quaternion Rotation = new Assimp.Quaternion();
        }

        public class ChannelData
        {
            public ChannelTranslationData[] transData;
            public ChannelRotationData[] rotData;
        }

        private static ChannelData GetChannelKeyframes(Pam.BoneChannel channel, double animFrameCount)
        {
            ChannelData channData = new ChannelData();
            int keyCount = 0;

            // Translation X
            if (channel.TranslationX != null)
            {
                keyCount = channel.TranslationX.Header.KeyframeCount_16bits + channel.TranslationX.Header.KeyframeCount_8bits;
                channData.transData = new ChannelTranslationData[keyCount];

                if (keyCount == 1)
                {
                    channData.transData = new ChannelTranslationData[1];
                    channData.transData[0] = new ChannelTranslationData();
                    channData.transData[0].keyframeID = 0;
                    channData.transData[0].Translation.X = channel.TranslationX.Header.MaxValue;
                }
                else
                {
                    if (keyCount == animFrameCount)
                    {
                        for (int i = 0; i < keyCount; i++)
                        {
                            channData.transData[i] = new ChannelTranslationData();
                            channData.transData[i].keyframeID = i;
                            channData.transData[i].Translation.X = channel.TranslationX.Keyframes[i].Value;
                        }
                    }

                    if (keyCount < animFrameCount)
                    {
                        channData.transData = new ChannelTranslationData[channel.TranslationX.Keyframes.Count];
                        for (int i = 0; i < channel.TranslationX.Keyframes.Count; i++)
                        {
                            channData.transData[i] = new ChannelTranslationData();
                            channData.transData[i].keyframeID = channel.TranslationX.Keyframes[i].FrameID_16bits + channel.TranslationX.Keyframes[i].FrameID_8bits;
                            channData.transData[i].Translation.X = channel.TranslationX.Keyframes[i].Value;
                        }
                    }
                }
            }

            if (channel.TranslationY != null)
            {
                // Translation Y
                keyCount = channel.TranslationY.Header.KeyframeCount_16bits + channel.TranslationY.Header.KeyframeCount_8bits;
                channData.transData = new ChannelTranslationData[keyCount];

                if (keyCount == 1)
                {
                    channData.transData = new ChannelTranslationData[1];
                    channData.transData[0] = new ChannelTranslationData();
                    channData.transData[0].keyframeID = 0;
                    channData.transData[0].Translation.Y = channel.TranslationY.Header.MaxValue;
                }
                else
                {
                    if (keyCount == animFrameCount)
                    {
                        for (int i = 0; i < keyCount; i++)
                        {
                            channData.transData[i] = new ChannelTranslationData();
                            channData.transData[i].keyframeID = i;
                            channData.transData[i].Translation.Y = channel.TranslationY.Keyframes[i].Value;
                        }
                    }

                    if (keyCount < animFrameCount)
                    {
                        channData.transData = new ChannelTranslationData[channel.TranslationY.Keyframes.Count];
                        for (int i = 0; i < channel.TranslationY.Keyframes.Count; i++)
                        {
                            channData.transData[i] = new ChannelTranslationData();
                            channData.transData[i].keyframeID = channel.TranslationY.Keyframes[i].FrameID_16bits + channel.TranslationY.Keyframes[i].FrameID_8bits;
                            channData.transData[i].Translation.Y = channel.TranslationY.Keyframes[i].Value;
                        }
                    }
                }
            }

            if (channel.TranslationZ != null)
            {
                // Translation Z
                keyCount = channel.TranslationZ.Header.KeyframeCount_16bits + channel.TranslationZ.Header.KeyframeCount_8bits;
                channData.transData = new ChannelTranslationData[keyCount];

                if (keyCount == 1)
                {
                    channData.transData = new ChannelTranslationData[1];
                    channData.transData[0] = new ChannelTranslationData();
                    channData.transData[0].keyframeID = 0;
                    channData.transData[0].Translation.Z = channel.TranslationZ.Header.MaxValue;
                }
                else
                {
                    if (keyCount == animFrameCount)
                    {
                        for (int i = 0; i < keyCount; i++)
                        {
                            channData.transData[i] = new ChannelTranslationData();
                            channData.transData[i].keyframeID = i;
                            channData.transData[i].Translation.Z = channel.TranslationZ.Keyframes[i].Value;
                        }
                    }

                    if (keyCount < animFrameCount)
                    {
                        channData.transData = new ChannelTranslationData[channel.TranslationZ.Keyframes.Count];
                        for (int i = 0; i < channel.TranslationZ.Keyframes.Count; i++)
                        {
                            channData.transData[i] = new ChannelTranslationData();
                            channData.transData[i].keyframeID = channel.TranslationZ.Keyframes[i].FrameID_16bits + channel.TranslationZ.Keyframes[i].FrameID_8bits;
                            channData.transData[i].Translation.Z = channel.TranslationZ.Keyframes[i].Value;
                        }
                    }
                }
            }

            if (channel.RotationX != null)
            {
                // Rotation X
                keyCount = channel.RotationX.Header.KeyframeCount_16bits + channel.RotationX.Header.KeyframeCount_8bits;
                channData.rotData = new ChannelRotationData[keyCount];

                if (keyCount == 1)
                {
                    channData.rotData = new ChannelRotationData[1];
                    channData.rotData[0] = new ChannelRotationData();
                    channData.rotData[0].keyframeID = 0;
                    channData.rotData[0].Rotation.X = channel.RotationX.Header.MaxValue;
                }
                else
                {
                    if (keyCount == animFrameCount)
                    {
                        for (int i = 0; i < keyCount; i++)
                        {
                            channData.rotData[i] = new ChannelRotationData();
                            channData.rotData[i].keyframeID = i;
                            channData.rotData[i].Rotation.X = channel.RotationX.Keyframes[i].Value;
                        }
                    }

                    if (keyCount < animFrameCount)
                    {
                        channData.rotData = new ChannelRotationData[channel.RotationX.Keyframes.Count];
                        for (int i = 0; i < channel.RotationX.Keyframes.Count; i++)
                        {
                            channData.rotData[i] = new ChannelRotationData();
                            channData.rotData[i].keyframeID = channel.RotationX.Keyframes[i].FrameID_16bits + channel.RotationX.Keyframes[i].FrameID_8bits;
                            channData.rotData[i].Rotation.X = channel.RotationX.Keyframes[i].Value;
                        }
                    }
                }
            }


            if (channel.RotationY != null)
            {
                // Rotation Y
                keyCount = channel.RotationY.Header.KeyframeCount_16bits + channel.RotationY.Header.KeyframeCount_8bits;
                channData.rotData = new ChannelRotationData[keyCount];

                if (keyCount == 1)
                {
                    channData.rotData = new ChannelRotationData[1];
                    channData.rotData[0] = new ChannelRotationData();
                    channData.rotData[0].keyframeID = 0;
                    channData.rotData[0].Rotation.Y = channel.RotationY.Header.MaxValue;
                }
                else
                {
                    if (keyCount == animFrameCount)
                    {
                        for (int i = 0; i < keyCount; i++)
                        {
                            channData.rotData[i] = new ChannelRotationData();
                            channData.rotData[i].keyframeID = i;
                            channData.rotData[i].Rotation.Y = channel.RotationY.Keyframes[i].Value;
                        }
                    }

                    if (keyCount < animFrameCount)
                    {
                        channData.rotData = new ChannelRotationData[channel.RotationY.Keyframes.Count];
                        for (int i = 0; i < channel.RotationY.Keyframes.Count; i++)
                        {
                            channData.rotData[i] = new ChannelRotationData();
                            channData.rotData[i].keyframeID = channel.RotationY.Keyframes[i].FrameID_16bits + channel.RotationY.Keyframes[i].FrameID_8bits;
                            channData.rotData[i].Rotation.Y = channel.RotationY.Keyframes[i].Value;
                        }
                    }
                }

            }

            if (channel.RotationZ != null)
            {
                // Rotation Z
                keyCount = channel.RotationZ.Header.KeyframeCount_16bits + channel.RotationZ.Header.KeyframeCount_8bits;
                channData.rotData = new ChannelRotationData[keyCount];

                if (keyCount == 1)
                {
                    channData.rotData = new ChannelRotationData[1];
                    channData.rotData[0] = new ChannelRotationData();
                    channData.rotData[0].keyframeID = 0;
                    channData.rotData[0].Rotation.Z = channel.RotationZ.Header.MaxValue;
                }
                else
                {
                    if (keyCount == animFrameCount)
                    {
                        for (int i = 0; i < keyCount; i++)
                        {
                            channData.rotData[i] = new ChannelRotationData();
                            channData.rotData[i].keyframeID = i;
                            channData.rotData[i].Rotation.Z = channel.RotationZ.Keyframes[i].Value;
                        }
                    }

                    if (keyCount < animFrameCount)
                    {
                        channData.rotData = new ChannelRotationData[channel.RotationZ.Keyframes.Count];
                        for (int i = 0; i < channel.RotationZ.Keyframes.Count; i++)
                        {
                            channData.rotData[i] = new ChannelRotationData();
                            channData.rotData[i].keyframeID = channel.RotationZ.Keyframes[i].FrameID_16bits + channel.RotationZ.Keyframes[i].FrameID_8bits;
                            channData.rotData[i].Rotation.Z = channel.RotationZ.Keyframes[i].Value;
                        }
                    }
                }

            }

            return channData;
        }
    }
}

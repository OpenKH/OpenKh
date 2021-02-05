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
            List<Assimp.Animation> FBXAnims = PAMtoFBXAnim(pam);
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
            for(int t = 0; t < pmo.header.TextureCount; t++)
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
                mtx.A1 = bn.Transform[0];
                mtx.A2 = bn.Transform[1];
                mtx.A3 = bn.Transform[2];
                mtx.A4 = bn.Transform[3];
                mtx.B1 = bn.Transform[4];
                mtx.B2 = bn.Transform[5];
                mtx.B3 = bn.Transform[6];
                mtx.B4 = bn.Transform[7];
                mtx.C1 = bn.Transform[8];
                mtx.C2 = bn.Transform[9];
                mtx.C3 = bn.Transform[10];
                mtx.C4 = bn.Transform[11];
                mtx.D1 = bn.Transform[12];
                mtx.D2 = bn.Transform[13];
                mtx.D3 = bn.Transform[14];
                mtx.D4 = bn.Transform[15];

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
                        if(chunk.SectionInfo_opt1.SectionBoneIndices[z] != 0xFF)
                        {
                            Pmo.BoneData currentBone = new Pmo.BoneData();

                            int currentIndex = chunk.SectionInfo_opt1.SectionBoneIndices[z];
                            currentBone = pmo.boneList[currentIndex];

                            string boneName = currentBone.JointName;
                            Assimp.Matrix4x4 mtx = new Assimp.Matrix4x4();
                            mtx.A1 = currentBone.Transform[0];
                            mtx.A2 = currentBone.Transform[1];
                            mtx.A3 = currentBone.Transform[2];
                            mtx.A4 = currentBone.Transform[3];
                            mtx.B1 = currentBone.Transform[4];
                            mtx.B2 = currentBone.Transform[5];
                            mtx.B3 = currentBone.Transform[6];
                            mtx.B4 = currentBone.Transform[7];
                            mtx.C1 = currentBone.Transform[8];
                            mtx.C2 = currentBone.Transform[9];
                            mtx.C3 = currentBone.Transform[10];
                            mtx.C4 = currentBone.Transform[11];
                            mtx.D1 = currentBone.Transform[12];
                            mtx.D2 = currentBone.Transform[13];
                            mtx.D3 = currentBone.Transform[14];
                            mtx.D4 = currentBone.Transform[15];
                            mtx.Transpose();

                            mtx.A4 *= 100.0f;
                            mtx.B4 *= 100.0f;
                            mtx.C4 *= 100.0f;

                            Matrix3x3 mtx3 = new Matrix3x3(mtx);

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

        private static List<Assimp.Animation> PAMtoFBXAnim(Pam pam)
        {
            List<Assimp.Animation> animationList = new List<Assimp.Animation>();

            for(int i = 0; i < pam.header.AnimationCount; i++)
            {
                Assimp.Animation anim = new Assimp.Animation();
                anim.Name = pam.animList[i].AnimEntry.AnimationName;
                anim.DurationInTicks = pam.animList[i].AnimHeader.FrameCount;
                anim.TicksPerSecond = pam.animList[i].AnimHeader.Framerate;

                for(int b = 0; b < pam.animList[i].AnimHeader.BoneCount; b++)
                {
                    Pam.BoneChannel chann = pam.animList[i].BoneChannels[b];
                    
                    //anim.NodeAnimationChannels[b].PositionKeys.Add(new VectorKey(chann.TranslationX.Header.));
                }
                

                animationList.Add(anim);
            }

            return animationList;
        }
    }
}

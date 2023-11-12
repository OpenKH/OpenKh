using OpenKh.AssimpUtils;
using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Kh2.Models.VIF;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenKh.Tools.Kh2MdlxEditor.Utils
{
    public class MdlxEditorImporter
    {
        public static bool TRIANGLE_INVERSE = false; // UNUSED for now
        public static bool KEEP_ORIGINAL_SHADOW = false;
        public static bool KEEP_ORIGINAL_SKELETON = true;

        public static Dictionary<int,int> materialToTexture = null;
        public static ModelTexture createModelTexture(Assimp.Scene scene, string filePath)
        {
            materialToTexture = new Dictionary<int, int>();

            string directoryPath = Path.GetDirectoryName(filePath);

            List<Imgd> imgdList = new List<Imgd>();

            // Note: Some materials may share a texture
            int uniqueTextureCount = 0;
            List<string> materialPaths = new List<string>();
            for(int i = 0; i < scene.Materials.Count; i++)
            {
                string texturePath = directoryPath + "\\" + Path.GetFileName(scene.Materials[i].TextureDiffuse.FilePath);
                if (!Path.HasExtension(texturePath))
                {
                    texturePath += ".png";
                }
                else if(!texturePath.EndsWith(".png"))
                {
                    throw new Exception("Texture is not PNG");
                }
                if (materialPaths.Contains(texturePath))
                {
                    materialToTexture.Add(i, materialPaths.IndexOf(texturePath));
                }
                else
                {
                    materialToTexture.Add(i, uniqueTextureCount);
                    uniqueTextureCount++;
                    materialPaths.Add(texturePath);
                    imgdList.Add(ImageUtils.pngToImgd(texturePath));
                }
            }

            // A write-read is necessary to reload the texture data
            ModelTexture modTex = new ModelTexture(imgdList.ToArray());
            Stream tempStream = new MemoryStream();
            modTex.Write(tempStream);

            return ModelTexture.Read(tempStream);
        }

        public static ModelSkeletal replaceMeshModelSkeletal(Assimp.Scene scene, ModelSkeletal oldModel, string filePath)
        {
            ModelSkeletal model = new ModelSkeletal();

            // If you want to convert only specific meshes for debugging purposes set them here. Otherwise leave the list empty
            List<int> DEBUG_ONLY_THESE_MESHES = new List<int> { };

            model.ModelHeader = oldModel.ModelHeader;
            model.BoneCount = oldModel.BoneCount;
            model.TextureCount = oldModel.TextureCount;
            model.BoneOffset = oldModel.BoneOffset;
            model.BoneDataOffset = oldModel.BoneDataOffset;
            model.GroupCount = scene.Meshes.Count;
            if (DEBUG_ONLY_THESE_MESHES.Count != 0)
                model.GroupCount = DEBUG_ONLY_THESE_MESHES.Count;
            model.Bones = oldModel.Bones;
            model.BoneData = oldModel.BoneData;
            model.Groups = new List<ModelSkeletal.SkeletalGroup>();

            if (!KEEP_ORIGINAL_SKELETON)
            {
                model.Bones = getSkeleton(scene);
                model.BoneCount = (ushort)model.Bones.Count;
            }

            int baseAddress = VifUtils.calcBaseAddress(model.Bones.Count, model.GroupCount);
            System.Numerics.Matrix4x4[] boneMatrices = ModelCommon.GetBoneMatrices(model.Bones);

            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                if (DEBUG_ONLY_THESE_MESHES.Count != 0 && !DEBUG_ONLY_THESE_MESHES.Contains(i))
                    continue;

                Assimp.Mesh mesh = scene.Meshes[i];

                VifMesh vifMesh = Kh2MdlxAssimp.getVifMeshFromAssimp(mesh, boneMatrices);
                List<DmaVifPacket> dmaVifPackets = VifProcessor.vifMeshToDmaVifPackets(vifMesh);

                // TEST
                ModelSkeletal.SkeletalGroup group = VifProcessor.getSkeletalGroup(dmaVifPackets, (uint)mesh.MaterialIndex, baseAddress);
                group.Header.TextureIndex = (uint)materialToTexture[mesh.MaterialIndex];

                model.Groups.Add(group);

                baseAddress += VifProcessor.getGroupSize(group);

                if (VifProcessor.currentApplyNormal)
                    group.Header.Specular = true;
                VifProcessor.NextMeshOptions();
            }

            foreach (ModelSkeletal.SkeletalGroup group in model.Groups)
            {
                group.Mesh = ModelSkeletal.getMeshFromGroup(group, ModelCommon.GetBoneMatrices(model.Bones)); // Comment this and save the model to see what's wrong with the VIF code
            }

            if (KEEP_ORIGINAL_SHADOW)
            {
                model.Shadow = oldModel.Shadow;
            }

            return model;
        }

        public static List<ModelCommon.Bone> getSkeleton(Assimp.Scene scene)
        {
            List<ModelCommon.Bone> mdlxBones = new List<ModelCommon.Bone>();

            List<Assimp.Node> assimpBones = getChildren(scene.RootNode);
            assimpBones.RemoveAt(0); // Remove Assimp root

            // Remove extra bones - Added with this tool due to a bug somehow when parsing to assimp
            for (int i = 0; i < assimpBones.Count; i++)
            {
                if (assimpBones[i].Parent.Name == "RootNode" && assimpBones[i].ChildCount == 0)
                {
                    assimpBones.RemoveAt(i);
                    i--;
                }
            }

            for (int i = 0; i < assimpBones.Count; i++)
            {
                Assimp.Node assimpBone = assimpBones[i];

                ModelCommon.Bone bone = new ModelCommon.Bone();
                bone.Index = (short)i;
                bone.ParentIndex = (short)assimpBones.IndexOf(assimpBone.Parent);

                assimpBones[i].Transform.Decompose(
                    out Assimp.Vector3D scale, 
                    out Assimp.Quaternion rotation, 
                    out Assimp.Vector3D translation);

                bone.ScaleX= scale.X;
                bone.ScaleY= scale.Y;
                bone.ScaleZ= scale.Z;
                bone.TranslationX= translation.X;
                bone.TranslationY= translation.Y;
                bone.TranslationZ= translation.Z;

                System.Numerics.Quaternion numQuat = new System.Numerics.Quaternion(rotation.X, rotation.Y, rotation.Z, rotation.W);

                System.Numerics.Vector3 rotationRadian = MathUtils.toRadian(MathUtils.ToEulerAngles(numQuat));
                bone.RotationX = rotationRadian.X;
                bone.RotationY = rotationRadian.Y;
                bone.RotationZ = rotationRadian.Z;
                bone.RotationW = 0;

                mdlxBones.Add(bone);
            }

            return mdlxBones;
        }

        private static int preventLoopLock = 0xFFFF;
        public static List<Assimp.Node> getChildren(Assimp.Node assimpNode)
        {
            List<Assimp.Node> children = new List<Assimp.Node>();

            foreach (Assimp.Node node in assimpNode.Children)
            {
                preventLoopLock--;
                if (preventLoopLock <= 0)
                    throw new Exception("Stuck on a loop while retrieving bones or too many bones (Over 0xFFFF)");

                children.Add(node);
                children.AddRange(getChildren(node));
            }

            return children;
        }
    }
}

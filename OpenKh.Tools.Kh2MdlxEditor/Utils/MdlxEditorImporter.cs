using OpenKh.AssimpUtils;
using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Kh2.Models.VIF;
using System;
using System.Collections.Generic;
using System.IO;
using System.Numerics;

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

            string directoryPath = Path.GetDirectoryName(filePath) ?? throw new DirectoryNotFoundException(filePath);

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

                if (!File.Exists(texturePath))
                {
                    texturePath = Path.Combine(directoryPath, $"texture{i:000}.png");
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

            // List to calc joint flag
            HashSet<int> bonesWithRig = new HashSet<int>();
            foreach(var mesh in scene.Meshes)
            {
                for(int i = 0; i < mesh.Bones.Count; i++)
                {
                    if (mesh.Bones[i].HasVertexWeights)
                        bonesWithRig.Add(i);
                }
            }

            for (int i = 0; i < assimpBones.Count; i++)
            {
                Assimp.Node assimpBone = assimpBones[i];
                ModelCommon.Bone bone = new ModelCommon.Bone();
                bone.Index = (short)i;
                bone.ParentIndex = (short)assimpBones.IndexOf(assimpBone.Parent);

                // Root
                if (i == 0)
                {
                    bone.ParentIndex = -1;
                    bone.ChildIndex = -1;
                }

                // Flags
                if (!bonesWithRig.Contains(i))
                {
                    bone.Flags = 3;
                }
                //Else 0 or 1

                Matrix4x4 transform = AssimpGeneric.ToNumerics(assimpBones[i].Transform);

                DecomposeEuler(transform, out Vector3 scale, out Vector3 rotation, out Vector3 position);

                bone.ScaleX = scale.X;
                bone.ScaleY = scale.Y;
                bone.ScaleZ = scale.Z;
                bone.TranslationX = position.X;
                bone.TranslationY = position.Y;
                bone.TranslationZ = position.Z;
                bone.RotationX = rotation.X;
                bone.RotationY = rotation.Y;
                bone.RotationZ = rotation.Z;
                bone.RotationW = 0;

                mdlxBones.Add(bone);
            }

            // Each bone has some kind of length in the Scale W value - it is unknown how it's derived except for the following
            List<bool> lengthProcessed = new List<bool>();
            for (int i = 0; i < mdlxBones.Count; i++) {
                lengthProcessed.Add(false);
            }
            foreach (ModelCommon.Bone bone in mdlxBones)
            {
                if(bone.TranslationX != 0 && bone.TranslationY == 0 && bone.TranslationZ == 0 &&
                    lengthProcessed[bone.ParentIndex] == false)
                {
                    mdlxBones[bone.ParentIndex].ScaleW = bone.TranslationX;
                    lengthProcessed[bone.ParentIndex] = true;
                }
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


        public static void DecomposeEuler(Matrix4x4 matrix, out Vector3 scale, out Vector3 rotation, out Vector3 position)
        {
            // Extract the translation
            position = new Vector3(matrix.M41, matrix.M42, matrix.M43);

            // Extract the scale
            scale = new Vector3(
                new Vector3(matrix.M11, matrix.M12, matrix.M13).Length(),
                new Vector3(matrix.M21, matrix.M22, matrix.M23).Length(),
                new Vector3(matrix.M31, matrix.M32, matrix.M33).Length()
            );

            // Remove scale from the matrix to isolate the rotation
            Matrix4x4 rotationMatrix = new Matrix4x4(
                matrix.M11 / scale.X, matrix.M12 / scale.X, matrix.M13 / scale.X, 0.0f,
                matrix.M21 / scale.Y, matrix.M22 / scale.Y, matrix.M23 / scale.Y, 0.0f,
                matrix.M31 / scale.Z, matrix.M32 / scale.Z, matrix.M33 / scale.Z, 0.0f,
                0.0f, 0.0f, 0.0f, 1.0f
            );

            // Use a small epsilon to solve floating-point inaccuracies
            const float epsilon = 1e-6f;

            // Extract the rotation angles from the rotation matrix
            rotation.Y = (float)Math.Asin(-rotationMatrix.M13); // Angle around Y

            float cosY = (float)Math.Cos(rotation.Y);

            if (Math.Abs(cosY) > epsilon)
            {
                // Finding angle around X
                float tanX = rotationMatrix.M33 / cosY; // A
                float tanY = rotationMatrix.M23 / cosY; // B
                rotation.X = (float)Math.Atan2(tanY, tanX);

                // Finding angle around Z
                tanX = rotationMatrix.M11 / cosY; // E
                tanY = rotationMatrix.M12 / cosY; // F
                rotation.Z = (float)Math.Atan2(tanY, tanX);
            }
            else
            {
                // Y is fixed
                rotation.X = 0;

                // Finding angle around Z
                float tanX = rotationMatrix.M22; // E
                float tanY = -rotationMatrix.M21; // F
                rotation.Z = (float)Math.Atan2(tanY, tanX);
            }
        }
    }
}

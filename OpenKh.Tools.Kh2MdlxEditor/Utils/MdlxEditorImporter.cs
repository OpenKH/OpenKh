using OpenKh.AssimpUtils;
using OpenKh.Kh2.Models;
using OpenKh.Kh2.Models.VIF;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MdlxEditor.Utils
{
    public class MdlxEditorImporter
    {
        private static bool TRIANGLE_INVERSE = true; // UNUSED for now
        private static bool KEEP_ORIGINAL_SHADOW = true;

        public static ModelSkeletal replaceMeshModelSkeletal(Assimp.Scene scene, ModelSkeletal oldModel)
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

            //-------
            // TEST
            //model.Bones = getSkeleton(scene);
            //model.BoneCount = (ushort)model.Bones.Count;
            //-------

            int baseAddress = VifUtils.calcBaseAddress(model.Bones.Count, model.GroupCount);
            Matrix4x4[] boneMatrices = ModelCommon.GetBoneMatrices(model.Bones);

            for (int i = 0; i < scene.Meshes.Count; i++)
            {
                if (DEBUG_ONLY_THESE_MESHES.Count != 0 && !DEBUG_ONLY_THESE_MESHES.Contains(i))
                    continue;

                Assimp.Mesh mesh = scene.Meshes[i];

                VifMesh vifMesh = Kh2MdlxAssimp.getVifMeshFromAssimp(mesh, boneMatrices);
                List<DmaVifPacket> dmaVifPackets = VifProcessor.vifMeshToDmaVifPackets(vifMesh);

                // TEST
                ModelSkeletal.SkeletalGroup group = VifProcessor.getSkeletalGroup(dmaVifPackets, (uint)mesh.MaterialIndex, baseAddress);

                model.Groups.Add(group);

                baseAddress += VifProcessor.getGroupSize(group);
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
    }
}

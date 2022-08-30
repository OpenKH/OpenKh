using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using System.Numerics;

namespace OpenKh.AssimpUtils
{
    public class Kh2MdlxAssimp
    {
        public static Assimp.Scene getAssimpScene(MdlxParser mParser)
        {
            Assimp.Scene scene = AssimpGeneric.GetBaseScene();

            // Divide the model in a mesh per texture and each mesh in its meshDescriptors
            Dictionary<int, List<MeshDescriptor>> dic_meshDescriptorsByMeshId = new Dictionary<int, List<MeshDescriptor>>();
            foreach (MeshDescriptor meshDesc in mParser.MeshDescriptors)
            {
                if (!dic_meshDescriptorsByMeshId.ContainsKey(meshDesc.TextureIndex))
                {
                    dic_meshDescriptorsByMeshId.Add(meshDesc.TextureIndex, new List<MeshDescriptor>());
                }
                dic_meshDescriptorsByMeshId[meshDesc.TextureIndex].Add(meshDesc);
            }

            // MESHES AND THEIR DATA
            for (int i = 0; i < dic_meshDescriptorsByMeshId.Keys.Count; i++)
            {
                Assimp.Mesh iMesh = new Assimp.Mesh("Mesh" + i, Assimp.PrimitiveType.Triangle);
                iMesh.UVComponentCount[0] = 2; // Required for some reason

                // TEXTURE
                Assimp.TextureSlot texture = new Assimp.TextureSlot();
                texture.FilePath = "Texture" + i;
                texture.TextureType = Assimp.TextureType.Diffuse;

                // MATERIAL
                Assimp.Material mat = new Assimp.Material();
                mat.Name = "Material" + i;
                mat.TextureDiffuse = texture;
                iMesh.MaterialIndex = i;
                scene.Materials.Add(mat);

                // BONES - Assimp requires all of the bones even if they don't have weights
                for (int j = 0; j < mParser.Bones.Count; j++)
                {
                    iMesh.Bones.Add(new Assimp.Bone("Bone" + j, Assimp.Matrix3x3.Identity, new Assimp.VertexWeight[0]));
                }

                scene.Meshes.Add(iMesh);

                // NODE
                Assimp.Node iMeshNode = new Assimp.Node("MeshNode" + i);
                iMeshNode.MeshIndices.Add(i);

                scene.RootNode.Children.Add(iMeshNode);
            }

            // VERTICES
            for (int i = 0; i < dic_meshDescriptorsByMeshId.Keys.Count; i++)
            {
                int currentVertex = 0; // Vertex index is provided per mesh, not global
                Assimp.Mesh iMesh = scene.Meshes[i];

                foreach (MeshDescriptor iMeshDesc in dic_meshDescriptorsByMeshId[i])
                {
                    foreach (PositionColoredTextured vertex in iMeshDesc.Vertices)
                    {
                        iMesh.Vertices.Add(new Assimp.Vector3D(vertex.X, vertex.Y, vertex.Z));
                        iMesh.TextureCoordinateChannels[0].Add(new Assimp.Vector3D(vertex.Tu, 1 - vertex.Tv, 0));

                        // VERTEX WEIGHTS
                        if (vertex.BoneAssign != null)
                        {
                            Assimp.Bone bone = AssimpGeneric.FindBone(iMesh.Bones, "Bone" + vertex.BoneAssign);
                            bone.VertexWeights.Add(new Assimp.VertexWeight(currentVertex, 1));
                        }

                        currentVertex++;
                    }
                }
            }

            // FACES
            foreach (Assimp.Mesh iMesh in scene.Meshes)
            {
                // It just so happens that vertices are ordered for their faces. Write the correct code later.
                int faceCount = iMesh.Vertices.Count / 3;
                for (int i = 0; i < faceCount; i++)
                {
                    int baseIndex = i * 3;
                    iMesh.Faces.Add(new Assimp.Face(new int[] { baseIndex, baseIndex + 1, baseIndex + 2 }));
                }
            }

            // BONES (Node hierarchy)
            foreach (Mdlx.Bone bone in mParser.Bones)
            {
                string boneName = "Bone" + bone.Index;
                Assimp.Node boneNode = new Assimp.Node(boneName);

                Assimp.Node parentNode;
                if (bone.Parent == -1) {
                    parentNode = scene.RootNode;
                }
                else {
                    parentNode = scene.RootNode.FindNode("Bone" + bone.Parent);
                }

                boneNode.Transform = AssimpGeneric.GetNodeTransformMatrix(new Vector3(bone.ScaleX, bone.ScaleY, bone.ScaleZ),
                                                                    new Vector3(bone.RotationX, bone.RotationY, bone.RotationZ),
                                                                    new Vector3(bone.TranslationX, bone.TranslationY, bone.TranslationZ));

                parentNode.Children.Add(boneNode);
            }

            // BONE OFFSET MATRIX - IMPORTANT NOTE: This is for DAE (Collada) FBX ignores the offset matrices
            foreach (Assimp.Mesh mesh in scene.Meshes)
            {
                // THIS IS TESTING DATA - Haven't figured out how to make it work

                foreach (Assimp.Bone bone in mesh.Bones)
                {
                    // Get hierarchy
                    Assimp.Node iNode = scene.RootNode.FindNode(bone.Name);
                    List<Assimp.Node> nodeHierarchy = new List<Assimp.Node>();
                    nodeHierarchy.Add(iNode);
                    while (iNode.Parent != null)
                    {
                        iNode = iNode.Parent;
                        nodeHierarchy.Add(iNode);
                    }

                    Assimp.Matrix4x4? boneOffsetMatrix = null;
                    // Calc matrix
                    for (int i = nodeHierarchy.Count - 1; i >= 0; i--)
                    {
                        if (boneOffsetMatrix == null)
                        {
                            boneOffsetMatrix = nodeHierarchy[i].Transform;
                        }
                        else
                        {
                            boneOffsetMatrix *= nodeHierarchy[i].Transform;
                        }
                    }

                    Matrix4x4 tempMat = AssimpGeneric.ToNumerics((Assimp.Matrix4x4)boneOffsetMatrix);
                    Matrix4x4.Invert(tempMat, out tempMat);

                    bone.OffsetMatrix = AssimpGeneric.ToAssimp(tempMat);

                    //bone.OffsetMatrix = Assimp.Matrix4x4.Identity;

                    //Assimp.Matrix4x4.inverse(boneOffsetMatrix, bone.OffsetMatrix);
                }
            }

            return scene;
        }
    }
}

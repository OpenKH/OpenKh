using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using OpenKh.Kh2.Models;
using OpenKh.Kh2.Models.VIF;
using OpenKh.Kh2Anim.Mset;
using OpenKh.Kh2Anim.Mset.Interfaces;
using System.Numerics;

namespace OpenKh.AssimpUtils
{
    public class Kh2MdlxAssimp
    {
        /****************
         * PROCESSES
         ****************/
        public static Assimp.Scene getAssimpScene(MdlxParser mParser)
        {
            Assimp.Scene scene = AssimpGeneric.GetBaseScene();

            // Divide the model in a mesh per texture and each mesh in its meshDescriptors
            Dictionary<int, List<MeshDescriptor>> dic_meshDescriptorsByMeshId = Kh2MdlxAssimp.getMeshDescriptos(mParser);

            // MESHES AND THEIR DATA
            for (int i = 0; i < dic_meshDescriptorsByMeshId.Keys.Count; i++)
            {
                Assimp.Mesh iMesh = new Assimp.Mesh("Mesh" + i, Assimp.PrimitiveType.Triangle);
                iMesh.UVComponentCount[0] = 2; // Required for some reason

                // TEXTURE
                Assimp.TextureSlot texture = new Assimp.TextureSlot();
                texture.FilePath = "Texture" + i + "." + System.Drawing.Imaging.ImageFormat.Png.ToString().ToLower(); /* Not planning to export them in another extension right. */
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

            mParser.skinnedBones = new bool[mParser.Bones.Count];
            // VERTICES
            for (int i = 0; i < dic_meshDescriptorsByMeshId.Keys.Count; i++)
            {
                int currentVertex = 0; // Vertex index is provided per mesh, not global
                Assimp.Mesh iMesh = scene.Meshes[i];

                foreach (MeshDescriptor iMeshDesc in dic_meshDescriptorsByMeshId[i])
                {
                    foreach (var (vertex, boneAssigns) in iMeshDesc.Vertices.Zip(iMeshDesc.VertexBoneWeights))
                    {
                        iMesh.Vertices.Add(new Assimp.Vector3D(vertex.X, vertex.Y, vertex.Z));
                        iMesh.TextureCoordinateChannels[0].Add(new Assimp.Vector3D(vertex.Tu, 1 - vertex.Tv, 0));

                        // VERTEX WEIGHTS
                        foreach (var boneAssign in boneAssigns)
                        {
                            Assimp.Bone bone = AssimpGeneric.FindBone(iMesh.Bones, "Bone" + boneAssign.MatrixIndex);
                            mParser.skinnedBones[boneAssign.MatrixIndex] = true;
                            bone.VertexWeights.Add(new Assimp.VertexWeight(currentVertex, boneAssign.Weight));
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

            List<Microsoft.Xna.Framework.Matrix> matricesToReverse = new List<Microsoft.Xna.Framework.Matrix>(0);
            List<string> matricesToReverseNames = new List<string>(0);

            // BONES (Node hierarchy)
            foreach (Mdlx.Bone bone in mParser.Bones)
            {
                string boneName = "Bone" + bone.Index;
                Assimp.Node boneNode = new Assimp.Node(boneName);

                Assimp.Node parentNode;
                if (bone.Parent == -1)
                {
                    parentNode = scene.RootNode;
                }
                else
                {
                    parentNode = scene.RootNode.FindNode("Bone" + bone.Parent);
                }

                boneNode.Transform = AssimpGeneric.GetNodeTransformMatrix(new Vector3(bone.ScaleX, bone.ScaleY, bone.ScaleZ),
                                                                    new Vector3(bone.RotationX, bone.RotationY, bone.RotationZ),
                                                                    new Vector3(bone.TranslationX, bone.TranslationY, bone.TranslationZ));
                
                matricesToReverse.Add(AssimpGeneric.ToXna(boneNode.Transform));
                matricesToReverseNames.Add(boneName);

                parentNode.Children.Add(boneNode);
            }

            OpenKh.Engine.Monogame.Helpers.MatrixRecursivity.ComputeMatrices(ref matricesToReverse, mParser);

            foreach (Assimp.Mesh mesh in scene.Meshes)
            {
                foreach (Assimp.Bone bone in mesh.Bones)
                {
                    bone.OffsetMatrix = AssimpGeneric.ToAssimp(Microsoft.Xna.Framework.Matrix.Invert(matricesToReverse[matricesToReverseNames.IndexOf(bone.Name)]));
                }
            }

            return scene;
        }

        public static Dictionary<int, List<MeshDescriptor>> getMeshDescriptos(MdlxParser mParser)
        {
            var output = new Dictionary<int, List<MeshDescriptor>>();
            foreach (MeshDescriptor meshDesc in mParser.MeshDescriptors)
            {
                if (!output.ContainsKey(meshDesc.TextureIndex))
                {
                    output.Add(meshDesc.TextureIndex, new List<MeshDescriptor>());
                }
                output[meshDesc.TextureIndex].Add(meshDesc);
            }
            return output;
        }

        public static Assimp.Scene getAssimpScene(ModelSkeletal model)
        {
            Assimp.Scene scene = AssimpGeneric.GetBaseScene();

            List<Assimp.TextureSlot> textures = new List<Assimp.TextureSlot>();
            List<Assimp.Material> materials = new List<Assimp.Material>();
            for (int i = 0; i < model.TextureCount; i++)
            {
                Assimp.TextureSlot texture = new Assimp.TextureSlot();
                texture.FilePath = "Texture" + i.ToString("D4");
                texture.TextureType = Assimp.TextureType.Diffuse;
                textures.Add(texture);
            }

            //Matrix4x4[] boneMatrices = ModelCommon.GetBoneMatrices(model.Bones);

            for (int i = 0; i < model.Groups.Count; i++)
            {
                ModelSkeletal.SkeletalGroup group = model.Groups[i];
                Assimp.Mesh iMesh = new Assimp.Mesh("Mesh" + i.ToString("D4"), Assimp.PrimitiveType.Triangle);
                iMesh.UVComponentCount[0] = 2; // Required for some reason

                // TEXTURE
                /*Assimp.TextureSlot texture = new Assimp.TextureSlot();
                texture.FilePath = "Texture" + group.Header.TextureIndex;
                texture.TextureType = Assimp.TextureType.Diffuse;*/

                // MATERIAL
                Assimp.Material mat = new Assimp.Material();
                mat.Name = "Material" + i.ToString("D4");
                mat.TextureDiffuse = textures[(int)group.Header.TextureIndex];
                //mat.TextureDiffuse = texture;
                iMesh.MaterialIndex = i;
                scene.Materials.Add(mat);
                //iMesh.MaterialIndex = (int)group.Header.TextureIndex;

                // BONES - Assimp requires all of the bones even if they don't have weights
                for (int j = 0; j < model.Bones.Count; j++)
                {
                    iMesh.Bones.Add(new Assimp.Bone("Bone" + j.ToString("D4"), Assimp.Matrix3x3.Identity, new Assimp.VertexWeight[0]));
                }

                // VERTICES
                int currentVertex = 0;
                foreach (ModelCommon.UVBVertex vertex in group.Mesh.Vertices)
                {
                    //Vector3 globalPosition = ModelCommon.getAbsolutePosition(vertex.BPositions, boneMatrices);

                    iMesh.Vertices.Add(new Assimp.Vector3D(vertex.Position.X, vertex.Position.Y, vertex.Position.Z));

                    iMesh.TextureCoordinateChannels[0].Add(new Assimp.Vector3D(vertex.U / 4096.0f, 1 - (vertex.V / 4096.0f), 0)); // Stored as int, convert to float

                    // VERTEX WEIGHTS
                    foreach (ModelCommon.BPosition bPosition in vertex.BPositions)
                    {
                        Assimp.Bone bone = AssimpGeneric.FindBone(iMesh.Bones, "Bone" + bPosition.BoneIndex.ToString("D4"));
                        float weight = bPosition.Position.W == 0 ? 1 : bPosition.Position.W;
                        bone.VertexWeights.Add(new Assimp.VertexWeight(currentVertex, weight));
                    }

                    // COLORS
                    if(vertex.Color != null)
                    {
                        float R = vertex.Color.R / 255f;
                        float G = vertex.Color.G / 255f;
                        float B = vertex.Color.B / 255f;
                        float A = vertex.Color.A / 255f;
                        iMesh.VertexColorChannels[0].Add(new Assimp.Color4D(R, G, B, A));
                    }

                    // NORMALS
                    if (vertex.Normal != null)
                    {
                        iMesh.Normals.Add(new Assimp.Vector3D(vertex.Normal.X, vertex.Normal.Y, vertex.Normal.Z));
                    }

                    currentVertex++;
                }

                // FACES
                foreach (List<int> triangle in group.Mesh.Triangles)
                {
                    iMesh.Faces.Add(new Assimp.Face(new int[] { triangle[0], triangle[1], triangle[2] }));
                }

                scene.Meshes.Add(iMesh);

                // NODE
                Assimp.Node iMeshNode = new Assimp.Node("MeshNode" + i.ToString("D4"));
                iMeshNode.MeshIndices.Add(i);

                scene.RootNode.Children.Add(iMeshNode);
            }

            // BONES (Node hierarchy)
            //Assimp.Node armatureNode = new Assimp.Node("Armature");
            //scene.RootNode.Children.Add(armatureNode);
            foreach (ModelCommon.Bone bone in model.Bones)
            {
                string boneName = "Bone" + bone.Index.ToString("D4");
                Assimp.Node boneNode = new Assimp.Node(boneName);

                Assimp.Node parentNode;
                if (bone.ParentIndex == -1)
                {
                    //parentNode = armatureNode;
                    parentNode = scene.RootNode;
                }
                else
                {
                    parentNode = scene.RootNode.FindNode("Bone" + bone.ParentIndex.ToString("D4"));
                }

                boneNode.Transform = AssimpGeneric.GetNodeTransformMatrix(new Vector3(bone.ScaleX, bone.ScaleY, bone.ScaleZ),
                                                                          new Vector3(bone.RotationX, bone.RotationY, bone.RotationZ),
                                                                          new Vector3(bone.TranslationX, bone.TranslationY, bone.TranslationZ));

                parentNode.Children.Add(boneNode);
            }

            return scene;
        }

        public static VifMesh getVifMeshFromAssimp(Assimp.Mesh mesh, Matrix4x4[] boneMatrices)
        {
            VifMesh vifMesh = new VifMesh();
            vifMesh.BoneMatrices = boneMatrices;

            // STEP 1 - Get all of the vertex data together

            List<VifCommon.VifVertex> verticesAssimpOrder = new List<VifCommon.VifVertex>();

            // Absolute position and UV coordinates
            for (int i = 0; i < mesh.Vertices.Count; i++)
            {
                VifCommon.VifVertex vertex = new VifCommon.VifVertex();

                vertex.AbsolutePosition = AssimpGeneric.ToNumerics(mesh.Vertices[i]);

                short u = (short)(mesh.TextureCoordinateChannels[0][i].X * 4096.0f);
                short v = (short)((1 - mesh.TextureCoordinateChannels[0][i].Y) * 4096.0f);
                vertex.UvCoord = new VifCommon.UVCoord(u, v);
                if (mesh.HasVertexColors(0))
                {
                    byte R = (byte)(mesh.VertexColorChannels[0][i].R * 255);
                    byte G = (byte)(mesh.VertexColorChannels[0][i].G * 255);
                    byte B = (byte)(mesh.VertexColorChannels[0][i].B * 255);
                    byte A = (byte)(mesh.VertexColorChannels[0][i].A * 255);
                    vertex.Color = new VifCommon.VertexColor(R, G, B, A);
                }
                if(mesh.HasNormals)
                {
                    vertex.Normal = new VifCommon.VertexNormal(mesh.Normals[i].X, mesh.Normals[i].Y, mesh.Normals[i].Z);
                }

                verticesAssimpOrder.Add(vertex);
            }

            // Weights (Assimp stores the weights in the bones, not the vertices)
            for (int i = 0; i < mesh.Bones.Count; i++)
            {
                for (int j = 0; j < mesh.Bones[i].VertexWeights.Count; j++)
                {
                    Assimp.VertexWeight vertexWeight = mesh.Bones[i].VertexWeights[j];
                    VifCommon.BoneRelativePosition relativePosition = new VifCommon.BoneRelativePosition();

                    relativePosition.BoneIndex = i;
                    relativePosition.Weight = vertexWeight.Weight;

                    if (relativePosition.BoneIndex > boneMatrices.Count())
                    {
                        throw new Exception("Weight to non-existent bone");
                    }
                    if (relativePosition.Weight < 0 || relativePosition.Weight > 1)
                    {
                        throw new Exception("Weight is under 0 or over 1");
                    }

                    verticesAssimpOrder[vertexWeight.VertexID].RelativePositions.Add(relativePosition);
                }
            }

            // Calc bone-relative positions
            for (int i = 0; i < verticesAssimpOrder.Count; i++)
            {
                VifCommon.VifVertex vertex = verticesAssimpOrder[i];
                for (int j = 0; j < vertex.RelativePositions.Count; j++)
                {
                    VifCommon.BoneRelativePosition boneRelativePos = vertex.RelativePositions[j];
                    boneRelativePos.Coord = ModelCommon.getRelativePosition(vertex.AbsolutePosition, boneMatrices[boneRelativePos.BoneIndex], boneRelativePos.Weight);
                }
            }

            // STEP 2 - Create the packet based on face order. Each face will add 3 vertices (Not compressed)

            for (int i = 0; i < mesh.Faces.Count; i++)
            {
                if (mesh.Faces[i].IndexCount != 3)
                {
                    throw new Exception("Face doesn't have exactly 3 vertices. VIF meshes only accept 3-vertex faces.");
                }

                VifCommon.VifVertex vertex1 = verticesAssimpOrder[mesh.Faces[i].Indices[0]];
                VifCommon.VifVertex vertex2 = verticesAssimpOrder[mesh.Faces[i].Indices[1]];
                VifCommon.VifVertex vertex3 = verticesAssimpOrder[mesh.Faces[i].Indices[2]];

                if (!vertex1.isValidVertex() ||
                   !vertex2.isValidVertex() ||
                   !vertex3.isValidVertex())
                {
                    throw new Exception("Vertex is not valid. Make sure that all of the vertices have at least 1 weight");
                }

                vifMesh.Faces.Add(new VifCommon.VifFace(vertex1, vertex2, vertex3));
            }

            return vifMesh;
        }

        public static void AddAnimation(Assimp.Scene assimpScene, Bar mdlxBar, AnimationBinary animation)
        {
            // Set basic data
            Assimp.Animation assimpAnimation = new Assimp.Animation();
            assimpAnimation.Name = "EXPORT";
            assimpAnimation.DurationInTicks = animation.MotionFile.InterpolatedMotionHeader.FrameCount;
            assimpAnimation.TicksPerSecond = animation.MotionFile.InterpolatedMotionHeader.FrameData.FramesPerSecond;
            assimpScene.Animations.Add(assimpAnimation);

            HashSet<float> keyframeTimes = animation.MotionFile.KeyTimes.ToHashSet();

            // Get real duration
            float minTime = 0;
            float maxTime = 0;
            foreach (var keyTime in keyframeTimes)
            {
                if(keyTime < minTime)
                    minTime = keyTime;
                if(keyTime > maxTime)
                    maxTime = keyTime;
            }
            assimpAnimation.DurationInTicks = maxTime - minTime;

            // Get absolute transformation matrices of the bones
            Dictionary<float, Matrix4x4[]> frameMatrices = getMatricesForKeyFrames(mdlxBar, animation, keyframeTimes);

            // Prepare channels per bone
            Dictionary<int, Assimp.NodeAnimationChannel> animationChannelsPerBone = new Dictionary<int, Assimp.NodeAnimationChannel>();
            for (int i = 0; i < animation.MotionFile.InterpolatedMotionHeader.BoneCount; i++)
            {
                Assimp.NodeAnimationChannel nodeAnimChannel = new Assimp.NodeAnimationChannel();
                nodeAnimChannel.NodeName = "Bone" + i.ToString("D4");
                animationChannelsPerBone.Add(i, nodeAnimChannel);
                assimpAnimation.NodeAnimationChannels.Add(nodeAnimChannel);
            }

            // Get bone data
            List<ModelCommon.Bone> modelBones = new List<ModelCommon.Bone>();
            foreach (Bar.Entry barEntry in mdlxBar)
            {
                if(barEntry.Type == Bar.EntryType.Model)
                {
                    ModelSkeletal modelFile = ModelSkeletal.Read(barEntry.Stream);
                    modelBones = modelFile.Bones;
                    break;
                }
            }

            // Set channels
            foreach (float keyTime in frameMatrices.Keys) // Frame
            {
                for (int j = 0; j < frameMatrices[keyTime].Length; j++) // Bone
                {
                    Assimp.NodeAnimationChannel channel = animationChannelsPerBone[j];

                    Matrix4x4 currentFrameBoneMatrix = frameMatrices[keyTime][j];

                    // Transform to local
                    if (modelBones[j].ParentIndex != -1)
                    {
                        Matrix4x4.Invert(frameMatrices[keyTime][modelBones[j].ParentIndex], out Matrix4x4 invertedParent);
                        currentFrameBoneMatrix *= invertedParent;
                    }

                    Assimp.Matrix4x4 assimpMatrix = AssimpGeneric.ToAssimp(currentFrameBoneMatrix);
                    assimpMatrix.Decompose(out Assimp.Vector3D scaling, out Assimp.Quaternion rotation, out Assimp.Vector3D translation);

                    Assimp.VectorKey positionKey = new Assimp.VectorKey(keyTime / assimpAnimation.TicksPerSecond, translation);
                    Assimp.VectorKey scalingKey = new Assimp.VectorKey(keyTime / assimpAnimation.TicksPerSecond, new Assimp.Vector3D(RoundFloat(scaling.X), RoundFloat(scaling.Y), RoundFloat(scaling.Z)));
                    //Assimp.VectorKey scalingKey = new Assimp.VectorKey(keyTime / assimpAnimation.TicksPerSecond, scaling);
                    Assimp.QuaternionKey rotationKey = new Assimp.QuaternionKey(keyTime / assimpAnimation.TicksPerSecond, rotation);

                    // Ignore duplicates
                    if(channel.PositionKeys.Count > 0)
                    {
                        Assimp.VectorKey previousPositionKey = channel.PositionKeys[channel.PositionKeys.Count - 1];
                        Assimp.VectorKey previousScalingKey = channel.ScalingKeys[channel.ScalingKeys.Count - 1];
                        Assimp.QuaternionKey previousRotationKey = channel.RotationKeys[channel.RotationKeys.Count - 1];

                        if (!positionKey.Equals(previousPositionKey))
                        {
                            channel.PositionKeys.Add(positionKey);
                        }
                        if (!scalingKey.Equals(previousScalingKey))
                        {
                            channel.ScalingKeys.Add(scalingKey);
                        }
                        if (!rotationKey.Equals(previousRotationKey))
                        {
                            channel.RotationKeys.Add(rotationKey);
                        }
                    }
                    else
                    {
                        channel.PositionKeys.Add(positionKey);
                        channel.ScalingKeys.Add(scalingKey);
                        channel.RotationKeys.Add(rotationKey);
                    }
                }
            }

            if(assimpScene.RootNode.FindNode("Armature") != null)
            {
                assimpAnimation.NodeAnimationChannels.Add(getArmatureChannel(keyframeTimes.ToArray()[0] / assimpAnimation.TicksPerSecond, assimpAnimation.DurationInTicks, animation.MotionFile.InterpolatedMotionHeader.FrameData.FramesPerSecond));
            }
        }

        /****************
         * UTILITIES
         ****************/

        private static Vector3 ToVector3(Vector4 pos) => new Vector3(pos.X, pos.Y, pos.Z);
        private static float RoundFloat(float value)
        {
            float reminder = value % 1;
            if (reminder > 0.999999 && reminder < 0.999999999999)
            {
              return value - reminder + 1;
            }
            else if (reminder > 0 && reminder < 0.00001)
            {
              return value - reminder;
            }
            return value;
        }

        private static Assimp.NodeAnimationChannel getArmatureChannel(double startFrame, double endFrame, double framesPerSecond)
        {
            Assimp.NodeAnimationChannel armatureChannel = new Assimp.NodeAnimationChannel();
            armatureChannel.NodeName = "Armature";

            armatureChannel.PositionKeys.Add(new Assimp.VectorKey(startFrame / framesPerSecond, new Assimp.Vector3D(0,0,0)));
            armatureChannel.ScalingKeys.Add(new Assimp.VectorKey(startFrame / framesPerSecond, new Assimp.Vector3D(1, 1, 1)));
            armatureChannel.RotationKeys.Add(new Assimp.QuaternionKey(startFrame / framesPerSecond, new Assimp.Quaternion(1, 0, 0, 0)));

            armatureChannel.PositionKeys.Add(new Assimp.VectorKey(endFrame / framesPerSecond, new Assimp.Vector3D(0, 0, 0)));
            armatureChannel.ScalingKeys.Add(new Assimp.VectorKey(endFrame / framesPerSecond, new Assimp.Vector3D(1, 1, 1)));
            armatureChannel.RotationKeys.Add(new Assimp.QuaternionKey(endFrame / framesPerSecond, new Assimp.Quaternion(1, 0, 0, 0)));

            return armatureChannel;
        }

        // Generates the absolute transformation matrices for each bone for the given frames
        // Makes use of IAnimMatricesProvider to generate the matrices
        private static Dictionary<float, Matrix4x4[]> getMatricesForKeyFrames (Bar mdlxBar, AnimationBinary animation, HashSet<float> keyframeTimes)
        {
            // Mdlx as stream is required
            MemoryStream modelStream = new MemoryStream();
            Bar.Write(modelStream, mdlxBar);
            modelStream.Position = 0;

            // Calculate matrices
            Dictionary<float, Matrix4x4[]> frameMatrices = new Dictionary<float, Matrix4x4[]>();
            Bar anbBarFile = Bar.Read(animation.toStream());
            foreach (float keyTime in keyframeTimes)
            {
                // I have no idea why this needs to be done for every frame but otherwise it won't work properly
                AnbIndir currentAnb = new AnbIndir(anbBarFile);
                IAnimMatricesProvider AnimMatricesProvider = currentAnb.GetAnimProvider(modelStream);
                frameMatrices.Add(keyTime, AnimMatricesProvider.ProvideMatrices(keyTime));
                modelStream.Position = 0;
            }

            return frameMatrices;
        }
    }
}

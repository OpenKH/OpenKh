using System.Diagnostics;
using System.Numerics;
using System.Windows.Media.Imaging;

namespace OpenKh.AssimpUtils
{
    public class AssimpGeneric
    {
        public static Microsoft.Xna.Framework.Matrix ToXna(Assimp.Matrix4x4 m) => new Microsoft.Xna.Framework.Matrix(m.A1, m.B1, m.C1, m.D1, m.A2, m.B2, m.C2, m.D2, m.A3, m.B3, m.C3, m.D3, m.A4, m.B4, m.C4, m.D4);
        public static Assimp.Matrix4x4 ToAssimp(Microsoft.Xna.Framework.Matrix m) => new Assimp.Matrix4x4(m.M11, m.M21, m.M31, m.M41, m.M12, m.M22, m.M32, m.M42, m.M13, m.M23, m.M33, m.M43, m.M14, m.M24, m.M34, m.M44);

        public static Assimp.Matrix4x4 ToAssimp(Matrix4x4 m) => new Assimp.Matrix4x4(m.M11, m.M21, m.M31, m.M41, m.M12, m.M22, m.M32, m.M42, m.M13, m.M23, m.M33, m.M43, m.M14, m.M24, m.M34, m.M44);
        //public static Matrix4x4 ToNumerics(Assimp.Matrix4x4 m) => new Matrix4x4(m.A1, m.A2, m.A3, m.A4, m.B1, m.B2, m.B3, m.B4, m.C1, m.C2, m.C3, m.C4, m.D1, m.D2, m.D3, m.D4); // Pretty sure this is wrong
        public static Matrix4x4 ToNumerics(Assimp.Matrix4x4 m) => new Matrix4x4(m.A1, m.B1, m.C1, m.D1, m.A2, m.B2, m.C2, m.D2, m.A3, m.B3, m.C3, m.D3, m.A4, m.B4, m.C4, m.D4);
        public static Assimp.Vector3D ToAssimp(Vector3 v) => new Assimp.Vector3D(v.X, v.Y, v.Z);
        public static Vector3 ToNumerics(Assimp.Vector3D v) => new Vector3(v.X, v.Y, v.Z);

        // File formats supported by Assimp
        public enum FileFormat
        {
            // Common
            collada,
            fbx,
            fbxa,
            obj,

            // Other
            x,
            stp,
            objnomtl,
            stl,
            stlb,
            ply,
            plyb,
            //3ds,
            gltf2,
            glb2,
            gltf,
            glb,
            assbin,
            assxml,
            x3d,
            m3d,
            m3da,
            //3mf,
            pbrt,
            assjson
        }

        public static Assimp.Scene getAssimpSceneFromFile(string filePath)
        {
            Assimp.AssimpContext assimp = new Assimp.AssimpContext();
            Assimp.Scene scene = assimp.ImportFile(filePath);

            return scene;
        }

        // Returns the file extension that the given format uses
        public static string GetFormatFileExtension(FileFormat format)
        {
            switch (format)
            {
                case FileFormat.collada:
                    return "dae";
                case FileFormat.fbx:
                case FileFormat.fbxa:
                    return "fbx";
                default:
                    return format.ToString();
            }
        }
        // Exports the given Assimp scene in the given format
        public static void ExportScene(Assimp.Scene scene, FileFormat format = FileFormat.fbx, string filename = "fileout.fbx")
        {
            Assimp.AssimpContext context = new Assimp.AssimpContext();
            context.ExportFile(scene, filename, format.ToString());
        }
        public static void ExportSceneWithDialog(Assimp.Scene scene, FileFormat format = FileFormat.fbx, string filename = "fileout")
        {
            SaveFileDialog sfd;
            sfd = new SaveFileDialog();
            sfd.Title = "Save file";
            sfd.FileName = filename + "." + GetFormatFileExtension(format);
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                Assimp.AssimpContext context = new Assimp.AssimpContext();
                context.ExportFile(scene, sfd.FileName, format.ToString());
            }
        }
        

        // Gets a basic Assimp scene. It also contains notes on how to make a valid Assimp scene for exporting
        public static Assimp.Scene GetBaseScene()
        {
            Assimp.Scene scene = new Assimp.Scene();
            scene.RootNode = new Assimp.Node("RootNode");

            return scene;

            /* To get a valid assimp scene you need the following:
             * A mesh.
             * A mesh node. One per mesh.
             * Material + texture for the mesh. (Optional)
             * Bones (Weights) for the mesh. Note that Assimp requires each mesh to have all bones even if they have no vertex weights (Optional)
             * Vertices for the mesh.
             * Faces for the mesh.
             * Nodes (Skeleton) as children of rootnode. One per bone. Same name as the bones.
             */
        }

        // Returns the Transform matrix used by Assimp nodes based on SRT data. Note: the rotation uses quaternions
        public static Assimp.Matrix4x4 GetNodeTransformMatrix(Vector3 scale, Vector3 radianRotation, Vector3 translation)
        {
            Assimp.Matrix4x4 nodeTransform = Assimp.Matrix4x4.Identity;

            nodeTransform *= Assimp.Matrix4x4.FromScaling(new Assimp.Vector3D(scale.X, scale.Y, scale.Z));

            Assimp.Matrix4x4 rotation = Assimp.Matrix4x4.FromRotationX(radianRotation.X) *
                                        Assimp.Matrix4x4.FromRotationY(radianRotation.Y) *
                                        Assimp.Matrix4x4.FromRotationZ(radianRotation.Z);

            nodeTransform *= rotation;

            nodeTransform *= Assimp.Matrix4x4.FromTranslation(new Assimp.Vector3D(translation.X, translation.Y, translation.Z));

            return nodeTransform;
        }

        // Finds a bone by name in a bone list
        public static Assimp.Bone FindBone(List<Assimp.Bone> list_bones, string boneName)
        {
            foreach(Assimp.Bone bone in list_bones)
            {
                if (bone.Name == boneName)
                    return bone;
            }

            return null;
        }

        // Adds the given weight to the mesh's bone. Usually uneeded since all meshes need to have all bones and it's done in one go
        public static void AddVertexWeight(Assimp.Mesh mesh, int boneId, int vertexId, float weight = 1)
        {
            bool boneExists = false;
            Assimp.Bone bone = FindBone(mesh.Bones, "Bone" + boneId);
            Assimp.VertexWeight vertexWeight = new Assimp.VertexWeight(vertexId, weight);

            if (bone != null)
            {
                bone.VertexWeights.Add(vertexWeight);
            }
            else
            {
                List<Assimp.VertexWeight> boneWeights = new List<Assimp.VertexWeight>();
                boneWeights.Add(vertexWeight);
                mesh.Bones.Add(new Assimp.Bone("Bone" + boneId, Assimp.Matrix3x3.Identity, boneWeights.ToArray()));
            }
        }

        // Not Assimp related but exporting images is needed
        public static void ExportBitmapSourceAsPng(BitmapSource image, string filePath)
        {
            using (var fileStream = new FileStream(filePath + ".png", FileMode.Create))
            {
                BitmapEncoder encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(image));
                encoder.Save(fileStream);
            }
        }
    }
}

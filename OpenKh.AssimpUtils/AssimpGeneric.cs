using System.Diagnostics;
using System.Numerics;
using System.Windows.Media.Imaging;

namespace OpenKh.AssimpUtils
{
    public class AssimpGeneric
    {
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
            TEST_decomposing(scale, radianRotation, translation);

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


        // TEST
        public static void TEST_decomposing(Vector3 scale, Vector3 radianRotation, Vector3 translation)
        {
            bool PRINT_DEBUG = false;

            Assimp.Matrix4x4 nodeTransform = Assimp.Matrix4x4.Identity;

            nodeTransform *= Assimp.Matrix4x4.FromScaling(new Assimp.Vector3D(scale.X, scale.Y, scale.Z));


            Assimp.Matrix4x4 rotation = Assimp.Matrix4x4.FromRotationX(radianRotation.X) *
                                        Assimp.Matrix4x4.FromRotationY(radianRotation.Y) *
                                        Assimp.Matrix4x4.FromRotationZ(radianRotation.Z);

            nodeTransform *= rotation;

            nodeTransform *= Assimp.Matrix4x4.FromTranslation(new Assimp.Vector3D(translation.X, translation.Y, translation.Z));

            //----------------------------------
            // TEST AREA

            // Original bone data
            Debug.WriteLineIf(PRINT_DEBUG,"DEBUG: IN Rotation in radians: " + radianRotation);

            // Immediate conversion test
            Quaternion toq = ToQuaternion(radianRotation);
            Debug.WriteLineIf(PRINT_DEBUG, "DEBUG: IMM to quaternion: " + toq);
            Vector3 toe = ToEulerAngles(toq);
            Debug.WriteLineIf(PRINT_DEBUG, "DEBUG: IMM quaternion to Euler: " + toe);

            // Matrix from quaternion
            Vector3 decomposedScale;
            Quaternion decomposedRotation;
            Vector3 decomposedTranslation;

            Matrix4x4 matrixFromQuaternion = Matrix4x4.CreateFromQuaternion(toq);
            Matrix4x4.Decompose(matrixFromQuaternion, out decomposedScale, out decomposedRotation, out decomposedTranslation);

            Debug.WriteLineIf(PRINT_DEBUG, "DEBUG: Decomposed QUA matrix in quaternion: " + decomposedRotation);
            Debug.WriteLineIf(PRINT_DEBUG, "DEBUG: Decomposed QUA matrix in radians: " + ToEulerAngles(new Assimp.Quaternion(decomposedRotation.X, decomposedRotation.Y, decomposedRotation.Z, decomposedRotation.W)));

            // Matrix from radians
            Assimp.Vector3D decomposedScaleMR;
            Assimp.Quaternion decomposedRotationMR;
            Assimp.Vector3D decomposedTranslationMR;
            rotation.Decompose(out decomposedScaleMR, out decomposedRotationMR, out decomposedTranslationMR);

            Debug.WriteLineIf(PRINT_DEBUG, "DEBUG: Decomposed RAD matrix in quaternion: " + decomposedRotation);
            Debug.WriteLineIf(PRINT_DEBUG, "DEBUG: Decomposed RAD matrix in radians: " + ToEulerAngles(new Quaternion(decomposedRotation.X, decomposedRotation.Y, decomposedRotation.Z, decomposedRotation.W)));
        }

        public static Quaternion ToQuaternion(Vector3 vIn)
        {
            Vector3 v = ToEulerDegrees(vIn);

            float cy = (float)Math.Cos(v.Z * 0.5);
            float sy = (float)Math.Sin(v.Z * 0.5);
            float cp = (float)Math.Cos(v.Y * 0.5);
            float sp = (float)Math.Sin(v.Y * 0.5);
            float cr = (float)Math.Cos(v.X * 0.5);
            float sr = (float)Math.Sin(v.X * 0.5);

            return new Quaternion
            {
                W = (cr * cp * cy + sr * sp * sy),
                X = (sr * cp * cy - cr * sp * sy),
                Y = (cr * sp * cy + sr * cp * sy),
                Z = (cr * cp * sy - sr * sp * cy)
            };

        }

        public static Vector3 ToEulerAngles(Quaternion q)
        {
            Vector3 angles = new();

            // roll / x
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch / y
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }

            // yaw / z
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }
        public static Assimp.Vector3D ToEulerAngles(Assimp.Quaternion q)
        {
            Assimp.Vector3D angles = new Assimp.Vector3D();

            // roll / x
            double sinr_cosp = 2 * (q.W * q.X + q.Y * q.Z);
            double cosr_cosp = 1 - 2 * (q.X * q.X + q.Y * q.Y);
            angles.X = (float)Math.Atan2(sinr_cosp, cosr_cosp);

            // pitch / y
            double sinp = 2 * (q.W * q.Y - q.Z * q.X);
            if (Math.Abs(sinp) >= 1)
            {
                angles.Y = (float)Math.CopySign(Math.PI / 2, sinp);
            }
            else
            {
                angles.Y = (float)Math.Asin(sinp);
            }

            // yaw / z
            double siny_cosp = 2 * (q.W * q.Z + q.X * q.Y);
            double cosy_cosp = 1 - 2 * (q.Y * q.Y + q.Z * q.Z);
            angles.Z = (float)Math.Atan2(siny_cosp, cosy_cosp);

            return angles;
        }

        public static Vector3 ToEulerDegrees(Vector3 radianss)
        {
            return new Vector3((float)(radianss.X * (180 / Math.PI)), (float)(radianss.Y * (180 / Math.PI)), (float)(radianss.Z * (180 / Math.PI)));
        }
        public static Vector3 ToEulerRadians(Vector3 degrees)
        {
            return new Vector3((float)(degrees.X * (Math.PI / 180)), (float)(degrees.Y * (Math.PI / 180)), (float)(degrees.Z * (Math.PI / 180)));
        }
    }
}

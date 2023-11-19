using COLLADASchema;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Ddd.PmoV4_2.Vertex;

namespace OpenKh.Tools.KhModels.Utils
{
    public static class DaeModels
    {
        public record DaeModel(
            IReadOnlyList<DaeBone> Bones,
            IReadOnlyList<DaeMaterial> Materials,
            IReadOnlyList<DaeMesh> Meshes,
            IReadOnlyList<DaeSkinController> SkinControllers,
            IReadOnlyList<DaeMesh> InstanceGeometries,
            float GeometryScaling);

        public record DaeInstanceGeometry(
            );

        public record DaeSkinController(
            DaeMesh Mesh,
            IReadOnlyList<DaeBone> Bones,
            IReadOnlyList<Matrix4x4> InvBindMatrices,
            IReadOnlyList<float> SkinWeights,
            IReadOnlyList<IReadOnlyList<DaeVertexWeight>> VertexWeightSets);

        public record DaeVertexWeight(
            int JointIndex,
            int WeightIndex);

        /// <param name="TriangleStripSets">
        /// From blender 3.2.0:
        /// 
        /// ```
        /// ERROR: Primitive type TRIANGLE_STRIPS is not supported.
        /// Ignoring mesh Mesh1109
        /// ```
        /// </param>
        public record DaeMesh(
            string Name,
            DaeMaterial? Material,
            IReadOnlyList<Vector3> Vertices,
            IReadOnlyList<Vector2> TextureCoordinates,
            IReadOnlyList<IReadOnlyList<DaeVertexPointer>> TriangleStripSets,
            IReadOnlyList<IReadOnlyList<DaeVertexPointer>> TriangleSets);

        public record DaeVertexPointer(
            int VertexIndex,
            int TextureCoordinateIndex);

        /// <param name="ParentIndex">`-1` for root</param>
        public record DaeBone(
            string Name,
            int ParentIndex,
            Vector3 RelativeScale,
            Vector3 RelativeRotation,
            Vector3 RelativeTranslation
        );

        public record DaeMaterial(
            string Name,
            string PngFilePath);
    }
}

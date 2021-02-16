using System.Linq;

namespace OpenKh.Ps2
{
    public class VertexBox
    {
        public int MatrixIndex { get; set; }
        public int[] VertexIndices { get; set; }
    }

    public class VertexIndexWeighted
    {
        public int MatrixIndex { get; set; }
        public int VertexIndex { get; set; }
    }

    public static class VpuPacketExtensions
    {
        public static VertexBox[] GetFromMatrixIndices(this VpuPacket vpu, int[] matrixIndices)
        {
            var matrixVertices = new VertexBox[vpu.VertexRange.Length];
            for (int i = 0, j = 0; i < vpu.VertexRange.Length; i++)
            {
                matrixVertices[i] = new VertexBox
                {
                    MatrixIndex = matrixIndices[i],
                    VertexIndices = Enumerable.Range(j, vpu.VertexRange[i]).ToArray()
                };
                j += vpu.VertexRange[i];
            }

            return matrixVertices;
        }

        public static VertexIndexWeighted[][] GetWeightedVertices(this VpuPacket vpu, VertexBox[] vertices)
        {
            var flatVertices = vertices.SelectMany(x => x.VertexIndices, (x, vertexIndex) => new VertexIndexWeighted[]
            {
                    new VertexIndexWeighted
                    {
                        MatrixIndex = x.MatrixIndex,
                        VertexIndex = vertexIndex
                    }
            }).ToArray();

            if ((vpu.VertexWeightedIndices?.Length ?? 0) == 0)
                return flatVertices;

            var index = 0;
            var outVertices = new VertexIndexWeighted[vpu.VertexWeightedCount][];
            foreach (var slice in vpu.VertexWeightedIndices)
                foreach (var desc in slice)
                    outVertices[index++] = desc.Select(x => flatVertices[x][0]).ToArray();

            return outVertices;
        }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.MapGen.Models
{
    public class CenterPointedMesh
    {
        public BigMesh bigMesh { get; }
        public Vector3 centerPoint { get; }

        public CenterPointedMesh(BigMesh bigMesh)
        {
            this.bigMesh = bigMesh;

            centerPoint = GetCenter(
                bigMesh.triangleStripList
                    .SelectMany(triangleStrip => triangleStrip.vertexIndices)
                    .Select(index => bigMesh.vertexList[index])
            );
        }

        public override string ToString() => centerPoint.ToString();

        private static Vector3 GetCenter(IEnumerable<Vector3> positions)
        {
            double x = 0, y = 0, z = 0;
            int n = 0;
            foreach (var one in positions)
            {
                ++n;
                x += one.X;
                y += one.Y;
                z += one.Z;
            }
            return new Vector3(
                (float)(x / n),
                (float)(y / n),
                (float)(z / n)
            );
        }
    }
}

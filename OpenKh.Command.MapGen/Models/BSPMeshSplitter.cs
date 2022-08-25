using OpenKh.Command.MapGen.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.MapGen.Models
{
    public class BSPMeshSplitter : ISpatialMeshCutter
    {
        public IEnumerable<CenterPointedMesh> Meshes { get; }

        private readonly int partitionSize;

        public IEnumerable<ISpatialMeshCutter> Cut()
        {
            var meshes = Meshes.ToArray();

            if (meshes.Length > partitionSize)
            {
                var range = new Range(meshes);
                if (range.yLen >= range.xLen)
                {
                    if (range.zLen >= range.yLen)
                    {
                        // z-cut
                        return new BSPMeshSplitter[]
                        {
                            new BSPMeshSplitter(meshes.Where(it => it.centerPoint.Z >= range.zCenter).ToArray()),
                            new BSPMeshSplitter(meshes.Where(it => it.centerPoint.Z < range.zCenter).ToArray()),
                        };
                    }
                    else
                    {
                        // y-cut
                        return new BSPMeshSplitter[]
                        {
                            new BSPMeshSplitter(meshes.Where(it => it.centerPoint.Y >= range.yCenter).ToArray()),
                            new BSPMeshSplitter(meshes.Where(it => it.centerPoint.Y < range.yCenter).ToArray()),
                        };
                    }
                }
                else
                {
                    // x-cut
                    return new BSPMeshSplitter[]
                    {
                        new BSPMeshSplitter(meshes.Where(it => it.centerPoint.X >= range.xCenter).ToArray()),
                        new BSPMeshSplitter(meshes.Where(it => it.centerPoint.X < range.xCenter).ToArray()),
                    };
                }
            }
            return new BSPMeshSplitter[] { this };
        }

        private class Range
        {
            public float xMin = float.MaxValue;
            public float xMax = float.MinValue;
            public float yMin = float.MaxValue;
            public float yMax = float.MinValue;
            public float zMin = float.MaxValue;
            public float zMax = float.MinValue;

            public float xLen;
            public float yLen;
            public float zLen;

            public float xCenter;
            public float yCenter;
            public float zCenter;

            public Range(CenterPointedMesh[] points)
            {
                foreach (var point in points)
                {
                    var position = point.centerPoint;
                    xMin = Math.Min(xMin, position.X);
                    xMax = Math.Max(xMax, position.X);
                    yMin = Math.Min(yMin, position.Y);
                    yMax = Math.Max(yMax, position.Y);
                    zMin = Math.Min(zMin, position.Z);
                    zMax = Math.Max(zMax, position.Z);
                }
                xLen = (xMax - xMin);
                yLen = (yMax - yMin);
                zLen = (zMax - zMin);
                xCenter = (xMax + xMin) / 2;
                yCenter = (yMax + yMin) / 2;
                zCenter = (zMax + zMin) / 2;
            }
        }

        public BSPMeshSplitter(IEnumerable<CenterPointedMesh> meshes, int partitionSize = 10)
        {
            Meshes = meshes;
            this.partitionSize = partitionSize;
        }
    }
}

using OpenKh.Command.MapGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKh.Command.MapGen.Utils
{
    public static class BigMeshSplitter
    {
        private const int MaxVertCount = 71;

        public static IEnumerable<BigMesh> Split(BigMesh bigMesh)
        {
            var stripIndex = 0;
            var maxStrip = bigMesh.triangleStripList.Count;
            while (stripIndex < maxStrip)
            {
                var smallMesh = new BigMesh
                {
                    textureIndex = bigMesh.textureIndex,
                    matDef = bigMesh.matDef,
                };

                var localIndexMap = new Dictionary<int, int>();
                var countStocked = 0;
                for (; stripIndex < maxStrip; stripIndex++)
                {
                    var triStrip = bigMesh.triangleStripList[stripIndex];

                    if (countStocked + triStrip.vertexIndices.Count > MaxVertCount)
                    {
                        break;
                    }

                    var newTriStrip = new BigMesh.TriangleStrip
                    {
                        uvList = triStrip.uvList.ToList(),
                        vertexColorList = triStrip.vertexColorList.ToList(),
                        vertexIndices = triStrip.vertexIndices
                            .Select(gi => MapToLocal(localIndexMap, gi))
                            .ToList(),
                    };

                    smallMesh.triangleStripList.Add(newTriStrip);

                    countStocked += newTriStrip.vertexIndices.Count;
                }

                var globalIndexMap = localIndexMap
                    .ToDictionary(pair => pair.Value, pair => pair.Key);

                smallMesh.textureIndex = bigMesh.textureIndex;

                for (int loop = 0; loop < localIndexMap.Count; loop++)
                {
                    var globalIndex = globalIndexMap[loop];
                    smallMesh.vertexList.Add(bigMesh.vertexList[globalIndex]);
                }

                yield return smallMesh;
            }
        }

        private static int MapToLocal(IDictionary<int, int> dict, int index)
        {
            if (!dict.TryGetValue(index, out int localIndex))
            {
                localIndex = dict.Count;
                dict[index] = localIndex;
            }
            return localIndex;
        }
    }
}

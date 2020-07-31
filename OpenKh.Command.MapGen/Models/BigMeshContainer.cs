using Assimp;
using OpenKh.Command.MapGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using static OpenKh.Command.MapGen.Models.MapGenConfig;

namespace OpenKh.Command.MapGen.Models
{
    public class BigMeshContainer
    {
        public List<BigMesh> MeshList { get; } = new List<BigMesh>();

        public List<MaterialDef> AllocatedMaterialDefs { get; } = new List<MaterialDef>();

        public BigMesh AllocateBigMeshForMaterial(MaterialDef matDef)
        {
            var it = MeshList.Find(it => it.matDef == matDef);
            if (it == null)
            {
                var textureIndex = -1;
                if (!matDef.nodraw)
                {
                    textureIndex = AllocatedMaterialDefs.Count;
                    AllocatedMaterialDefs.Add(matDef);
                }

                var newOne = new BigMesh
                {
                    matDef = matDef,
                    textureIndex = textureIndex,
                };
                MeshList.Add(newOne);
                return newOne;
            }
            else
            {
                return it;
            }
        }

        public IEnumerable<MaterialDef> GetMaterialDefList() =>
            MeshList.Select(one => one.matDef);
    }
}

using System;
using System.Collections.Generic;
using System.Numerics;
using System.Text;

namespace OpenKh.Kh2.Models.VIF
{
    /*
     * A Vif Mesh contains all of the vertices that form a mesh.
     * Each vertex has its UV coordinates, its position is space defined by their weights to bones, and the flag that indicates how to construct the faces.
     * Additionally it has its absolute position in space for ease of use.
     */
    public class VifMesh
    {
        public List<VifCommon.VifFace> Faces;
        public Matrix4x4[] BoneMatrices;

        public VifMesh()
        {
            Faces = new List<VifCommon.VifFace>();
        }
    }
}

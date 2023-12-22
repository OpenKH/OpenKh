using Assimp;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.AssimpSupplemental
{
    internal record NodeRef
    {
        public int ParentIndex { get; set; }
        public Node ArmatureNode { get; set; }
        public Bone? MeshBone { get; set; }

        public NodeRef(int parentIndex, Node armatureNode, Bone? meshBone)
        {
            ParentIndex = parentIndex;
            ArmatureNode = armatureNode;
            MeshBone = meshBone;
        }
    }
}

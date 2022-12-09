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

        public NodeRef(int parentIndex, Node armatureNode)
        {
            ParentIndex = parentIndex;
            ArmatureNode = armatureNode;
        }
    }
}

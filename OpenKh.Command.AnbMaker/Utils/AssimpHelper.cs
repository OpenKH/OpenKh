using Assimp;
using OpenKh.Command.AnbMaker.Utils.AssimpSupplemental;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils
{
    internal class AssimpHelper
    {
        public static NodeRef[] FlattenNodes(Node topNode)
        {
            var list = new List<NodeRef>();

            var stack = new Stack<NodeRef>();
            stack.Push(new NodeRef(-1, topNode));

            while (stack.Any())
            {
                var nodeRef = stack.Pop();
                var idx = list.Count;
                list.Add(nodeRef);

                foreach (var sub in nodeRef.ArmatureNode.Children.Reverse())
                {
                    stack.Push(new NodeRef(idx, sub));
                }
            }

            return list.ToArray();
        }
    }
}

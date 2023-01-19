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
        public static NodeRef[] FlattenNodes(Node topNode, Mesh mesh)
        {
            var boneDict = mesh.Bones
                .ToDictionary(bone => bone.Name, bone => bone);

            var list = new List<NodeRef>();

            var stack = new Stack<NodeRef>();
            stack.Push(new NodeRef(-1, topNode, boneDict[topNode.Name]));

            while (stack.Any())
            {
                var nodeRef = stack.Pop();
                var idx = list.Count;
                list.Add(nodeRef);

                foreach (var sub in nodeRef.ArmatureNode.Children.Reverse())
                {
                    stack.Push(new NodeRef(idx, sub, boneDict[topNode.Name]));
                }
            }

            return list.ToArray();
        }

        public static Node FindRootBone(Node rootNode, string rootName)
        {
            rootName = rootName ?? "bone000";

            var found = rootNode.FindNode(rootName);
            if (found == null)
            {
                static string DumpTree(Node target)
                {
                    var writer = new StringWriter();

                    void VisitNode(Node at, int depth)
                    {
                        writer.WriteLine($"{new string(' ', 2 * depth)}- {at.Name}");

                        foreach (var child in at.Children)
                        {
                            VisitNode(child, depth + 1);
                        }
                    }

                    VisitNode(target, 0);

                    return writer.ToString();
                }

                throw new Exception($"Find node by name \"{rootName}\" not found. It can be one of the following node name:\n\n{DumpTree(rootNode)}");
            }
            return found;
        }
    }
}

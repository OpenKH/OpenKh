using OpenKh.Command.MapGen.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.MapGen.Utils
{
    public static class FlattenSpatialNode
    {
        public static IEnumerable<ISpatialNodeCutter> From(ISpatialNodeCutter cutter)
        {
            var final = new List<ISpatialNodeCutter>();

            var stack = new Stack<ISpatialNodeCutter>();
            stack.Push(cutter);
            while (stack.Any())
            {
                var one = stack.Pop();

                var children = one.Cut().ToArray();
                if (children.Count() == 1)
                {
                    final.Add(one);
                }
                else
                {
                    foreach (var it in children)
                    {
                        stack.Push(it);
                    }
                }
            }

            return final;
        }
    }
}

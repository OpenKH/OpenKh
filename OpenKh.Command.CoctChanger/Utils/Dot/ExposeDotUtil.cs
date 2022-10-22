using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenKh.Command.CoctChanger.Utils.Dot
{
    internal class ExposeDotUtil
    {
        internal void ExportCoct(Coct coct, string modelOut)
        {
            if (!coct.Nodes.Any())
            {
                return;
            }

            var nodes = new List<DNode>();
            var edges = new List<DEdge>();

            void WalkNode(int entry1Idx, string parentNodeName)
            {
                var node = coct.Nodes[entry1Idx];
                var children = new int[] { node.Child1, node.Child2, node.Child3, node.Child4, node.Child5, node.Child6, node.Child7, node.Child8 }
                    .Select(it => (int)(ushort)it)
                    .Where(it => it != 0xFFFF);

                var nodeName = $"N{entry1Idx}";

                var isRoot = entry1Idx == 0;

                nodes.Add(new DNode
                {
                    Name = nodeName,
                    Attributes = {
                        { "label", "" },
                        { "style", "filled" },
                        { "fillcolor", isRoot ? "forestgreen" : "palegreen" },
                    }
                });

                if (parentNodeName != null)
                {
                    edges.Add(new DEdge { Start = parentNodeName, End = nodeName, });
                }

                foreach (var child in children)
                {
                    WalkNode(child, nodeName);
                }

                foreach (var mesh in node.Meshes)
                {
                    WalkMesh(mesh, nodeName);
                }
            }

            int nextMeshIdx = 0;

            void WalkMesh(Coct.CollisionMesh mesh, string parentNodeName)
            {
                var nodeName = $"M{nextMeshIdx++}";

                nodes.Add(new DNode
                {
                    Name = nodeName,
                    Attributes = {
                        { "label", "" },
                        { "style", "filled" },
                        { "fillcolor", "tomato" },
                    }
                });

                if (parentNodeName != null)
                {
                    edges.Add(new DEdge { Start = parentNodeName, End = nodeName, });
                }

                foreach (var plane in mesh.Collisions)
                {
                    //WalkPlane(plane, nodeName);
                }
            }

            int nextPlaneIdx = 0;

            void WalkPlane(Coct.Collision plane, string parentNodeName)
            {
                var nodeName = $"P{nextPlaneIdx++}";

                nodes.Add(new DNode
                {
                    Name = nodeName,
                    Attributes = {
                        { "label", "" },
                        { "style", "filled" },
                        { "fillcolor", "darkviolet" },
                    }
                });

                if (parentNodeName != null)
                {
                    edges.Add(new DEdge { Start = parentNodeName, End = nodeName, });
                }
            }

            WalkNode(0, null);

            {
                var writer = new StringWriter();
                writer.WriteLine("digraph draw_octal_tree {");
                writer.WriteLine(
                    new DNode
                    {
                        Name = "graph",
                        Attributes = {
                            { "layout", "twopi" },
                        },
                    }
                );
                writer.WriteLine(
                    new DNode
                    {
                        Name = "node",
                        Attributes = {
                            { "shape", "circle" },
                        }
                    }
                );
                foreach (var node in nodes)
                {
                    writer.WriteLine(node);
                }
                foreach (var edge in edges)
                {
                    writer.WriteLine(edge);
                }
                writer.WriteLine("}");
                File.WriteAllText(modelOut, writer.ToString());
            }
        }

        private class DNode
        {
            public string Name { get; set; }
            public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

            public override string ToString() => $"{Name} [{string.Join(", ", Attributes.Select(pair => $"{pair.Key} = {EscapeValue(pair.Value)}"))}];";
        }

        private class DEdge
        {
            public string Start { get; set; }
            public string End { get; set; }
            public IDictionary<string, string> Attributes { get; set; } = new Dictionary<string, string>();

            public override string ToString() => $"{Start} -> {End} [{string.Join(", ", Attributes.Select(pair => $"{pair.Key} = {EscapeValue(pair.Value)}"))}];";
        }

        private static string EscapeValue(string value)
        {
            return (string.IsNullOrEmpty(value) || Regex.IsMatch(value, " \\\\\"\t\r\n"))
                ? "\"" + (value ?? "").Replace("\"", "\"\"").Replace("\\", "\\\\") + "\""
                : value;
        }
    }
}

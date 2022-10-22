using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace OpenKh.Command.DoctChanger.Utils.Dot
{
    internal class ExposeDotUtil
    {
        internal void ExportDoct(Doct doct, string modelOut)
        {
            if (!doct.Entry1List.Any())
            {
                return;
            }

            var nodes = new List<DNode>();
            var edges = new List<DEdge>();

            void WalkEntry1(int entry1Idx, string parentNodeName)
            {
                var entry1 = doct.Entry1List[entry1Idx];
                var children = new int[] { entry1.Child1, entry1.Child2, entry1.Child3, entry1.Child4, entry1.Child5, entry1.Child6, entry1.Child7, entry1.Child8 }
                    .Select(it => (int)(ushort)it)
                    .Where(it => it != 0xFFFF);

                var nodeName = $"D{entry1Idx}";

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
                    WalkEntry1(child, nodeName);
                }

                for (int idx = entry1.Entry2Index; idx < entry1.Entry2LastIndex; idx++)
                {
                    WalkEntry2(idx, nodeName);
                }
            }

            void WalkEntry2(int entry2Idx, string parentNodeName)
            {
                var entry2 = doct.Entry2List[entry2Idx];

                var nodeName = $"E{entry2Idx}";

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
            }

            WalkEntry1(0, null);

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

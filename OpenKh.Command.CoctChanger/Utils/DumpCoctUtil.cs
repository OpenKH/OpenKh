using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

namespace OpenKh.Command.CoctChanger.Utils
{
    public class DumpCoctUtil
    {
        private readonly Coct coct;
        private readonly TextWriter writer;

        public DumpCoctUtil(Coct coct, TextWriter writer)
        {
            this.coct = coct;
            this.writer = writer;

            if (coct.Nodes.Any())
            {
                DumpNode(0, 0);
            }
        }

        private void DumpNode(int index, int indent)
        {
            if (index == -1)
            {
                return;
            }

            var node = coct.Nodes[index];
            writer.WriteLine($"{new string(' ', indent)}{ObjDumpUtil.FormatObj(node, it => it.BoundingBox)}");

            foreach (var mesh in node.Meshes)
            {
                DumpMeshInfo(mesh, indent + 1);
            }

            DumpNode(node.Child1, indent + 1);
            DumpNode(node.Child2, indent + 1);
            DumpNode(node.Child3, indent + 1);
            DumpNode(node.Child4, indent + 1);
            DumpNode(node.Child5, indent + 1);
            DumpNode(node.Child6, indent + 1);
            DumpNode(node.Child7, indent + 1);
            DumpNode(node.Child8, indent + 1);
        }

        private void DumpMeshInfo(Coct.CollisionMesh mesh, int indent)
        {
            writer.WriteLine($"{new string(' ', indent)}{ObjDumpUtil.FormatObj(mesh, it => it.BoundingBox, it => it.Visibility, it => it.Group)}");

            foreach (var face in mesh.Collisions)
            {
                DumpFace(face, indent + 1);
            }
        }

        private void DumpFace(Coct.Collision face, int indent)
        {
            var dump = ObjDumpUtil.FormatObj(face
                , it => it.BoundingBox
                , it => it.Plane
                , it => it.Ground
                , it => it.FloorLevel
            )
                .Add(
                    "Attributes",
                    face.Attributes.Flags.ToString("X8")
                )
                .Add(
                    "Vertices",
                    string.Join(
                        ", ",
                        new int[] {
                            face.Vertex1,
                            face.Vertex2,
                            face.Vertex3,
                            face.Vertex4,
                        }
                            .Where(it => it != -1)
                            .Select(it => coct.VertexList[it])
                    )
                );
            writer.WriteLine($"{new string(' ', indent)}{dump}");
        }
    }
}

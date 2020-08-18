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

            if (coct.CollisionMeshGroupList.Any())
            {
                DumpEntry1(0, 0);
            }
        }

        private void DumpEntry1(int index, int indent)
        {
            if (index == -1)
            {
                return;
            }

            var entry = coct.CollisionMeshGroupList[index];
            writer.WriteLine($"{new string(' ', indent)}{ObjDumpUtil.FormatObj(entry, it => it.BoundingBox)}");

            foreach (var mesh in entry.Meshes)
            {
                DumpEntry2(mesh, indent + 1);
            }

            DumpEntry1(entry.Child1, indent + 1);
            DumpEntry1(entry.Child2, indent + 1);
            DumpEntry1(entry.Child3, indent + 1);
            DumpEntry1(entry.Child4, indent + 1);
            DumpEntry1(entry.Child5, indent + 1);
            DumpEntry1(entry.Child6, indent + 1);
            DumpEntry1(entry.Child7, indent + 1);
            DumpEntry1(entry.Child8, indent + 1);
        }

        private void DumpEntry2(Coct.CollisionMesh mesh, int indent)
        {
            writer.WriteLine($"{new string(' ', indent)}{ObjDumpUtil.FormatObj(mesh, it => it.BoundingBox, it => it.v10, it => it.v12)}");

            foreach (var face in mesh.Collisions)
            {
                DumpEntry3(face, indent + 1);
            }
        }

        private void DumpEntry3(Coct.Collision face, int indent)
        {
            var dump = ObjDumpUtil.FormatObj(face
                , it => it.BoundingBox
                , it => it.Plane
                , it => it.v00
            )
                .Add(
                    "SurfaceFlags", 
                    face.SurfaceFlags.Flags.ToString("X8")
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

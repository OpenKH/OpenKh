using Assimp;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.DoctChanger.Utils
{
    public class ExposeMapDoctUtil
    {
        internal void Export(Doct doct, string modelOut)
        {
            var scene = GetBaseScene();

            var depthMatIdx = new List<int>();

            for (int x = 0; x < 10; x++)
            {
                var mat = new Material();
                mat.Name = $"Depth{x}";
                mat.ColorDiffuse = new Color4D(0.1f * x, 0, 0);
                depthMatIdx.Add(scene.Materials.Count);
                scene.Materials.Add(mat);
            }

            new Walker(
                doct: doct,
                depthToMatIdx: idx => depthMatIdx[idx],
                addMeshToScene: mesh =>
                {
                    var meshIdx = scene.Meshes.Count;
                    scene.Meshes.Add(mesh);
                    return meshIdx;
                }
            )
                .Walk(
                    entry1Idx: 0,
                    node: scene.RootNode,
                    ancestors: new int[0]
                );

            Assimp.AssimpContext context = new Assimp.AssimpContext();
            context.ExportFile(scene, modelOut, "fbx");
        }

        private static Scene GetBaseScene()
        {
            Assimp.Scene scene = new Assimp.Scene();
            scene.RootNode = new Assimp.Node("RootNode");
            return scene;
        }

        private class Walker
        {
            private Doct doct;
            private Func<int, int> depthToMatIdx;
            private Func<Mesh, int> addMeshToScene;

            internal Walker(Doct doct, Func<int, int> depthToMatIdx, Func<Mesh, int> addMeshToScene)
            {
                this.doct = doct;
                this.depthToMatIdx = depthToMatIdx;
                this.addMeshToScene = addMeshToScene;
            }

            internal void Walk(
                int entry1Idx,
                Node node,
                IEnumerable<int> ancestors
            )
            {
                var entry1 = doct.Entry1List[entry1Idx];
                var bbox = entry1.BoundingBox;

                var depth = ancestors.Count();

                var mesh = new Mesh(PrimitiveType.Polygon);
                mesh.Name = $"Mesh{entry1Idx}_D{depth}_{string.Join("_", ancestors)}";

                mesh.Vertices.Add(new Vector3D(bbox.MinX, bbox.MinY, bbox.MinZ));
                mesh.Vertices.Add(new Vector3D(bbox.MaxX, bbox.MinY, bbox.MinZ));
                mesh.Vertices.Add(new Vector3D(bbox.MinX, bbox.MaxY, bbox.MinZ));
                mesh.Vertices.Add(new Vector3D(bbox.MaxX, bbox.MaxY, bbox.MinZ));
                mesh.Vertices.Add(new Vector3D(bbox.MinX, bbox.MinY, bbox.MaxZ));
                mesh.Vertices.Add(new Vector3D(bbox.MaxX, bbox.MinY, bbox.MaxZ));
                mesh.Vertices.Add(new Vector3D(bbox.MinX, bbox.MaxY, bbox.MaxZ));
                mesh.Vertices.Add(new Vector3D(bbox.MaxX, bbox.MaxY, bbox.MaxZ));

                mesh.MaterialIndex = depthToMatIdx(depth);

                void AddFace(params int[] indices) => mesh.Faces.Add(new Face(indices));

                // left handed
                AddFace(0, 1, 3, 2); // bottom
                AddFace(4, 6, 7, 5); // top
                AddFace(0, 4, 5, 1); // N
                AddFace(3, 7, 5, 1); // E
                AddFace(2, 6, 7, 3); // S
                AddFace(0, 4, 6, 2); // W

                var upToThis = ancestors.Append(entry1Idx);

                foreach (var child in new int[] { entry1.Child1, entry1.Child2, entry1.Child3, entry1.Child4, entry1.Child5, entry1.Child6, entry1.Child7, entry1.Child8 }
                    .Where(child => child != -1)
                )
                {
                    Walk(
                        entry1Idx: child,
                        node: node,
                        ancestors: upToThis
                    );
                }

                node.MeshIndices.Add(addMeshToScene(mesh));
            }
        }
    }
}

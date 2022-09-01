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
        internal void Export(Doct doct, Func<int, Mesh> groupIdxToMesh, string modelOut)
        {
            var scene = GetBaseScene();

            int bgMatIdx;
            {
                var mat = new Material();
                mat.Name = $"BackgroundModel";
                mat.ColorDiffuse = new Color4D(0, 1f, 0);
                bgMatIdx = (scene.Materials.Count);
                scene.Materials.Add(mat);
            }

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
                },
                groupIdxToMesh: groupIdxToMesh,
                bgMesh =>
                {
                    bgMesh.MaterialIndex = bgMatIdx;
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
            private readonly Doct _doct;
            private readonly Func<int, int> _depthToMatIdx;
            private readonly Func<Mesh, int> _addMeshToScene;
            private readonly Func<int, Mesh> _groupIdxToMesh;
            private readonly Action<Mesh> _tweakBgMesh;

            internal Walker(Doct doct, Func<int, int> depthToMatIdx, Func<Mesh, int> addMeshToScene, Func<int, Mesh> groupIdxToMesh, Action<Mesh> tweakBgMesh)
            {
                _doct = doct;
                _depthToMatIdx = depthToMatIdx;
                _addMeshToScene = addMeshToScene;
                _groupIdxToMesh = groupIdxToMesh;
                _tweakBgMesh = tweakBgMesh;
            }

            internal void Walk(
                int entry1Idx,
                Node node,
                IEnumerable<int> ancestors
            )
            {
                var entry1 = _doct.Entry1List[entry1Idx];
                var bbox = entry1.BoundingBox;

                var depth = ancestors.Count();

                var mesh = new Mesh(PrimitiveType.Polygon);
                mesh.Name = $"Mesh{entry1Idx}_D{depth}_{string.Join("_", ancestors)}";

                void AddVert(float x, float y, float z) => mesh.Vertices.Add(new Vector3D(x, -y, -z));

                AddVert(bbox.MinX, bbox.MinY, bbox.MinZ);
                AddVert(bbox.MaxX, bbox.MinY, bbox.MinZ);
                AddVert(bbox.MinX, bbox.MaxY, bbox.MinZ);
                AddVert(bbox.MaxX, bbox.MaxY, bbox.MinZ);
                AddVert(bbox.MinX, bbox.MinY, bbox.MaxZ);
                AddVert(bbox.MaxX, bbox.MinY, bbox.MaxZ);
                AddVert(bbox.MinX, bbox.MaxY, bbox.MaxZ);
                AddVert(bbox.MaxX, bbox.MaxY, bbox.MaxZ);

                mesh.MaterialIndex = _depthToMatIdx(depth);

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

                node.MeshIndices.Add(_addMeshToScene(mesh));

                for (int x = entry1.Entry2Index; x < entry1.Entry2LastIndex; x++)
                {
                    var backgroundMesh = _groupIdxToMesh(x);
                    backgroundMesh.Name = $"Mesh{entry1Idx}_Group{x}_{backgroundMesh.FaceCount}Tris";

                    _tweakBgMesh(backgroundMesh);

                    node.MeshIndices.Add(_addMeshToScene(backgroundMesh));
                }
            }
        }
    }
}

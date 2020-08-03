using NLog;
using OpenKh.Command.MapGen.Models;
using OpenKh.Command.MapGen.Utils;
using OpenKh.Common;
using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Text;
using Xe.Graphics;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace OpenKh.Command.MapGen.Utils
{
    public class MapBuilder
    {
        private const int MaxVertCount = 71;

        private Mdlx.M4 mapModel;
        private BigMeshContainer bigMeshContainer;
        private ModelTexture modelTex;
        private CollisionBuilder collisionBuilder;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private readonly MapGenConfig config;

        public MapBuilder(string modelFile, MapGenConfig config, Func<MaterialDef, Imgd> imageLoader)
        {
            this.config = config;

            ConvertModelIntoMapModel(modelFile, config);

            logger.Debug($"Starting collision plane builder.");

            collisionBuilder = new CollisionBuilder(bigMeshContainer.MeshList);

            logger.Debug($"Output: {collisionBuilder.coct.CollisionList.Count:#,##0} collision planes");

            {
                var matDefList = bigMeshContainer.AllocatedMaterialDefs;
                var imageSets = matDefList
                    .Select(matDef => new ImageSet { image = imageLoader(matDef), matDef = matDef, })
                    .ToArray();

                var build = new ModelTexture.Build
                {
                    images = imageSets
                        .Select(set => set.image)
                        .ToArray(),

                    offsetData = null, // auto serial number map

                    textureTransfer = null, // auto create

                    gsInfo = imageSets
                        .Select(
                            set =>
                            {
                                var gsInfo = new ModelTexture.UserGsInfo(set.image);
                                gsInfo.AddressMode.AddressU =
                                    Enum.Parse<ModelTexture.TextureWrapMode>(
                                        set.matDef.textureOptions.addressU
                                        ?? config.textureOptions.addressU
                                        ?? "RegionRepeat"
                                    );
                                gsInfo.AddressMode.AddressV =
                                    Enum.Parse<ModelTexture.TextureWrapMode>(
                                        set.matDef.textureOptions.addressV
                                        ?? config.textureOptions.addressV
                                        ?? "RegionRepeat"
                                    );
                                return gsInfo;
                            }
                        )
                        .ToArray(),
                };

                modelTex = new ModelTexture(build);
            }
        }

        class ImageSet
        {
            public Imgd image;
            public MaterialDef matDef;
        }

        private void ConvertModelIntoMapModel(string modelFile, MapGenConfig config)
        {
            logger.Debug($"Loading 3D model file \"{modelFile}\" using Assimp.");

            var assimp = new Assimp.AssimpContext();
            var scene = assimp.ImportFile(modelFile, Assimp.PostProcessSteps.PreTransformVertices);

            bigMeshContainer = new BigMeshContainer();

            var scale = config.scale;

            Matrix4x4 matrix = Matrix4x4.Identity;

            if (config.applyMatrix != null)
            {
                var m = config.applyMatrix;

                if (m.Length == 16)
                {
                    matrix = new Matrix4x4(
                        m[0], m[1], m[2], m[3],
                        m[4], m[5], m[6], m[7],
                        m[8], m[9], m[10], m[11],
                        m[12], m[13], m[14], m[15]
                    );

                    logger.Debug($"Apply matrix: {matrix}");
                }
            }
            else
            {
                matrix *= scale;
            }

            logger.Debug($"Starting triangle strip conversion for {scene.Meshes.Count} meshes.");

            foreach (var inputMesh in scene.Meshes)
            {
                logger.Debug($"Mesh: {inputMesh.Name} ({inputMesh.FaceCount:#,##0} faces, {inputMesh.VertexCount:#,##0} vertices)");

                var modelMat = scene.Materials[inputMesh.MaterialIndex];

                var matDef = config.FindMaterial(modelMat.Name ?? "default") ?? MaterialDef.CreateFallbackFor(modelMat.Name);
                if (matDef.ignore)
                {
                    logger.Info($"This mesh \"{inputMesh.Name}\" is not rendered due to ignore flag of material \"{modelMat.Name}\".");
                    continue;
                }

                var kh2Mesh = bigMeshContainer.AllocateBigMeshForMaterial(matDef);

                var diffuseTextureFile = modelMat.TextureDiffuse.FilePath;
                if (!string.IsNullOrEmpty(diffuseTextureFile))
                {
                    logger.Debug($"The mesh \"{inputMesh.Name}\" material \"{matDef.name}\" has filepath \"{diffuseTextureFile}\" for diffuse texture. It will be associated with material's fromFile2.");
                    matDef.fromFile2 = diffuseTextureFile;
                }

                var kh2BaseVert = kh2Mesh.vertexList.Count;

                List<int> vertexToLocal = new List<int>();

                foreach (var inputVertex in inputMesh.Vertices)
                {
                    var vertex = Vector3.Transform(
                        new Vector3(inputVertex.X, inputVertex.Y, inputVertex.Z),
                        matrix
                    );

                    var index = kh2Mesh.vertexList.IndexOf(vertex);
                    if (index < 0)
                    {
                        index = kh2Mesh.vertexList.Count;
                        kh2Mesh.vertexList.Add(vertex);
                    }

                    vertexToLocal.Add(index);
                }

                var localFaces = inputMesh.Faces
                    .Select(
                        set => set.Indices
                            .Select(index => new VertPair { uvColorIndex = index, vertexIndex = vertexToLocal[index] })
                            .ToArray()
                    )
                    .ToArray();

                var inputTexCoords = inputMesh.TextureCoordinateChannels.First();
                var inputVertexColorList = inputMesh.VertexColorChannels.First();

                var hasVertexColor = inputMesh.VertexColorChannelCount >= 1;

                var maxIntensity = matDef.maxIntensity;

                foreach (var triStripInput in TriangleFansToTriangleStrips(localFaces))
                {
                    var triStripOut = new BigMesh.TriangleStrip();

                    foreach (var vertPair in triStripInput)
                    {
                        triStripOut.vertexIndices.Add(kh2BaseVert + vertPair.vertexIndex);

                        triStripOut.uvList.Add(Get2DCoord(inputTexCoords[vertPair.uvColorIndex]));

                        if (hasVertexColor)
                        {
                            triStripOut.vertexColorList.Add(ConvertVertexColor(inputVertexColorList[vertPair.uvColorIndex], maxIntensity));
                        }
                        else
                        {
                            triStripOut.vertexColorList.Add(new Color(maxIntensity, maxIntensity, maxIntensity, 255));
                        }
                    }

                    kh2Mesh.triangleStripList.Add(triStripOut);
                }

                logger.Debug($"Output: {kh2Mesh.vertexList.Count:#,##0} vertices, {kh2Mesh.triangleStripList.Count:#,##0} triangle strips.");
            }

            logger.Debug($"The conversion has done.");

            logger.Debug($"Starting mesh splitter and vif packets builder.");

            mapModel = new Mdlx.M4
            {
                VifPackets = new List<Mdlx.VifPacketDescriptor>(),
            };

            foreach (var bigMesh in bigMeshContainer.MeshList
                .Where(it => it.textureIndex != -1)
            )
            {
                foreach (var smallMesh in BigMeshSplitter.Split(bigMesh))
                {
                    var dmaPack = new MapVifPacketBuilder(smallMesh);

                    mapModel.VifPackets.Add(
                        new Mdlx.VifPacketDescriptor
                        {
                            VifPacket = dmaPack.vifPacket.ToArray(),
                            TextureId = smallMesh.textureIndex,
                            DmaPerVif = new ushort[] {
                                dmaPack.firstVifPacketQwc,
                                0,
                            }
                        }
                    );
                }
            }

            logger.Debug($"Output: {mapModel.VifPackets.Count:#,##0} vif packets.");

            logger.Debug($"The builder has done.");

            logger.Debug($"Starting vifPacketRenderingGroup builder.");

            // first group: render all

            mapModel.vifPacketRenderingGroup = new List<ushort[]>(
                new ushort[][] {
                        Enumerable.Range(0, mapModel.VifPackets.Count)
                            .Select(it => Convert.ToUInt16(it))
                            .ToArray()
                }
            );

            logger.Debug($"Output: {mapModel.vifPacketRenderingGroup.Count:#,##0} groups.");

            mapModel.DmaChainIndexRemapTable = new List<ushort>(
                Enumerable.Range(0, mapModel.VifPackets.Count)
                    .Select(it => Convert.ToUInt16(it))
                    .ToArray()
            );
        }

        class VertPair
        {
            /// <summary>
            /// compacted vertex index
            /// </summary>
            public int vertexIndex;

            /// <summary>
            /// uv and vertex color index
            /// </summary>
            public int uvColorIndex;

            public override string ToString() => $"{vertexIndex}";
        }

        private IEnumerable<IEnumerable<VertPair>> TriangleFansToTriangleStrips(VertPair[][] faces)
        {
            var list = faces
                .SelectMany(face => TriangleFanToTriangleStrips(face))
                .Select(it => it.ToList())
                .ToList();

            while (true)
            {
                var anyOutnerJoin = false;

                for (int x = 0; x < list.Count; x++)
                {
                    while (true)
                    {
                        var anyInnerJoin = false;

                        var v0 = list[x][0].vertexIndex;
                        var v1 = list[x][1].vertexIndex;
                        var v2 = list[x][list[x].Count - 2].vertexIndex;
                        var v3 = list[x][list[x].Count - 1].vertexIndex;

                        for (int y = x + 1; y < list.Count; y++)
                        {
                            if (list[y][0].vertexIndex == v2 && list[y][1].vertexIndex == v3 && list[x].Count + list[y].Count - 2 < MaxVertCount)
                            {
                                list[x].AddRange(list[y].Skip(2));
                                list.RemoveAt(y);
                                y--;
                                anyInnerJoin = true;
                                break;
                            }
                            if (list[y][list[y].Count - 2].vertexIndex == v0 && list[y][list[y].Count - 1].vertexIndex == v1 && list[x].Count + list[y].Count - 2 < MaxVertCount)
                            {
                                list[x].InsertRange(0, list[y].SkipLast(2));
                                list.RemoveAt(y);
                                y--;
                                anyInnerJoin = true;
                                break;
                            }
                        }

                        anyOutnerJoin |= anyInnerJoin;

                        if (!anyInnerJoin)
                        {
                            break;
                        }
                    }
                }

                if (!anyOutnerJoin)
                {
                    break;
                }
            }
            return list;
        }

        private static IEnumerable<IEnumerable<VertPair>> TriangleFanToTriangleStrips(IList<VertPair> list)
        {
            switch (list.Count)
            {
                case 3:
                    yield return list;
                    break;
                case 4:
                    yield return new VertPair[] { list[0], list[1], list[3], list[2] };
                    break;
                default:
                    throw new NotSupportedException();
            }
        }

        private static Color ConvertVertexColor(Assimp.Color4D clr, byte max) => new Color(
            (byte)(clr.R * max),
            (byte)(clr.G * max),
            (byte)(clr.B * max),
            (byte)(clr.A * max)
        );

        private static Vector2 Get2DCoord(Assimp.Vector3D vector3D) => new Vector2(vector3D.X, 1 - vector3D.Y);


        public List<Bar.Entry> GetBarEntries()
        {
            var entries = new List<Bar.Entry>();

            {
                var mapBin = new MemoryStream();
                Mdlx.CreateFromMapModel(mapModel).Write(mapBin);
                mapBin.Position = 0;

                entries.Add(
                    new Bar.Entry
                    {
                        Name = config.bar?.model?.name ?? "MAP",
                        Type = Bar.EntryType.Model,
                        Stream = mapBin,
                    }
                );
            }

            {
                var texBin = new MemoryStream();
                modelTex.Write(texBin);
                texBin.Position = 0;

                entries.Add(
                    new Bar.Entry
                    {
                        Name = config.bar?.texture?.name ?? "MAP",
                        Type = Bar.EntryType.ModelTexture,
                        Stream = texBin,
                    }
                );
            }

            {
                var doctBin = new MemoryStream();
                collisionBuilder.doct.Write(doctBin);
                doctBin.Position = 0;

                entries.Add(
                    new Bar.Entry
                    {
                        Name = config.bar?.doct?.name ?? "eh_1",
                        Type = Bar.EntryType.MeshOcclusion,
                        Stream = doctBin,
                    }
                );
            }

            {
                var coctBin = new MemoryStream();
                collisionBuilder.coct.Write(coctBin);
                coctBin.Position = 0;

                entries.Add(
                    new Bar.Entry
                    {
                        Name = config.bar?.coct?.name ?? "ID_e",
                        Type = Bar.EntryType.MapCollision,
                        Stream = coctBin,
                    }
                );
            }

            return entries;
        }
    }
}

using Assimp;
using NLog;
using OpenKh.Command.MapGen.Models;
using OpenKh.Kh2;
using OpenKh.Kh2.TextureFooter;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Reflection;
using Xe.Graphics;
using Matrix4x4 = System.Numerics.Matrix4x4;

namespace OpenKh.Command.MapGen.Utils
{
    public class MapBuilder
    {
        private const int MaxVertCount = 71;

        private ModelBackground mapModel;
        private BigMeshContainer bigMeshContainer;
        private List<BigMesh> smallMeshList = new List<BigMesh>();
        private ModelTexture modelTex;
        private CollisionBuilder collisionBuilder;
        private DoctBuilder doctBuilder;
        private Logger logger = LogManager.GetCurrentClassLogger();
        private readonly MapGenConfig config;

        public MapBuilder(string modelFile, MapGenConfig config, Func<MaterialDef, Imgd> imageLoader)
        {
            this.config = config;

            var singleFaces = ConvertModelIntoFaces(modelFile, config);

            logger.Debug($"Loading process has done.");

            logger.Debug($"Starting doct (including model composer) and coct.");

            var materialContainer = new MaterialContainer();

            if (!config.nodoct)
            {
                logger.Debug($"Running doct builder.");

                doctBuilder = new DoctBuilder(
                    new BSPNodeSplitter(
                        singleFaces
                            .Where(it => !it.matDef.nodraw),
                        new BSPNodeSplitter.Option()
                    )
                );

                logger.Debug($"Finished.");

                if (doctBuilder.vifPacketRenderingGroup != null)
                {
                    logger.Debug($"Using vifPacketRenderingGroup built by doct.");

                    mapModel = new ModelBackground
                    {
                        Chunks = new List<ModelBackground.ModelChunk>(),
                    };

                    var vifPacketRenderingGroup = new List<ushort[]>();

                    foreach (var faceArray in doctBuilder.vifPacketRenderingGroup)
                    {
                        var groups = faceArray.GroupBy(it => it.matDef);

                        var vifPacketIdx = new List<ushort>();

                        foreach (var facesByMaterial in groups)
                        {
                            var pairMade = new VertMaker(facesByMaterial);

                            var newPairs = TriangleToTriangleStripsOptimized(pairMade.VertPairsList.ToArray());

                            var dmaPack = new MapVifPacketBuilder(
                                pairMade.CoordList,
                                newPairs
                                    .SelectMany(
                                        (vertPair_, faceIndex) =>
                                        {
                                            var vertPair = vertPair_.ToArray();

                                            return Enumerable.Range(0, vertPair.Count())
                                                .Select(
                                                    vertIdx =>
                                                    {
                                                        return new MapVifPacketBuilder.Index
                                                        {
                                                            Flag = MapVifPacketBuilder.Index.GetSuitableFlag(vertIdx),
                                                            CoordIndex = Convert.ToByte(vertPair[vertIdx].coordIndex),
                                                            UV = pairMade.VertList[vertIdx].uv,
                                                            Color = pairMade.VertList[vertIdx].color,
                                                        };
                                                    }
                                                )
                                                .ToArray();
                                        }
                                    )
                                    .ToArray()
                            );

                            var matDef = facesByMaterial.Key;

                            vifPacketIdx.Add(Convert.ToUInt16(mapModel.Chunks.Count));

                            mapModel.Chunks.Add(
                                new ModelBackground.ModelChunk
                                {
                                    VifPacket = dmaPack.vifPacket.ToArray(),
                                    TextureId = materialContainer.Add(matDef),
                                    DmaPerVif = new ushort[] {
                                        dmaPack.firstVifPacketQwc,
                                        0,
                                    },
                                    TransparencyFlag = matDef.transparentFlag ?? 0,
                                    UVScrollIndex = matDef.uvscIndex ?? 0,
                                }
                            );
                        }

                        vifPacketRenderingGroup.Add(vifPacketIdx.ToArray());
                    }

                    mapModel.vifPacketRenderingGroup = vifPacketRenderingGroup;

                    mapModel.DmaChainIndexRemapTable = new List<ushort>(
                        Enumerable.Range(0, mapModel.Chunks.Count)
                            .Select(it => Convert.ToUInt16(it))
                            .ToArray()
                    );

                    //foreach (var bigMesh in bigMeshContainer.MeshList)
                    //{
                    //    foreach (var smallMesh in BigMeshSplitter.Split(bigMesh))
                    //    {
                    //        var dmaPack = new MapVifPacketBuilder(smallMesh);

                    //        smallMeshList.Add(smallMesh);

                    //        if (smallMesh.textureIndex != -1)
                    //        {
                    //            bigMesh.vifPacketIndices.Add(Convert.ToUInt16(mapModel.Chunks.Count));
                    //            smallMesh.vifPacketIndices.Add(Convert.ToUInt16(mapModel.Chunks.Count));

                    //            mapModel.Chunks.Add(
                    //                new ModelBackground.ModelChunk
                    //                {
                    //                    VifPacket = dmaPack.vifPacket.ToArray(),
                    //                    TextureId = smallMesh.textureIndex,
                    //                    DmaPerVif = new ushort[] {
                    //                dmaPack.firstVifPacketQwc,
                    //                0,
                    //                    },
                    //                    TransparencyFlag = smallMesh.matDef.transparentFlag ?? 0,
                    //                    UVScrollIndex = smallMesh.matDef.uvscIndex ?? 0,
                    //                }
                    //            );
                    //        }
                    //    }
                    //}

                    logger.Debug($"Output: {mapModel.vifPacketRenderingGroup.Count:#,##0} groups having total {mapModel.vifPacketRenderingGroup.Select(it => it.Length).Sum():#,##0} chunks.");
                }
            }

            if (!config.nococt)
            {
                logger.Debug($"Running collision plane builder.");

                collisionBuilder = new CollisionBuilder(
                    new BSPNodeSplitter(
                        singleFaces
                            .Where(it => !it.matDef.noclip),
                        new BSPNodeSplitter.Option()
                    )
                );

                {
                    var coct = collisionBuilder.coct;
                    var numNodes = coct.Nodes.Count;
                    var numTotalMeshes = coct.Nodes.Select(node => node.Meshes.Count).Sum();
                    var numTotalCollisions = coct.Nodes.Select(node => node.Meshes.Select(it => it.Collisions.Count).Sum()).Sum();
                    var numVerts = coct.VertexList.Count;

                    logger.Debug($"{numNodes:#,##0} nodes, {numTotalMeshes:#,##0} total meshes, {numTotalCollisions:#,##0} total collisions, {numVerts:#,##0} vertices.");
                }

                logger.Debug($"Finished.");
            }

            logger.Debug($"Running texture generator.");

            {
                //var matDefList = bigMeshContainer.AllocatedMaterialDefs;
                var imageSets = materialContainer.Materials
                    .Select(matDef => new ImageSet { image = imageLoader(matDef), matDef = matDef, })
                    .ToArray();

                var footerData = new MemoryStream();
                {
                    var footer = new TextureFooterData();
                    foreach (var uvscItem in config.uvscList)
                    {
                        footer.UvscList.Add(
                            new UvScroll
                            {
                                TextureIndex = uvscItem.index,
                                UScrollSpeed = uvscItem.u,
                                VScrollSpeed = uvscItem.v,
                            }
                        );
                    }
                    footer.Write(footerData);
                }

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
                                        ?? "Repeat"
                                    );
                                gsInfo.AddressMode.AddressV =
                                    Enum.Parse<ModelTexture.TextureWrapMode>(
                                        set.matDef.textureOptions.addressV
                                        ?? config.textureOptions.addressV
                                        ?? "Repeat"
                                    );
                                return gsInfo;
                            }
                        )
                        .ToArray(),

                    footerData = footerData.ToArray(),
                };

                modelTex = new ModelTexture(build);
            }

            logger.Debug($"The builder has done.");
        }

        private class Vert
        {
            internal Vector2 uv;
            internal Color color;
        }

        private class VertMaker
        {
            public List<VertPair[]> VertPairsList { get; } = new List<VertPair[]>();
            public List<Vector3> CoordList { get; } = new List<Vector3>();
            public List<Vert> VertList { get; } = new List<Vert>();

            public VertMaker(IEnumerable<SingleFace> faces)
            {
                var idx = 0;

                foreach (var face in faces)
                {
                    var vertPairs = new List<VertPair>();
                    for (int x = 0, cx = face.positionList.Count(); x < cx; x++)
                    {
                        var position = face.positionList[x];

                        var coordIdx = CoordList.IndexOf(position);
                        if (coordIdx < 0)
                        {
                            coordIdx = CoordList.Count;
                            CoordList.Add(position);
                        }

                        VertList.Add(
                            new Vert
                            {
                                uv = face.uvList[x],
                                color = face.colorList[x],
                            }
                        );

                        vertPairs.Add(
                            new VertPair
                            {
                                index = idx,
                                coordIndex = coordIdx,
                            }
                        );

                        idx++;
                    }

                    VertPairsList.Add(vertPairs.ToArray());
                }
            }
        }

        private class ImageSet
        {
            public Imgd image;
            public MaterialDef matDef;
        }

        private List<SingleFace> ConvertModelIntoFaces(string modelFile, MapGenConfig config)
        {
            logger.Debug($"Loading 3D model file \"{modelFile}\" using Assimp.");

            var assimp = new Assimp.AssimpContext();
            var scene = assimp.ImportFile(modelFile, Assimp.PostProcessSteps.PreTransformVertices | Assimp.PostProcessSteps.Triangulate);

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

            var faces = new List<SingleFace>();

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

                var diffuseTextureFile = modelMat.TextureDiffuse.FilePath;
                if (!string.IsNullOrEmpty(diffuseTextureFile))
                {
                    if (config.reuseImd)
                    {
                        logger.Debug($"The mesh \"{inputMesh.Name}\" material \"{matDef.name}\" has filepath \"{diffuseTextureFile}\" for diffuse texture. It will be associated with material's fromFile3. Setting preferable imd file to fromFile2 due to reuseImd flag.");

                        matDef.fromFile2 = Path.ChangeExtension(diffuseTextureFile, ".imd");
                        matDef.fromFile3 = diffuseTextureFile;
                    }
                    else
                    {
                        logger.Debug($"The mesh \"{inputMesh.Name}\" material \"{matDef.name}\" has filepath \"{diffuseTextureFile}\" for diffuse texture. It will be associated with material's fromFile2.");

                        matDef.fromFile3 = diffuseTextureFile;
                    }
                }

                Vector3 Transform(Vector3D inputVertex)
                {
                    return Vector3.Transform(
                        new Vector3(inputVertex.X, inputVertex.Y, inputVertex.Z),
                        matrix
                    );
                }

                var maxColorIntensity = matDef.maxColorIntensity
                    ?? config.maxColorIntensity
                    ?? 128;
                var maxAlpha = matDef.maxAlpha
                    ?? config.maxAlpha
                    ?? 128;

                Color ConvertVertexColor(Assimp.Color4D clr) => new Color(
                    (byte)Math.Min(255, clr.R * maxColorIntensity),
                    (byte)Math.Min(255, clr.G * maxColorIntensity),
                    (byte)Math.Min(255, clr.B * maxColorIntensity),
                    (byte)Math.Min(255, clr.A * maxAlpha)
                );

                Vector2 Get2DCoord(Assimp.Vector3D vector3D) => new Vector2(
                    vector3D.X,
                    1 - vector3D.Y
                );

                Vector3 GetCenterOf(Vector3[] items)
                {
                    if (items.Length == 0)
                    {
                        return Vector3.Zero;
                    }
                    else if (items.Length == 1)
                    {
                        return items[0];
                    }
                    else
                    {
                        var center = Vector3.Zero;
                        var cx = items.Length;
                        for (int x = 0; x < cx; x++)
                        {
                            center += items[x] / cx;
                        }
                        return center;
                    }
                }

                var inputTexCoords = inputMesh.TextureCoordinateChannels.First();
                var inputVertexColorList = inputMesh.VertexColorChannels.First();
                var hasVertexColor = inputMesh.VertexColorChannelCount >= 1;

                foreach (var face in inputMesh.Faces)
                {
                    var positionList = face.Indices
                        .Select(index => Transform(inputMesh.Vertices[index]))
                        .ToArray();

                    faces.Add(
                        new SingleFace
                        {
                            referencePosition = GetCenterOf(positionList),

                            positionList = face.Indices
                                .Select(index => Transform(inputMesh.Vertices[index]))
                                .ToArray(),
                            uvList = face.Indices
                                .Select(index => Get2DCoord(inputTexCoords[index]))
                                .ToArray(),
                            colorList = hasVertexColor
                                ? face.Indices
                                    .Select(index => ConvertVertexColor(inputVertexColorList[index]))
                                    .ToArray()
                                : Enumerable.Repeat(
                                    new Color(maxColorIntensity, maxColorIntensity, maxColorIntensity, maxAlpha),
                                    face.Indices.Count
                                )
                                    .ToArray(),
                            matDef = matDef,
                        }
                    );
                }
            }

            /*

            mapModel = new ModelBackground
            {
                Chunks = new List<ModelBackground.ModelChunk>(),
            };

            foreach (var bigMesh in bigMeshContainer.MeshList)
            {
                foreach (var smallMesh in BigMeshSplitter.Split(bigMesh))
                {
                    var dmaPack = new MapVifPacketBuilder(smallMesh);

                    smallMeshList.Add(smallMesh);

                    if (smallMesh.textureIndex != -1)
                    {
                        bigMesh.vifPacketIndices.Add(Convert.ToUInt16(mapModel.Chunks.Count));
                        smallMesh.vifPacketIndices.Add(Convert.ToUInt16(mapModel.Chunks.Count));

                        mapModel.Chunks.Add(
                            new ModelBackground.ModelChunk
                            {
                                VifPacket = dmaPack.vifPacket.ToArray(),
                                TextureId = smallMesh.textureIndex,
                                DmaPerVif = new ushort[] {
                                    dmaPack.firstVifPacketQwc,
                                    0,
                                },
                                TransparencyFlag = smallMesh.matDef.transparentFlag ?? 0,
                                UVScrollIndex = smallMesh.matDef.uvscIndex ?? 0,
                            }
                        );
                    }
                }
            }

            logger.Debug($"Output: {mapModel.Chunks.Count:#,##0} vif packets.");

            logger.Debug($"The builder has done.");

            logger.Debug($"Emitting initial inefficient vifPacketRenderingGroup for fallback purpose.");

            // first group: render all

            mapModel.vifPacketRenderingGroup = new List<ushort[]>(
                new ushort[][] {
                    Enumerable.Range(0, mapModel.Chunks.Count)
                        .Select(it => Convert.ToUInt16(it))
                        .ToArray()
                }
            );

            logger.Debug($"Output: {mapModel.vifPacketRenderingGroup.Count:#,##0} groups having total {mapModel.vifPacketRenderingGroup.Select(it => it.Length).Sum():#,##0} chunks.");

            mapModel.DmaChainIndexRemapTable = new List<ushort>(
                Enumerable.Range(0, mapModel.Chunks.Count)
                    .Select(it => Convert.ToUInt16(it))
                    .ToArray()
            );
            */

            return faces;
        }

        class VertPair
        {
            /// <summary>
            /// compacted vertex coord index
            /// </summary>
            public int coordIndex;

            /// <summary>
            /// uv and vertex color index
            /// </summary>
            public int index;

            public override string ToString() => $"{coordIndex}";
        }

        private delegate IEnumerable<IEnumerable<VertPair>> TriangleToTriangleStripsConverter(VertPair[][] faces);

        private IEnumerable<IEnumerable<VertPair>> TriangleToTriangleStripsNoOpts(VertPair[][] faces) => faces;

        private IEnumerable<IEnumerable<VertPair>> TriangleToTriangleStripsOptimized(VertPair[][] faces)
        {
            IEnumerable<IEnumerable<VertPair>> TriangleToTriangleStrips(IEnumerable<VertPair> list)
            {
                switch (list.Count())
                {
                    case 3:
                        yield return list;
                        break;
                    case 4:
                        throw new NotSupportedException();
                }
            }

            var list = faces
                .SelectMany(face => TriangleToTriangleStrips(face))
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

                        var v0 = list[x][0].coordIndex;
                        var v1 = list[x][1].coordIndex;
                        var v2 = list[x][list[x].Count - 2].coordIndex;
                        var v3 = list[x][list[x].Count - 1].coordIndex;

                        for (int y = x + 1; y < list.Count; y++)
                        {
                            if (list[y][0].coordIndex == v2 && list[y][1].coordIndex == v3 && list[x].Count + list[y].Count - 2 < MaxVertCount)
                            {
                                list[x].AddRange(list[y].Skip(2));
                                list.RemoveAt(y);
                                y--;
                                anyInnerJoin = true;
                                break;
                            }
                            if (list[y][list[y].Count - 2].coordIndex == v0 && list[y][list[y].Count - 1].coordIndex == v1 && list[x].Count + list[y].Count - 2 < MaxVertCount)
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

        public List<Bar.Entry> GetBarEntries(Action<string, MemoryStream> trySaveTo = null)
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

                trySaveTo?.Invoke(config.bar?.model?.toFile, mapBin);
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

                trySaveTo?.Invoke(config.bar?.texture?.toFile, texBin);
            }

            if (!config.nodoct)
            {
                var doctBin = new MemoryStream();
                doctBuilder.doct.Write(doctBin);
                doctBin.Position = 0;

                entries.Add(
                    new Bar.Entry
                    {
                        Name = config.bar?.doct?.name ?? "eh_1",
                        Type = Bar.EntryType.DrawOctalTree,
                        Stream = doctBin,
                    }
                );

                trySaveTo?.Invoke(config.bar?.doct?.toFile, doctBin);
            }

            if (!config.nococt)
            {
                var coctBin = new MemoryStream();
                collisionBuilder.coct.Write(coctBin);
                coctBin.Position = 0;

                entries.Add(
                    new Bar.Entry
                    {
                        Name = config.bar?.coct?.name ?? "ID_e",
                        Type = Bar.EntryType.CollisionOctalTree,
                        Stream = coctBin,
                    }
                );

                trySaveTo?.Invoke(config.bar?.coct?.toFile, coctBin);
            }

            return entries;
        }

    }
}

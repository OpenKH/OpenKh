using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Parsers;
using OpenKh.Engine.Renders;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.Models;
using OpenKh.Game.Shaders;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using System.Collections.Generic;
using System.Linq;
using OpenKh.Kh2.Models;

namespace OpenKh.Game.States
{
    public class MapState : IState
    {
        private readonly static BlendState DefaultBlendState = new BlendState()
        {
            ColorSourceBlend = Blend.SourceAlpha,
            AlphaSourceBlend = Blend.SourceAlpha,
            ColorDestinationBlend = Blend.InverseSourceAlpha,
            AlphaDestinationBlend = Blend.InverseSourceAlpha,
            ColorBlendFunction = BlendFunction.Add,
            AlphaBlendFunction = BlendFunction.Add,
            BlendFactor = Color.White,
            MultiSampleMask = int.MaxValue,
            IndependentBlendEnable = false
        };

        private Kernel _kernel;
        private IDataContent _dataContent;
        private ArchiveManager _archiveManager;
        private GraphicsDeviceManager _graphics;
        private InputManager _input;
        private List<MeshGroup> _models = new List<MeshGroup>();
        private List<MeshGroup> _bobModels = new List<MeshGroup>();
        private KingdomShader _shader;
        private Camera _camera;

        private int _worldId = 2;
        private int _placeId = 4;
        private int _objEntryId = 0x236; // PLAYER
        private bool _enableCameraMovement = true;
        private List<BobDescriptor> _bobDescs = new List<BobDescriptor>();

        public void Initialize(StateInitDesc initDesc)
        {
            _kernel = initDesc.Kernel;
            _dataContent = initDesc.DataContent;
            _archiveManager = initDesc.ArchiveManager;
            _graphics = initDesc.GraphicsDevice;
            _input = initDesc.InputManager;
            _shader = new KingdomShader(initDesc.ContentManager);
            _camera = new Camera()
            {
                CameraPosition = new Vector3(0, 100, 200),
                CameraRotationYawPitchRoll = new Vector3(90, 0, 10),
            };

            BasicallyForceToReloadEverything();
        }

        public void Destroy()
        {
        }

        public void Update(DeltaTimes deltaTimes)
        {
            if (_enableCameraMovement)
            {
                const double Speed = 100.0;
                var speed = (float)(deltaTimes.DeltaTime * Speed);

                if (_input.W) _camera.CameraPosition += Vector3.Multiply(_camera.CameraLookAtX, speed * 5);
                if (_input.S) _camera.CameraPosition -= Vector3.Multiply(_camera.CameraLookAtX, speed * 5);
                if (_input.A) _camera.CameraPosition -= Vector3.Multiply(_camera.CameraLookAtY, speed * 5);
                if (_input.D) _camera.CameraPosition += Vector3.Multiply(_camera.CameraLookAtY, speed * 5);

                if (_input.Up) _camera.CameraRotationYawPitchRoll += new Vector3(0, 0, 1 * speed);
                if (_input.Down) _camera.CameraRotationYawPitchRoll -= new Vector3(0, 0, 1 * speed);
                if (_input.Left) _camera.CameraRotationYawPitchRoll += new Vector3(1 * speed, 0, 0);
                if (_input.Right) _camera.CameraRotationYawPitchRoll -= new Vector3(1 * speed, 0, 0);
            }
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            _camera.AspectRatio = _graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight;


            _graphics.GraphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullClockwiseFace
            };
            _graphics.GraphicsDevice.DepthStencilState = new DepthStencilState();
            _graphics.GraphicsDevice.BlendState = DefaultBlendState;

            _shader.Pass(pass =>
            {
                _shader.ProjectionView = _camera.Projection;
                _shader.WorldView = _camera.World;
                _shader.ModelView = Matrix.Identity;
                pass.Apply();

                foreach (var mesh in _models)
                {
                    if (mesh.MeshDescriptors != null)
                    {
                        RenderMeshNew(pass, mesh, true);
                        RenderMeshNew(pass, mesh, false);
                    }
                    else
                    {
                        RenderMesh(pass, mesh);
                    }
                }

                foreach (var bobDesc in _bobDescs)
                {
                    var modelView = Matrix.CreateRotationX(bobDesc.RotationX) *
                        Matrix.CreateRotationY(bobDesc.RotationY) *
                        Matrix.CreateRotationZ(bobDesc.RotationZ) *
                        Matrix.CreateScale(bobDesc.ScalingX, bobDesc.ScalingY, bobDesc.ScalingZ) *
                        Matrix.CreateTranslation(bobDesc.PositionX, bobDesc.PositionY, bobDesc.PositionZ);

                    _shader.ProjectionView = _camera.Projection;
                    _shader.WorldView = _camera.World;
                    _shader.ModelView = modelView;
                    pass.Apply();

                    RenderMesh(pass, _bobModels[bobDesc.BobIndex]);
                }
            });
        }

        private void RenderMesh(EffectPass pass, MeshGroup mesh)
        {
            var index = 0;
            foreach (var part in mesh.Parts)
            {
                if (part.Indices.Length == 0)
                    continue;

                if (part.IsOpaque)
                {
                    var textureIndex = part.TextureId & 0xffff;
                    if (textureIndex < mesh.Textures.Length)
                        SetRenderTexture(pass, mesh.Textures[textureIndex]);

                    _graphics.GraphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        mesh.Segments[index].Vertices,
                        0,
                        mesh.Segments[index].Vertices.Length,
                        part.Indices,
                        0,
                        part.Indices.Length / 3);
                }

                index = (index + 1) % mesh.Segments.Length;
            }

            index = 0;
            foreach (var part in mesh.Parts)
            {
                if (part.Indices.Length == 0)
                    continue;

                if (!part.IsOpaque)
                {
                    var textureIndex = part.TextureId & 0xffff;
                    if (textureIndex < mesh.Textures.Length)
                        SetRenderTexture(pass, mesh.Textures[textureIndex]);
                    
                    _graphics.GraphicsDevice.DrawUserIndexedPrimitives(
                        PrimitiveType.TriangleList,
                        mesh.Segments[index].Vertices,
                        0,
                        mesh.Segments[index].Vertices.Length,
                        part.Indices,
                        0,
                        part.Indices.Length / 3);
                }

                index = (index + 1) % mesh.Segments.Length;
            }
        }

        private void RenderMeshNew(EffectPass pass, MeshGroup mesh, bool passRenderOpaque)
        {
            foreach (var meshDescriptor in mesh.MeshDescriptors)
            {
                if (meshDescriptor.IsOpaque != passRenderOpaque)
                    continue;
                if (meshDescriptor.Indices.Length == 0)
                    continue;

                var textureIndex = meshDescriptor.TextureIndex & 0xffff;
                if (textureIndex < mesh.Textures.Length)
                    SetRenderTexture(pass, mesh.Textures[textureIndex]);

                _graphics.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    meshDescriptor.Vertices,
                    0,
                    meshDescriptor.Vertices.Length,
                    meshDescriptor.Indices,
                    0,
                    meshDescriptor.Indices.Length / 3);
            }
        }

        private void SetRenderTexture(EffectPass pass, KingdomTexture texture)
        {
            if (_shader.Texture0 != texture.Texture2D)
            {
                _shader.Texture0 = texture.Texture2D;
                switch (texture.ModelTexture.TextureAddressMode.AddressU)
                {
                    case ModelTexture.TextureWrapMode.Clamp:
                        _shader.TextureRegionU = KingdomShader.DefaultTextureRegion;
                        _shader.TextureWrapModeU = TextureWrapMode.Clamp;
                        break;
                    case ModelTexture.TextureWrapMode.Repeat:
                        _shader.TextureRegionU = KingdomShader.DefaultTextureRegion;
                        _shader.TextureWrapModeU = TextureWrapMode.Repeat;
                        break;
                    case ModelTexture.TextureWrapMode.RegionClamp:
                        _shader.TextureRegionU = texture.RegionU;
                        _shader.TextureWrapModeU = TextureWrapMode.Clamp;
                        break;
                    case ModelTexture.TextureWrapMode.RegionRepeat:
                        _shader.TextureRegionU = texture.RegionU;
                        _shader.TextureWrapModeU = TextureWrapMode.Repeat;
                        break;
                }
                switch (texture.ModelTexture.TextureAddressMode.AddressV)
                {
                    case ModelTexture.TextureWrapMode.Clamp:
                        _shader.TextureRegionV = KingdomShader.DefaultTextureRegion;
                        _shader.TextureWrapModeV = TextureWrapMode.Clamp;
                        break;
                    case ModelTexture.TextureWrapMode.Repeat:
                        _shader.TextureRegionV = KingdomShader.DefaultTextureRegion;
                        _shader.TextureWrapModeV = TextureWrapMode.Repeat;
                        break;
                    case ModelTexture.TextureWrapMode.RegionClamp:
                        _shader.TextureRegionV = texture.RegionV;
                        _shader.TextureWrapModeV = TextureWrapMode.Clamp;
                        break;
                    case ModelTexture.TextureWrapMode.RegionRepeat:
                        _shader.TextureRegionV = texture.RegionV;
                        _shader.TextureWrapModeV = TextureWrapMode.Repeat;
                        break;
                }

                pass.Apply();
            }
        }

        private void BasicallyForceToReloadEverything()
        {
            _models.Clear();
            _bobModels.Clear();

            LoadMap(_worldId, _placeId);
            LoadObjEntry(_objEntryId);
        }

        private void LoadObjEntry(string name)
        {
            var model = _kernel.ObjEntries.FirstOrDefault(x => x.ModelName == name);
            if (model == null)
                return;

            var fileName = $"obj/{model.ModelName}.mdlx";

            using var stream = _dataContent.FileOpen(fileName);
            var entries = Bar.Read(stream);
            var modelEntryName = entries.FirstOrDefault(x => x.Type == Bar.EntryType.Model)?.Name;

            _archiveManager.LoadArchive(entries);
            AddMesh(FromMdlx(_graphics.GraphicsDevice, _archiveManager, modelEntryName, "tim_"));
        }

        private void LoadObjEntry(int id)
        {
            var model = _kernel.ObjEntries.FirstOrDefault(x => x.ObjectId == id);
            if (model == null)
                return;

            LoadObjEntry(model.ModelName);
        }

        private void LoadMap(int worldIndex, int placeIndex)
        {
            Log.Info($"Map={worldIndex},{placeIndex}");

            string fileName;
            if (_kernel.IsReMix)
                fileName = $"map/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.map";
            else
                fileName = $"map/{_kernel.Language}/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.map";

            var entries = _dataContent.FileOpen(fileName).Using(Bar.Read);
            AddMesh(FromMdlx(_graphics.GraphicsDevice, entries, "SK0"));
            AddMesh(FromMdlx(_graphics.GraphicsDevice, entries, "SK1"));
            AddMesh(FromMdlx(_graphics.GraphicsDevice, entries, "MAP"));

            _bobDescs = entries.ForEntry("out", Bar.EntryType.BobDescriptor, BobDescriptor.Read) ?? new List<BobDescriptor>();
            var bobModels = entries.ForEntries("BOB", Bar.EntryType.Model, Mdlx.Read).ToList();
            var bobTextures = entries.ForEntries("BOB", Bar.EntryType.ModelTexture, ModelTexture.Read).ToList();

            for (var i = 0; i < bobModels.Count; i++)
            {
                var bobMesh = FromMdlx(_graphics.GraphicsDevice, bobModels[i], bobTextures[i]);
                if (bobMesh != null)
                    _bobModels.Add(bobMesh);
            }
        }

        private static MeshGroup FromMdlx(
            GraphicsDevice graphics, ArchiveManager archiveManager, string modelName, string textureName)
        {
            Log.Info($"Load model={modelName} texture={textureName}");
            return FromMdlx(graphics,
                archiveManager.Get<Mdlx>(modelName),
                archiveManager.Get<ModelTexture>(textureName));
        }

        private static MeshGroup FromMdlx(GraphicsDevice graphics, IEnumerable<Bar.Entry> entries, string name)
        {
            return FromMdlx(graphics,
                entries.ForEntry(name, Bar.EntryType.Model, Mdlx.Read),
                entries.ForEntry(name, Bar.EntryType.ModelTexture, ModelTexture.Read));
        }

        private static MeshGroup FromMdlx(GraphicsDevice graphics, Mdlx mdlx, ModelTexture modelTextures)
        {
            if (mdlx == null)
            {
                Log.Warn($"model is null");
                return null;
            }
            if (modelTextures == null)
                Log.Warn($"texture is null");

            var textures = modelTextures?.Images?
                .Select(texture => new KingdomTexture(texture, graphics)).ToArray() ?? new KingdomTexture[0];

            var mdlxParsed = new MdlxParser(mdlx);

            MeshGroup.Segment[] segments = null;
            MeshGroup.Part[] parts = null;
            var model = new MdlxParser(mdlx).Model;
            if (model?.Parts != null && model?.Segments != null)
            { // DEPRECATED
                segments = model.Segments.Select(segment => new MeshGroup.Segment
                {
                    Vertices = segment.Vertices.Select(vertex => new VertexPositionColorTexture
                    {
                        Position = new Vector3(vertex.X, vertex.Y, vertex.Z),
                        TextureCoordinate = new Vector2(vertex.U, vertex.V),
                        Color = new Color((vertex.Color >> 16) & 0xff, (vertex.Color >> 8) & 0xff, vertex.Color & 0xff, (vertex.Color >> 24) & 0xff)
                    }).ToArray()
                }).ToArray();

                parts = model.Parts.Select(part => new MeshGroup.Part
                {
                    Indices = part.Indices,
                    SegmentId = part.SegmentIndex,
                    TextureId = part.TextureIndex,
                    IsOpaque = part.IsOpaque,
                }).ToArray();
            }

            return new MeshGroup
            {
                Segments = segments,
                Parts = parts,
                MeshDescriptors = mdlxParsed.MeshDescriptors?
                    .Select(x => new MeshDesc
                    {
                        Vertices = x.Vertices
                            .Select(v => new VertexPositionColorTexture(
                                new Vector3(v.X, v.Y, v.Z),
                                new Color((v.Color >> 16) & 0xff, (v.Color >> 8) & 0xff, v.Color & 0xff, (v.Color >> 24) & 0xff),
                                new Vector2(v.Tu, v.Tv)))
                            .ToArray(),
                        Indices = x.Indices,
                        TextureIndex = x.TextureIndex,
                        IsOpaque = x.IsOpaque
                    })
                    .ToList(),
                Textures = textures
            };
        }

        private void AddMesh(MeshGroup mesh)
        {
            if (mesh == null)
            {
                Log.Warn("AddMesh received null");
                return;
            }

            _models.Add(mesh);
        }


        #region DEBUG

        private class DebugPlace
        {
            public int Index { get; set; }
            public int WorldId { get; set; }
            public int PlaceId { get; set; }
            public ushort MessageId { get; set; }
        }

        private int _debugType = 0;
        private int _debugPlaceCursor = 0;
        private int _debugObjentryCursor = 0;
        private DebugPlace[] _places;

        public void DebugUpdate(IDebug debug)
        {
            if (_input.IsDebug)
            {
                if (!IsDebugMode())
                {
                    EnableDebugMode();

                    if (_places == null)
                        _places = DebugLoadPlaceList();

                    _debugObjentryCursor = _kernel.ObjEntries
                        .Select((entry, i) => new { entry, i })
                        .FirstOrDefault(x => x.entry.ObjectId == _objEntryId)?.i ?? 0;
                }
                else
                    DisableDebugMode();
            }

            if (IsDebugMode())
            {
                if (_input.IsDebugLeft)
                    _debugType--;
                else if (_input.IsDebugRight)
                    _debugType++;
                _debugType %= 3;

                if (_debugType == 0)
                    DebugUpdatePlaceList();
                else if (_debugType == 1)
                    DebugUpdateObjentryList();
                else if (_debugType == 2)
                    if (_input.IsCross)
                        debug.State = 0;
            }
        }

        public void DebugDraw(IDebug debug)
        {
            if (IsDebugMode())
            {
                if (_debugType == 0)
                    DebugDrawPlaceList(debug);
                else if (_debugType == 1)
                    DebugDrawObjentryList(debug);
                else if (_debugType == 2)
                    debug.Println("Press X to return to title screen");
            }
            else
            {
                debug.Println($"MAP: {Constants.WorldIds[_worldId]}{_placeId:D02}");
                debug.Println($"POS ({_camera.CameraPosition.X:F0}, {_camera.CameraPosition.Y:F0}, {_camera.CameraPosition.Z:F0})");
                debug.Println($"YPR ({_camera.CameraRotationYawPitchRoll.X:F0}, {_camera.CameraRotationYawPitchRoll.Y:F0}, {_camera.CameraRotationYawPitchRoll.Z:F0})");
            }
        }

        private bool IsDebugMode() => _enableCameraMovement == false;
        private void EnableDebugMode() => _enableCameraMovement = false;
        private void DisableDebugMode() => _enableCameraMovement = true;
        private int Increment(int n) => n + (_input.IsShift ? 10 : 1);
        private int Decrement(int n) => n - (_input.IsShift ? 10 : 1);

        private void DebugUpdatePlaceList()
        {
            if (_input.IsDebugUp) _debugPlaceCursor = Decrement(_debugPlaceCursor);
            else if (_input.IsDebugDown) _debugPlaceCursor = Increment(_debugPlaceCursor);
            if (_debugPlaceCursor < 0)
                _debugPlaceCursor = _places.Length - 1;
            _debugPlaceCursor %= _places.Length;

            if (_input.IsCross)
            {
                var map = _places[_debugPlaceCursor];
                _worldId = map.WorldId;
                _placeId = map.PlaceId;

                BasicallyForceToReloadEverything();
                DisableDebugMode();
            }
        }

        private void DebugDrawPlaceList(IDebug debug)
        {
            debug.Println("MAP SELECTION");
            debug.Println("");

            foreach (var place in _places.Skip(_debugPlaceCursor))
            {
                debug.Print($"{(place.Index == _debugPlaceCursor ? '>' : ' ')} ");
                debug.Print($"{Constants.WorldIds[place.WorldId]}{place.PlaceId:D02} ");
                debug.Println(place.MessageId);
            }
        }

        private DebugPlace[] DebugLoadPlaceList() => _kernel.Places
            .Select(x => new
            {
                World = x.Key,
                Places = x.Value.Select((place, i) => new
                {
                    Index = i,
                    Place = place
                })
            })
            .SelectMany(x => x.Places, (x, place) => new DebugPlace
            {
                WorldId = Constants.WorldIds
                    .Select((World, Index) => new { World, Index })
                    .Where(e => e.World == x.World)
                    .Select(x => x.Index).FirstOrDefault(),
                PlaceId = place.Index,
                MessageId = place.Place.MessageId
            })
            .Select((x, i) =>
            {
                x.Index = i;
                return x;
            })
            .ToArray();

        private void DebugUpdateObjentryList()
        {
            if (_input.IsDebugUp) _debugObjentryCursor = Decrement(_debugObjentryCursor);
            else if (_input.IsDebugDown) _debugObjentryCursor = Increment(_debugObjentryCursor);
            if (_debugObjentryCursor < 0)
                _debugObjentryCursor = _kernel.ObjEntries.Count - 1;
            _debugObjentryCursor %= _kernel.ObjEntries.Count;

            if (_input.IsCross)
            {
                _objEntryId = _kernel.ObjEntries.Skip(_debugObjentryCursor).FirstOrDefault()?.ObjectId ?? 0;

                BasicallyForceToReloadEverything();
                DisableDebugMode();
            }
        }

        private void DebugDrawObjentryList(IDebug debug)
        {
            debug.Println("OBJENTRY SELECTION");
            debug.Println("");

            var index = 0;
            foreach (var entry in _kernel.ObjEntries)
            {
                if (index >= _debugObjentryCursor)
                {
                    debug.Print($"{(index == _debugObjentryCursor ? '>' : ' ')} ");
                    debug.Print($"{entry.ObjectId:X04} ");
                    debug.Println(entry.ModelName.Replace('_', '-'));
                }

                index++;
            }
        }

        #endregion
    }
}

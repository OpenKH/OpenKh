using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Parsers;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.Models;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.States
{
    public class MapState : IState
    {
        private Kernel _kernel;
        private ArchiveManager _archiveManager;
        private GraphicsDeviceManager _graphics;
        private InputManager _input;
        private List<Mesh> _models = new List<Mesh>();
        private BasicEffect _effect;
        private Camera _camera;

        private int _worldId = 2;
        private int _placeId = 4;
        private int _objEntryId = 0x236; // PLAYER
        private bool _enableCameraMovement = true;

        public void Initialize(StateInitDesc initDesc)
        {
            _kernel = initDesc.Kernel;
            _archiveManager = initDesc.ArchiveManager;
            _graphics = initDesc.GraphicsDevice;
            _input = initDesc.InputManager;
            _effect = new BasicEffect(_graphics.GraphicsDevice);
            _camera = new Camera()
            {
                CameraPosition = new Vector3(0, 100, 200),
                CameraRotationYawPitchRoll = new Vector3(90, 0, 10),
            };

            _graphics.GraphicsDevice.BlendState = BlendState.NonPremultiplied;

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

                if (_input.W) _camera.CameraPosition += Vector3.Multiply(_camera.CameraLookAt, speed * 5);
                if (_input.S) _camera.CameraPosition -= Vector3.Multiply(_camera.CameraLookAt, speed * 5);

                if (_input.Up) _camera.CameraRotationYawPitchRoll += new Vector3(0, 0, 1 * speed);
                if (_input.Down) _camera.CameraRotationYawPitchRoll -= new Vector3(0, 0, 1 * speed);
                if (_input.Left) _camera.CameraRotationYawPitchRoll += new Vector3(1 * speed, 0, 0);
                if (_input.Right) _camera.CameraRotationYawPitchRoll -= new Vector3(1 * speed, 0, 0);
            }
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            _camera.AspectRatio = _graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight;

            _effect.VertexColorEnabled = true;
            _effect.Projection = _camera.Projection;
            _effect.World = _camera.World;

            _effect.GraphicsDevice.RasterizerState = new RasterizerState()
            {
                CullMode = CullMode.CullClockwiseFace
            };
            _effect.GraphicsDevice.DepthStencilState = new DepthStencilState();


            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (var mesh in _models)
                {
                    var index = 0;
                    foreach (var part in mesh.Parts)
                    {
                        if (part.Indices.Length == 0)
                            continue;

                        var textureIndex = part.TextureId & 0xffff;
                        if (textureIndex < mesh.Textures.Length)
                        {
                            var texture = mesh.Textures[textureIndex];
                            if (_effect.Texture != texture)
                            {
                                _effect.Texture = texture;
                                _effect.TextureEnabled = _effect.Texture != null;
                                pass.Apply();
                            }
                        }

                        _graphics.GraphicsDevice.DrawUserIndexedPrimitives(
                            PrimitiveType.TriangleList,
                            mesh.Segments[index].Vertices,
                            0,
                            mesh.Segments[index].Vertices.Length,
                            part.Indices,
                            0,
                            part.Indices.Length / 3);

                        index = (index + 1) % mesh.Segments.Length;
                    }
                }
            }
        }

        private void BasicallyForceToReloadEverything()
        {
            _models.Clear();
            LoadMap(_worldId, _placeId);
            LoadObjEntry(_objEntryId);
        }

        private void LoadObjEntry(string name)
        {
            var model = _kernel.ObjEntries.FirstOrDefault(x => x.ModelName == name);
            if (model == null)
                return;

            var internalName = "p_ex";
            switch (model.ObjectType)
            {
                case Objentry.Type.ZAKO:
                    internalName = "m_ex";
                    break;
                case Objentry.Type.PLAYER:
                    internalName = "p_ex";
                    break;
                case Objentry.Type.NPC:
                    internalName = "h_ex";
                    internalName = "n_ex";
                    break;
                case Objentry.Type.WEAPON:
                    internalName = "w_ex";
                    break;
                case Objentry.Type.F_OBJ:
                    internalName = "f_ex";
                    break;
                case Objentry.Type.BOSS:
                    internalName = "b_ex";
                    break;
            }

            var fileName = $"obj/{model.ModelName}.mdlx";
            _archiveManager.LoadArchive(fileName);
            AddMesh(FromMdlx(_graphics.GraphicsDevice, _archiveManager, internalName, "tim_"));
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
            var fileName = $"map/{_kernel.Language}/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.map";

            _archiveManager.LoadArchive(fileName);
            AddMesh(FromMdlx(_graphics.GraphicsDevice, _archiveManager, "MAP", "MAP"));
            AddMesh(FromMdlx(_graphics.GraphicsDevice, _archiveManager, "SK0", "SK0"));
        }

        private static Mesh FromMdlx(
            GraphicsDevice graphics, ArchiveManager archiveManager, string modelName, string textureName)
        {
            var mdlx = archiveManager.Get<Mdlx>(modelName);
            var textures = archiveManager.Get<ModelTexture>(textureName);
            if (mdlx == null)
                return null;

            var model = new MdlxParser(mdlx).Model;

            return new Mesh
            {
                Segments = model.Segments.Select(segment => new Mesh.Segment
                {
                    Vertices = segment.Vertices.Select(vertex => new VertexPositionColorTexture
                    {
                        Position = new Vector3(vertex.X, vertex.Y, vertex.Z),
                        TextureCoordinate = new Vector2(vertex.U, vertex.V),
                        Color = new Color((vertex.Color >> 16) & 0xff, (vertex.Color >> 8) & 0xff, vertex.Color & 0xff, (vertex.Color >> 24) & 0xff)
                    }).ToArray()
                }).ToArray(),
                Parts = model.Parts.Select(part => new Mesh.Part
                {
                    Indices = part.Indices,
                    SegmentId = part.SegmentId,
                    TextureId = part.TextureId
                }).ToArray(),
                Textures = textures?.Images?.Select(texture => texture.CreateTexture(graphics)).ToArray() ?? new Texture2D[0]
            };
        }

        private void AddMesh(Mesh mesh)
        {
            if (mesh == null)
                return;

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
                _objEntryId = _debugObjentryCursor;

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

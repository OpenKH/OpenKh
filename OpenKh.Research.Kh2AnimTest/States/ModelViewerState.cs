using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.MonoGame;
using OpenKh.Kh2.Ard;
using OpenKh.Research.Kh2AnimTest.Debugging;
using OpenKh.Research.Kh2AnimTest.Entities;
using OpenKh.Research.Kh2AnimTest.Infrastructure;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace OpenKh.Research.Kh2AnimTest.States
{
    public class ModelViewerState : IState, IGameContext
    {
        public Kernel Kernel { get; private set; }
        private IDataContent _dataContent;
        private ArchiveManager _archiveManager;
        private GraphicsDeviceManager _graphics;
        private InputManager _input;
        private IStateChange _stateChange;
        private KingdomShader _shader;
        private Camera _camera;
        private List<ObjectEntity> _objectEntities = new List<ObjectEntity>();

        private bool _enableCameraMovement = true;

        private readonly static BlendState AlphaBlendState = new BlendState()
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

        public void DebugDraw(IDebug debug)
        {
            return;
        }

        public void DebugUpdate(IDebug debug)
        {
            return;
        }

        public void Destroy()
        {
            return;
        }

        public void Initialize(StateInitDesc initDesc)
        {
            Kernel = initDesc.Kernel;
            _dataContent = initDesc.DataContent;
            _archiveManager = initDesc.ArchiveManager;
            _graphics = initDesc.GraphicsDevice;
            _input = initDesc.InputManager;
            _stateChange = initDesc.StateChange;
            _shader = new KingdomShader(initDesc.ContentManager);
            _camera = new Camera()
            {
                CameraPosition = new Vector3(0, 50, 200),
                CameraRotationYawPitchRoll = new Vector3(90, 0, 10),
            };

            BasicallyForceToReloadEverything();
        }

        private void BasicallyForceToReloadEverything()
        {
            _objectEntities.Clear();

            SpawnEntity(
                new SpawnPoint.Entity
                {
                    ObjectId = 0x236, // PLAYER
                }
            );
        }

        public void LoadPlace(int worldId, int placeId, int spawnIndex)
        {
            return;
        }

        public void LoadTitleScreen()
        {
            return;
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

            _graphics.GraphicsDevice.RasterizerState = RasterizerState.CullClockwise;

            _shader.Pass(pass =>
            {
                _graphics.GraphicsDevice.BlendState = BlendState.Opaque;
                _shader.UseAlphaMask = true;

                DrawAllMeshes(pass, true);

                _graphics.GraphicsDevice.BlendState = AlphaBlendState;
                _shader.UseAlphaMask = false;

                DrawAllMeshes(pass, false);
            });

            return;
        }

        private void DrawAllMeshes(EffectPass pass, bool passRenderOpaque)
        {
            _graphics.GraphicsDevice.DepthStencilState = passRenderOpaque ? DepthStencilState.Default : DepthStencilState.DepthRead;

            _shader.ProjectionView = _camera.Projection;
            _shader.WorldView = _camera.World;
            _shader.ModelView = Matrix.Identity;
            pass.Apply();

            _graphics.GraphicsDevice.DepthStencilState = DepthStencilState.Default;

            foreach (var entity in _objectEntities.Where(x => x.Mesh != null))
            {
                _shader.ProjectionView = _camera.Projection;
                _shader.WorldView = _camera.World;
                _shader.ModelView = entity.GetMatrix();
                pass.Apply();

                RenderMesh(pass, entity.Mesh, passRenderOpaque);
            }
        }

        private void RenderMesh(EffectPass pass, MeshGroup mesh, bool passRenderOpaque)
        {
            for (int index = 0; index < mesh.Parts.Length && index < mesh.Segments.Length; index++)
            {
                MeshGroup.Part part = mesh.Parts[index];
                MeshGroup.Segment segment = mesh.Segments[index];

                if (part.Indices.Length == 0 || part.IsOpaque != passRenderOpaque)
                    continue;

                var textureIndex = part.TextureId & 0xffff;
                if (textureIndex < mesh.Textures.Length)
                    _shader.SetRenderTexture(pass, mesh.Textures[textureIndex]);

                _graphics.GraphicsDevice.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    segment.Vertices,
                    0,
                    segment.Vertices.Length,
                    part.Indices,
                    0,
                    part.Indices.Length / 3);
            }
        }

        private void SpawnEntity(SpawnPoint.Entity entityDesc)
        {
            var entity = ObjectEntity.FromSpawnPoint(Kernel, entityDesc);
            entity.LoadMesh(_graphics.GraphicsDevice);

            _objectEntities.Add(entity);
        }
    }
}

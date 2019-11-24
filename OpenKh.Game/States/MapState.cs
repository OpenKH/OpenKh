using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.Models;
using System.Collections.Generic;

namespace OpenKh.Game.States
{
    public class MapState : IState
    {
        private GraphicsDeviceManager _graphics;
        private InputManager _input;
        private List<Mesh> _mesh;
        private BasicEffect _effect;

        public Vector3 CameraPosition { get; set; }
        public Vector3 CameraLookAt { get; set; }
        public Vector3 CameraUp { get; set; }

        public void Initialize(StateInitDesc initDesc)
        {
            _graphics = initDesc.GraphicsDevice;
            _input = initDesc.InputManager;
            _effect = new BasicEffect(_graphics.GraphicsDevice);

            CameraPosition = new Vector3(0, 40, 20);
            CameraLookAt = Vector3.Zero;
            CameraUp = Vector3.UnitZ;

            _mesh = new List<Mesh>
            {
                Mesh.FromSample()
            };
        }

        public void Destroy()
        {
        }

        public void Update(DeltaTimes deltaTimes)
        {
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            var aspectRatio = _graphics.PreferredBackBufferWidth / (float)_graphics.PreferredBackBufferHeight;
            var fieldOfView = MathHelper.PiOver4;
            var nearClipPlane = 1;
            var farClipPlane = 200;

            _effect.Projection = Matrix.CreatePerspectiveFieldOfView(
                fieldOfView, aspectRatio, nearClipPlane, farClipPlane);
            _effect.View = Matrix.CreateLookAt(CameraPosition, CameraPosition + CameraLookAt, CameraUp);

            foreach (var pass in _effect.CurrentTechnique.Passes)
            {
                pass.Apply();

                foreach (var mesh in _mesh)
                {
                    _graphics.GraphicsDevice.DrawUserPrimitives(
                        mesh.PrimitiveType, mesh.Vertices, mesh.Start, mesh.Count);
                }
            }
        }
    }
}

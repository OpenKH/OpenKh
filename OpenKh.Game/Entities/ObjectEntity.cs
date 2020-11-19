using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Motion;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using OpenKh.Kh2.Extensions;
using System.IO;
using System.Linq;

namespace OpenKh.Game.Entities
{
    public class ObjectEntity : IEntity, IModelMotion
    {
        private Mdlx _model;

        public ObjectEntity(Kernel kernel, int objectId)
        {
            Kernel = kernel;
            ObjectId = objectId;
            Scaling = new Vector3(1, 1, 1);
        }

        public Kernel Kernel { get; }

        public int ObjectId { get; }

        public string ObjectName => Kernel.ObjEntries
            .FirstOrDefault(x => x.ObjectId == ObjectId)?.ModelName;

        public MeshGroup Mesh { get; private set; }

        public Kh2MotionEngine Motion { get; set; }

        public Vector3 Position { get; set; }

        public Vector3 Rotation { get; set; }

        public Vector3 Scaling { get; set; }

        public float Time { get; set; }

        public void LoadMesh(GraphicsDevice graphics)
        {
            var objEntry = Kernel.ObjEntries.FirstOrDefault(x => x.ObjectId == ObjectId);
            if (objEntry == null)
            {
                Log.Warn($"Object ID {ObjectId} not found.");
                return;
            }

            var modelName = $"obj/{objEntry.ModelName}.mdlx";
            using var stream = Kernel.DataContent.FileOpen(modelName);
            var entries = Bar.Read(stream);
            _model = entries.ForEntry(x => x.Type == Bar.EntryType.Model, Mdlx.Read);
            var texture = entries.ForEntry("tim_", Bar.EntryType.ModelTexture, ModelTexture.Read);
            Mesh = MeshLoader.FromKH2(graphics, _model, texture);

            try
            {
                var msetName = $"obj/{objEntry.AnimationName}";
                if (Kernel.DataContent.FileExists(msetName))
                {
                    var msetEntries = Kernel.DataContent.FileOpen(msetName).Using(Bar.Read);
                    Motion = new Kh2MotionEngine(msetEntries)
                    {
                        CurrentAnimationIndex = 0
                    };
                }
                else
                {
                    Motion = null;
                    Log.Warn($"MSET {objEntry.AnimationName} does not exist");
                }
            }
            catch (System.NotImplementedException)
            {
                Motion = null;
            }
        }

        public void Update(float deltaTime)
        {
            Time += deltaTime;
            Motion?.ApplyMotion(this, Time);
        }

        public void ApplyMotion(System.Numerics.Matrix4x4[] matrices)
        {
            Mesh.MeshDescriptors = MeshLoader.FromKH2(_model, matrices).MeshDescriptors;
        }

        public static MeshGroup FromFbx(GraphicsDevice graphics, string filePath)
        {
            const float Scale = 96.0f;
            var assimp = new Assimp.AssimpContext();
            var scene = assimp.ImportFile(filePath, Assimp.PostProcessSteps.PreTransformVertices);
            var baseFilePath = Path.GetDirectoryName(filePath);

            return new MeshGroup()
            {
                MeshDescriptors = scene.Meshes
                    .Select(x =>
                    {
                        var vertices = new VertexPositionColorTexture[x.Vertices.Count];
                        for (var i = 0; i < vertices.Length; i++)
                        {
                            vertices[i].Position.X = x.Vertices[i].X * Scale;
                            vertices[i].Position.Y = x.Vertices[i].Y * Scale;
                            vertices[i].Position.Z = x.Vertices[i].Z * Scale;
                            vertices[i].TextureCoordinate.X = x.TextureCoordinateChannels[0][i].X;
                            vertices[i].TextureCoordinate.Y = 1.0f - x.TextureCoordinateChannels[0][i].Y;
                            vertices[i].Color = Color.White;
                        }

                        return new MeshDesc
                        {
                            Vertices = vertices,
                            Indices = x.Faces.SelectMany(f => f.Indices).ToArray(),
                            IsOpaque = true,
                            TextureIndex = x.MaterialIndex
                        };
                    }).ToList(),
                Textures = scene.Materials.Select(x =>
                {
                    var path = Path.Join(baseFilePath, $"{x.Name}.png");
                    return new PngKingdomTexture(path, graphics);
                }).ToArray(),
            };
        }

        public static ObjectEntity FromSpawnPoint(Kernel kernel, SpawnPoint.Entity spawnPoint) =>
            new ObjectEntity(kernel, spawnPoint.ObjectId)
            {
                Position = new Vector3(spawnPoint.PositionX, -spawnPoint.PositionY, -spawnPoint.PositionZ),
                Rotation = new Vector3(spawnPoint.RotationX, spawnPoint.RotationY, spawnPoint.RotationZ),
            };
    }
}

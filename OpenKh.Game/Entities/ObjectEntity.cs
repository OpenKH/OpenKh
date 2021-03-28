using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Motion;
using OpenKh.Common;
using OpenKh.Game.Infrastructure;
using OpenKh.Bbs;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using OpenKh.Kh2.Extensions;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using OpenKh.Engine.Parsers;
using OpenKh.Engine;
using System.Numerics;

namespace OpenKh.Game.Entities
{
    public class ObjectEntity : IEntity, IMonoGameModel
    {
        private Mdlx _model;
        public List<ObjectCollision> ObjectCollisions { get; set; }

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

        public bool IsPlayer { get; set; }

        public bool IsMeshLoaded => Model != null;

        public IModelMotion Model { get; private set; }

        public Kh2MotionEngine Motion { get; set; }

        public List<MeshDescriptor> MeshDescriptors => Model?.MeshDescriptors;

        public IKingdomTexture[] Textures { get; private set; }

        public bool IsVisible { get; set; } = true;

        public Vector3 Position { get; set; }

        public Vector3 Rotation { get; set; }

        public Vector3 Scaling { get; set; }

        public float Time { get; set; }

        public float MotionTime { get; set; }

        public void LoadMesh(GraphicsDevice graphics)
        {
            var objEntry = Kernel.ObjEntries.FirstOrDefault(x => x.ObjectId == ObjectId);
            if (objEntry == null)
            {
                Log.Warn("Object ID {0} not found.", ObjectId);
                return;
            }

            IsPlayer = objEntry.ObjectType == Objentry.Type.PLAYER;

            var modelName = $"obj/{objEntry.ModelName}.mdlx";
            using var stream = Kernel.DataContent.FileOpen(modelName);
            var entries = Bar.Read(stream);
            _model = entries.ForEntry(x => x.Type == Bar.EntryType.Model, Mdlx.Read);
            Model = MeshLoader.FromKH2(_model);

            var texture = entries.ForEntry("tim_", Bar.EntryType.ModelTexture, ModelTexture.Read);
            Textures = texture.LoadTextures(graphics).ToArray();

            ObjectCollisions = entries.ForEntry(x => x.Type == Bar.EntryType.ModelCollision && x.Stream.Length > 0,
                ObjectCollision.Read) ?? new List<ObjectCollision>();

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
                    Motion = new Kh2MotionEngine();
                    Log.Warn("MSET {0} does not exist", objEntry.AnimationName);
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
            MotionTime += deltaTime;
            Motion?.ApplyMotion(Model, MotionTime);
        }

        public void SetMotion(Motion motion)
        {
            MotionTime = 0;
            Motion?.UseCustomMotion(motion);
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
                        var vertices = new PositionColoredTextured[x.Vertices.Count];
                        for (var i = 0; i < vertices.Length; i++)
                        {
                            vertices[i].X = x.Vertices[i].X * Scale;
                            vertices[i].Y = x.Vertices[i].Y * Scale;
                            vertices[i].Z = x.Vertices[i].Z * Scale;
                            vertices[i].Tu = x.TextureCoordinateChannels[0][i].X;
                            vertices[i].Tv = 1.0f - x.TextureCoordinateChannels[0][i].Y;
                            vertices[i].R = 1.0f;
                            vertices[i].G = 1.0f;
                            vertices[i].B = 1.0f;
                            vertices[i].A = 1.0f;
                        }

                        return new MeshDescriptor
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

        public static (IModelMotion, IKingdomTexture[]) BBSMeshLoader(GraphicsDevice graphics, string FilePath, IModelMotion Model, IKingdomTexture[] Textures)
        {
            const float Scale = 100.0f;
            var file = File.OpenRead(FilePath);
            Pmo pmo = Pmo.Read(file);
            Model = new PmoParser(pmo, Scale);

            List<Tim2KingdomTexture> BbsTextures = new List<Tim2KingdomTexture>();
            Textures = new IKingdomTexture[pmo.header.TextureCount];

            for (int i = 0; i < pmo.header.TextureCount; i++)
            {
                BbsTextures.Add(new Tim2KingdomTexture(pmo.texturesData[i], graphics));
                Textures[i] = BbsTextures[i];
            }

            return (Model, Textures);
        }

        public static ObjectEntity FromSpawnPoint(Kernel kernel, SpawnPoint.Entity spawnPoint) =>
            new ObjectEntity(kernel, kernel.GetRealObjectId(spawnPoint.ObjectId))
            {
                Position = new Vector3(spawnPoint.PositionX, -spawnPoint.PositionY, -spawnPoint.PositionZ),
                Rotation = new Vector3(spawnPoint.RotationX, spawnPoint.RotationY, spawnPoint.RotationZ),
            };
    }
}

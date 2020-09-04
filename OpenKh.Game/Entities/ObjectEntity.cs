using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.MonoGame;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using OpenKh.Kh2.Extensions;
using System.Linq;

namespace OpenKh.Game.Entities
{
    public class ObjectEntity : IEntity
    {
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

        public Vector3 Position { get; set; }

        public Vector3 Rotation { get; set; }

        public Vector3 Scaling { get; set; }

        public void LoadMesh(GraphicsDevice graphics, ArchiveManager archiveManager)
        {
            var objEntry = Kernel.ObjEntries.FirstOrDefault(x => x.ObjectId == ObjectId);
            if (objEntry == null)
            {
                Log.Warn($"Object ID {ObjectId} not found.");
                return;
            }

            var fileName = $"obj/{objEntry.ModelName}.mdlx";
            var msetFileName = $"obj/{objEntry.AnimationName}";

            using var stream = Kernel.DataContent.FileOpen(fileName);
            var entries = Bar.Read(stream);
            var model = entries.ForEntry(x => x.Type == Bar.EntryType.Model, Mdlx.Read);
            var texture = entries.ForEntry("tim_", Bar.EntryType.ModelTexture, ModelTexture.Read);

            if (!string.IsNullOrEmpty(objEntry.AnimationName))
                archiveManager.LoadArchive(msetFileName);
            Mesh = MeshLoader.FromKH2(graphics, model, texture, archiveManager, () => Kernel.DataContent.FileOpen(fileName));
        }

        public static ObjectEntity FromSpawnPoint(Kernel kernel, SpawnPoint.Entity spawnPoint) =>
            new ObjectEntity(kernel, spawnPoint.ObjectId)
            {
                Position = new Vector3(spawnPoint.PositionX, -spawnPoint.PositionY, -spawnPoint.PositionZ),
                Rotation = new Vector3(spawnPoint.RotationX, spawnPoint.RotationY, spawnPoint.RotationZ),
            };
    }
}

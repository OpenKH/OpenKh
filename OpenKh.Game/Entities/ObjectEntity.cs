using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.Models;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using OpenKh.Kh2.Extensions;
using System;
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

        public MeshGroup Mesh { get; private set; }

        public Vector3 Position { get; set; }

        public Vector3 Rotation { get; set; }

        public Vector3 Scaling { get; set; }

        public void LoadMesh(GraphicsDevice graphics)
        {
            if (ObjectId < 0 || ObjectId >= Kernel.ObjEntries.Count)
                throw new ArgumentOutOfRangeException(nameof(ObjectId),
                    $"Object ID {ObjectId:X)} is out of range.");

            var objEntry = Kernel.ObjEntries.Items.FirstOrDefault(x => x.ObjectId == ObjectId);
            var fileName = $"obj/{objEntry.ModelName}.mdlx";

            using var stream = Kernel.DataContent.FileOpen(fileName);
            var entries = Bar.Read(stream);
            var model = entries.ForEntry(x => x.Type == Bar.EntryType.Model, Mdlx.Read);
            var texture = entries.ForEntry("tim_", Bar.EntryType.ModelTexture, ModelTexture.Read);
            Mesh = MeshLoader.FromKH2(graphics, model, texture);
        }

        public static ObjectEntity FromSpawnPoint(Kernel kernel, SpawnPoint.Entity spawnPoint) =>
            new ObjectEntity(kernel, spawnPoint.ObjectId)
            {
                Position = new Vector3(spawnPoint.PositionX, spawnPoint.PositionY, spawnPoint.PositionZ),
                Rotation = new Vector3(spawnPoint.RotationX, spawnPoint.RotationY, spawnPoint.RotationZ),
            };
    }
}

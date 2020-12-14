using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using OpenKh.Game.Debugging;
using OpenKh.Game.Entities;
using OpenKh.Kh2;
using OpenKh.Kh2.Ard;
using OpenKh.Kh2.Extensions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.Infrastructure
{
    public class Kh2Field : IField
    {
        private readonly Kernel _kernel;
        private readonly GraphicsDevice _graphicsDevice;
        private readonly List<ObjectEntity> _actors = new List<ObjectEntity>();

        private Bar _binarcArd;
        private int _spawnScriptMap;
        private int _spawnScriptBtl;
        private int _spawnScriptEvt;

        public Kh2Field(
            Kernel kernel,
            Dictionary<string, string> settings,
            GraphicsDevice graphicsDevice)
        {
            _kernel = kernel;
            _graphicsDevice = graphicsDevice;

            _spawnScriptMap = settings.GetInt("SpawnScriptMap", 0x06);
            _spawnScriptBtl = settings.GetInt("SpawnScriptBtl", 0x01);
            _spawnScriptEvt = settings.GetInt("SpawnScriptEvt", 0x16);
        }

        public void LoadMapArd(int worldIndex, int placeIndex)
        {
            string fileName;
            if (_kernel.IsReMix)
                fileName = $"ard/{_kernel.Language}/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.ard";
            else
                fileName = $"ard/{Constants.WorldIds[worldIndex]}{placeIndex:D02}.ard";

            RemoveAllActors();

            _binarcArd = _kernel.DataContent.FileOpen(fileName).Using(Bar.Read);
            RunSpawnScript(_binarcArd, "map", _spawnScriptMap);
            RunSpawnScript(_binarcArd, "btl", _spawnScriptBtl);
            RunSpawnScript(_binarcArd, "evt", _spawnScriptEvt);
        }

        public void Update(double deltaTime)
        {
            foreach (var entity in _actors.Where(x => x.IsMeshLoaded))
                entity.Update((float)deltaTime);
        }

        public void ForEveryModel(Action<IEntity, IMonoGameModel> action)
        {
            foreach (var actor in _actors)
                action(actor, actor);
        }

        public void AddActor(int objectId)
        {

        }

        public void AddActor(SpawnPoint.Entity entityDesc)
        {
            var entity = ObjectEntity.FromSpawnPoint(_kernel, entityDesc);
            entity.LoadMesh(_graphicsDevice);

            _actors.Add(entity);
        }

        public void RemoveAllActors()
        {
            _actors.Clear();
        }


        private void RunSpawnScript(
            IEnumerable<Bar.Entry> barEntries, string spawnScriptName, int programId)
        {
            var spawnScript = barEntries.ForEntry(spawnScriptName, Bar.EntryType.SpawnScript, SpawnScript.Read);
            if (spawnScript == null)
                return;

            var program = spawnScript.FirstOrDefault(x => x.ProgramId == programId);
            if (program == null)
                return;

            foreach (var function in program.Functions)
            {
                switch (function.Opcode)
                {
                    case SpawnScript.Operation.Spawn:
                        var spawn = function.AsString(0);
                        Log.Info($"Loading spawn {spawn}");
                        var spawnPoints = barEntries.ForEntry(spawn, Bar.EntryType.SpawnPoint, SpawnPoint.Read);
                        if (spawnPoints != null)
                        {
                            foreach (var spawnPoint in spawnPoints)
                            {
                                foreach (var desc in spawnPoint.Entities)
                                    AddActor(desc);
                            }
                        }
                        else
                            Log.Warn($"Unable to find spawn \"{spawn}\".");
                        break;
                }
            }
        }
    }
}

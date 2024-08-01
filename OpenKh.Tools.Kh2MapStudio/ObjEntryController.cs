using Microsoft.Xna.Framework.Graphics;
using OpenKh.Common;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Parsers;
using OpenKh.Kh2;
using OpenKh.Tools.Kh2MapStudio.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Tools.Kh2MapStudio
{
    class ObjEntryController : IObjEntryController, IDisposable
    {
        private static readonly MeshGroup EmptyMeshGroup = new MeshGroup
        {
            Textures = new KingdomTexture[0],
            MeshDescriptors = new List<MeshDescriptor>(0)
        };

        private readonly Dictionary<int, MeshGroup> _meshGroups = new Dictionary<int, MeshGroup>();
        private readonly Dictionary<string, int> _objEntryLookup;
        private readonly Dictionary<int, string> _objEntryLookupReversed;
        private readonly GraphicsDevice _graphics;
        private readonly string _objPath;

        public ObjEntryController(GraphicsDevice graphics, string objPath, string objEntryFileName) :
            this(File.OpenRead(objEntryFileName).Using(Objentry.Read))
        {
            _graphics = graphics;
            _objPath = objPath;
        }

        public ObjEntryController(List<Objentry> objEntries)
        {
            ObjectEntries = objEntries.OrderBy(x => x.ModelName).ToArray();
            _objEntryLookup = ObjectEntries
                .ToDictionary(x => x.ModelName, x => (int)x.ObjectId);
            _objEntryLookupReversed = ObjectEntries
                .ToDictionary(x => (int)x.ObjectId, x => x.ModelName);
        }

        public IEnumerable<Objentry> ObjectEntries { get; }

        public void Dispose()
        {
            foreach (var meshGroup in _meshGroups)
                foreach (var texture in meshGroup.Value.Textures)
                    texture.Dispose();
            _meshGroups.Clear();
        }

        //Below: First tries to get the name from the specified index.
        //If that fails, fall back to warning the user with the name.
        public string GetName(int objectId)
        {
            if (_objEntryLookupReversed.TryGetValue(objectId, out var objEntryName))
            {
                return objEntryName;
            }
            return "ENTITY ID NOT PRESENT IN OBJENTRY";
        }

        public MeshGroup this[int objId]
        {
            get
            {
                if (_meshGroups.TryGetValue(objId, out var meshGroup))
                    return meshGroup;

                // Fix OBJIds being out of range by falling back onto a value of 1.
                if (!_objEntryLookupReversed.ContainsKey(objId))
                {
                    objId = 1; // Default to 1 if out of range
                }

                var objEntryName = _objEntryLookupReversed[objId];
                var baseModelPath = Path.Combine(_objPath, objEntryName);
                var moddedFolderPath = Path.Combine(_objPath, "..", "mapstudio");  // Move up one level and then to 'mapstudio'
                var moddedModelPath = Path.Combine(moddedFolderPath, objEntryName);
                var baseModelFileName = baseModelPath + ".mdlx";
                var moddedModelFileName = moddedModelPath + ".mdlx";

                // Determine the correct file path to load from, prioritizing modded path
                var modelFileName = File.Exists(moddedModelFileName) ? moddedModelFileName : baseModelFileName;

                MeshGroup LoadMeshGroup(string fileName)
                {
                    if (!File.Exists(fileName))
                        return EmptyMeshGroup;

                    var mdlxEntries = File.OpenRead(fileName).Using(Bar.Read);
                    var modelEntry = mdlxEntries.FirstOrDefault(x => x.Type == Bar.EntryType.Model);
                    if (modelEntry == null)
                        return EmptyMeshGroup;

                    var model = Mdlx.Read(modelEntry.Stream);
                    ModelTexture textures = null;

                    var textureEntry = mdlxEntries.FirstOrDefault(x => x.Type == Bar.EntryType.ModelTexture);
                    if (textureEntry != null)
                        textures = ModelTexture.Read(textureEntry.Stream);

                    var modelMotion = MeshLoader.FromKH2(model);
                    modelMotion.ApplyMotion(modelMotion.InitialPose);
                    return new MeshGroup
                    {
                        MeshDescriptors = modelMotion.MeshDescriptors,
                        Textures = textures == null ? new IKingdomTexture[0] : textures.LoadTextures(_graphics).ToArray()
                    };
                }

                meshGroup = LoadMeshGroup(modelFileName);

                // Check if model or texture is missing and load from fallback if necessary. Loads a simple pyramid model.
                if (meshGroup == EmptyMeshGroup || meshGroup.Textures.Length == 0)
                {
                    var fallbackModelFileName = Path.Combine(_objPath, "F_HB700.mdlx");
                    var fallbackMeshGroup = LoadMeshGroup(fallbackModelFileName);

                    if (meshGroup == EmptyMeshGroup)
                    {
                        meshGroup = fallbackMeshGroup;
                    }
                    else if (meshGroup.Textures.Length == 0)
                    {
                        meshGroup.Textures = fallbackMeshGroup.Textures;
                    }
                }

                _meshGroups[objId] = meshGroup;
                return meshGroup;
            }
        }


        public MeshGroup this[string objName] => this[_objEntryLookup[objName]];
    }
}

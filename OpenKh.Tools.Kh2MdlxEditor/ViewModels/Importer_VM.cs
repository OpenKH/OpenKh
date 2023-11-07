using OpenKh.AssimpUtils;
using OpenKh.Kh2.Models.VIF;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    public class Importer_VM : INotifyPropertyChanged
    {
        public Main2_VM MainVM { get; set; }

        private bool _modelLoaded { get; set; }
        public bool ModelLoaded
        {
            get { return _modelLoaded; }
            set
            {
                _modelLoaded = value;
                OnPropertyChanged("ModelLoaded");
            }
        }

        public string FilePath { get; set; }
        public Assimp.Scene ExternalModel { get; set; }
        public ObservableCollection<ImportMeshOptions> MeshOptionsList { get; set; }

        public bool KeepShadow { get; set; }
        public byte VertexLimitPerPacket { get; set; }
        public byte MemoryLimitPerPacket { get; set; }

        public Importer_VM(Main2_VM mainVM)
        {
            this.MainVM = mainVM;
            ModelLoaded = false;
            MeshOptionsList = new ObservableCollection<ImportMeshOptions>();

            KeepShadow = false;
            VertexLimitPerPacket = (byte)VifProcessor.VERTEX_LIMIT;
            MemoryLimitPerPacket = (byte)VifProcessor.MEMORY_LIMIT;
        }

        public class ImportMeshOptions
        {
            public int Id { get; set; }
            public int PolyCount { get; set; }
            public string Texture { get; set; }
            public bool HasColors { get; set; }
            public bool HasNoColors { get; set; }
            public bool ApplyColors { get; set; }
            public bool HasNormals { get; set; }
            public bool HasNoNormals { get; set; }
            public bool ApplyNormals { get; set; }
        }

        public void loadModel(string filepath)
        {
            ExternalModel = AssimpGeneric.getAssimpSceneFromFile(filepath);

            MeshOptionsList.Clear();
            foreach (Assimp.Mesh mesh in ExternalModel.Meshes)
            {
                ImportMeshOptions meshOptions = new ImportMeshOptions();
                meshOptions.Id = MeshOptionsList.Count;
                meshOptions.PolyCount = mesh.FaceCount;
                meshOptions.Texture = Path.GetFileNameWithoutExtension(ExternalModel.Materials[mesh.MaterialIndex].TextureDiffuse.FilePath);
                meshOptions.ApplyColors = false;
                meshOptions.ApplyNormals = false;

                meshOptions.HasNoColors = true;
                meshOptions.HasNoNormals = true;

                meshOptions.HasColors = true;
                if (mesh.VertexColorChannels[0].Count > 0)
                {
                    foreach (Assimp.Color4D color in mesh.VertexColorChannels[0])
                    {
                        if (color == null)
                        {
                            meshOptions.HasColors = false;
                            break;
                        }
                    }
                }
                else
                    meshOptions.HasColors = false;

                meshOptions.HasNormals = true;
                if (mesh.Normals.Count > 0)
                {
                    foreach (Assimp.Vector3D normal in mesh.Normals)
                    {
                        if (normal == null)
                        {
                            meshOptions.HasNormals = false;
                            break;
                        }
                    }
                }
                else
                    meshOptions.HasNormals = false;

                MeshOptionsList.Add(meshOptions);
            }

            FilePath = filepath;
            ModelLoaded = true;
        }

        public void ImportModel()
        {
            if (FilePath == null)
                return;

            MdlxEditorImporter.KEEP_ORIGINAL_SHADOW = KeepShadow;
            VifProcessor.VERTEX_LIMIT = VertexLimitPerPacket;
            VifProcessor.MEMORY_LIMIT = MemoryLimitPerPacket;

            List<VifProcessor.MeshOptions> newMeshOptions = new List<VifProcessor.MeshOptions>();
            foreach(ImportMeshOptions meshOptions in MeshOptionsList)
            {
                newMeshOptions.Add(new VifProcessor.MeshOptions(meshOptions.HasColors, meshOptions.ApplyColors, meshOptions.HasNormals, meshOptions.ApplyNormals));
            }

            VifProcessor.LoadOptions(newMeshOptions);

            MainVM.replaceModel(FilePath);
        }


        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string name)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(name));
        }
    }
}

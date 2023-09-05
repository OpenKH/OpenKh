using OpenKh.Kh2.Models;
using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Collections.ObjectModel;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Model
{
    public class ModuleModelMeshes_VM
    {
        public ObservableCollection<MeshWrapper> Meshes { get; set; }

        public ModuleModelMeshes_VM()
        {
            Meshes = new ObservableCollection<MeshWrapper>();
            loadMeshes();
        }

        public void loadMeshes()
        {
            if (MdlxService.Instance.ModelFile?.Groups == null || MdlxService.Instance.ModelFile.Groups.Count < 0)
                return;

            Meshes.Clear();
            for (int i = 0; i < MdlxService.Instance.ModelFile.Groups.Count; i++)
            {
                MeshWrapper wrapper = new MeshWrapper();
                wrapper.Id = i;
                wrapper.Name = "Mesh " + i;
                wrapper.Group = MdlxService.Instance.ModelFile.Groups[i];

                Meshes.Add(wrapper);
            }
        }

        public int fun_moveMeshUp(MeshWrapper wrapper)
        {
            if (wrapper.Id <= 0)
                return -1;

            moveMesh(wrapper, wrapper.Id - 1);
            loadMeshes();

            return wrapper.Id - 1;
        }
        public int fun_moveMeshDown(MeshWrapper wrapper)
        {
            if (wrapper.Id >= Meshes.Count - 1)
                return -1;

            moveMesh(wrapper, wrapper.Id + 1);
            loadMeshes();

            return wrapper.Id + 1;
        }

        public void moveMesh(MeshWrapper wrapper, int index)
        {
            ModelSkeletal.SkeletalGroup tempGroup = MdlxService.Instance.ModelFile.Groups[wrapper.Id];


            MdlxService.Instance.ModelFile.Groups.RemoveAt(wrapper.Id);
            MdlxService.Instance.ModelFile.Groups.Insert(index, tempGroup);
        }

        public class MeshWrapper
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public string PolyCount { get; set; }
            public ModelSkeletal.SkeletalGroup Group { get; set; }
        }
    }
}

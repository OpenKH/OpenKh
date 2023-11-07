using OpenKh.Kh2.Models;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Model
{
    public class ModelBoneViewer_VM
    {
        public ModelSkeletal ModelFile { get; set; }
        public ModelBoneViewer_VM() { }
        public ModelBoneViewer_VM(ModelSkeletal modelFile)
        {
            ModelFile = modelFile;
        }
    }
}

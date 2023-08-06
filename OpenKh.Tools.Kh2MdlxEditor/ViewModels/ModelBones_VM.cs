using OpenKh.Kh2.Models;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    public class ModelBones_VM
    {
        public ModelSkeletal ModelFile { get; set; }
        public ModelBones_VM() { }
        public ModelBones_VM(ModelSkeletal modelFile)
        {
            ModelFile = modelFile;
        }
    }
}

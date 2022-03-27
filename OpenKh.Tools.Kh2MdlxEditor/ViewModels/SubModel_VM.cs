using OpenKh.Kh2;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    internal class SubModel_VM
    {
        public Mdlx.SubModel subModel { get; set; }

        public SubModel_VM() { }
        public SubModel_VM(Mdlx.SubModel subModel)
        {
            this.subModel = subModel;
        }
    }
}

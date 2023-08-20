using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Windows;

namespace OpenKh.Tools.Kh2ObjectEditor.ViewModel
{
    public class ModuleLoader_VM : NotifyPropertyChangedBase
    {
        public Visibility TabModelEnabled { get { return Mdlx_Service.Instance.MdlxBar == null ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility TabTexturesEnabled { get { return Mdlx_Service.Instance.MdlxBar == null ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility TabMotionsEnabled { get { return Mset_Service.Instance.MsetBar == null ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility TabParticlesEnabled { get { return Apdx_Service.Instance.ApdxBar == null ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility TabAIEnabled { get { return Mdlx_Service.Instance.BdxFile == null ? Visibility.Collapsed : Visibility.Visible; } }
    }
}

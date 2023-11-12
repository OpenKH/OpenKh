using OpenKh.Tools.Kh2ObjectEditor.Services;
using OpenKh.Tools.Kh2ObjectEditor.Utils;
using System.Windows;

namespace OpenKh.Tools.Kh2ObjectEditor.ViewModel
{
    public class ModuleLoader_VM : NotifyPropertyChangedBase
    {
        public Visibility TabModelEnabled { get { return MdlxService.Instance.MdlxBar == null ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility TabTexturesEnabled { get { return MdlxService.Instance.MdlxBar == null ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility TabMotionsEnabled { get { return MsetService.Instance.MsetBar == null ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility TabCollisionsEnabled { get { return MdlxService.Instance.CollisionFile == null ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility TabParticlesEnabled { get { return ApdxService.Instance.ApdxBar == null ? Visibility.Collapsed : Visibility.Visible; } }
        public Visibility TabAIEnabled { get { return MdlxService.Instance.BdxFile == null ? Visibility.Collapsed : Visibility.Visible; } }
    }
}

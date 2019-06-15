using OpenKh.Kh2;
using OpenKh.Tools.LayoutViewer.Models;
using OpenKh.Tools.LayoutViewer.Service;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class SequenceGroupsViewModel : GenericListModel<SequenceGroupModel>
    {
        public SequenceGroupsViewModel(Layout layout, TexturesViewModel texturesViewModel, EditorDebugRenderingService editorDebugRenderingService) :
            base(layout.SequenceGroups.Select((_, i) => new SequenceGroupModel(layout, i, texturesViewModel, editorDebugRenderingService)))
        {

        }

        public int SelectedSequenceGroupIndex
        {
            get => SelectedIndex;
            set => SelectedIndex = value;
        }

        protected override void OnSelectedItem(SequenceGroupModel item)
        {
            base.OnSelectedItem(item);
            OnPropertyChanged(nameof(SelectedSequenceGroupIndex));
            OnPropertyChanged(nameof(SelectedItem));
        }

        protected override SequenceGroupModel OnNewItem()
        {
            throw new System.NotImplementedException();
        }
    }
}

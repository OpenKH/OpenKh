using kh.kh2;
using OpenKh.Tools.LayoutViewer.Models;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class SequenceGroupsViewModel : GenericListModel<SequenceGroupModel>
    {
        public SequenceGroupsViewModel(Layout layout) :
            base(layout.SequenceGroups.Select((_, i) => new SequenceGroupModel(layout, i)))
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

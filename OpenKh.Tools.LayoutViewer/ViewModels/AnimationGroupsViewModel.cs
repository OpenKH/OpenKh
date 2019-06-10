using OpenKh.Kh2;
using OpenKh.Tools.LayoutViewer.Models;
using System.Linq;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class AnimationGroupsViewModel : GenericListModel<AnimationGroupModel>
    {
        public AnimationGroupsViewModel(Sequence sequence) :
            base(sequence.AnimationGroups.Select((_, i) => new AnimationGroupModel(sequence, i)))
        {

        }

        public int SelectedAnimationGroupIndex
        {
            get => SelectedIndex;
            set => SelectedIndex = value;
        }

        protected override void OnSelectedItem(AnimationGroupModel item)
        {
            base.OnSelectedItem(item);
            OnPropertyChanged(nameof(SelectedAnimationGroupIndex));
            OnPropertyChanged(nameof(SelectedItem));
        }

        protected override AnimationGroupModel OnNewItem()
        {
            throw new System.NotImplementedException();
        }
    }
}

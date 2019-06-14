using OpenKh.Kh2;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using Xe.Tools;

namespace OpenKh.Tools.LayoutViewer.Models
{
    public class SequenceGroupModel : BaseNotifyPropertyChanged
    {
        private readonly Layout layout;
        private readonly int index;
        private readonly Service.EditorDebugRenderingService editorDebugRenderingService;
        private int _selectedIndex = -1;
        private SequencePropertyModel selectedItem;

        public SequenceGroupModel(Layout layout, int index, Service.EditorDebugRenderingService editorDebugRenderingService)
        {
            this.layout = layout;
            this.index = index;
            this.editorDebugRenderingService = editorDebugRenderingService;
        }

        public Layout.SequenceGroup SequenceGroup => layout.SequenceGroups[index];

        public string Name => $"Sequence {index}";

        public short L1Index
        {
            get => SequenceGroup.L1Index;
            set
            {
                var oldValue = SequenceGroup.L1Index;
                SequenceGroup.L1Index = (short)Math.Max(0, Math.Min(layout.SequenceProperties.Count, value));
                FixupL1Count();

                if (SequenceGroup.L1Index != oldValue)
                    OnPropertyChanged(nameof(Items));
            }
        }

        public short L1Count
        {
            get => SequenceGroup.L1Count;
            set
            {
                var oldValue = SequenceGroup.L1Count;
                SequenceGroup.L1Count = value;

                FixupL1Count();
                if (SequenceGroup.L1Count != oldValue)
                    OnPropertyChanged(nameof(Items));
            }
        }

        public int Unknown04
        {
            get => SequenceGroup.Unknown04;
            set => SequenceGroup.Unknown04 = value;
        }

        public int Unknown08
        {
            get => SequenceGroup.Unknown08;
            set => SequenceGroup.Unknown08 = value;
        }

        public int Unknown0c
        {
            get => SequenceGroup.Unknown0c;
            set => SequenceGroup.Unknown0c = value;
        }

        public int Unknown10
        {
            get => SequenceGroup.Unknown10;
            set => SequenceGroup.Unknown10 = value;
        }

        public ObservableCollection<SequencePropertyModel> Items =>
            new ObservableCollection<SequencePropertyModel>(
                Enumerable.Range(L1Index, L1Count)
                .Select(x => new SequencePropertyModel(x, layout, editorDebugRenderingService)));

        public SequencePropertyModel SelectedItem
        {
            get => selectedItem;
            set
            {
                selectedItem = value;
                editorDebugRenderingService.Reset();
                OnPropertyChanged();
            }
        }

        public int SelectedIndex
        {
            get => _selectedIndex;
            set
            {
                _selectedIndex = value;
                OnPropertyChanged(nameof(IsItemSelected));
            }
        }

        public bool IsItemSelected => SelectedIndex >= 0;

        public override string ToString() => Name;

        private void FixupL1Count()
        {
            var overflow = SequenceGroup.L1Index + SequenceGroup.L1Count - layout.SequenceProperties.Count;
            if (overflow > 0)
            {
                L1Count = (short)(L1Count - overflow);
                OnPropertyChanged(nameof(L1Count));
            }
        }
    }
}

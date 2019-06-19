using OpenKh.Kh2;
using OpenKh.Tools.LayoutViewer.Models;
using System.Collections.Generic;
using System.Linq;
using Xe.Tools.Wpf.Models;
using static OpenKh.Tools.LayoutViewer.ViewModels.LayoutSelectionViewModel;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class LayoutSelectionViewModel : GenericListModel<LayoutSelectionEntryModel>
    {
        public class LayoutSelectionEntryModel
        {
            public LayoutSelectionEntryModel(LayoutEntryModel layoutEntry)
            {
                Entry = layoutEntry;
            }

            public LayoutEntryModel Entry { get; }

            public override string ToString() => Entry.Layout.Name;
        }

        public LayoutSelectionViewModel(IEnumerable<Bar.Entry> barEntries)
            : this(GetLayoutImagePairs(barEntries))
        {
        }

        public LayoutSelectionViewModel(IEnumerable<LayoutEntryModel> list)
            : this(list.Select(x => new LayoutSelectionEntryModel(x)))
        {
        }

        public LayoutSelectionViewModel(IEnumerable<LayoutSelectionEntryModel> list)
            : base(list)
        {
        }

        public LayoutEntryModel SelectedLayoutEntry { get; set; }

        protected override LayoutSelectionEntryModel OnNewItem()
        {
            throw new System.NotImplementedException();
        }

        private static LayoutEntryModel GetLayoutImagePair(IEnumerable<Bar.Entry> barEntries) =>
            new LayoutEntryModel
            {
                Layout = barEntries
                        .Where(x => x.Type == Bar.EntryType.Layout)
                        .Select(x => new LayoutEntryPropertyModel<Layout>
                        {
                            Name = x.Name,
                            Value = Layout.Read(x.Stream)
                        })
                        .FirstOrDefault(),
                Images = barEntries
                        .Where(x => x.Type == Bar.EntryType.Imgz)
                        .Select(x => new LayoutEntryPropertyModel<List<Imgd>>
                        {
                            Name = x.Name,
                            Value = Imgz.Open(x.Stream).ToList()
                        })
                        .FirstOrDefault()
            };

        private static IEnumerable<LayoutEntryModel> GetLayoutImagePairs(IEnumerable<Bar.Entry> barEntries) =>
            barEntries.GroupBy(x => x.Name)
                .Select(x => GetLayoutImagePair(x))
                .Where(x => x.Layout != null || x.Images != null);
    }
}

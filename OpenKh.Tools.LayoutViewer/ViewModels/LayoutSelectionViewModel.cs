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

            public override string ToString() => Entry.Name;
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

        private static LayoutEntryModel GetLayoutImagePair(IEnumerable<Bar.Entry> barEntries)
        {
            var layout = barEntries
                .Where(x => x.Type == Bar.EntryType.Layout)
                .Select(x => Layout.Read(x.Stream))
                .FirstOrDefault();

            var images = barEntries
                .Where(x => x.Type == Bar.EntryType.Imgd || x.Type == Bar.EntryType.Imgz)
                .Select(x => x.Type == Bar.EntryType.Imgz ? Imgz.Open(x.Stream) : new[] { Imgd.Read(x.Stream) })
                .FirstOrDefault();

            return new LayoutEntryModel
            {
                Name = barEntries.FirstOrDefault()?.Name,
                Layout = layout,
                Images = images.ToList()
            };
        }

        private static IEnumerable<LayoutEntryModel> GetLayoutImagePairs(IEnumerable<Bar.Entry> barEntries) =>
            barEntries.GroupBy(x => x.Name)
                .Select(x => GetLayoutImagePair(x))
                .Where(x => x.Layout != null || x.Images != null);
    }
}

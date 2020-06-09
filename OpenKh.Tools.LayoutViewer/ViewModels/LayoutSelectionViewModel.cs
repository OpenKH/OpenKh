using OpenKh.Common;
using OpenKh.Kh2;
using OpenKh.Tools.LayoutViewer.Models;
using System.Collections.Generic;
using System.Linq;
using Xe.Tools;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class LayoutSelectionViewModel : BaseNotifyPropertyChanged
    {
        private LayoutSelectionEntryModel selectedLayout;
        private ImagesSelectionEntryModel selectedImages;

        public class LayoutSelectionEntryModel
        {
            private readonly Bar.Entry _entry;

            public LayoutSelectionEntryModel(Bar.Entry entry)
            {
                _entry = entry;
            }

            public string Name => _entry.Name;
            public Layout Layout => Layout.Read(_entry.Stream.FromBegin());

            public override string ToString() => Name;
        }

        public class ImagesSelectionEntryModel
        {
            private readonly Bar.Entry _entry;

            public ImagesSelectionEntryModel(Bar.Entry entry)
            {
                _entry = entry;
            }

            public string Name => _entry.Name;
            public List<Imgd> Layout => Imgz.Read(_entry.Stream.FromBegin()).ToList();

            public override string ToString() => Name;
        }

        public IEnumerable<LayoutSelectionEntryModel> LayoutItems { get; }
        public IEnumerable<ImagesSelectionEntryModel> ImagesItems { get; }

        public LayoutSelectionEntryModel SelectedLayout
        {
            get => selectedLayout;
            set
            {
                selectedLayout = value;
                OnPropertyChanged(nameof(IsItemSelected));
            }
        }

        public ImagesSelectionEntryModel SelectedImages
        {
            get => selectedImages;
            set
            {
                selectedImages = value;
                OnPropertyChanged(nameof(IsItemSelected));
            }
        }

        public LayoutEntryModel SelectedLayoutEntry => IsItemSelected ? new LayoutEntryModel
        {
            Layout = new LayoutEntryPropertyModel<Layout>
            {
                Name = SelectedLayout.Name,
                Value = SelectedLayout.Layout
            },
            Images = new LayoutEntryPropertyModel<List<Imgd>>
            {
                Name = SelectedImages.Name,
                Value = SelectedImages.Layout
            }
        } : null;

        public bool IsItemSelected => SelectedLayout != null && SelectedImages != null;

        public LayoutSelectionViewModel(IEnumerable<Bar.Entry> barEntries)
        {
            LayoutItems = barEntries
                .Where(x => x.Type == Bar.EntryType.Layout)
                .Select(x => new LayoutSelectionEntryModel(x)).ToList();
            ImagesItems = barEntries
                .Where(x => x.Type == Bar.EntryType.Imgz)
                .Select(x => new ImagesSelectionEntryModel(x)).ToList();
        }
    }
}

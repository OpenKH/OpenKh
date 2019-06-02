using kh.tools.common;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.Drawing;
using Xe.Tools;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class MainViewModel : BaseNotifyPropertyChanged
    {
        private SequenceGroupsViewModel sequenceGroups;
        private Layout selectedLayout;
        private IEnumerable<Imgd> selectedImages;
        private int frameIndex;
        private int selectedSequenceGroupIndex;

        public string Title { get; }

        public IDrawing Drawing { get; }

        public Layout SelectedLayout
        {
            get => selectedLayout; private set
            {
                selectedLayout = value;
                OnPropertyChanged(nameof(SelectedLayout));
            }
        }

        public IEnumerable<Imgd> SelectedImages
        {
            get => selectedImages; private set
            {
                selectedImages = value;
                OnPropertyChanged(nameof(SelectedImages));
            }
        }

        public SequenceGroupsViewModel SequenceGroups
        {
            get => sequenceGroups;
            private set
            {
                sequenceGroups = value;
                OnPropertyChanged(nameof(SequenceGroups));
            }
        }

        public int SelectedSequenceGroupIndex
        {
            get => selectedSequenceGroupIndex;
            set
            {
                selectedSequenceGroupIndex = value;
                OnPropertyChanged(nameof(SelectedSequenceGroupIndex));
            }
        }

        public int FrameIndex
        {
            get => frameIndex;
            set
            {
                frameIndex = value;
                OnPropertyChanged(nameof(FrameIndex));
            }
        }

        public MainViewModel()
        {
            Title = Utilities.GetApplicationName();
            Drawing = new DrawingDirect3D();
        }

        private void OpenFile(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                if (!Bar.IsValid(stream))
                    throw new InvalidDataException("Not a bar file");

                OpenBarContent(Bar.Open(stream));
            }
        }

        private void OpenBarContent(List<Bar.Entry> entries)
        {
            var layout = entries.Where(x => x.Type == Bar.EntryType.Layout).Select(x => Layout.Read(x.Stream)).First();
            var images = entries.Where(x => x.Type == Bar.EntryType.Imgz).Select(x => Imgz.Open(x.Stream)).First();
            OpenLayout(layout, images);
        }

        private void OpenLayout(Layout layout, IEnumerable<Imgd> images)
        {
            SequenceGroups = new SequenceGroupsViewModel(layout);
            SelectedLayout = layout;
            SelectedImages = images;
        }
    }
}

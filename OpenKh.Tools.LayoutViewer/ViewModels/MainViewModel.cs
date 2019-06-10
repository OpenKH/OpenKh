using kh.tools.common;
using OpenKh.Kh2;
using OpenKh.Tools.LayoutViewer.Models;
using OpenKh.Tools.LayoutViewer.Views;
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
        private int _targetFramesPerSecond;
        private bool _isSequencePlaying;

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

        public bool IsSequencePlaying
        {
            get => _isSequencePlaying;
            set
            {
                _isSequencePlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(ActualFramesPerSecond));
            }
        }

        public int TargetFramesPerSecond
        {
            get => _targetFramesPerSecond;
            set
            {
                _targetFramesPerSecond = value;
                OnPropertyChanged();
            }
        }

        public int ActualFramesPerSecond => IsSequencePlaying ? _targetFramesPerSecond : 0;

        public MainViewModel()
        {
            Title = Utilities.GetApplicationName();
            Drawing = new DrawingDirect3D();
            _isSequencePlaying = true;
            _targetFramesPerSecond = 60;
        }

        public void OpenFile(string fileName, bool doNotShowLayoutSelectionDialog = false)
        {
            using (var stream = File.OpenRead(fileName))
            {
                if (!Bar.IsValid(stream))
                    throw new InvalidDataException("Not a bar file");

                OpenBarContent(Bar.Open(stream), doNotShowLayoutSelectionDialog);
            }
        }

        private void OpenBarContent(List<Bar.Entry> entries, bool doNotShowLayoutSelectionDialog = false)
        {
            LayoutEntryModel layoutEntryModel;
            
            if (!doNotShowLayoutSelectionDialog && entries.Count(x => x.Type == Bar.EntryType.Layout) > 1)
            {
                var vm = new LayoutSelectionViewModel(entries);
                var dialog = new LayoutSelectionDialog()
                {
                    DataContext = vm
                };

                layoutEntryModel = dialog.ShowDialog() == true ? vm.SelectedLayoutEntry : null;
            }
            else
            {
                var barLayoutEntry = entries.FirstOrDefault(x => x.Type == Bar.EntryType.Layout);
                var layout = barLayoutEntry != null ? Layout.Read(barLayoutEntry.Stream) : null;
                var images = entries.Where(x => x.Type == Bar.EntryType.Imgz).Select(x => Imgz.Open(x.Stream)).First();

                layoutEntryModel = new LayoutEntryModel
                {
                    Name = barLayoutEntry?.Name,
                    Layout = layout,
                    Images = images
                };
            }

            if (layoutEntryModel != null)
                OpenLayout(layoutEntryModel);
        }

        private void OpenLayout(LayoutEntryModel layoutEntryModel)
        {
            SequenceGroups = new SequenceGroupsViewModel(layoutEntryModel.Layout);
            SelectedLayout = layoutEntryModel.Layout;
            SelectedImages = layoutEntryModel.Images;
        }
    }
}

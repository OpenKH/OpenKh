using OpenKh.Kh2;
using OpenKh.Tools.LayoutViewer.Interfaces;
using OpenKh.Tools.LayoutViewer.Service;
using System.Collections.Generic;
using System.Windows;
using Xe.Drawing;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class LayoutEditorViewModel : BaseNotifyPropertyChanged
    {
        private Layout _layout;
        private IEnumerable<Imgd> _images;
        private int _sequenceIndex;
        private int _frameIndex;
        private bool _isPlaying;
        private SequenceGroupsViewModel _sequenceGroups;
        private readonly IElementNames _elementNames;

        public IDrawing Drawing { get; }

        public EditorDebugRenderingService EditorDebugRenderingService { get; }

        public Layout Layout
        {
            get => _layout;
            set
            {
                _layout = value;
                OnPropertyChanged();
            }
        }

        public IEnumerable<Imgd> Images
        {
            get => _images;
            set
            {
                _images = value;
                OnPropertyChanged();
            }
        }

        public int SequenceIndex
        {
            get => _sequenceIndex;
            set
            {
                _sequenceIndex = value;
                OnPropertyChanged();
            }
        }

        public int FrameIndex
        {
            get => _frameIndex;
            set
            {
                _frameIndex = value;
                OnPropertyChanged();
            }
        }

        public bool IsPlaying
        {
            get => _isPlaying;
            set
            {
                _isPlaying = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(TimelinePlayVisibility));
                OnPropertyChanged(nameof(TimelinePauseVisibility));
            }
        }

        public string LayoutName { get => _elementNames.LayoutName; set => _elementNames.LayoutName = value; }
        public string ImagesName { get => _elementNames.ImagesName; set => _elementNames.ImagesName = value; }

        public SequenceGroupsViewModel SequenceGroups
        {
            get => _sequenceGroups;
            set
            {
                _sequenceGroups = value;
                OnPropertyChanged();
            }
        }

        public RelayCommand TimelinePlayCommand { get; set; }
        public RelayCommand TimelinePauseCommand { get; set; }
        public RelayCommand TimelineRestartCommand { get; set; }

        public Visibility TimelinePlayVisibility => IsPlaying ? Visibility.Collapsed : Visibility.Visible;
        public Visibility TimelinePauseVisibility => IsPlaying ? Visibility.Visible : Visibility.Collapsed;

        public LayoutEditorViewModel(
            IElementNames elementNames,
            EditorDebugRenderingService editorDebugRenderingService)
        {
            _elementNames = elementNames;
            Drawing = new DrawingDirect3D();
            EditorDebugRenderingService = editorDebugRenderingService;
            IsPlaying = true;

            TimelinePlayCommand = new RelayCommand(x =>
            {
                IsPlaying = true;
            }, x => true);

            TimelinePauseCommand = new RelayCommand(x =>
            {
                IsPlaying = false;
            }, x => true);

            TimelineRestartCommand = new RelayCommand(x =>
            {
                FrameIndex = 0;
            }, x => true);
        }
    }
}

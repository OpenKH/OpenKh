using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using OpenKh.Tools.Common.Rendering;
using OpenKh.Tools.LayoutViewer.Interfaces;
using OpenKh.Tools.LayoutViewer.Service;
using System.Collections.Generic;
using System.Windows;
using Xe.Drawing;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class LayoutEditorViewModel : BaseNotifyPropertyChanged, ISaveBar
    {
        private Layout _layout;
        private IEnumerable<Imgd> _images;
        private int _sequenceIndex;
        private int _frameIndex;
        private bool _isPlaying;
        private SequenceGroupsViewModel _sequenceGroups;
        private readonly IElementNames _elementNames;
        private readonly IEditorSettings _editorSettings;

        public ISpriteDrawing Drawing { get; }

        public EditorDebugRenderingService EditorDebugRenderingService { get; }

        public System.Windows.Media.Color Background => _editorSettings.EditorBackground;

        public Layout Layout
        {
            get => _layout;
            set
            {
                _layout = value;
                OnPropertyChanged();
                OnPropertyChanged(nameof(MaxFramesCount));
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
                OnPropertyChanged(nameof(MaxFramesCount));
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

        public int MaxFramesCount => SequenceIndex >= 0 ? Layout?.GetFrameLengthFromSequenceGroup(SequenceIndex) ?? 0 : 0;

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

        public string LayoutName { get => _elementNames.AnimationName; set => _elementNames.AnimationName = value; }
        public string ImagesName { get => _elementNames.SpriteName; set => _elementNames.SpriteName = value; }

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
            IEditorSettings editorSettings,
            EditorDebugRenderingService editorDebugRenderingService)
        {
            _elementNames = elementNames;
            _editorSettings = editorSettings;
            Drawing = new SpriteDrawingDirect3D();
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

        public IEnumerable<Bar.Entry> Save(IEnumerable<Bar.Entry> barEntries) => barEntries
            .ForEntry(Bar.EntryType.Layout, LayoutName, 0, entry => Layout.Write(entry.Stream))
            .ForEntry(Bar.EntryType.Imgz, ImagesName, 0, entry => Imgz.Write(entry.Stream, Images));
    }
}

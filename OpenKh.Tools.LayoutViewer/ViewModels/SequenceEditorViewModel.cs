using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using OpenKh.Tools.Common;
using OpenKh.Tools.Common.Rendering;
using OpenKh.Tools.LayoutViewer.Interfaces;
using OpenKh.Tools.LayoutViewer.Service;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using Xe.Tools;
using Xe.Tools.Wpf.Commands;
using Xe.Tools.Wpf.Models;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class SequenceEditorViewModel : BaseNotifyPropertyChanged
    {
        private class AnimationGroupEntryModel
        {
            public AnimationGroupEntryModel(int index, Sequence.AnimationGroup animationGroup)
            {
                Index = index;
                AnimationGroup = animationGroup;
            }

            public int Index { get; }
            public Sequence.AnimationGroup AnimationGroup { get; }

            public override string ToString() => $"Animation group {Index}";
        }

        private class AnimationGroupListModel : GenericListModel<AnimationGroupEntryModel>
        {
            public AnimationGroupListModel(Sequence sequence) :
                this(sequence.AnimationGroups.Select((x, i) => new AnimationGroupEntryModel(i, x)))
            {
            }

            public AnimationGroupListModel(IEnumerable<AnimationGroupEntryModel> list) :
                base(list)
            {
            }

            protected override AnimationGroupEntryModel OnNewItem()
            {
                throw new System.NotImplementedException();
            }
        }

        private readonly IElementNames _elementNames;
        private readonly IEditorSettings _editorSettings;
        private int _frameIndex;
        private Sequence selectedSequence;
        private AnimationGroupListModel animationGroupList;
        private int selectedAnimationGroupIndex;
        private SpriteViewModel selectedSprite;

        public ISpriteDrawing Drawing { get; }
        public EditorDebugRenderingService EditorDebugRenderingService { get; }
        public System.Windows.Media.Color Background => _editorSettings.EditorBackground;

        public object AnimationGroupList
        {
            get => animationGroupList;
            private set
            {
                animationGroupList = value as AnimationGroupListModel;
                OnPropertyChanged();
            }
        }

        public Sequence SelectedSequence
        {
            get => selectedSequence;
            set
            {
                selectedSequence = value;
                AnimationGroupList = new AnimationGroupListModel(value);
            }
        }

        public Imgd SelectedImage { get; set; }

        public int SelectedAnimationGroupIndex
        {
            get => selectedAnimationGroupIndex;
            set
            {
                selectedAnimationGroupIndex = value;
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

        public int MaxFramesCount => SelectedAnimationGroupIndex >= 0 ?
            SelectedSequence.GetFrameLengthFromAnimationGroup(SelectedAnimationGroupIndex) : 0;

        public GenericListModel<SpriteViewModel> Sprites { get; }
        public SpriteViewModel SelectedSprite
        {
            get => selectedSprite;
            set
            {
                selectedSprite = value;
                OnPropertyChanged(nameof(AddSpriteCommand));
                OnPropertyChanged(nameof(RemoveSpriteCommand));
                OnPropertyChanged(nameof(DuplicateSpriteCommand));
                OnPropertyChanged(nameof(EditSpriteCommand));
                OnPropertyChanged(nameof(MoveUpCommand));
                OnPropertyChanged(nameof(MoveDownCommand));
            }
        }
        public int SelectedSpriteIndex { get; set; }

        public RelayCommand AddSpriteCommand { get; }
        public RelayCommand RemoveSpriteCommand { get; }
        public RelayCommand DuplicateSpriteCommand { get; }
        public RelayCommand EditSpriteCommand { get; }
        public RelayCommand MoveUpCommand { get; }
        public RelayCommand MoveDownCommand { get; }

        public SequenceEditorViewModel(
            Sequence sequence,
            Imgd texture,
            IElementNames elementNames,
            IEditorSettings editorSettings,
            EditorDebugRenderingService editorDebugRenderingService)
        {
            Drawing = new SpriteDrawingDirect3D();

            SelectedSequence = sequence;
            SelectedImage = texture;
            _elementNames = elementNames;
            _editorSettings = editorSettings;
            EditorDebugRenderingService = editorDebugRenderingService;

            Sprites = new GenericListModel<SpriteViewModel>(SelectedSequence.Frames.Select(x => new SpriteViewModel(x, texture)));
            AddSpriteCommand = new RelayCommand(_ =>
            {
                SelectedSequence.Frames.Add(new Sequence.Frame());
            }, x => SelectedSequence != null);
            RemoveSpriteCommand = new RelayCommand(_ =>
            {
                var spriteIndexReference = SelectedSequence.FramesEx
                    .Where(x => x.FrameIndex == SelectedSpriteIndex)
                    .Select((_, i) => i)
                    .ToList();
                if (spriteIndexReference.Count > 0)
                {
                    var userAction = MessageBox.Show(
$@"The sprite {SelectedSpriteIndex} you want to remove is referenced by {spriteIndexReference.Count} sprite part:
[{string.Join(", ", spriteIndexReference)}]
By deleting it, all the sprite part will be forced to reference the very first sprite to avoid corruption, still the graphics will look wrong.

Are you sure to delete the sprite?",
                        "Remove sprite",
                        MessageBoxButton.YesNo,
                        MessageBoxImage.Warning);

                    if (userAction != MessageBoxResult.Yes)
                        return;
                }

                RemoveSprite(SelectedSpriteIndex);
            }, x => SelectedSequence != null);
            DuplicateSpriteCommand = new RelayCommand(_ =>
            {
                var sprite = SelectedSprite;
                SelectedSequence.Frames.Add(new Sequence.Frame
                {
                    Unknown00 = sprite.Unknown,
                    Left = sprite.Left,
                    Top = sprite.Top,
                    Right = sprite.Right,
                    Bottom = sprite.Bottom,
                    UTranslation = sprite.UTranslation,
                    VTranslation = sprite.VTranslation,
                    ColorLeft = sprite.Sprite.ColorLeft,
                    ColorTop = sprite.Sprite.ColorTop,
                    ColorRight = sprite.Sprite.ColorRight,
                    ColorBottom = sprite.Sprite.ColorBottom,
                });
            }, x => SelectedSequence != null);
            EditSpriteCommand = new RelayCommand(_ =>
            {

            }, x => SelectedSequence != null && false);
            MoveUpCommand = new RelayCommand(_ =>
            {
                SwapSprite(SelectedSpriteIndex, SelectedSpriteIndex - 1);
                SelectedSpriteIndex--;
            }, x => SelectedSequence != null && SelectedSpriteIndex > 0);
            MoveDownCommand = new RelayCommand(_ =>
            {
                SwapSprite(SelectedSpriteIndex, SelectedSpriteIndex + 1);
                SelectedSpriteIndex++;
            }, x => SelectedSequence != null && SelectedSpriteIndex < Sprites.Items.Count - 1);
        }

        private void RemoveSprite(int selectedSpriteIndex)
        {
            foreach (var spritePart in SelectedSequence.FramesEx)
            {
                if (spritePart.FrameIndex >= selectedSpriteIndex)
                {
                    if (spritePart.FrameIndex > selectedSpriteIndex)
                        spritePart.FrameIndex--;
                    else
                        spritePart.FrameIndex = 0;
                }
            }

            Sprites.Items.RemoveAt(SelectedSpriteIndex);
            SelectedSequence.Frames.RemoveAt(selectedSpriteIndex);
        }

        private void SwapSprite(int i, int j)
        {
            //foreach (var spritePart in SelectedSequence.FramesEx)
            //{
            //    if (spritePart.FrameIndex == i)
            //        spritePart.FrameIndex = j;
            //    else if (spritePart.FrameIndex == j)
            //        spritePart.FrameIndex = i;
            //}

            var a = Sprites.Items[i].Sprite;
            Sprites.Items[i].Sprite = Sprites.Items[j].Sprite;
            Sprites.Items[j].Sprite = a;

        }
    }
}

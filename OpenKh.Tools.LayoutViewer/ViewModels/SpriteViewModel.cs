using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Tools.Common.Rendering;
using System;
using System.Collections.Generic;
using System.Windows.Media;
using Xe.Tools;

namespace OpenKh.Tools.LayoutViewer.ViewModels
{
    public class SpriteViewModel : BaseNotifyPropertyChanged
    {
        public SpriteViewModel(Sequence.Sprite sprite, Imgd image)
        {
            Drawing = new SpriteDrawingDirect3D();
            Sprite = sprite;
            Image = image;
        }

        public ISpriteDrawing Drawing { get; }
        public Sequence.Sprite Sprite { get; set; }
        public Imgd Image { get; }
        public Sequence Sequence => MockSequence();

        public int Unknown
        {
            get => Sprite.Unknown00;
            set => Sprite.Unknown00 = value;
        }

        public int Left
        {
            get => Sprite.Left;
            set
            {
                Sprite.Left = value;
                OnPropertyChanged(nameof(Sequence));
            }
        }
        public int Top
        {
            get => Sprite.Top;
            set
            {
                Sprite.Top = value;
                OnPropertyChanged(nameof(Sequence));
            }
        }
        public int Right
        {
            get => Sprite.Right;
            set
            {
                Sprite.Right = value;
                OnPropertyChanged(nameof(Sequence));
            }
        }
        public int Bottom
        {
            get => Sprite.Bottom;
            set
            {
                Sprite.Bottom = value;
                OnPropertyChanged(nameof(Sequence));
            }
        }

        public float UTranslation
        {
            get => Sprite.UTranslation;
            set => Sprite.UTranslation = value;
        }
        public float VTranslation
        {
            get => Sprite.VTranslation;
            set => Sprite.VTranslation = value;
        }

        public Color Color0
        {
            get => Convert(Sprite.ColorLeft);
            set => Sprite.ColorLeft = Convert(value);
        }
        public Color Color1
        {
            get => Convert(Sprite.ColorTop);
            set => Sprite.ColorTop = Convert(value);
        }
        public Color Color2
        {
            get => Convert(Sprite.ColorRight);
            set => Sprite.ColorRight = Convert(value);
        }
        public Color Color3
        {
            get => Convert(Sprite.ColorBottom);
            set => Sprite.ColorBottom = Convert(value);
        }

        private Sequence MockSequence() => new Sequence
        {
            AnimationGroups = new List<Sequence.AnimationGroup>
            {
                new Sequence.AnimationGroup
                {
                    Animations = new List<Sequence.Animation>
                    {
                        new Sequence.Animation
                        {
                            SpriteGroupIndex = 0,
                            FrameStart = 0,
                            FrameEnd = 10,
                            ScaleStart = 1,
                            ScaleEnd = 1,
                            ScaleXStart = 1,
                            ScaleXEnd = 1,
                            ScaleYStart = 1,
                            ScaleYEnd = 1,
                            ColorStart = 0x80808080U,
                            ColorEnd = 0x80808080U,
                        }
                    },
                    LoopStart = 0,
                    LoopEnd = 10,
                }
            },
            SpriteGroups = new List<List<Sequence.SpritePart>>
                {
                    new List<Sequence.SpritePart>
                    {
                        new Sequence.SpritePart
                        {
                            Left = 0,
                            Top = 0,
                            Right = Math.Max(Left, Right) - Math.Min(Left, Right),
                            Bottom = Math.Max(Top, Bottom) - Math.Min(Top, Bottom),
                            SpriteIndex = 0
                        }
                    }
                },
            Sprites = new List<Sequence.Sprite>
            {
                Sprite
            }
        };

        private static Color Convert(uint color) => Color.FromScRgb(
            ((color >> 24) & 0xFF) / 128.0f,
            ((color >> 0) & 0xFF) / 128.0f,
            ((color >> 8) & 0xFF) / 128.0f,
            ((color >> 16) & 0xFF) / 128.0f);

        private static uint Convert(Color color) =>
            ((uint)(color.ScR * 128.0f) >> 0) |
            ((uint)(color.ScG * 128.0f) >> 8) |
            ((uint)(color.ScB * 128.0f) >> 16) |
            ((uint)(color.ScA * 128.0f) >> 24);
    }
}

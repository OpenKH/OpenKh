using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Tools.LayoutEditor.Interfaces;
using System;
using System.Collections.Generic;

namespace OpenKh.Tools.LayoutEditor.Models
{
    public class SpriteModel : IDisposable
    {
        private readonly ISpriteDrawing _drawing;
        private readonly Sequence _sequence;
        private readonly SequenceRenderer _renderer;
        private readonly ITextureBinder _textureBinder;
        private readonly IEditorSettings _settings;
        private ISpriteTexture _spriteTexture;
        private int _frameIndex;

        public Sequence.Sprite Sprite { get; }
        public IntPtr TextureId { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public SpriteModel(
            Sequence.Sprite sprite,
            ISpriteDrawing drawing,
            ISpriteTexture atlasTexture,
            ITextureBinder textureBinder,
            IEditorSettings settings)
        {
            Sprite = sprite;

            _drawing = drawing;
            _sequence = MockSequence();
            _renderer = new SequenceRenderer(_sequence, drawing, atlasTexture);
            _textureBinder = textureBinder;
            _settings = settings;
            _settings.OnChangeBackground += (o, e) => Draw(0, 0);

            SizeChanged();
        }

        public IntPtr Draw(float x, float y)
        {
            _drawing.SetViewport(0, Width, 0, Height);
            _drawing.DestinationTexture = _spriteTexture;
            _drawing.Clear(_settings.EditorBackground);
            _renderer.Draw(0, _frameIndex++, x, y);
            _drawing.Flush();
            _drawing.DestinationTexture = null;

            return TextureId;
        }

        public void SizeChanged()
        {
            var newWidth = Math.Max(1, Math.Max(Sprite.Left, Sprite.Right) -
                Math.Min(Sprite.Left, Sprite.Right));
            var newHeight = Math.Max(1, Math.Max(Sprite.Top, Sprite.Bottom) -
                Math.Max(1, Math.Min(Sprite.Top, Sprite.Bottom)));
            if (newWidth == Width && newHeight == Height)
                return;

            if (TextureId != IntPtr.Zero)
                _textureBinder.UnbindTexture(TextureId);
            _spriteTexture?.Dispose();

            Width = newWidth;
            Height = newHeight;
            _sequence.SpriteGroups[0][0].Right = Width;
            _sequence.SpriteGroups[0][0].Bottom = Height;
            _spriteTexture = _drawing.CreateSpriteTexture(Width, Height);
            TextureId = _textureBinder.BindTexture(_spriteTexture);

            Draw(0, 0);
        }

        public void Dispose()
        {
            if (TextureId != IntPtr.Zero)
                _textureBinder.UnbindTexture(TextureId);
            _spriteTexture?.Dispose();
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
                            Right = Width,
                            Bottom = Height,
                            SpriteIndex = 0
                        }
                    }
                },
            Sprites = new List<Sequence.Sprite>
                {
                    Sprite
                }
        };
    }
}

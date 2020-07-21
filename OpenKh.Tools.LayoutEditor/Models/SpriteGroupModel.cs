using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using OpenKh.Tools.LayoutEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.LayoutEditor.Models
{
    public class SpriteGroupModel : IDisposable
    {
        private readonly ISpriteDrawing _drawing;
        private readonly ITextureBinder _textureBinder;
        private readonly IEditorSettings _settings;
        private readonly Sequence _sequence;
        private readonly Sequence _mockSequence;
        private readonly SequenceRenderer _renderer;
        private int _spriteGroupIndex;
        private int _frameIndex;
        private ISpriteTexture _spriteTexture;

        public List<Sequence.SpritePart> SpriteGroup => _sequence.SpriteGroups[_spriteGroupIndex];
        public IntPtr TextureId { get; set; }
        public int Width { get; private set; }
        public int Height { get; private set; }

        public SpriteGroupModel(
            Sequence sequence,
            int spriteGroupIndex,
            ISpriteDrawing drawing,
            ISpriteTexture atlasTexture,
            ITextureBinder textureBinder,
            IEditorSettings settings)
        {
            _drawing = drawing;
            _textureBinder = textureBinder;
            _settings = settings;
            _sequence = sequence;
            _spriteGroupIndex = spriteGroupIndex;
            _mockSequence = MockSequence();
            _renderer = new SequenceRenderer(_mockSequence, drawing, atlasTexture);

            _settings.OnChangeBackground += (o, e) => Draw(0, 0);
            SizeChanged();
        }

        public IntPtr Draw(float x, float y, Action<ISpriteDrawing> pre = null, Action<ISpriteDrawing> post = null)
        {
            _drawing.SetViewport(0, Width, 0, Height);
            _drawing.DestinationTexture = _spriteTexture;
            _drawing.Clear(_settings.EditorBackground);
            pre?.Invoke(_drawing);

            var posX = -SpriteGroup.Min(x => Math.Min(x.Left, x.Right));
            var posY = -SpriteGroup.Min(x => Math.Min(x.Top, x.Bottom));
            _renderer.Draw(0, _frameIndex++, posX + x, posY + y);
            _drawing.Flush();

            post?.Invoke(_drawing);
            _drawing.DestinationTexture = null;

            return TextureId;
        }

        public void SizeChanged()
        {
            var rect = SpriteGroup.GetVisibilityRectangleForFrameGroup();
            if (rect.Width == Width && rect.Height == Height)
                return;

            Width = rect.Width;
            Height = rect.Height;

            _spriteTexture?.Dispose();
            _spriteTexture = _drawing.CreateSpriteTexture(Width, Height);

            if (TextureId == IntPtr.Zero)
                TextureId = _textureBinder.BindTexture(_spriteTexture);
            else
                _textureBinder.RebindTexture(TextureId, _spriteTexture);

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
                                SpriteGroupIndex = _spriteGroupIndex,
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
            SpriteGroups = _sequence.SpriteGroups,
            Sprites = _sequence.Sprites
        };
    }
}

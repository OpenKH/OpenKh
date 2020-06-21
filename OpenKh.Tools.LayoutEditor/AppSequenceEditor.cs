using ImGuiNET;
using OpenKh.Kh2;
using OpenKh.Tools.LayoutEditor.Interfaces;
using System.Numerics;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Tools.Common.CustomImGui;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Renders;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using OpenKh.Engine.Renderers;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Tools.LayoutEditor
{
    public class AppSequenceEditor : IApp, IDisposable
    {
        private class SpriteProperty
        {
            private readonly ISpriteDrawing _drawing;
            private readonly Sequence _sequence;
            private readonly SequenceRenderer _renderer;
            private int _frameIndex;

            public Sequence.Frame Sprite { get; }
            public ISpriteTexture SpriteTexture { get; set; }
            public IntPtr TextureId { get; set; }
            public int Width { get; }
            public int Height { get; }

            public SpriteProperty(
                Sequence.Frame sprite,
                ISpriteDrawing drawing,
                ISpriteTexture texture)
            {
                Sprite = sprite;
                Width = Math.Max(sprite.Left, sprite.Right) -
                    Math.Min(sprite.Left, sprite.Right);
                Height = Math.Max(sprite.Top, sprite.Bottom) -
                    Math.Min(sprite.Top, sprite.Bottom);

                SpriteTexture = drawing.CreateSpriteTexture(Width, Height);
                _drawing = drawing;
                _sequence = MockSequence();
                _renderer = new SequenceRenderer(_sequence, drawing, texture);
            }

            public IntPtr Draw(float x, float y)
            {
                _drawing.SetViewport(0, Width, 0, Height);
                _drawing.DestinationTexture = SpriteTexture;
                _drawing.Clear(new ColorF(1, 0, 1, 1));
                _renderer.Draw(0, _frameIndex++, x, y);
                _drawing.Flush();
                _drawing.DestinationTexture = null;

                return TextureId;
            }
            
            private Sequence MockSequence() => new Sequence
            {
                AnimationGroups = new List<Sequence.AnimationGroup>
                {
                    new Sequence.AnimationGroup
                    {
                        AnimationIndex = 0,
                        Count = 1,
                        LoopStart = 0,
                        LoopEnd = 10,
                    }
                },
                Animations = new List<Sequence.Animation>
                {
                    new Sequence.Animation
                    {
                        FrameGroupIndex = 0,
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
                FrameGroups = new List<Sequence.FrameGroup>
                {
                    new Sequence.FrameGroup
                    {
                        Start = 0,
                        Count = 1,
                    }
                },
                FramesEx = new List<Sequence.FrameEx>
                {
                    new Sequence.FrameEx
                    {
                        Left = 0,
                        Top = 0,
                        Right = Width,
                        Bottom = Height,
                        FrameIndex = 0
                    }
                },
                Frames = new List<Sequence.Frame>
                {
                    Sprite
                }
            };
        }

        private readonly Sequence _sequence;
        private readonly Imgd _image;
        private readonly MonoGameImGuiBootstrap _bootStrap;
        private readonly GraphicsDevice _graphics;
        private readonly KingdomShader _shader;
        private readonly MonoSpriteDrawing _drawing;
        private readonly ISpriteTexture _texture;
        private readonly IntPtr _textureId;
        private readonly SequenceRenderer _renderer;

        private int _selectedSprite = 0;
        private ISpriteTexture _destinationTexture;
        private IntPtr _destinationTextureId;
        private List<SpriteProperty> _sprites;

        public AppSequenceEditor(MonoGameImGuiBootstrap bootstrap, Sequence sequence, Imgd image)
        {
            _sequence = sequence;
            _image = image;

            _bootStrap = bootstrap;
            _graphics = bootstrap.GraphicsDevice;
            _shader = new KingdomShader(bootstrap.Content);
            _drawing = new MonoSpriteDrawing(_graphics, _shader);
            _texture = _drawing.CreateSpriteTexture(_image);
            _renderer = new SequenceRenderer(_sequence, _drawing, _texture);

            _destinationTexture = _drawing.CreateSpriteTexture(1024, 1024);
            _textureId = BindTexture(_texture);
            _destinationTextureId = BindTexture(_destinationTexture);

            _sprites = sequence.Frames
                .Select(x => AsSpriteProperty(x))
                .ToList();
        }

        public bool Run()
        {
            ForGrid(
                new GridElement("SpriteList", 1, 256, true, DrawSpriteList),
                new GridElement("Animation", 1, 0, false, DrawAnimation),
                new GridElement("Right", 1, 256, true, DrawRight));

            return true;
        }

        private void DrawSpriteList()
        {
            for (int i = 0; i < _sprites.Count; i++)
            {
                var sprite = _sprites[i];
                if (ImGui.Selectable($"##sprite{i}",
                    _selectedSprite == i,
                    ImGuiSelectableFlags.None,
                    new Vector2(0, sprite.Height)))
                    _selectedSprite = i;

                ImGui.SameLine();
                ImGui.Image(sprite.TextureId, new Vector2(sprite.Width, sprite.Height));
            }
        }

        private unsafe void DrawAnimation()
        {
            var width = ImGui.GetWindowContentRegionWidth();
            var height = ImGui.GetWindowHeight();

            _drawing.DestinationTexture = _destinationTexture;
            _drawing.Clear(new ColorF(1, 0, 1, 1));
            _drawing.SetViewport(0, 1024, 0, 1024);
            _renderer.Draw(1, 10, width / 2.0f, height / 2.0f);
            _drawing.Flush();
            _drawing.DestinationTexture = null;

            ImGui.Image(_destinationTextureId, new Vector2(1024, 1024),
                GetUv(_texture, 0, 0), new Vector2(1, 1));
        }

        private void DrawRight()
        {
            ImGui.Text("Right");
            ImGui.Text("Right");
            ImGui.Text("Right");
            ImGui.Text("Right");
            ImGui.Text("Right");
            ImGui.Text("Right");
            ImGui.Text("Right");
        }

        public void Dispose()
        {
            foreach (var sprite in _sprites)
            {
                _bootStrap.UnbindTexture(sprite.TextureId);
                sprite.SpriteTexture.Dispose();
            }

            _bootStrap.UnbindTexture(_destinationTextureId);
            _destinationTexture.Dispose();
        }

        private SpriteProperty AsSpriteProperty(Sequence.Frame sprite)
        {
            var spriteProperty = new SpriteProperty(sprite, _drawing, _texture);
            spriteProperty.TextureId = BindTexture(spriteProperty.SpriteTexture);
            spriteProperty.Draw(0, 0);

            return spriteProperty;
        }

        private IntPtr BindTexture(ISpriteTexture sprite) => _bootStrap.BindTexture(
            (sprite as MonoSpriteDrawing.CSpriteTexture).Texture);

        private static Vector2 GetUv(ISpriteTexture texture, int x, int y) =>
            new Vector2((float)x / texture.Width, (float)y / texture.Height);
    }
}

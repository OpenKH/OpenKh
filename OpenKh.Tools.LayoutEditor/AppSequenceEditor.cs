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
using System.Windows.Controls;

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
        private int _selectedAnimGroup = 1;
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
                new GridElement("Animation", 2, 0, false, DrawAnimation),
                new GridElement("Right", 1.5f, 256, true, DrawRight));

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
            _renderer.Draw(_selectedAnimGroup, 0, width / 2.0f, height / 2.0f);
            _drawing.Flush();
            _drawing.DestinationTexture = null;

            ImGui.Image(_destinationTextureId, new Vector2(1024, 1024),
                GetUv(_texture, 0, 0), new Vector2(1, 1));
        }

        private void DrawRight()
        {
            var animationGroup = _sequence.AnimationGroups[_selectedAnimGroup];
            AnimationGroupEdit(animationGroup);

            for (var i = 0; i < animationGroup.Count; i++)
            {
                if (ImGui.CollapsingHeader($"Animation {i + 1}"))
                {
                    AnimationEdit(_sequence.Animations[animationGroup.AnimationIndex + i]);
                }
            }
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

        private void AnimationGroupEdit(Sequence.AnimationGroup animationGroup)
        {
            var doNotLoop = animationGroup.DoNotLoop != 0;
            if (ImGui.Checkbox("Do not loop", ref doNotLoop))
                animationGroup.DoNotLoop = (short)(doNotLoop ? 1 : 0);

            int unknown06 = animationGroup.Unknown06;
            if (ImGui.InputInt("Unknown06", ref unknown06))
                animationGroup.Unknown06 = (short)unknown06;

            var loopPair = new int[] { animationGroup.LoopStart, animationGroup.LoopEnd };
            if (ImGui.InputInt2("Loop frame", ref loopPair[0]))
            {
                animationGroup.LoopStart = loopPair[0];
                animationGroup.LoopEnd = loopPair[1];
            }

            int unknown10 = animationGroup.Unknown10;
            if (ImGui.InputInt("Unknown10", ref unknown10))
                animationGroup.Unknown10 = unknown10;

            int unknown14 = animationGroup.Unknown14;
            if (ImGui.InputInt("Unknown14", ref unknown14))
                animationGroup.Unknown14 = unknown14;

            int unknown18 = animationGroup.Unknown18;
            if (ImGui.InputInt("Unknown18", ref unknown18))
                animationGroup.Unknown18 = unknown18;

            int unknown1c = animationGroup.Unknown1C;
            if (ImGui.InputInt("Unknown1c", ref unknown1c))
                animationGroup.Unknown1C = unknown1c;

            int unknown20 = animationGroup.Unknown20;
            if (ImGui.InputInt("Unknown20", ref unknown20))
                animationGroup.Unknown20 = unknown20;
        }

        private void AnimationEdit(Sequence.Animation animation)
        {
            int flags = animation.Flags;
            if (ImGui.InputInt("Flags", ref flags))
                animation.Flags = flags;

            var framePair = new int[] { animation.FrameStart, animation.FrameEnd };
            if (ImGui.DragInt2("Frame lifespan", ref framePair[0]))
            {
                animation.FrameStart = framePair[0];
                animation.FrameEnd = framePair[1];
            }

            var xaPair = new int[] { animation.Xa0, animation.Xa1 };
            if (ImGui.DragInt2("Pivot X", ref xaPair[0]))
            {
                animation.Xa0 = xaPair[0];
                animation.Xa1 = xaPair[1];
            }

            var yaPair = new int[] { animation.Ya0, animation.Ya1 };
            if (ImGui.DragInt2("Pivot Y", ref yaPair[0]))
            {
                animation.Ya0 = yaPair[0];
                animation.Ya1 = yaPair[1];
            }

            var xbPair = new int[] { animation.Xb0, animation.Xb1 };
            if (ImGui.DragInt2("Translation X", ref xbPair[0]))
            {
                animation.Xb1 = xbPair[0];
                animation.Xb1 = xbPair[1];
            }

            var ybPair = new int[] { animation.Yb0, animation.Yb1 };
            if (ImGui.DragInt2("Translation Y", ref ybPair[0]))
            {
                animation.Yb0 = ybPair[0];
                animation.Yb1 = ybPair[1];
            }

            var unk3xPair = new int[] {
                animation.Unknown30, animation.Unknown34, animation.Unknown38, animation.Unknown3c
            };
            if (ImGui.DragInt4("Unknown3x", ref unk3xPair[0]))
            {
                animation.Unknown30 = unk3xPair[0];
                animation.Unknown34 = unk3xPair[1];
                animation.Unknown38 = unk3xPair[2];
                animation.Unknown3c = unk3xPair[3];
            }

            var rotationPair = new Vector2(animation.RotationStart, animation.RotationEnd);
            if (ImGui.DragFloat2("Rotation", ref rotationPair))
            {
                animation.RotationStart = rotationPair.X;
                animation.RotationEnd = rotationPair.Y;
            }

            var scalePair = new Vector2(animation.ScaleStart, animation.ScaleEnd);
            if (ImGui.DragFloat2("Scale", ref scalePair, 0.05f))
            {
                animation.ScaleStart = scalePair.X;
                animation.ScaleEnd = scalePair.Y;
            }

            var scaleXPair = new Vector2(animation.ScaleXStart, animation.ScaleXEnd);
            if (ImGui.DragFloat2("Scale X", ref scaleXPair, 0.05f))
            {
                animation.ScaleXStart = scaleXPair.X;
                animation.ScaleXEnd = scaleXPair.Y;
            }

            var scaleYPair = new Vector2(animation.ScaleYStart, animation.ScaleYEnd);
            if (ImGui.DragFloat2("Scale Y", ref scaleYPair, 0.05f))
            {
                animation.ScaleYStart = scaleYPair.X;
                animation.ScaleYEnd = scaleYPair.Y;
            }

            var unk6xPair = new Vector4(
                animation.Unknown60, animation.Unknown64, animation.Unknown68, animation.Unknown6c);
            if (ImGui.DragFloat4("Unknown6x", ref unk6xPair, 0.05f))
            {
                animation.Unknown60 = unk6xPair.X;
                animation.Unknown64 = unk6xPair.Y;
                animation.Unknown68 = unk6xPair.Z;
                animation.Unknown6c = unk6xPair.W;
            }

            var bounceXPair = new int[] { animation.BounceXStart, animation.BounceXEnd };
            if (ImGui.DragInt2("Bounce X", ref bounceXPair[0]))
            {
                animation.BounceXStart = bounceXPair[0];
                animation.BounceXEnd = bounceXPair[1];
            }

            var bounceYPair = new int[] { animation.BounceYStart, animation.BounceYEnd };
            if (ImGui.DragInt2("Bounce Y", ref bounceYPair[0]))
            {
                animation.BounceYStart = bounceYPair[0];
                animation.BounceYEnd = bounceYPair[1];
            }

            int unk80 = animation.Unknwon80;
            if (ImGui.DragInt("Unknown80", ref unk80))
                animation.Unknwon80 = unk80;

            int blendMode = animation.ColorBlend;
            if (ImGui.Combo("Blend mode", ref blendMode, new string[]
                { "Normal", "Additive", "Subtractive" }, 3))
                animation.ColorBlend = blendMode;

            var colorStart = ConvertColor(animation.ColorStart);
            if (ImGui.ColorPicker4("Mask start", ref colorStart))
                animation.ColorStart = ConvertColor(colorStart);

            var colorEnd = ConvertColor(animation.ColorEnd);
            if (ImGui.ColorPicker4("Mask end", ref colorEnd))
                animation.ColorEnd = ConvertColor(colorEnd);

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

        private static Vector4 ConvertColor(uint color) => new Vector4(
            ((color >> 0) & 0xFF) / 128.0f,
            ((color >> 8) & 0xFF) / 128.0f,
            ((color >> 16) & 0xFF) / 128.0f,
            ((color >> 24) & 0xFF) / 128.0f);

        private static uint ConvertColor(Vector4 color) =>
            ((uint)(color.X * 128f) << 0) |
            ((uint)(color.Y * 128f) << 8) |
            ((uint)(color.Z * 128f) << 16) |
            ((uint)(color.W * 128f) << 24);
    }
}

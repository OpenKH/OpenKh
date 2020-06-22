using ImGuiNET;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
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
using OpenKh.Tools.LayoutEditor.Models;

namespace OpenKh.Tools.LayoutEditor
{
    public class AppSequenceEditor : IApp, ITextureBinder, IDisposable
    {
        private readonly Sequence _sequence;
        private readonly Imgd _image;
        private readonly MonoGameImGuiBootstrap _bootStrap;
        private readonly GraphicsDevice _graphics;
        private readonly KingdomShader _shader;
        private readonly MonoSpriteDrawing _drawing;
        private readonly ISpriteTexture _atlasTexture;
        private readonly SequenceRenderer _renderer;

        private bool _isFrameEditDialogOpen;
        private SpriteEditDialog _frameEditDialog;

        private int _selectedSprite = 0;
        private int _selectedAnimGroup = 1;
        private int _animationFrameCurrent;
        private int _animationFrameCount;
        private ISpriteTexture _destinationTexture;
        private IntPtr _destinationTextureId;
        private List<SpriteModel> _sprites;

        private string FrameEditDialogTitle => $"Sprite edit #{_selectedSprite}";

        public int SelectedAnimGroup
        {
            get => _selectedAnimGroup;
            set
            {
                _selectedAnimGroup = value;
                _animationFrameCurrent = 0;
                _animationFrameCount = SequenceExtensions
                    .GetFrameLengthFromAnimationGroup(_sequence, _selectedAnimGroup);
            }
        }

        public AppSequenceEditor(MonoGameImGuiBootstrap bootstrap, Sequence sequence, Imgd image)
        {
            _sequence = sequence;
            _image = image;

            _bootStrap = bootstrap;
            _graphics = bootstrap.GraphicsDevice;
            _shader = new KingdomShader(bootstrap.Content);
            _drawing = new MonoSpriteDrawing(_graphics, _shader);
            _atlasTexture = _drawing.CreateSpriteTexture(_image);
            _renderer = new SequenceRenderer(_sequence, _drawing, _atlasTexture);

            _destinationTexture = _drawing.CreateSpriteTexture(1024, 1024);
            _destinationTextureId = this.BindTexture(_destinationTexture);

            _sprites = sequence.Frames
                .Select(x => AsSpriteProperty(x))
                .ToList();
        }

        public void Menu()
        {
            ForMenu("Sprite", () =>
            {
                ForMenuItem("Edit...", () => _isFrameEditDialogOpen = true);
            });
        }

        public bool Run()
        {
            bool dummy = true;
            if (ImGui.BeginPopupModal(FrameEditDialogTitle, ref dummy,
                ImGuiWindowFlags.Popup | ImGuiWindowFlags.Modal | ImGuiWindowFlags.AlwaysAutoResize))
            {
                _frameEditDialog.Run();
                ImGui.EndPopup();
            }

            ForGrid(
                new GridElement("SpriteList", 1, 256, true, DrawSpriteList),
                new GridElement("Animation", 2, 0, false, DrawAnimation),
                new GridElement("Right", 1.5f, 256, true, DrawRight));

            if (_isFrameEditDialogOpen)
            {
                ImGui.OpenPopup(FrameEditDialogTitle);
                _isFrameEditDialogOpen = false;
                _frameEditDialog = new SpriteEditDialog(
                    _sprites[_selectedSprite],
                    _drawing,
                    _atlasTexture,
                    this);
            }

            return true;
        }

        private void DrawSpriteList()
        {
            // Animate only the selected sprite
            if (_selectedSprite >= 0)
                _sprites[_selectedSprite].Draw(0, 0);

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

        private void DrawAnimationGroupList()
        {
            for (int i = 0; i < _sequence.AnimationGroups.Count; i++)
            {
                if (ImGui.Selectable($"Animation Group {i}\n",
                    SelectedAnimGroup == i))
                    SelectedAnimGroup = i;
            }
        }

        private unsafe void DrawAnimation()
        {
            if (ImGui.BeginCombo("", $"Animation Group {SelectedAnimGroup}",
                ImGuiComboFlags.PopupAlignLeft))
            {
                DrawAnimationGroupList();
                ImGui.EndCombo();
            }
            ImGui.SameLine();
            if (ImGui.Button("-", new Vector2(30, 0)) && SelectedAnimGroup > 0)
                SelectedAnimGroup--;
            ImGui.SameLine();
            if (ImGui.Button("+", new Vector2(30, 0)) &&
                SelectedAnimGroup < _sequence.AnimationGroups.Count - 1)
                SelectedAnimGroup++;

            ImGui.SliderInt("Frame", ref _animationFrameCurrent, 0, _animationFrameCount);

            var width = ImGui.GetWindowContentRegionWidth();
            var height = ImGui.GetWindowHeight();

            _drawing.DestinationTexture = _destinationTexture;
            _drawing.Clear(new ColorF(1, 0, 1, 1));
            _drawing.SetViewport(0, 1024, 0, 1024);
            _renderer.Draw(_selectedAnimGroup, _animationFrameCurrent++, width / 2.0f, height / 2.0f);
            _drawing.Flush();
            _drawing.DestinationTexture = null;

            ImGui.Image(_destinationTextureId, new Vector2(1024, 1024),
                GetUv(_atlasTexture, 0, 0), new Vector2(1, 1));
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
                sprite.Dispose();

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

            var colorStart = Utilities.ConvertColor(animation.ColorStart);
            if (ImGui.ColorPicker4("Mask start", ref colorStart))
                animation.ColorStart = Utilities.ConvertColor(colorStart);

            var colorEnd = Utilities.ConvertColor(animation.ColorEnd);
            if (ImGui.ColorPicker4("Mask end", ref colorEnd))
                animation.ColorEnd = Utilities.ConvertColor(colorEnd);
        }

        private SpriteModel AsSpriteProperty(Sequence.Frame sprite) =>
            new SpriteModel(sprite, _drawing, _atlasTexture, this);

        private static Vector2 GetUv(ISpriteTexture texture, int x, int y) =>
            new Vector2((float)x / texture.Width, (float)y / texture.Height);

        public IntPtr BindTexture(Texture2D texture) =>
            _bootStrap.BindTexture(texture);

        public void UnbindTexture(IntPtr id) =>
            _bootStrap.UnbindTexture(id);
    }
}

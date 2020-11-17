using ImGuiNET;
using OpenKh.Tools.LayoutEditor.Interfaces;
using System;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using System.IO;
using OpenKh.Tools.Common.CustomImGui;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework.Graphics;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renders;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Renderers;
using System.Numerics;

namespace OpenKh.Tools.LayoutEditor
{
    public class AppLayoutEditor : IApp, ISaveBar, ITextureBinder, IDisposable
    {
        private const string SequenceEditorDialogName = "Sequence editor";
        private readonly MonoGameImGuiBootstrap _bootStrap;
        private readonly IEditorSettings _settings;
        private readonly Layout _layout;
        private readonly List<Imgd> _images;
        private readonly GraphicsDevice _graphics;
        private readonly KingdomShader _shader;
        private readonly MonoSpriteDrawing _drawing;
        private readonly ISpriteTexture _destinationTexture;
        private readonly IntPtr _destinationTextureId;
        private readonly DefaultDebugLayoutRenderer _debugRender;
        private readonly List<ISpriteTexture> _spriteTextures;
        private readonly LayoutRenderer _renderer;

        private int _selectedLayoutIndex;
        private int _animationFrameCurrent;
        private bool _isOpeningSequenceEditor;
        private AppSequenceEditor _sequenceEditor;

        public int SelectedLayoutIndex
        {
            get => _selectedLayoutIndex;
            set
            {
                _selectedLayoutIndex = value;
                _animationFrameCurrent = 0;
                //_sequencerSelectedAnimation = 0;
                //
                //var animationGroup = _sequence.AnimationGroups[_selectedAnimGroup];
                //_animationFrameCount = SequenceExtensions.GetFrameLength(animationGroup);
                //_debugSequenceRenderer.AnimationGroup = animationGroup;
                //_sequencer.SelectedAnimationGroupIndex = value;
            }
        }

        public AppLayoutEditor(
            MonoGameImGuiBootstrap bootstrap,
            IEditorSettings settings,
            Layout layout,
            IEnumerable<Imgd> images)
        {
            _bootStrap = bootstrap;
            _settings = settings;
            _layout = layout;
            _images = images.ToList();

            _graphics = bootstrap.GraphicsDevice;
            _shader = new KingdomShader(bootstrap.Content);
            _drawing = new MonoSpriteDrawing(_graphics, _shader);
            _destinationTexture = _drawing.CreateSpriteTexture(1024, 1024);
            _destinationTextureId = this.BindTexture(_destinationTexture);
            
            _debugRender = new DefaultDebugLayoutRenderer();
            _spriteTextures = images.Select(x => _drawing.CreateSpriteTexture(x)).ToList();
            _renderer = new LayoutRenderer(_layout, _drawing, _spriteTextures);
        }

        public void Menu()
        {
        }

        public bool Run()
        {
            const float PreviewWidthMul = 3f;
            const float PreviewWidthMax = 512f;
            const float EditorWidthMul = 2f;
            const float EditorWidthMax = 512f;
            const float TotalWidthMul = PreviewWidthMul + EditorWidthMul;

            var windowSize = ImGui.GetWindowSize();
            var previewWidth = Math.Min(windowSize.X / TotalWidthMul * PreviewWidthMul, PreviewWidthMax);
            var editorWidth = Math.Min(windowSize.X / TotalWidthMul * EditorWidthMul, EditorWidthMax);
            previewWidth = windowSize.X - editorWidth;

            ForChild(nameof(LayoutPreview), previewWidth, 0, false, LayoutPreview);
            ImGui.SameLine();
            ForChild(nameof(LayoutEditing), editorWidth, 0, true, LayoutEditing);

            if (_sequenceEditor != null)
            {
                bool dummy = true;
                if (ImGui.BeginPopupModal(SequenceEditorDialogName, ref dummy,
                    ImGuiWindowFlags.Popup | ImGuiWindowFlags.Modal | ImGuiWindowFlags.MenuBar))
                {
                    const float ChildWindowScale = 0.75f;
                    var RecommendedWidth = windowSize.X * ChildWindowScale;
                    var RecommendedHeight = windowSize.Y * ChildWindowScale;
                    var cursor = ImGui.GetCursorPos();
                    ImGui.SetCursorPosX(RecommendedWidth);
                    ImGui.SetCursorPosY(RecommendedHeight);
                    ImGui.SetCursorPosX(cursor.X);
                    ImGui.SetCursorPosY(cursor.Y);

                    ForMenuBar(_sequenceEditor.Menu);
                    _sequenceEditor.Run();
                    
                    ImGui.EndPopup();
                }
            }

            if (_isOpeningSequenceEditor)
            {
                _isOpeningSequenceEditor = false;
                ImGui.OpenPopup(SequenceEditorDialogName);
            }

            _animationFrameCurrent++;
            return true;
        }

        private void LayoutPreview()
        {
            if (ImGui.BeginCombo("", $"Layout Index {SelectedLayoutIndex}",
                ImGuiComboFlags.PopupAlignLeft))
            {
                for (int i = 0; i < _layout.SequenceGroups.Count; i++)
                {
                    if (ImGui.Selectable($"Layout #{i}\n",
                        SelectedLayoutIndex == i))
                        SelectedLayoutIndex = i;
                }
                ImGui.EndCombo();
            }
            ImGui.SameLine();
            if (ImGui.Button("-", new Vector2(30, 0)) && SelectedLayoutIndex > 0)
                SelectedLayoutIndex--;
            ImGui.SameLine();
            if (ImGui.Button("+", new Vector2(30, 0)) &&
                SelectedLayoutIndex < _layout.SequenceGroups.Count - 1)
                SelectedLayoutIndex++;

            Timeline();

            ForChild("Preview", 0, 0, false, () =>
            {
                const float PositionX = 128f;
                const float PositionY = 32f;
                const float ViewportWidth = 1024f;
                const float ViewportHeight = 1024f;
                var width = ImGui.GetWindowContentRegionWidth();
                var height = ImGui.GetWindowHeight();
                var backgroundColorInverse = new ColorF(
                    1f - _settings.EditorBackground.R,
                    1f - _settings.EditorBackground.G,
                    1f - _settings.EditorBackground.B,
                    1f);

                _drawing.DestinationTexture = _destinationTexture;
                _drawing.Clear(_settings.EditorBackground);
                _drawing.SetViewport(-PositionX, ViewportWidth - PositionX, -PositionY, ViewportHeight - PositionY);


                _renderer.FrameIndex = _animationFrameCurrent;
                _renderer.SelectedSequenceGroupIndex = SelectedLayoutIndex;

                if (!_settings.IsViewportOnTop)
                    DrawGameViewport(backgroundColorInverse);
                _renderer.Draw();
                if (_settings.IsViewportOnTop)
                    DrawGameViewport(backgroundColorInverse);

                _drawing.Flush();
                _drawing.DestinationTexture = null;

                float maxU = 1f / ViewportWidth * width;
                float maxV = 1f / ViewportHeight * height;
                ImGui.Image(_destinationTextureId, new Vector2(width, height),
                    GetUv(_destinationTexture, 0, 0), new Vector2(maxU, maxV));
            });
        }

        private void DrawGameViewport(ColorF backgroundColorInverse)
        {
            const float OriginalViewportWidth = 512f;
            const float RemixViewportWidth = 684f;
            const float ViewportHeight = 416f;

            if (_settings.ShowViewportOriginal)
                _drawing.DrawRectangle(-1, -1, OriginalViewportWidth + 2, ViewportHeight + 2, backgroundColorInverse);

            if (_settings.ShowViewportRemix)
                _drawing.DrawRectangle(
                -(RemixViewportWidth - OriginalViewportWidth) / 2 - 1, -1,
                RemixViewportWidth + 2, ViewportHeight + 2, backgroundColorInverse);

            _drawing.Flush();
        }

        private unsafe void Timeline()
        {
            var frameCount = _layout.GetFrameLengthFromSequenceGroup(SelectedLayoutIndex);

            ImGui.SliderInt("Frame", ref _animationFrameCurrent, 0, frameCount,
                $"%i/{frameCount}");
        }

        private void LayoutEditing()
        {
            var sequenceGroup = _layout.SequenceGroups[SelectedLayoutIndex];
            for (var i = 0; i < sequenceGroup.Sequences.Count; i++)
            {
                if (ImGui.CollapsingHeader($"Sequence property {i + 1}"))
                {
                    SequencePropertyEdit(sequenceGroup.Sequences[i], i);
                }
            }
        }

        private void SequencePropertyEdit(Layout.SequenceProperty sequenceProperty, int index)
        {
            var textureIndex = sequenceProperty.TextureIndex;
            if (ImGui.DragInt($"Texture index##{index}", ref textureIndex))
                sequenceProperty.TextureIndex = Math.Min(Math.Max(textureIndex, 0), _images.Count - 1);

            var sequenceIndex = sequenceProperty.SequenceIndex;
            if (ImGui.DragInt($"Sequence index##{index}", ref sequenceIndex))
            {
                sequenceProperty.SequenceIndex = Math.Min(Math.Max(sequenceIndex, 0), _layout.SequenceItems.Count - 1);
                var sequence = _layout.SequenceItems[sequenceProperty.SequenceIndex];
                sequenceProperty.AnimationGroup = Math.Min(Math.Max(sequenceProperty.AnimationGroup, 0), sequence.AnimationGroups.Count - 1);

            }

            var animGroupIndex = sequenceProperty.AnimationGroup;
            if (ImGui.DragInt($"Animation group index##{index}", ref animGroupIndex))
            {
                var sequence = _layout.SequenceItems[sequenceProperty.SequenceIndex];
                sequenceProperty.AnimationGroup = Math.Min(Math.Max(animGroupIndex, 0), sequence.AnimationGroups.Count - 1);
            }

            var frameStart = sequenceProperty.ShowAtFrame;
            if (ImGui.DragInt($"Show at frame##{index}", ref frameStart))
                sequenceProperty.ShowAtFrame = frameStart;

            var position = new int[] { sequenceProperty.PositionX, sequenceProperty.PositionY };
            if (ImGui.DragInt2($"Position##{index}", ref position[0]))
            {
                sequenceProperty.PositionX = position[0];
                sequenceProperty.PositionY = position[1];
            }

            if (ImGui.Button($"Modify inner sequence##{index}"))
                OpenInnerSequence(
                    sequenceProperty.SequenceIndex,
                    sequenceProperty.TextureIndex,
                    sequenceProperty.AnimationGroup);
        }

        private void OpenInnerSequence(int sequenceIndex, int textureIndex, int animationGroup)
        {
            _sequenceEditor?.Dispose();
            _sequenceEditor = new AppSequenceEditor(_bootStrap, _settings,
                _layout.SequenceItems[sequenceIndex],
                _images[textureIndex]);
            _isOpeningSequenceEditor = true;
        }

        public Bar.Entry SaveAnimation(string name)
        {
            var stream = new MemoryStream();
            _layout.Write(stream);

            return new Bar.Entry
            {
                Name = name,
                Stream = stream,
                Type = Bar.EntryType.Layout
            };
        }

        public Bar.Entry SaveTexture(string name)
        {
            var stream = new MemoryStream();
            Imgz.Write(stream, _images);

            return new Bar.Entry
            {
                Name = name,
                Stream = stream,
                Type = Bar.EntryType.Imgz
            };
        }

        public void Dispose()
        {
            _sequenceEditor?.Dispose();
        }

        public IntPtr BindTexture(Texture2D texture) =>
            _bootStrap.BindTexture(texture);

        public void UnbindTexture(IntPtr id) =>
            _bootStrap.UnbindTexture(id);

        public void RebindTexture(IntPtr id, Texture2D texture) =>
            _bootStrap.RebindTexture(id, texture);

        private static Vector2 GetUv(ISpriteTexture texture, int x, int y) =>
            new Vector2((float)x / texture.Width, (float)y / texture.Height);

    }
}

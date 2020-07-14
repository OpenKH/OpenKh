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

            var windowSize = ImGui.GetIO().DisplaySize.X;
            var previewWidth = Math.Min(windowSize / TotalWidthMul * PreviewWidthMul, PreviewWidthMax);
            var editorWidth = Math.Min(windowSize / TotalWidthMul * EditorWidthMul, EditorWidthMax);
            previewWidth = windowSize - editorWidth;

            ForChild(nameof(LayoutPreview), previewWidth, 0, false, LayoutPreview);
            ImGui.SameLine();
            ForChild(nameof(LayoutEditing), editorWidth, 0, true, LayoutEditing);

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
                _drawing.SetViewport(0, ViewportWidth, 0, ViewportHeight);

                // This draws the screen border
                _drawing.DrawRectangle(-1, -1, 512f + 2, 416f + 2, backgroundColorInverse);
                _drawing.Flush();

                _renderer.FrameIndex = _animationFrameCurrent;
                _renderer.SelectedSequenceGroupIndex = SelectedLayoutIndex;
                _renderer.Draw();
                _drawing.Flush();
                _drawing.DestinationTexture = null;

                float maxU = 1f / ViewportWidth * width;
                float maxV = 1f / ViewportHeight * height;
                ImGui.Image(_destinationTextureId, new Vector2(width, height),
                    GetUv(_destinationTexture, 0, 0), new Vector2(maxU, maxV));
            });
        }

        private unsafe void Timeline()
        {
            var frameCount = _layout.GetFrameLengthFromSequenceGroup(SelectedLayoutIndex);

            ImGui.SliderInt("Frame", ref _animationFrameCurrent, 0, frameCount,
                $"%i/{frameCount}");
        }

        private void LayoutEditing()
        {
            ImGui.Text("Here will go editor controls");
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

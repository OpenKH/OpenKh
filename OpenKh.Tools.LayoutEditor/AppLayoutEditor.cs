using ImGuiNET;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using OpenKh.Tools.Common.CustomImGui;
using OpenKh.Tools.LayoutEditor.Interfaces;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Windows;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using Xe.Tools.Wpf.Dialogs;

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
        private static readonly List<FileDialogFilter> ImdFilter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("Image IMGD", "imd")
            .AddAllFiles();
        private bool _isReplacingTexture;
        private int _replaceTextureIndex;

        private float ZoomFactor = 1;

        private float PanFactorX = 0;
        private float PanFactorY = 0;

        private int PastScrollValue = 0;

        private float PastMouseX = 0;
        private float PastMouseY = 0;

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
            _destinationTexture = _drawing.CreateSpriteTexture(2048, 2048);
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

            // Handle texture replacement popup
            if (_isReplacingTexture)
            {
                ImGui.OpenPopup("Replace Texture Index");
                _isReplacingTexture = false;
            }

            bool replacePopupOpen = true;
            if (ImGui.BeginPopupModal("Replace Texture Index", ref replacePopupOpen, ImGuiWindowFlags.AlwaysAutoResize))
            {
                ImGui.Text($"Select texture index to replace (0-{_images.Count - 1}):");
                ImGui.InputInt("Texture Index", ref _replaceTextureIndex);
                _replaceTextureIndex = Math.Max(0, Math.Min(_replaceTextureIndex, _images.Count - 1));

                if (ImGui.Button("Select File", new Vector2(120, 0)))
                {
                    var indexToReplace = _replaceTextureIndex;
                    Xe.Tools.Wpf.Dialogs.FileDialog.OnOpen(fileName =>
                    {
                        try
                        {
                            using var stream = File.OpenRead(fileName);
                            var importedImage = Imgd.Read(stream);

                            var originalImage = _images[indexToReplace];

                            // Check if aspect ratio is the same (must be square if original was square)
                            bool originalIsSquare = originalImage.Size.Width == originalImage.Size.Height;
                            bool newIsSquare = importedImage.Size.Width == importedImage.Size.Height;

                            if (originalIsSquare != newIsSquare)
                            {
                                MessageBox.Show($"Format mismatch!\nOriginal texture: {originalImage.Size.Width}x{originalImage.Size.Height}\nNew texture: {importedImage.Size.Width}x{importedImage.Size.Height}\n\nBoth textures must have the same aspect ratio.",
                                    "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Error);
                                return;
                            }

                            // If not square, check if aspect ratio matches
                            if (!originalIsSquare)
                            {
                                float originalRatio = (float)originalImage.Size.Width / originalImage.Size.Height;
                                float newRatio = (float)importedImage.Size.Width / importedImage.Size.Height;

                                if (Math.Abs(originalRatio - newRatio) > 0.01f)
                                {
                                    MessageBox.Show($"Aspect ratio mismatch!\nOriginal: {originalImage.Size.Width}x{originalImage.Size.Height} (ratio: {originalRatio:F2})\nNew: {importedImage.Size.Width}x{importedImage.Size.Height} (ratio: {newRatio:F2})",
                                        "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Error);
                                    return;
                                }
                            }

                            // Calculate scale factors
                            float scaleX = (float)importedImage.Size.Width / originalImage.Size.Width;
                            float scaleY = (float)importedImage.Size.Height / originalImage.Size.Height;

                            // Dispose old sprite texture
                            _spriteTextures[indexToReplace].Dispose();

                            // Replace the image and sprite texture
                            _images[indexToReplace] = importedImage;
                            _spriteTextures[indexToReplace] = _drawing.CreateSpriteTexture(importedImage);

                            // Update sprite UV coordinates ONLY for sequences that use this specific texture index
                            int updatedSequences = 0;
                            foreach (var sequenceGroup in _layout.SequenceGroups)
                            {
                                foreach (var sequenceProperty in sequenceGroup.Sequences)
                                {
                                    // Only update if this sequence property uses the texture we're replacing
                                    if (sequenceProperty.TextureIndex == indexToReplace)
                                    {
                                        var sequenceItem = _layout.SequenceItems[sequenceProperty.SequenceIndex];
                                        foreach (var sprite in sequenceItem.Sprites)
                                        {
                                            // Scale the UV coordinates
                                            sprite.Left = (short)(sprite.Left * scaleX);
                                            sprite.Top = (short)(sprite.Top * scaleY);
                                            sprite.Right = (short)(sprite.Right * scaleX);
                                            sprite.Bottom = (short)(sprite.Bottom * scaleY);
                                        }
                                        updatedSequences++;
                                    }
                                }
                            }

                            string scaleInfo = (scaleX != 1.0f || scaleY != 1.0f)
                                ? $"\nScaled UV coordinates by {scaleX}x (width) and {scaleY}x (height) for {updatedSequences} sequence(s) using this texture."
                                : "\nNo scaling needed - resolution unchanged.";

                            MessageBox.Show($"Successfully replaced texture at index {indexToReplace}!{scaleInfo}",
                                "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception ex)
                        {
                            MessageBox.Show($"Failed to replace texture:\n{ex.Message}",
                                "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }, ImdFilter);
                    ImGui.CloseCurrentPopup();
                }

                ImGui.SameLine();
                if (ImGui.Button("Cancel", new Vector2(120, 0)))
                {
                    ImGui.CloseCurrentPopup();
                }

                ImGui.EndPopup();
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

            ImGui.SameLine();
            if (ImGui.Button("Add", new Vector2(50, 0)))
            {
                // Validate that we have sequences and textures to reference
                if (_layout.SequenceItems.Count == 0 || _images.Count == 0)
                {
                    MessageBox.Show("Cannot add sequence group: No sequences or textures available.",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
                else
                {
                    var newSequenceGroup = new Layout.SequenceGroup
                    {
                        Sequences = new List<Layout.SequenceProperty>
            {
                new Layout.SequenceProperty
                {
                    SequenceIndex = 0,      // Points to _layout.SequenceItems[0]
                    TextureIndex = 0,       // Points to _images[0]
                    AnimationGroup = 0,     // Uses first AnimationGroup from that Sequence
                    ShowAtFrame = 0,
                    PositionX = 0,
                    PositionY = 0
                }
            }
                    };

                    _layout.SequenceGroups.Add(newSequenceGroup);

                    // Select the newly created sequence group
                    SelectedLayoutIndex = _layout.SequenceGroups.Count - 1;
                }
            }

            ImGui.SameLine();
            if (ImGui.Button("Remove", new Vector2(70, 0)) && _layout.SequenceGroups.Count > 1)
            {
                // Remove the currently selected sequence group
                _layout.SequenceGroups.RemoveAt(SelectedLayoutIndex);

                // Update selection to stay valid
                SelectedLayoutIndex = Math.Max(0, Math.Min(SelectedLayoutIndex, _layout.SequenceGroups.Count - 1));
            }

            Timeline();

            ForChild("Preview", 0, 0, false, () =>
            {
                float PositionX = 128f;
                float PositionY = 32f;
                const float ViewportWidth = 2048;
                const float ViewportHeight = 2048;
                var width = ImGui.GetWindowContentRegionMax().X;
                var height = ImGui.GetWindowHeight();
                var backgroundColorInverse = new ColorF(
                    1f - _settings.EditorBackground.R,
                    1f - _settings.EditorBackground.G,
                    1f - _settings.EditorBackground.B,
                    1f);

                _drawing.DestinationTexture = _destinationTexture;
                _drawing.Clear(_settings.EditorBackground);

                //Only apply zoom when mouse is hovering over this child window
                if (ImGui.IsWindowHovered())
                {
                    if (Mouse.GetState().ScrollWheelValue - PastScrollValue > 0)
                        ZoomFactor -= 0.05F;

                    if (Mouse.GetState().ScrollWheelValue - PastScrollValue < 0)
                        ZoomFactor += 0.05F;

                    PastScrollValue = Mouse.GetState().ScrollWheelValue;
                }

                if (Keyboard.GetState().IsKeyDown(Keys.Right))
                    _renderer.PanFactorX += 5;

                if (Keyboard.GetState().IsKeyDown(Keys.Left))
                    _renderer.PanFactorX -= 5;

                if (Keyboard.GetState().IsKeyDown(Keys.Up))
                    _renderer.PanFactorY -= 5;

                if (Keyboard.GetState().IsKeyDown(Keys.Down))
                    _renderer.PanFactorY += 5;


                _drawing.SetViewport(-PositionX, ViewportWidth - PositionX, -PositionY, ViewportHeight - PositionY);
                _drawing.SetProjection(ViewportWidth - PositionX, ViewportHeight - PositionY, (ViewportWidth - PositionX) * ZoomFactor, (ViewportHeight - PositionY) * ZoomFactor, 1);

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
            const float ReFinedViewportWidthFirst = 970.6F;
            const float ReFinedViewportWidthSecond = 1479F;
            const float ViewportHeight = 416f;

            if (_settings.ShowViewportOriginal)
                _drawing.DrawRectangle(-1 + _renderer.PanFactorX, -1 + _renderer.PanFactorY, OriginalViewportWidth + 2, ViewportHeight + 2, backgroundColorInverse, 1);

            if (_settings.ShowViewportRemix)
            {
                _drawing.DrawRectangle(
                -(RemixViewportWidth - OriginalViewportWidth) / 2 - 1 + _renderer.PanFactorX, -1 + _renderer.PanFactorY,
                RemixViewportWidth + 2, ViewportHeight + 2, backgroundColorInverse, 1);
            }

            if (_settings.ShowViewportReFined)
            {
                _drawing.DrawRectangle(
                -(ReFinedViewportWidthFirst - OriginalViewportWidth) / 2 - 1 + _renderer.PanFactorX, -1 + _renderer.PanFactorY,
                ReFinedViewportWidthFirst + 2, ViewportHeight + 2, backgroundColorInverse, 1);

                _drawing.DrawRectangle(
                -(ReFinedViewportWidthSecond - OriginalViewportWidth) / 2 - 1 + _renderer.PanFactorX, -1 + _renderer.PanFactorY,
                ReFinedViewportWidthSecond + 2, ViewportHeight + 2, backgroundColorInverse, 1);
            }

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

            // Texture management section
            ImGui.Text($"Textures: {_images.Count}");

            if (ImGui.Button("Import IMGD Texture", new Vector2(180, 0)))
            {
                Xe.Tools.Wpf.Dialogs.FileDialog.OnOpen(fileName =>
                {
                    try
                    {
                        using var stream = File.OpenRead(fileName);
                        var importedImage = Imgd.Read(stream);

                        // Create sprite texture for rendering BEFORE adding to collections
                        // This ensures both collections stay synchronized if texture creation fails
                        var newSpriteTexture = _drawing.CreateSpriteTexture(importedImage);

                        // Only add to collections if sprite texture creation succeeded
                        _images.Add(importedImage);
                        _spriteTextures.Add(newSpriteTexture);

                        MessageBox.Show($"Successfully imported texture!\nNew texture index: {_images.Count - 1}",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Failed to import texture:\n{ex.Message}",
                            "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }, ImdFilter);
            }

            ImGui.SameLine();
            if (ImGui.Button("Replace Texture", new Vector2(140, 0)) && _images.Count > 0)
            {
                _replaceTextureIndex = 0;
                _isReplacingTexture = true;
            }

            ImGui.SameLine();
            if (ImGui.Button("Remove Last Texture", new Vector2(160, 0)) && _images.Count > 1)
            {
                // Remove the last texture
                _spriteTextures[_spriteTextures.Count - 1].Dispose();
                _spriteTextures.RemoveAt(_spriteTextures.Count - 1);
                _images.RemoveAt(_images.Count - 1);

                // Update any sequence properties that reference the removed texture
                foreach (var seqGroup in _layout.SequenceGroups)
                {
                    foreach (var seqProp in seqGroup.Sequences)
                    {
                        if (seqProp.TextureIndex >= _images.Count)
                        {
                            seqProp.TextureIndex = Math.Max(0, _images.Count - 1);
                        }
                    }
                }
            }

            ImGui.Separator();

            // Add sequence property button
            if (ImGui.Button("Add Sequence Property", new Vector2(-1, 30)))
            {
                var newSequenceProperty = new Layout.SequenceProperty
                {
                    SequenceIndex = 0,
                    TextureIndex = 0,
                    AnimationGroup = 0,
                    ShowAtFrame = 0,
                    PositionX = 0,
                    PositionY = 0
                };

                sequenceGroup.Sequences.Add(newSequenceProperty);
            }

            ImGui.Separator();
            ImGui.Spacing();

            // List all sequence properties
            for (var i = 0; i < sequenceGroup.Sequences.Count; i++)
            {
                if (ImGui.CollapsingHeader($"Sequence property {i + 1}"))
                {
                    SequencePropertyEdit(sequenceGroup.Sequences[i], i);

                    ImGui.Spacing();
                    ImGui.PushStyleColor(ImGuiCol.Button, new Vector4(0.8f, 0.2f, 0.2f, 1.0f)); // Red button
                    if (ImGui.Button($"Remove property {i + 1}##remove_{i}", new Vector2(-1, 0)) && sequenceGroup.Sequences.Count > 1)
                    {
                        sequenceGroup.Sequences.RemoveAt(i);
                        ImGui.PopStyleColor();
                        break; // Exit loop since we modified the collection
                    }
                    ImGui.PopStyleColor();
                    ImGui.Spacing();
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

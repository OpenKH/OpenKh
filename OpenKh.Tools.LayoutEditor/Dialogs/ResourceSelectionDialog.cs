using ImGuiNET;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;
using Xe.Tools.Wpf.Dialogs;

namespace OpenKh.Tools.LayoutEditor.Dialogs
{
    public class ResourceSelectionDialog : IDisposable
    {
        private readonly Bar.Entry[] _animations;
        private readonly Bar.Entry[] _textures;
        private int _selectedAnimIndex;
        private int _selectedTextureIndex;

        private static readonly List<FileDialogFilter> ImdFilter = FileDialogFilterComposer
            .Compose()
            .AddExtensions("Image IMGD", "imd")
            .AddAllFiles();

        public bool HasResourceBeenSelected { get; private set; }
        public Bar.Entry SelectedAnimation { get; private set; }
        public Bar.Entry SelectedTexture { get; private set; }

        public ResourceSelectionDialog(
            IEnumerable<Bar.Entry> entries,
            Bar.EntryType animationType,
            Bar.EntryType textureType)
        {
            _animations = entries
                .Where(x => x.Type == animationType && x.Index == 0)
                .ToArray();
            _textures = entries
                .Where(x => x.Type == textureType && x.Index == 0)
                .ToArray();
        }

        public void Run()
        {
            ImGui.Text("The selected file contains multiple elements.");
            ImGui.Text("Please choose the appropiate sub-files you want to load.");

            ImGui.Columns(2, "resources", true);
            ImGui.Text("Animation");
            for (var i = 0; i < _animations.Length; i++)
            {
                if (ImGui.Selectable($"{_animations[i].Name}##anm",
                    _selectedAnimIndex == i,
                    ImGuiSelectableFlags.DontClosePopups))
                    _selectedAnimIndex = i;
            }

            ImGui.NextColumn();
            ImGui.Text("Texture");
            for (var i = 0; i < _textures.Length; i++)
            {
                if (ImGui.Selectable($"{_textures[i].Name}##tex",
                    _selectedTextureIndex == i,
                    ImGuiSelectableFlags.DontClosePopups))
                    _selectedTextureIndex = i;
            }

            ImGui.Columns(1);
            ImGui.Separator();

            // Add Replace Texture button
            if (ImGui.Button("Replace Selected Texture"))
            {
                ReplaceTexture(_selectedTextureIndex);
            }
            ImGui.SameLine();

            if (ImGui.Button("Open"))
            {
                HasResourceBeenSelected = true;
                SelectedAnimation = _animations[_selectedAnimIndex];
                SelectedTexture = _textures[_selectedTextureIndex];
                ImGui.CloseCurrentPopup();
            }
        }

        private void ReplaceTexture(int textureIndex)
        {
            FileDialog.OnOpen(fileName =>
            {
                try
                {
                    using var stream = File.OpenRead(fileName);
                    var importedImage = Imgd.Read(stream);

                    // Read the current texture
                    _textures[textureIndex].Stream.Position = 0;
                    var currentImage = Imgd.Read(_textures[textureIndex].Stream);

                    // Check if aspect ratio is the same
                    bool originalIsSquare = currentImage.Size.Width == currentImage.Size.Height;
                    bool newIsSquare = importedImage.Size.Width == importedImage.Size.Height;

                    if (originalIsSquare != newIsSquare)
                    {
                        MessageBox.Show($"Format mismatch!\nOriginal texture: {currentImage.Size.Width}x{currentImage.Size.Height}\nNew texture: {importedImage.Size.Width}x{importedImage.Size.Height}\n\nBoth textures must have the same aspect ratio.",
                            "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Error);
                        return;
                    }

                    if (!originalIsSquare)
                    {
                        float originalRatio = (float)currentImage.Size.Width / currentImage.Size.Height;
                        float newRatio = (float)importedImage.Size.Width / importedImage.Size.Height;

                        if (Math.Abs(originalRatio - newRatio) > 0.01f)
                        {
                            MessageBox.Show($"Aspect ratio mismatch!\nOriginal: {currentImage.Size.Width}x{currentImage.Size.Height} (ratio: {originalRatio:F2})\nNew: {importedImage.Size.Width}x{importedImage.Size.Height} (ratio: {newRatio:F2})",
                                "Invalid Format", MessageBoxButton.OK, MessageBoxImage.Error);
                            return;
                        }
                    }

                    // Calculate scale factors
                    float scaleX = (float)importedImage.Size.Width / currentImage.Size.Width;
                    float scaleY = (float)importedImage.Size.Height / currentImage.Size.Height;

                    // Replace the texture in the BAR entry
                    var newTextureStream = new MemoryStream();
                    importedImage.Write(newTextureStream);
                    newTextureStream.Position = 0;
                    _textures[textureIndex].Stream = newTextureStream;

                    // Update sprite UV coordinates ONLY for sprites used by animations that reference this specific texture
                    // We need to check if this is a sequence (2DD) or layout (2LD) file
                    bool isSequenceFile = _animations[_selectedAnimIndex].Type == Bar.EntryType.Seqd;

                    int updatedSprites = 0;

                    if (isSequenceFile)
                    {
                        // For sequence files (2DD), all sprites in the sequence use the same texture,
                        // so we can safely scale all sprites
                        _animations[_selectedAnimIndex].Stream.Position = 0;
                        var sequence = Sequence.Read(_animations[_selectedAnimIndex].Stream);

                        foreach (var sprite in sequence.Sprites)
                        {
                            sprite.Left = (short)(sprite.Left * scaleX);
                            sprite.Top = (short)(sprite.Top * scaleY);
                            sprite.Right = (short)(sprite.Right * scaleX);
                            sprite.Bottom = (short)(sprite.Bottom * scaleY);
                            updatedSprites++;
                        }

                        // Write the updated sequence back
                        var updatedSequenceStream = new MemoryStream();
                        sequence.Write(updatedSequenceStream);
                        updatedSequenceStream.Position = 0;
                        _animations[_selectedAnimIndex].Stream = updatedSequenceStream;

                        string scaleInfo = (scaleX != 1.0f || scaleY != 1.0f)
                            ? $"\nScaled UV coordinates by {scaleX}x (width) and {scaleY}x (height) for {updatedSprites} sprite(s)."
                            : "\nNo scaling needed - resolution unchanged.";

                        MessageBox.Show($"Successfully replaced texture in BAR file!{scaleInfo}\n\nClick 'Open' to load the file with the new texture.",
                            "Success", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                    else
                    {
                        // For layout files (2LD), we cannot scale sprites here because we don't know
                        // which sequence properties reference which textures without loading the full layout structure.
                        // This scenario should be handled in AppLayoutEditor instead.
                        MessageBox.Show($"Successfully replaced texture in BAR file!\n\nNote: For layout files (2LD), UV coordinate scaling must be done through the main editor's 'Replace Texture' feature after opening the file, which properly tracks texture-to-sequence relationships.\n\nClick 'Open' to load the file with the new texture.",
                            "Success - Manual Scaling Required", MessageBoxButton.OK, MessageBoxImage.Information);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Failed to replace texture:\n{ex.Message}",
                        "Error", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }, ImdFilter);
        }

        public void Dispose()
        {

        }
    }
}

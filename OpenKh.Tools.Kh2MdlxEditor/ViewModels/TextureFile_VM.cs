using OpenKh.Kh2;
using OpenKh.Tools.Kh2MdlxEditor.Utils;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;

namespace OpenKh.Tools.Kh2MdlxEditor.ViewModels
{
    internal class TextureFile_VM
    {
        private Action<ModelTexture> _onTextureReplaced;

        public ModelTexture textureData { get; set; }
        public ObservableCollection<ModelTexture.Texture> textures { get; set; }

        public TextureFile_VM() { }
        public TextureFile_VM(ModelTexture textureFile, Action<ModelTexture> onTextureReplaced = null)
        {
            textureData = textureFile;
            textures = new ObservableCollection<ModelTexture.Texture>(textureData.Images);
            _onTextureReplaced = onTextureReplaced;
        }

        public void removeTextureAt(int index)
        {
            textures.RemoveAt(index);
            textureData.Images.RemoveAt(index);
        }

        public void addTexture(string filename)
        {
            textures.Add(ImageUtils.pngToTexture(filename));
            textureData.Images.Add(ImageUtils.pngToTexture(filename));
        }

        // Replace texture at specific index
        public void replaceTextureAt(int index, string filename)
        {
            if (index < 0 || index >= textures.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(index), "Invalid texture index");
            }

            // Convert PNG to Imgd format
            Imgd newImgd = ImageUtils.pngToImgd(filename);

            // Build new texture list with replacement
            List<Imgd> imgdList = new List<Imgd>();
            for (int i = 0; i < textureData.Images.Count; i++)
            {
                if (i == index)
                {
                    // Use the new texture
                    imgdList.Add(newImgd);
                }
                else
                {
                    // Convert existing texture back to Imgd
                    var existingTexture = textureData.Images[i];
                    Imgd existingImgd = new Imgd(
                        existingTexture.Size,
                        existingTexture.PixelFormat,
                        existingTexture.GetData(),
                        existingTexture.GetClut(),
                        false
                    );
                    imgdList.Add(existingImgd);
                }
            }

            // Store the footer data if it exists
            byte[] footerData = null;
            if (textureData.TextureFooterData != null)
            {
                using (var memStream = new MemoryStream())
                {
                    textureData.TextureFooterData.Write(memStream);
                    footerData = memStream.ToArray();
                }
            }

            // Rebuild the ModelTexture with the new texture list
            var build = new ModelTexture.Build
            {
                images = imgdList,
                footerData = footerData
            };

            ModelTexture newModelTexture = new ModelTexture(build);

            // Write and read back to ensure proper formatting
            Stream tempStream = new MemoryStream();
            newModelTexture.Write(tempStream);
            tempStream.Position = 0;
            textureData = ModelTexture.Read(tempStream);

            // Notify parent (Main2_VM) that texture was replaced so it can update its reference
            _onTextureReplaced?.Invoke(textureData);

            // Update the observable collection for UI
            textures.Clear();
            foreach (var texture in textureData.Images)
            {
                textures.Add(texture);
            }
        }
    }
}

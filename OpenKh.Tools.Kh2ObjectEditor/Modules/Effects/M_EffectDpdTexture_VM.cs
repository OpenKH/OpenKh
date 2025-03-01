using ModelingToolkit.Objects;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Kh2ObjectEditor.Modules.Effects
{
    public class M_EffectDpdTexture_VM
    {

        public Dpd ThisDpd { get; set; }
        public ObservableCollection<DpdTextureWrapper> TextureWrappers { get; set; }

        public M_EffectDpdTexture_VM(Dpd dpd)
        {
            ThisDpd = dpd;
            TextureWrappers = new ObservableCollection<DpdTextureWrapper>();
            loadWrappers();
        }

        public void loadWrappers()
        {
            if (ThisDpd?.TexturesList == null || ThisDpd.TexturesList.Count <= 0)
                return;

            TextureWrappers.Clear();

            // Group textures by their shTexDbp value
            var groupedTextures = ThisDpd.TexturesList
                .Select((texture, index) => new DpdTextureWrapper
                {
                    Id = index,
                    Texture = texture,
                    SizeX = texture.Size.Width,
                    SizeY = texture.Size.Height,
                })
                .GroupBy(wrapper => wrapper.Texture.shTexDbp)  // Group by shTexDbp value
                .OrderBy(group => group.Key)  // Optional: sort groups by shTexDbp (ascending)
                .ToList();

            // Now process each group and update the name accordingly
            foreach (var group in groupedTextures)
            {
                var firstWrapper = group.First();  // Get the first texture in the group

                foreach (var wrapper in group)
                {
                    // If it's the first texture, keep its original name
                    if (wrapper.Id == firstWrapper.Id)
                    {
                        wrapper.Name = $"Texture {wrapper.Id}";
                    }
                    else
                    {
                        // For the rest of the textures, mark them as "COMBO with {firstWrapper.Id}"
                        wrapper.Name = $"Texture {wrapper.Id} (COMBO with {firstWrapper.Id})";
                    }
                    TextureWrappers.Add(wrapper);
                }
            }
        }

        public BitmapSource getTexture(int index)
        {
            var group = TextureWrappers
                .Where(t => t.Texture.shTexDbp == TextureWrappers[index].Texture.shTexDbp)
                .ToList();

            if (group.Count == 1)
            {
                //if only one texture in the group, return as normal
                MtMaterial mat = GetMaterial(group[0].Texture);
                return mat.GetAsBitmapImage();
            }

            //get max width/height for combo texture
            int maxWidth = group.Max(t => t.SizeX + t.Texture.shX);
            int maxHeight = group.Max(t => t.SizeY + t.Texture.shY);

            //blank bitmap for combined image
            Bitmap combinedBitmap = new Bitmap(maxWidth, maxHeight);
            using (Graphics g = Graphics.FromImage(combinedBitmap))
            {
                g.Clear(System.Drawing.Color.Transparent);

                //adjust each texture position based on shX and shY values
                foreach (var wrapper in group)
                {
                    MtMaterial mat = GetMaterial(wrapper.Texture);
                    Bitmap img = mat.DiffuseTextureBitmap;
                    g.DrawImage(img, wrapper.Texture.shX, wrapper.Texture.shY);
                }
            }

            //new bitmap to include individual texture at the top, and a combined view below.
            int individualHeight = group.Max(t => t.SizeY);
            int combinedHeight = maxHeight + individualHeight + 64;  //64px space between textures

            //make final bitmap
            Bitmap finalBitmap = new Bitmap(maxWidth, combinedHeight);
            using (Graphics g = Graphics.FromImage(finalBitmap))
            {
                g.Clear(System.Drawing.Color.Transparent);

                //draw individual texture above
                var selectedTextureWrapper = group.FirstOrDefault(t => t == TextureWrappers[index]);


                //get material for final texture
                MtMaterial selectedMat = GetMaterial(selectedTextureWrapper.Texture);
                Bitmap selectedImg = selectedMat.DiffuseTextureBitmap;


                //now draw the individual texture
                g.DrawImage(selectedImg, selectedTextureWrapper.Texture.shX, selectedTextureWrapper.Texture.shY);

                //prepare the combined texture
                int offsetY = individualHeight + 64;  //offset it 64px down

                g.DrawImage(combinedBitmap, 0, offsetY);
            }

            return System.Windows.Interop.Imaging.CreateBitmapSourceFromHBitmap(
                finalBitmap.GetHbitmap(),
                IntPtr.Zero,
                System.Windows.Int32Rect.Empty,
                BitmapSizeOptions.FromEmptyOptions()
            );
        }

        public static byte SwapBits34(byte x)
        {
            byte bit3 = (byte)((x & 8) << 1);
            byte bit4 = (byte)((x & 16) >> 1);
            byte rest = (byte)(x & ~(8 | 16));
            return (byte)(rest | bit3 | bit4);
        }

        public void ExportTexture(int index)
        {
            Dpd.Texture selectedTexture = TextureWrappers[index].Texture;

            //find all textures with the same shTexDbp
            var groupedTextures = TextureWrappers
                .Where(wrapper => wrapper.Texture.shTexDbp == selectedTexture.shTexDbp)
                .OrderBy(wrapper => wrapper.Id)
                .ToList();

            if (groupedTextures.Count == 1)
            {
                //if single texture, export normally
                ExportSingleTexture(index);
                return;
            }

            //prompt export path
            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Select folder to export combined textures";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string exportPath = fbd.SelectedPath;
                ExportCombinedTexture(groupedTextures, exportPath);
            }
        }


        private void ExportSingleTexture(int index)
        {
            Dpd.Texture texture = TextureWrappers[index].Texture;
            MtMaterial mat = GetMaterial(texture);

            SaveFileDialog sfd = new SaveFileDialog
            {
                Title = "Export image as PNG",
                FileName = $"Effect_{index}.png",
                Filter = "PNG Files|*.png"
            };

            if (sfd.ShowDialog() == DialogResult.OK)
            {
                mat.ExportAsPng(sfd.FileName);
            }
        }

        private void ExportCombinedTexture(List<DpdTextureWrapper> groupedTextures, string exportPath)
        {
            //get max width and height based on shX, shY, SizeX, & SizeY
            int maxWidth = groupedTextures.Max(wrapper => wrapper.Texture.shX + wrapper.SizeX);
            int maxHeight = groupedTextures.Max(wrapper => wrapper.Texture.shY + wrapper.SizeY);

            //crete a new combined image
            Bitmap combinedBitmap = new Bitmap(maxWidth, maxHeight);
            using (Graphics g = Graphics.FromImage(combinedBitmap))
            {
                g.Clear(System.Drawing.Color.Transparent); //transparent background

                //draw each texture in its correct position using shX and shY as coordinates
                foreach (var wrapper in groupedTextures)
                {
                    MtMaterial mat = GetMaterial(wrapper.Texture);
                    Bitmap textureBitmap = mat.DiffuseTextureBitmap;

                    //draw the texture at the appropriate position based on its shX, shY
                    g.DrawImage(textureBitmap, wrapper.Texture.shX, wrapper.Texture.shY);
                }
            }

            //generate a file name based on the shTexDbp value
            string fileName = $"Effect_Combined_{groupedTextures.First().Texture.shTexDbp}.png";
            string filePath = System.IO.Path.Combine(exportPath, fileName);
            combinedBitmap.Save(filePath, System.Drawing.Imaging.ImageFormat.Png);
        }

        public void ExportAllTextures()
        {
            if (TextureWrappers == null || TextureWrappers.Count == 0)
            {
                System.Windows.Forms.MessageBox.Show("No textures to export.", "Export Error",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Warning);
                return;
            }

            System.Windows.Forms.FolderBrowserDialog fbd = new System.Windows.Forms.FolderBrowserDialog();
            fbd.Description = "Select folder to export all textures";
            if (fbd.ShowDialog() == System.Windows.Forms.DialogResult.OK)
            {
                string exportPath = fbd.SelectedPath;

                //group textures by shTexDbp value
                var groupedTextures = TextureWrappers
                    .GroupBy(wrapper => wrapper.Texture.shTexDbp)
                    .ToList();

                foreach (var group in groupedTextures)
                {
                    ExportCombinedTexture(group.ToList(), exportPath);
                }

                System.Windows.Forms.MessageBox.Show("All textures exported successfully!", "Export Complete",
                    System.Windows.Forms.MessageBoxButtons.OK, System.Windows.Forms.MessageBoxIcon.Information);
            }
        }
        
        // Must have the same width and height and palette size
        public void ReplaceTexture(int index)
        {
            string filePath = "";
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Image Files|*.png";

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    filePath = openFileDialog.FileName;
                }
            }

            if (filePath == "") {
                return;
            }

            try
            {
                Bitmap pngBitmap = new Bitmap(filePath);
                Dpd.Texture texture = TextureWrappers[index].Texture;

                if(pngBitmap.Size.Width != texture.Size.Width ||
                   pngBitmap.Size.Height != texture.Size.Height)
                {
                    MessageBox.Show("Height/Width doesn't match original", "Image import error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                if(pngBitmap.Palette.Entries.Length > texture.Palette.Length)
                {
                    MessageBox.Show("Palette color count must be the same size or lower ("+ texture.Palette.Length + ")", "Image import error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }

                MtMaterial mat = GetMaterial(texture);
                mat.DiffuseTextureBitmap = pngBitmap;

                if(texture.format == 0x14) {
                    mat.BitmapToDataClut(16);
                }
                else {
                    mat.BitmapToDataClut(256);
                }

                texture.Data = mat.Data;
                texture.Palette = mat.Clut;

                for (int i = 0; i < texture.Data.Length; i++)
                {
                    texture.Data[i] = SwapBits34(texture.Data[i]);
                }

                // Alpha fix
                for (int i = 0; i < texture.Palette.Length; i++)
                {
                    if (i % 4 == 3)
                    {
                        if (texture.Palette[i] > 0)
                        {
                            texture.Palette[i] = (byte)((texture.Palette[i]  - 1) / 2);
                        }
                    }
                }

                if (texture.format == 0x14)
                {
                    byte[] tempData = new byte[mat.Data.Length/2];
                    for (int i = 0; i < texture.Data.Length; i+=2)
                    {
                        byte tempByte = (byte)(texture.Data[i] << 4);
                        tempByte += texture.Data[i + 1];
                        tempData[i / 2] = tempByte;
                    }
                }
            }
            catch(Exception ex)
            {
                MessageBox.Show("There was an error importing the image", "Image import error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private MtMaterial GetMaterial(Dpd.Texture texture)
        {
            MtMaterial mat = new MtMaterial();
            mat.Data = new byte[texture.Data.Length];
            Array.Copy(texture.Data, mat.Data, texture.Data.Length);
            mat.Clut = new byte[texture.Palette.Length];
            Array.Copy(texture.Palette, mat.Clut, texture.Palette.Length);
            mat.Width = texture.Size.Width;
            mat.Height = texture.Size.Height;
            mat.ColorSize = 1;
            mat.PixelHasAlpha = true;

            for(int i = 0; i < mat.Data.Length; i++)
            {
                mat.Data[i] = SwapBits34(mat.Data[i]);
            }

            // Alpha fix
            for (int i = 0; i < mat.Clut.Length; i++)
            {
                if (i % 4 == 3)
                {
                    if (mat.Clut[i] > 0)
                    {
                        mat.Clut[i] = (byte)(mat.Clut[i] * 2 - 1);
                    }
                }
            }

            if (texture.format == 0x14)
            {
                mat.Data = new byte[texture.Data.Length * 2]; // 2 pixels per byte
                for (int i = 0; i < texture.Data.Length; i++)
                {
                    byte biPixel = texture.Data[i];
                    mat.Data[(i * 2) + 1] = (byte)(biPixel >> 4);
                    mat.Data[i * 2] = (byte)(biPixel & 0b00001111);
                }
            }

            mat.GenerateBitmap();

            return mat;
        }

        public class DpdTextureWrapper
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public int SizeX { get; set; }
            public int SizeY { get; set; }
            public Dpd.Texture Texture { get; set; }
        }
    }
}

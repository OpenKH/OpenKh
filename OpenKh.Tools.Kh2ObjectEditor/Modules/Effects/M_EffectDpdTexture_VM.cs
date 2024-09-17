using ModelingToolkit.Objects;
using OpenKh.Kh2;
using System;
using System.Collections.ObjectModel;
using System.Drawing;
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
            if (ThisDpd?.TexturesList == null || ThisDpd.TexturesList.Count < 0)
                return;

            TextureWrappers.Clear();
            for (int i = 0; i < ThisDpd.TexturesList.Count; i++)
            {
                DpdTextureWrapper wrapper = new DpdTextureWrapper();
                wrapper.Id = i;
                wrapper.Name = "Texture " + i;
                wrapper.Texture = ThisDpd.TexturesList[i];
                wrapper.SizeX = wrapper.Texture.Size.Width;
                wrapper.SizeY = wrapper.Texture.Size.Height;

                TextureWrappers.Add(wrapper);
            }
        }

        public BitmapSource getTexture(int index)
        {
            Dpd.Texture texture = TextureWrappers[index].Texture;

            MtMaterial mat = GetMaterial(texture);
            return mat.GetAsBitmapImage();
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
            Dpd.Texture texture = TextureWrappers[index].Texture;
            MtMaterial mat = GetMaterial(texture);

            System.Windows.Forms.SaveFileDialog sfd;
            sfd = new System.Windows.Forms.SaveFileDialog();
            sfd.Title = "Export image as PNG";
            sfd.FileName = "Effect_" + index + ".png";
            sfd.ShowDialog();
            if (sfd.FileName != "")
            {
                mat.ExportAsPng(sfd.FileName);
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

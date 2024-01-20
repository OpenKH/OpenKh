using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renders;
using OpenKh.Imaging;
using OpenKh.Kh2;

namespace OpenKh.Engine.MonoGame
{
    public static class Extensions
    {
        public static Texture2D CreateTexture(this IImage image, GraphicsDevice graphicsDevice)
        {
            var size = image.Size;
            var texture = new Texture2D(graphicsDevice, size.Width, size.Height);
            if(image.PixelFormat != PixelFormat.Rgba1555)
            {
                texture.SetData(image.AsRgba8888());
            }
            else
            {
                texture = new Texture2D(graphicsDevice, 1, 1);
                byte[] pix = new byte[4];
                pix[0] = 0xFF;
                pix[1] = 0xFF;
                pix[2] = 0xFF;
                pix[3] = 0xFF;
                texture.SetData(pix);
            }

            return texture;
        }

        public static void SetRenderTexture(this KingdomShader shader, EffectPass pass, IKingdomTexture texture)
        {
            if (shader.Texture0 != texture.Texture2D)
            {
                shader.Texture0 = texture.Texture2D;
                switch (texture.AddressU)
                {
                    case ModelTexture.TextureWrapMode.Clamp:
                        shader.SetTextureRegionUDefault();
                        shader.TextureWrapModeU = TextureWrapMode.Clamp;
                        break;
                    case ModelTexture.TextureWrapMode.Repeat:
                        shader.SetTextureRegionUDefault();
                        shader.TextureWrapModeU = TextureWrapMode.Repeat;
                        break;
                    case ModelTexture.TextureWrapMode.RegionClamp:
                        shader.SetTextureRegionU(texture.RegionU);
                        shader.TextureWrapModeU = TextureWrapMode.Clamp;
                        break;
                    case ModelTexture.TextureWrapMode.RegionRepeat:
                        shader.SetTextureRegionU(texture.RegionU);
                        shader.TextureWrapModeU = TextureWrapMode.Repeat;
                        break;
                }
                switch (texture.AddressV)
                {
                    case ModelTexture.TextureWrapMode.Clamp:
                        shader.SetTextureRegionVDefault();
                        shader.TextureWrapModeV = TextureWrapMode.Clamp;
                        break;
                    case ModelTexture.TextureWrapMode.Repeat:
                        shader.SetTextureRegionVDefault();
                        shader.TextureWrapModeV = TextureWrapMode.Repeat;
                        break;
                    case ModelTexture.TextureWrapMode.RegionClamp:
                        shader.SetTextureRegionV(texture.RegionV);
                        shader.TextureWrapModeV = TextureWrapMode.Clamp;
                        break;
                    case ModelTexture.TextureWrapMode.RegionRepeat:
                        shader.SetTextureRegionV(texture.RegionV);
                        shader.TextureWrapModeV = TextureWrapMode.Repeat;
                        break;
                }

                pass.Apply();
            }
        }

        public static void SetRenderTexture(this KingdomShader shader, EffectPass pass, Texture2D texture)
        {
            if (shader.Texture0 != texture)
            {
                shader.Texture0 = texture;
                shader.SetTextureRegionVDefault();
                shader.SetTextureRegionUDefault();
                shader.TextureWrapModeU = TextureWrapMode.Clamp;
                shader.TextureWrapModeV = TextureWrapMode.Clamp;
                pass.Apply();
            }
        }
    }
}

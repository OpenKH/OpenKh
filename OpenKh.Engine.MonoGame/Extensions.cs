using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renders;
using OpenKh.Imaging;
using OpenKh.Kh2;

namespace OpenKh.Engine.MonoGame
{
    public static class Extensions
    {
        public static Texture2D CreateTexture(this IImageRead image, GraphicsDevice graphicsDevice)
        {
            var size = image.Size;
            var texture = new Texture2D(graphicsDevice, size.Width, size.Height);
            texture.SetData(image.AsBgra8888());

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

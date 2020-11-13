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

            texture.SetData(image.AsRgba8888());

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
                        shader.TextureRegionU = KingdomShader.DefaultTextureRegion;
                        shader.TextureWrapModeU = TextureWrapMode.Clamp;
                        break;
                    case ModelTexture.TextureWrapMode.Repeat:
                        shader.TextureRegionU = KingdomShader.DefaultTextureRegion;
                        shader.TextureWrapModeU = TextureWrapMode.Repeat;
                        break;
                    case ModelTexture.TextureWrapMode.RegionClamp:
                        shader.TextureRegionU = texture.RegionU;
                        shader.TextureWrapModeU = TextureWrapMode.Clamp;
                        break;
                    case ModelTexture.TextureWrapMode.RegionRepeat:
                        shader.TextureRegionU = texture.RegionU;
                        shader.TextureWrapModeU = TextureWrapMode.Repeat;
                        break;
                }
                switch (texture.AddressV)
                {
                    case ModelTexture.TextureWrapMode.Clamp:
                        shader.TextureRegionV = KingdomShader.DefaultTextureRegion;
                        shader.TextureWrapModeV = TextureWrapMode.Clamp;
                        break;
                    case ModelTexture.TextureWrapMode.Repeat:
                        shader.TextureRegionV = KingdomShader.DefaultTextureRegion;
                        shader.TextureWrapModeV = TextureWrapMode.Repeat;
                        break;
                    case ModelTexture.TextureWrapMode.RegionClamp:
                        shader.TextureRegionV = texture.RegionV;
                        shader.TextureWrapModeV = TextureWrapMode.Clamp;
                        break;
                    case ModelTexture.TextureWrapMode.RegionRepeat:
                        shader.TextureRegionV = texture.RegionV;
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
                shader.TextureRegionU = KingdomShader.DefaultTextureRegion;
                shader.TextureRegionV = KingdomShader.DefaultTextureRegion;
                shader.TextureWrapModeU = TextureWrapMode.Clamp;
                shader.TextureWrapModeV = TextureWrapMode.Clamp;
                pass.Apply();
            }
        }
    }
}

using Microsoft.Xna.Framework.Graphics;

namespace OpenKh.Engine.MonoGame
{
    public static class GraphicsDeviceExtensions
    {
        public static void RenderMeshNew(this GraphicsDevice graphics, KingdomShader shader,
            EffectPass pass, IMonoGameModel model, bool passRenderOpaque)
        {
            if (model?.MeshDescriptors == null)
                return;

            foreach (var meshDescriptor in model.MeshDescriptors)
            {
                if (meshDescriptor.Indices.Length == 0 || meshDescriptor.IsOpaque != passRenderOpaque)
                    continue;

                var textureIndex = meshDescriptor.TextureIndex & 0xffff;
                if (textureIndex < model.Textures.Length)
                    shader.SetRenderTexture(pass, model.Textures[textureIndex]);

                graphics.DrawUserIndexedPrimitives(
                    PrimitiveType.TriangleList,
                    meshDescriptor.Vertices,
                    0,
                    meshDescriptor.Vertices.Length,
                    meshDescriptor.Indices,
                    0,
                    meshDescriptor.Indices.Length / 3,
                    MeshLoader.PositionColoredTexturedVertexDeclaration);
            }
        }
    }
}

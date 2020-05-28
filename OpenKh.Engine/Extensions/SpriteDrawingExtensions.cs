using OpenKh.Engine.Renders;

namespace OpenKh.Engine.Extensions
{
    public static class SpriteDrawingExtensions
    {
        public static void SetProjection(this ISpriteDrawing spriteDrawing,
            float width, float height, float internalWidth, float internalHeight, float ratio)
        {
            var heightRatio = internalHeight / height;
            width *= heightRatio;
            height *= heightRatio;
            width *= ratio;

            var left = (internalWidth - width) / 2;
            spriteDrawing.SetViewport(left, width + left, 0, height);
        }
    }
}

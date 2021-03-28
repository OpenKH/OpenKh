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

        public static void FillRectangle(this ISpriteDrawing drawing, float x, float y, float width, float height, ColorF color)
        {
            drawing.AppendSprite(new SpriteDrawingContext()
                .Source(0, 0, 1, 1)
                .Position(x, y)
                .DestinationSize(width, height)
                .Color(color));
        }

        public static void DrawRectangle(this ISpriteDrawing drawing, float x, float y, float width, float height, ColorF color, float thickness = 1.0f)
        {
            drawing.FillRectangle(x, y, width, thickness, color);
            drawing.FillRectangle(x, y + height - 1, width - 1, thickness, color);
            drawing.FillRectangle(x, y, thickness, height, color);
            drawing.FillRectangle(x + width - 1, y, thickness, height, color);
        }
    }
}

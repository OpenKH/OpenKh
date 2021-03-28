using System;
using System.Drawing;

namespace OpenKh.Common
{
    public static class RectangleExtensions
    {
        public static Rectangle FlipX(this Rectangle rectangle) =>
            Rectangle.FromLTRB(rectangle.Right, rectangle.Top, rectangle.Left, rectangle.Bottom);

        public static Rectangle FlipY(this Rectangle rectangle) =>
            Rectangle.FromLTRB(rectangle.Left, rectangle.Bottom, rectangle.Right, rectangle.Top);

        public static Rectangle GetVisibility(this Rectangle rectangle)
        {
            if (rectangle.Width == 0 || rectangle.Height == 0)
                return Rectangle.Empty;

            if (rectangle.Width < 0)
                rectangle = rectangle.FlipX();
            if (rectangle.Height < 0)
                rectangle = rectangle.FlipY();

            return rectangle;
        }

        public static Rectangle Union(this Rectangle rect, Rectangle rectangle)
        {
            rect = rect.GetVisibility();
            rectangle = rectangle.GetVisibility();

            if (rect.IsEmpty)
                return rectangle;
            if (rectangle.IsEmpty)
                return rect;

            return Rectangle.FromLTRB(
                Math.Min(rect.Left, rectangle.Left),
                Math.Min(rect.Top, rectangle.Top),
                Math.Max(rect.Right, rectangle.Right),
                Math.Max(rect.Bottom, rectangle.Bottom));
        }

        public static Rectangle Traslate(this Rectangle rect, int x, int y) =>
            new Rectangle(rect.X + x, rect.Y + y, rect.Width, rect.Height);

        public static Rectangle Multiply(this Rectangle rect, float x, float y) =>
            Rectangle.FromLTRB(
                (int)Math.Round(rect.Left * x),
                (int)Math.Round(rect.Top * y),
                (int)Math.Round(rect.Right * x),
                (int)Math.Round(rect.Bottom * y));

        public static RectangleF ToRectangleF(this Rectangle rectangle) =>
            new RectangleF(rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height);
    }
}

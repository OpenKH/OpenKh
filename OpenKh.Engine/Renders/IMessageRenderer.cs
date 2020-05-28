namespace OpenKh.Engine.Renders
{
    public class DrawContext
    {
        public bool IgnoreDraw;
        public double xStart;
        public double x;
        public double y;
        public ColorF Color;
        public double WidthMultiplier;
        public double Scale;
        public double Width;
        public double Height;
        public double WindowWidth;

        public DrawContext()
        {
            Reset();
        }

        public void Reset()
        {
            Color = new ColorF(1.0f, 1.0f, 1.0f, 1.0f);
            WidthMultiplier = 1.0;
            Scale = 1.0;
        }

        public void NewLine(int fontHeight)
        {
            x = xStart;
            y += fontHeight * Scale;
        }
    }

    public interface IMessageRenderer
    {
        void Draw(DrawContext context, string message);

        void Draw(DrawContext context, byte[] data);
    }
}

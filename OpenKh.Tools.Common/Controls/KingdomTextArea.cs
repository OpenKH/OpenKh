using OpenKh.Tools.Common.Models;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Common.Controls
{
    public class KingdomTextArea : Control
    {
        private const int FontWidth = 18;
        private const int FontHeight = 24;

        public static DependencyProperty ContextProperty =>
            DependencyPropertyUtils.GetDependencyProperty<KingdomTextArea, KingdomTextContext>(nameof(Context), (o, x) => o.SetContext(x));

        private BitmapSource _imageFont;
        private int _charPerRow;

        public KingdomTextContext Context
        {
            get => GetValue(ContextProperty) as KingdomTextContext;
            set => SetValue(ContextProperty, value);
        }

        protected override void OnRender(DrawingContext drawingContext)
        {
            if (_imageFont == null)
                return;

            DrawBackground(drawingContext);
            DrawText(drawingContext, "Hello world!");
        }

        protected void DrawBackground(DrawingContext drawingContext)
        {
            var background = Background;
            if (background == null)
                return;

            var renderSize = RenderSize;
            drawingContext.DrawRectangle(background, null, new Rect(0.0, 0.0, renderSize.Width, renderSize.Height));
        }

        protected void DrawText(DrawingContext dc, string text)
        {
            double x = 0.0;
            foreach (var ch in text)
            {
                DrawChar(dc, x, 0.0, 0);
                x += FontWidth;
            }
        }

        protected void DrawChar(DrawingContext dc, double x, double y, int index)
        {
            DrawChar(dc, x, y, index % _charPerRow, index / _charPerRow);
        }

        protected void DrawChar(DrawingContext dc, double x, double y, int sourceX, int sourceY)
        {
            var croppedBitmap = new CroppedBitmap(_imageFont,
                new Int32Rect(sourceX * FontWidth, sourceY * FontHeight, FontWidth, FontHeight));

            dc.DrawImage(croppedBitmap, new Rect(x, y, FontWidth, FontHeight));
        }

        private void SetContext(KingdomTextContext context)
        {
            _imageFont = context.Font.GetWindowsMediaImage();
            _charPerRow = context.Font.Size.Width / FontWidth;

            InvalidateVisual();
        }
    }
}

using OpenKh.Tools.Common.Models;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace OpenKh.Tools.Common.Controls
{
    public class KingdomTextArea : Control
    {
        public static DependencyProperty ContextProperty =>
            DependencyPropertyUtils.GetDependencyProperty<KingdomTextArea, KingdomTextContext>(nameof(Context), (o, x) => o.SetContext(x));

        private ImageSource _imageFont;

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

        protected void DrawText(DrawingContext drawingContext, string text)
        {
            drawingContext.DrawImage(_imageFont, new Rect
            (
                0, 0, _imageFont.Width, _imageFont.Height
            ));
        }

        private void SetContext(KingdomTextContext context)
        {
            _imageFont = context.Font.GetWindowsMediaImage();

            InvalidateVisual();
        }
    }
}

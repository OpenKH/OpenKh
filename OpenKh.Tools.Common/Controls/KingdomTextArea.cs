using OpenKh.Kh2.Messages;
using OpenKh.Tools.Common.Models;
using System.Collections.Generic;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace OpenKh.Tools.Common.Controls
{
    public class KingdomTextArea : Control
    {
        private class DrawContext
        {
            public double x;
            public double y;
        }

        private const int FontWidth = 18;
        private const int FontHeight = 24;

        public static DependencyProperty ContextProperty =>
            DependencyPropertyUtils.GetDependencyProperty<KingdomTextArea, KingdomTextContext>(nameof(Context), (o, x) => o.SetContext(x));

        private IMessageEncode _encode;
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
            Draw(drawingContext, "Hello world!");
        }

        protected void DrawBackground(DrawingContext drawingContext)
        {
            var background = Background;
            if (background == null)
                return;

            var renderSize = RenderSize;
            drawingContext.DrawRectangle(background, null, new Rect(0.0, 0.0, renderSize.Width, renderSize.Height));
        }

        protected void Draw(DrawingContext dc, string text)
        {
            var commands = MsgSerializer.DeserializeText(text);
            Draw(dc, commands);
        }

        protected void Draw(DrawingContext dc, IEnumerable<MessageCommandModel> commands)
        {
            var context = new DrawContext();
            foreach (var command in commands)
                Draw(dc, context, command);
        }

        private void Draw(DrawingContext dc, DrawContext context, MessageCommandModel command)
        {
            if (command.Command == MessageCommand.PrintText)
                DrawText(dc, context, command);
        }

        private void DrawText(DrawingContext dc, DrawContext context, MessageCommandModel command)
        {
            var data = _encode.Encode(new List<MessageCommandModel>
            {
                command
            });

            DrawText(dc, context, data);
        }

        private void DrawText(DrawingContext dc, DrawContext context, byte[] data)
        {
            foreach (var ch in data)
            {
                if (ch >= 0x20)
                {
                    DrawChar(dc, context.x, context.y, ch - 0x20);
                    context.x += FontWidth;
                }
                else if (ch == 1)
                {
                    context.x += FontWidth;
                }
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
            _encode = context.Encode;
            _imageFont = context.Font.GetWindowsMediaImage();
            _charPerRow = context.Font.Size.Width / FontWidth;

            InvalidateVisual();
        }
    }
}

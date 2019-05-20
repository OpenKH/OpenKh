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
            public double xStart;
            public double x;
            public double y;
        }

        private const int FontWidth = 18;
        private const int FontHeight = 24;
        private const int IconWidth = 24;
        private const int IconHeight = 24;

        public static DependencyProperty ContextProperty =>
            DependencyPropertyUtils.GetDependencyProperty<KingdomTextArea, KingdomTextContext>(nameof(Context), (o, x) => o.SetContext(x));

        private byte[] _fontSpacing;
        private byte[] _iconSpacing;
        private BitmapSource _imageFont;
        private BitmapSource _imageIcon;
        private int _charPerRow;
        private int _iconPerRow;
        private IMessageEncode _encode;

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
            else if (command.Command == MessageCommand.PrintComplex)
                DrawText(dc, context, command);
            else if (command.Command == MessageCommand.PrintIcon)
                DrawIcon(dc, context, command.Data[0]);
            else if (command.Command == MessageCommand.NewLine)
            {
                context.x = context.xStart;
                context.y += FontHeight;
            }
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
                    int chIndex = ch - 0x20;
                    DrawChar(dc, context.x, context.y, chIndex);
                    context.x += _fontSpacing?[chIndex] ?? FontWidth;
                }
                else if (ch == 1)
                {
                    context.x += 6;
                }
            }
        }

        private void DrawIcon(DrawingContext dc, DrawContext context, byte data)
        {
            if (_imageIcon != null)
                DrawIcon(dc, context.x, context.y, data);

            context.x += _iconSpacing?[data] ?? IconWidth;
        }

        protected void DrawChar(DrawingContext dc, double x, double y, int index) =>
            DrawChar(dc, x, y, (index % _charPerRow) * FontWidth, (index / _charPerRow) * FontHeight);

        protected void DrawChar(DrawingContext dc, double x, double y, int sourceX, int sourceY) =>
            DrawImage(dc, _imageFont, x, y, sourceX, sourceY, FontWidth, FontHeight);

        protected void DrawIcon(DrawingContext dc, double x, double y, int index) =>
            DrawIcon(dc, x, y, (index % _iconPerRow) * IconWidth, (index / _iconPerRow) * IconHeight);

        protected void DrawIcon(DrawingContext dc, double x, double y, int sourceX, int sourceY) =>
            DrawImage(dc, _imageIcon, x, y, sourceX, sourceY, IconWidth, IconHeight);

        protected void DrawImage(DrawingContext dc, BitmapSource bitmap, double x, double y, int sourceX, int sourceY, int width, int height)
        {
            var croppedBitmap = new CroppedBitmap(bitmap,
                new Int32Rect(sourceX, sourceY, width, height));

            dc.DrawImage(croppedBitmap, new Rect(x, y, width, height));
        }

        private void SetContext(KingdomTextContext context)
        {
            _fontSpacing = context.FontSpacing;
            _iconSpacing = context.IconSpacing;
            _imageFont = context.Font?.GetWindowsMediaImage();
            _imageIcon = context.Icon?.GetWindowsMediaImage();
            _charPerRow = context.Font?.Size.Width / FontWidth ?? 1;
            _iconPerRow = context.Icon?.Size.Width / IconWidth ?? 1;
            _encode = context.Encode;

            InvalidateVisual();
        }
    }
}

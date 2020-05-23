using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System;
using System.Collections.Generic;
using System.Drawing;
using Xe.Drawing;

namespace OpenKh.Engine.Renders
{
    public class RenderingMessageContext
    {
        public IImageRead Font { get; set; }
        public IImageRead Font2 { get; set; }
        public IImageRead Icon { get; set; }
        public byte[] FontSpacing { get; set; }
        public byte[] IconSpacing { get; set; }
        public IMessageEncoder Encoder { get; set; }

        public int FontWidth { get; set; }
        public int FontHeight { get; set; }
        public int TableHeight { get; set; }
    }

    public class Kh2MessageRenderer : IMessageRenderer, IDisposable
    {
        private const int IconWidth = Constants.FontIconWidth;
        private const int IconHeight = Constants.FontIconHeight;

        private readonly IDrawing _drawing;
        private readonly RenderingMessageContext _msgContext;

        private readonly byte[] _fontSpacing;
        private readonly byte[] _iconSpacing;
        private readonly IImageRead _imageFont;
        private readonly IImageRead _imageFont2;
        private readonly IImageRead _imageIcon;
        private readonly ISurface _surfaceFont;
        private readonly ISurface _surfaceFont2;
        private readonly ISurface _surfaceIcon;
        private readonly int _charPerRow;
        private readonly int _iconPerRow;
        private readonly int _tableHeight;
        private readonly int _charTableHeight;
        private readonly IMessageEncode _encode;

        public Kh2MessageRenderer(
            IDrawing drawing,
            RenderingMessageContext context)
        {
            _drawing = drawing;
            _msgContext = context;

            _fontSpacing = context.FontSpacing;
            _iconSpacing = context.IconSpacing;
            _imageFont = context.Font;
            _imageFont2 = context.Font2;
            _imageIcon = context.Icon;
            _charPerRow = context.Font?.Size.Width / context.FontWidth ?? 1;
            _iconPerRow = context.Icon?.Size.Width / IconWidth ?? 1;
            _tableHeight = context.TableHeight;
            _charTableHeight = context.TableHeight / context.FontHeight * context.FontHeight;
            _encode = context.Encoder;

            if (_imageFont != null) InitializeSurface(ref _surfaceFont, _imageFont);
            if (_imageFont2 != null) InitializeSurface(ref _surfaceFont2, _imageFont2);
            if (_imageIcon != null) InitializeSurface(ref _surfaceIcon, _imageIcon);
        }

        public void Draw(DrawContext drawContext,  string message) =>
            Draw(drawContext, MsgSerializer.DeserializeText(message));

        public void Draw(DrawContext drawContext, byte[] data)
        {
            var commands = _msgContext.Encoder.Decode(data);
            Draw(drawContext, commands);
        }

        public void Draw(DrawContext drawContext, IEnumerable<MessageCommandModel> commands)
        {
            if (commands == null)
                return;

            foreach (var command in commands)
                Draw(drawContext, command);
        }

        private void Draw(DrawContext context, MessageCommandModel command)
        {
            if (command.Command == MessageCommand.PrintText)
                DrawText(context, command);
            else if (command.Command == MessageCommand.PrintComplex)
                DrawText(context, command);
            else if (command.Command == MessageCommand.PrintIcon)
                DrawIcon(context, command.Data[0]);
            else if (command.Command == MessageCommand.Color)
                SetColor(context, command.Data);
            else if (command.Command == MessageCommand.Reset)
                context.Reset();
            else if (command.Command == MessageCommand.Clear)
            {
                context.NewLine(_msgContext.FontHeight);
                context.y += 4;
                _drawing.FillRectangle(new RectangleF(
                    8,
                    (float)context.y,
                    Math.Max(1.0f, (float)(context.WindowWidth - 16)),
                    2), Color.White);
                context.y += 4;
            }
            else if (command.Command == MessageCommand.Position)
            {
                context.x = command.PositionX;
                context.y = command.PositionY;
            }
            else if (command.Command == MessageCommand.TextWidth)
                context.WidthMultiplier = command.TextWidth;
            else if (command.Command == MessageCommand.TextScale)
                context.Scale = command.TextScale;
            else if (command.Command == MessageCommand.Tabulation)
                context.x += 16; // TODO this is not the real tabulation size
            else if (command.Command == MessageCommand.NewLine)
                context.NewLine(_msgContext.FontHeight);

            context.Width = Math.Max(context.Width, context.x);
            context.Height = Math.Max(context.Height, context.y + _msgContext.FontHeight * context.Scale);
        }

        private void SetColor(DrawContext context, byte[] data)
        {
            context.Color.R = data[0] / 255.0f;
            context.Color.G = data[1] / 255.0f;
            context.Color.B = data[2] / 255.0f;
            context.Color.A = data[3] / 128.0f;
        }

        private void DrawText(DrawContext context, MessageCommandModel command)
        {
            if (_msgContext.Encoder == null)
                return;

            var data = _msgContext.Encoder.Encode(new List<MessageCommandModel>
            {
                command
            });

            DrawText(context, data);
        }

        private void DrawText(DrawContext context, byte[] data)
        {
            for (int i = 0; i < data.Length; i++)
            {
                byte ch = data[i];
                int spacing;

                if (ch >= 0x20)
                {
                    int chIndex = ch - 0x20;
                    if (!context.IgnoreDraw)
                        DrawChar(context, chIndex);
                    spacing = _fontSpacing?[chIndex] ?? _msgContext.FontWidth;
                }
                else if (ch >= 0x19 && ch <= 0x1f)
                {
                    int chIndex = data[++i] + (ch - 0x19) * 0x100 + 0xE0;
                    if (!context.IgnoreDraw)
                        DrawChar(context, chIndex);
                    spacing = _fontSpacing?[chIndex] ?? _msgContext.FontWidth;
                }
                else if (ch == 1)
                {
                    spacing = 6;
                }
                else
                {
                    spacing = 0;
                }

                context.x += spacing * context.WidthMultiplier * context.Scale;
            }
        }

        private void DrawIcon(DrawContext context, byte index)
        {
            if (_surfaceIcon != null)
                DrawIcon(context, (index % _iconPerRow) * IconWidth, (index / _iconPerRow) * IconHeight);

            context.x += _iconSpacing?[index] ?? IconWidth;
        }

        protected void DrawChar(DrawContext context, int index)
        {
            DrawChar(context, (index % _charPerRow) * _msgContext.FontWidth, (index / _charPerRow) * _msgContext.FontHeight);
        }

        protected void DrawChar(DrawContext context, int sourceX, int sourceY)
        {
            ISurface surfaceFont;

            var tableIndex = sourceY / _charTableHeight;
            sourceY %= _charTableHeight;

            if ((tableIndex & 1) != 0)
                surfaceFont = _surfaceFont2;
            else
                surfaceFont = _surfaceFont;

            if ((tableIndex & 2) != 0)
                sourceY += _tableHeight;

            if (surfaceFont == null)
                return;

            DrawImageScale(context, surfaceFont, sourceX, sourceY, _msgContext.FontWidth, _msgContext.FontHeight);
        }

        protected void DrawIcon(DrawContext context, int sourceX, int sourceY) =>
            DrawImage(_surfaceIcon, context.x, context.y, sourceX, sourceY, IconWidth, IconHeight, 1.0, 1.0, new ColorF(1.0f, 1.0f, 1.0f, 1.0f));

        protected void DrawImageScale(DrawContext context, ISurface surface, int sourceX, int sourceY, int width, int height) =>
            DrawImage(surface, context.x, context.y, sourceX, sourceY, width, height, context.WidthMultiplier * context.Scale, context.Scale, context.Color);

        protected void DrawImage(ISurface surface, double x, double y, int sourceX, int sourceY, int width, int height, double scaleX, double scaleY, ColorF color)
        {
            var dstWidth = width * scaleX;
            var dstHeight = height * scaleY;
            var src = new Rectangle(sourceX, sourceY, width, height);
            var dst = new Rectangle((int)x, (int)y, (int)dstWidth, (int)dstHeight);

            _drawing.DrawSurface(surface, src, dst, color);
        }

        private void InitializeSurface(ref ISurface surface, IImageRead image)
        {
            surface?.Dispose();
            surface = _drawing?.CreateSurface(image);
        }

        public void Dispose()
        {
            _surfaceFont?.Dispose();
            _surfaceFont2?.Dispose();
            _surfaceIcon?.Dispose();
        }
    }
}

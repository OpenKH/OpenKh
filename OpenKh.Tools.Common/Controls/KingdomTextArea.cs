using OpenKh.Tools.Common.Controls;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Common.Models;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using Xe.Drawing;

namespace OpenKh.Tools.Common.Controls
{
    public class KingdomTextArea : DrawPanel
    {
        protected class DrawContext
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

        private const int IconWidth = Constants.FontIconWidth;
        private const int IconHeight = Constants.FontIconHeight;
        private const int CharactersPerTextureBlock = 392;
        private const int CharactersPerTexture = 784;
        public static DependencyProperty ContextProperty =
            DependencyPropertyUtils.GetDependencyProperty<KingdomTextArea, KingdomTextContext>(
                nameof(Context), (o, x) => o.SetContext(x));

        public static DependencyProperty MessageCommandsProperty =
            DependencyPropertyUtils.GetDependencyProperty<KingdomTextArea, IEnumerable<MessageCommandModel>>(
                nameof(MessageCommands), (o, x) => o.SetTextCommands(x));

        public static DependencyProperty BackgroundProperty =
            DependencyPropertyUtils.GetDependencyProperty<KingdomTextArea, System.Windows.Media.Color>(
                nameof(Background), (o, x) => { });

        private byte[] _fontSpacing;
        private byte[] _iconSpacing;
        private IImageRead _imageFont;
        private IImageRead _imageFont2;
        private IImageRead _imageIcon;
        private ISurface _surfaceFont;
        private ISurface _surfaceFont2;
        private ISurface _surfaceIcon;
        private int _charPerRow;
        private int _iconPerRow;
        private int _tableHeight;
        private int _charTableHeight;
        private IMessageEncode _encode;

        public KingdomTextContext Context
        {
            get => GetValue(ContextProperty) as KingdomTextContext;
            set => SetValue(ContextProperty, value);
        }

        public IEnumerable<MessageCommandModel> MessageCommands
        {
            get => GetValue(MessageCommandsProperty) as IEnumerable<MessageCommandModel>;
            set => SetValue(MessageCommandsProperty, value);
        }

        public System.Windows.Media.Color Background
        {
            get => (System.Windows.Media.Color)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        protected override System.Windows.Size MeasureOverride(System.Windows.Size availableSize)
        {
            var drawContext = new DrawContext()
            {
                IgnoreDraw = true
            };

            Draw(drawContext, MessageCommands);

            return new System.Windows.Size(drawContext.Width, drawContext.Height);
        }

        protected override void OnDrawCreate()
        {
            base.OnDrawCreate();
            GetOrInitializeSurface(ref _surfaceFont, _imageFont);
            GetOrInitializeSurface(ref _surfaceFont2, _imageFont2);
            GetOrInitializeSurface(ref _surfaceIcon, _imageIcon);
        }

        protected override void OnDrawDestroy()
        {
            base.OnDrawDestroy();
        }

        protected override void OnDrawBegin()
        {
            base.OnDrawBegin();

            DrawBackground();

            if (_surfaceFont == null)
                return;
            Draw(new DrawContext(), MessageCommands);
            Drawing.Flush();
        }

        protected override void OnDrawEnd()
        {
            base.OnDrawEnd();
        }

        protected void DrawBackground()
        {
            var backgroundColor = Background;
            var color = Color.FromArgb(
                backgroundColor.A,
                backgroundColor.R,
                backgroundColor.G,
                backgroundColor.B);

            Drawing.Clear(color);
        }

        protected void Draw(DrawContext drawContext, string text)
        {
            var commands = MsgSerializer.DeserializeText(text);
            Draw(drawContext, commands);
        }

        protected void Draw(DrawContext drawContext, IEnumerable<MessageCommandModel> commands)
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
                context.NewLine(Context.FontHeight);
                context.y += 4;
                Drawing.FillRectangle(new RectangleF(8, (float)context.y, Math.Max(1.0f, (float)(ActualWidth - 16)), 2), Color.White);
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
                context.x += 16; // TODO random number
            else if (command.Command == MessageCommand.NewLine)
                context.NewLine(Context.FontHeight);

            context.Width = Math.Max(context.Width, context.x);
            context.Height = Math.Max(context.Height, context.y + Context.FontHeight * context.Scale);
        }

        private void SetColor(DrawContext context, byte[] data)
        {
            context.Color.R = data[0] / 255.0f;
            context.Color.G = data[1] / 255.0f;
            context.Color.B = data[2] / 255.0f;
            context.Color.A = Math.Min(data[3] * 2, Byte.MaxValue) / 255.0f;
        }

        private void DrawText(DrawContext context, MessageCommandModel command)
        {
            if (_encode == null)
                return;

            var data = _encode.Encode(new List<MessageCommandModel>
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
                    spacing = _fontSpacing?[chIndex] ?? Context.FontWidth;
                }
                else if (ch >= 0x19 && ch <= 0x1f)
                {
                    int chIndex = data[++i] + (ch - 0x19) * 0x100 + 0xE0;
                    if (!context.IgnoreDraw)
                        DrawChar(context, chIndex);
                    spacing = _fontSpacing?[chIndex] ?? Context.FontWidth;
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
            DrawChar(context, (index % _charPerRow) * Context.FontWidth, (index / _charPerRow) * Context.FontHeight);
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

            DrawImageScale(context, surfaceFont, sourceX, sourceY, Context.FontWidth, Context.FontHeight);
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

            Drawing.DrawSurface(surface, src, dst, color);
        }

        private void SetContext(KingdomTextContext context)
        {
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

            if (_imageFont != null)
                InitializeSurface(ref _surfaceFont, _imageFont);

            if (_imageFont2 != null)
                InitializeSurface(ref _surfaceFont2, _imageFont2);

            if (_imageIcon != null)
                InitializeSurface(ref _surfaceIcon, _imageIcon);

            InvalidateVisual();
        }

        private void SetTextCommands(IEnumerable<MessageCommandModel> textCommands)
        {
            InvalidateMeasure();
            InvalidateVisual();
        }

        private void GetOrInitializeSurface(ref ISurface surface, IImageRead image)
        {
            if (surface != null)
                return;

            if (surface == null && image == null)
                return;

            InitializeSurface(ref surface, image);
        }

        private void InitializeSurface(ref ISurface surface, IImageRead image)
        {
            surface?.Dispose();
            surface = Drawing?.CreateSurface(image);
        }
    }
}

using kh.tools.common.Controls;
using OpenKh.Imaging;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Common.Models;
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
            public double xStart;
            public double x;
            public double y;
        }

        private const int FontWidth = 18;
        private const int FontHeight = 24;
        private const int IconWidth = 24;
        private const int IconHeight = 24;

        public static DependencyProperty ContextProperty =
            DependencyPropertyUtils.GetDependencyProperty<KingdomTextArea, KingdomTextContext>(
                nameof(Context), (o, x) => o.SetContext(x));

        public static DependencyProperty MessageCommandsProperty =
            DependencyPropertyUtils.GetDependencyProperty<KingdomTextArea, IEnumerable<MessageCommandModel>>(
                nameof(MessageCommands), (o, x) => o.SetTextCommands(x));

        private byte[] _fontSpacing;
        private byte[] _iconSpacing;
        private IImageRead _imageFont;
        private IImageRead _imageIcon;
        private ISurface _surfaceFont;
        private ISurface _surfaceIcon;
        private int _charPerRow;
        private int _iconPerRow;
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

        protected override void OnDrawCreate()
        {
            base.OnDrawCreate();
            GetOrInitializeSurface(ref _surfaceFont, _imageFont);
            GetOrInitializeSurface(ref _surfaceIcon, _imageIcon);
        }

        protected override void OnDrawDestroy()
        {
            base.OnDrawDestroy();
        }

        protected override void OnDrawBegin()
        {
            base.OnDrawBegin();

            if (Drawing == null)
                return;
            DrawBackground();

            if (_surfaceFont == null)
                return;
            Draw(MessageCommands);
        }

        protected override void OnDrawEnd()
        {
            base.OnDrawEnd();
        }

        protected void DrawBackground()
        {
            Drawing.Clear(System.Drawing.Color.Magenta);
        }

        protected void Draw(string text)
        {
            var commands = MsgSerializer.DeserializeText(text);
            Draw(commands);
        }

        protected void Draw(IEnumerable<MessageCommandModel> commands)
        {
            if (commands == null)
                return;

            var context = new DrawContext();
            foreach (var command in commands)
                Draw(context, command);
        }

        private void Draw(DrawContext context, MessageCommandModel command)
        {
            if (command.Command == MessageCommand.PrintText)
                DrawText(context, command);
            else if (command.Command == MessageCommand.PrintComplex)
                DrawText(context, command);
            else if (command.Command == MessageCommand.PrintIcon)
                DrawIcon(context, command.Data[0]);
            else if (command.Command == MessageCommand.NewLine)
            {
                context.x = context.xStart;
                context.y += FontHeight;
            }
        }

        private void DrawText(DrawContext context, MessageCommandModel command)
        {
            var data = _encode.Encode(new List<MessageCommandModel>
            {
                command
            });

            DrawText(context, data);
        }

        private void DrawText(DrawContext context, byte[] data)
        {
            foreach (var ch in data)
            {
                if (ch >= 0x20)
                {
                    int chIndex = ch - 0x20;
                    DrawChar(context, chIndex);
                    context.x += _fontSpacing?[chIndex] ?? FontWidth;
                }
                else if (ch == 1)
                {
                    context.x += 6;
                }
            }
        }

        private void DrawIcon(DrawContext context, byte index)
        {
            if (_surfaceIcon != null)
                DrawIcon(context, (index % _iconPerRow) * IconWidth, (index / _iconPerRow) * IconHeight);

            context.x += _iconSpacing?[index] ?? IconWidth;
        }

        protected void DrawChar(DrawContext context, int index) =>
            DrawChar(context, (index % _charPerRow) * FontWidth, (index / _charPerRow) * FontHeight);

        protected void DrawChar(DrawContext context, int sourceX, int sourceY) =>
            DrawImage(context, _surfaceFont, sourceX, sourceY, FontWidth, FontHeight);

        protected void DrawIcon(DrawContext context, int sourceX, int sourceY) =>
            DrawImage(context, _surfaceIcon, sourceX, sourceY, IconWidth, IconHeight);

        protected void DrawImage(DrawContext context, ISurface surface, int sourceX, int sourceY, int width, int height)
        {
            var src = new Rectangle(sourceX, sourceY, width, height);
            var dst = new Rectangle((int)context.x, (int)context.y, width, height);

            Drawing.DrawSurface(surface, src, dst);
        }

        private void SetContext(KingdomTextContext context)
        {
            _fontSpacing = context.FontSpacing;
            _iconSpacing = context.IconSpacing;
            _imageFont = context.Font;
            _imageIcon = context.Icon;
            _charPerRow = context.Font?.Size.Width / FontWidth ?? 1;
            _iconPerRow = context.Icon?.Size.Width / IconWidth ?? 1;
            _encode = context.Encode;

            InitializeSurface(ref _surfaceFont, _imageFont);
            InitializeSurface(ref _surfaceIcon, _imageIcon);

            InvalidateVisual();
        }

        private void SetTextCommands(IEnumerable<MessageCommandModel> textCommands)
        {
            InvalidateVisual();
        }

        private void GetOrInitializeSurface(ref ISurface surface, IImageRead image)
        {
            if (surface != null)
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

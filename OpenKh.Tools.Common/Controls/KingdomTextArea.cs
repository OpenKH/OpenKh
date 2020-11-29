using OpenKh.Kh2.Messages;
using System.Collections.Generic;
using System.Drawing;
using System.Windows;
using OpenKh.Engine.Renders;

namespace OpenKh.Tools.Common.Controls
{
    public class KingdomTextArea : DrawPanel
    {
        public static DependencyProperty ContextProperty =
            DependencyPropertyUtils.GetDependencyProperty<KingdomTextArea, RenderingMessageContext>(
                nameof(Context), (o, x) => o.SetContext(x));

        public static DependencyProperty MessageCommandsProperty =
            DependencyPropertyUtils.GetDependencyProperty<KingdomTextArea, IEnumerable<MessageCommandModel>>(
                nameof(MessageCommands), (o, x) => o.SetTextCommands(x));

        public static DependencyProperty BackgroundProperty =
            DependencyPropertyUtils.GetDependencyProperty<KingdomTextArea, System.Windows.Media.Color>(
                nameof(Background), (o, x) => { });

        private Kh2MessageRenderer _messageRenderer;

        public RenderingMessageContext Context
        {
            get => GetValue(ContextProperty) as RenderingMessageContext;
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
        }

        protected override void OnDrawDestroy()
        {
            base.OnDrawDestroy();
        }

        protected override void OnDrawBegin()
        {
            base.OnDrawBegin();

            DrawBackground();

            Draw(new DrawContext()
            {
                GlobalScale = 1.0
            }, MessageCommands);
            Drawing.Flush();
        }

        protected override void OnDrawEnd()
        {
            base.OnDrawEnd();
        }

        protected void DrawBackground()
        {
            var backgroundColor = Background;
            Drawing.Clear(ColorF.FromRgba(backgroundColor.R, backgroundColor.G, backgroundColor.B, backgroundColor.A));
        }

        protected void Draw(DrawContext drawContext, IEnumerable<MessageCommandModel> commands)
        {
            drawContext.WindowWidth = ActualWidth;
            _messageRenderer?.Draw(drawContext, commands);
        }

        private void SetContext(RenderingMessageContext context)
        {
            _messageRenderer = new Kh2MessageRenderer(Drawing, new RenderingMessageContext
            {
                Font = context.Font,
                Font2 = context.Font2,
                Icon = context.Icon,
                FontSpacing = context.FontSpacing,
                IconSpacing = context.IconSpacing,
                Encoder = context.Encoder,
                FontWidth = context.FontWidth,
                FontHeight = context.FontHeight,
                TableHeight = context.TableHeight,
            });

            InvalidateVisual();
        }

        private void SetTextCommands(IEnumerable<MessageCommandModel> textCommands)
        {
            InvalidateMeasure();
            InvalidateVisual();
        }
    }
}

using kh.kh2;
using System;
using System.Drawing;
using Xe.Drawing;

namespace kh.tools.layout.Renderer
{
    public class SequenceRenderer
    {
        private class Context
        {
            public int FrameIndex { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float ScaleX { get; set; }
            public float ScaleY { get; set; }
            public ColorF Color { get; set; }
            public int ColorBlendType { get; set; }
            public float Left { get; set; }
            public float Top { get; set; }
            public float Right { get; set; }
            public float Bottom { get; set; }

            public Context Clone() => new Context
            {
                FrameIndex = FrameIndex,
                PositionX = PositionX,
                PositionY = PositionY,
                ScaleX = ScaleX,
                ScaleY = ScaleY,
                Color = Color,
                ColorBlendType = ColorBlendType,
                Left = Left,
                Top = Top,
                Right = Right,
                Bottom = Bottom
            };
        }

        private const int SincInterpolationFlag = 0x00000001;
        private const int ScalingFlag = 0x00000040;
        private const int ColorMaskingFlag = 0x00000400;
        private const int ColorInterpolationFlag = 0x00000080;
        private const int TraslateFlag = 0x00004000;

        private readonly Sequence sequence;
        private readonly IDrawing drawing;
        private readonly ISurface surface;

        public SequenceRenderer(Sequence sequence, IDrawing drawing, ISurface surface)
        {
            this.sequence = sequence;
            this.drawing = drawing;
            this.surface = surface;
        }

        public void Draw(int animationGroupIndex, int frameIndex, float positionX, float positionY) =>
            DrawAnimationGroup(new Context
            {
                FrameIndex = frameIndex,
                PositionX = positionX,
                PositionY = positionY
            }, sequence.Q5Items[animationGroupIndex]);

        private void DrawAnimationGroup(Context contextParent, Sequence.Q5 q5)
        {
            var index = q5.Q4Index;
            var count = q5.Count;
            var context = contextParent.Clone();

            //if (q5.Tick1 == 0)
            //    context.CurrentFrameIndex = 0;
            //else if (q5.Tick2 == 0)
            //    context.CurrentFrameIndex = Math.Min(context.CurrentFrameIndex, q5.Tick1);
            //else
            //    context.CurrentFrameIndex = (context.CurrentFrameIndex < q5.Tick1) ? context.CurrentFrameIndex :
            //        (q5.Tick1 + ((context.CurrentFrameIndex - q5.Tick1) % (q5.Tick2 - q5.Tick1)));

            if (q5.Tick2 != 0)
                context.FrameIndex = (context.FrameIndex < q5.Tick1) ? context.FrameIndex :
                (q5.Tick1 + ((context.FrameIndex - q5.Tick1) % (q5.Tick2 - q5.Tick1)));

            for (var i = 0; i < count; i++)
            {
                DrawAnimation(context, sequence.Q4Items[index + i]);
            }
        }

        private void DrawAnimation(Context contextParent, Sequence.Q4 q4)
        {
            // 0000 0001 = (0 = SINC INTERPOLATION, 1 = LINEAR INTERPOLATION)
            // 0000 0008 = (0 = BOUNCING START FROM CENTER, 1 = BOUNCING START FROM X / MOVE FROM Y)
            // 0000 0010 = (0 = ENABLE BOUNCING, 1 = IGNORE BOUNCING)
            // 0000 0020 = (0 = ENABLE ROTATION, 1 = IGNORE ROTATION)
            // 0000 0040 = (0 = ENABLE SCALING, 1 = IGNORE SCALING)
            // 0000 0080 = (0 = ENABLE COLOR FADING, 1 = IGNORE COLOR FADING)
            // 0000 0400 = (0 = ENABLE COLOR MASKING, 1 = IGNORE COLOR MASKING)
            // 0000 4000 = (0 = ENABLE XYB, 1 = IGNORE XYB)

            if (contextParent.FrameIndex < q4.FrameStart || contextParent.FrameIndex > q4.FrameEnd)
                return;

            var context = contextParent.Clone();
            var delta = (float)(context.FrameIndex - q4.FrameStart) / (q4.FrameEnd - q4.FrameStart);
            float t;

            if ((q4.Flags & SincInterpolationFlag) != 0)
                t = delta;
            else
                t = (float)Math.Sin(delta * Math.PI / 2);

            context.PositionX += Lerp(t, q4.Xa0, q4.Xa1);
            context.PositionY += Lerp(t, q4.Ya0, q4.Ya1);
            context.ColorBlendType = q4.ColorBlend;

            if ((q4.Flags & ScalingFlag) != 0)
            {
                var scale = Lerp(t, q4.ScaleStart, q4.ScaleEnd);
                var scaleX = Lerp(t, q4.ScaleXStart, q4.ScaleXEnd);
                var scaleY = Lerp(t, q4.ScaleYStart, q4.ScaleYEnd);
                context.ScaleX = scale * scaleX;
                context.ScaleY = scale * scaleY;
            }
            else
            {
                context.ScaleX = 1.0f;
                context.ScaleY = 1.0f;
            }

            if ((q4.Flags & ColorMaskingFlag) != 0)
            {
                if ((q4.Flags & ColorInterpolationFlag) != 0)
                {
                    context.Color = Lerp(t,
                        ConvertColor(q4.ColorStart),
                        ConvertColor(q4.ColorEnd));
                }
                else
                {
                    context.Color = ConvertColor(q4.ColorStart);
                }
            }
            else
                context.Color = ConvertColor(q4.ColorStart);

            if ((q4.Flags & TraslateFlag) != 0)
            {
                context.PositionX += Lerp(t, q4.Xb0, q4.Xb1);
                context.PositionY += Lerp(t, q4.Yb0, q4.Yb1);
            }

            // CALCULATE TRANSOFRMATIONS AND INTERPOLATIONS
            DrawFrameGroup(context, sequence.Q3Items[q4.Q3Index]);
        }

        private void DrawFrameGroup(Context context, Sequence.Q3 q3)
        {
            var index = q3.Start;
            var count = q3.Count;
            for (var i = 0; i < count; i++)
            {
                DrawFrameExtended(context, sequence.Q2Items[index + i]);
            }
        }

        private void DrawFrameExtended(Context contextParent, Sequence.Q2 q)
        {
            var context = contextParent.Clone();
            context.Left = q.Left * context.ScaleX;
            context.Top = q.Top * context.ScaleY;
            context.Right = q.Right * context.ScaleX;
            context.Bottom = q.Bottom * context.ScaleY;

            DrawFrame(context, sequence.Q1Items[q.Q1Index]);
        }

        private void DrawFrame(Context context, Sequence.Q1 q)
        {
            drawing.DrawSurface(surface,
                Rectangle.FromLTRB(q.Left, q.Top, q.Right, q.Bottom),
                RectangleF.FromLTRB(context.PositionX + context.Left, context.PositionY + context.Top,
                    context.PositionX + context.Right, context.PositionY + context.Bottom),
                Multiply(ConvertColor(q.ColorLeft), context.Color),
                Multiply(ConvertColor(q.ColorTop), context.Color),
                Multiply(ConvertColor(q.ColorRight), context.Color),
                Multiply(ConvertColor(q.ColorBottom), context.Color));
        }

        public static ColorF Multiply(ColorF a, ColorF b) =>
            new ColorF(
                a.R * b.R,
                a.G * b.G,
                a.B * b.B,
                a.A * b.A);

        private static ColorF ConvertColor(int color) => new ColorF(
            ((color >> 0) & 0xFF) / 128.0f,
            ((color >> 8) & 0xFF) / 128.0f,
            ((color >> 16) & 0xFF) / 128.0f,
            ((color >> 24) & 0xFF) / 128.0f);
        private static float Lerp(float m, float x1, float x2) => (x1 * (1.0f - m) + x2 * m);
        private static double Lerp(double m, double x1, double x2) => (x1 * (1.0 - m) + x2 * m);
        private static ColorF Lerp(double m, ColorF x1, ColorF x2) => new ColorF(
            (float)Lerp(m, x1.R, x2.R),
            (float)Lerp(m, x1.G, x2.G),
            (float)Lerp(m, x1.B, x2.B),
            (float)Lerp(m, x1.A, x2.A));
    }
}

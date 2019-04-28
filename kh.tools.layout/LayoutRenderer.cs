using kh.kh2;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xe.Drawing;

namespace kh.tools.layout
{
    public class LayoutRenderer
    {
        private class Context
        {
            public ISurface Surface { get; set; }
            public Sequence Sequence { get; set; }
            public int CurrentFrameIndex { get; set; }
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
                Surface = Surface,
                Sequence = Sequence,
                CurrentFrameIndex = CurrentFrameIndex,
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

        private readonly Layout layout;
        private readonly IDrawing drawing;
        private readonly ISurface[] surfaces;

        public int FrameIndex { get; set; }

        public LayoutRenderer(Layout layout, IDrawing drawing, IEnumerable<ISurface> surfaces)
        {
            this.layout = layout;
            this.drawing = drawing;
            this.surfaces = surfaces.ToArray();
        }

        public void Draw()
        {
            DrawLayoutGroup(layout.L2Items[1]);
            FrameIndex++;
        }

        private void DrawLayoutGroup(Layout.L2 l2)
        {
            var index = l2.L1Index;
            var count = l2.L1Count;
            for (var i = 0; i < count; i++)
            {
                DrawLayout(layout.L1Items[index + i]);
            }
        }

        private void DrawLayout(Layout.L1 l1)
        {
            var context = new Context
            {
                Surface = surfaces[l1.TextureIndex],
                Sequence = layout.SequenceItems[l1.SequenceIndex],
                CurrentFrameIndex = FrameIndex - l1.ShowAtFrame,
                PositionX = l1.PositionX,
                PositionY = l1.PositionY
            };

            if (context.CurrentFrameIndex >= 0)
            {
                DrawAnimationGroup(context, context.Sequence.Q5Items[l1.AnimationGroup]);
            }
        }

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
                context.CurrentFrameIndex = (context.CurrentFrameIndex < q5.Tick1) ? context.CurrentFrameIndex :
                (q5.Tick1 + ((context.CurrentFrameIndex - q5.Tick1) % (q5.Tick2 - q5.Tick1)));

            for (var i = 0; i < count; i++)
            {
                DrawAnimation(context, context.Sequence.Q4Items[index + i]);
            }
        }

        private void DrawAnimation(Context pParent, Sequence.Q4 q4)
        {
            // 0000 0001 = (0 = SINC INTERPOLATION, 1 = LINEAR INTERPOLATION)
            // 0000 0008 = (0 = BOUNCING START FROM CENTER, 1 = BOUNCING START FROM X / MOVE FROM Y)
            // 0000 0010 = (0 = ENABLE BOUNCING, 1 = IGNORE BOUNCING)
            // 0000 0020 = (0 = ENABLE ROTATION, 1 = IGNORE ROTATION)
            // 0000 0040 = (0 = ENABLE SCALING, 1 = IGNORE SCALING)
            // 0000 0080 = (0 = ENABLE COLOR FADING, 1 = IGNORE COLOR FADING)
            // 0000 0400 = (0 = ENABLE COLOR MASKING, 1 = IGNORE COLOR MASKING)
            // 0000 4000 = (0 = ENABLE XYB, 1 = IGNORE XYB)
            
            if (pParent.CurrentFrameIndex < q4.FrameStart || pParent.CurrentFrameIndex > q4.FrameEnd)
                return;

            var context = pParent.Clone();
            var delta = (float)(context.CurrentFrameIndex - q4.FrameStart) / (q4.FrameEnd - q4.FrameStart);
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
                context.Color = new ColorF(Color.White);

            if ((q4.Flags & TraslateFlag) != 0)
            {
                context.PositionX += Lerp(t, q4.Xb0, q4.Xb1);
                context.PositionY += Lerp(t, q4.Yb0, q4.Yb1);
            }

            // CALCULATE TRANSOFRMATIONS AND INTERPOLATIONS
            DrawFrameGroup(context, context.Sequence.Q3Items[q4.Q3Index]);
        }

        private void DrawFrameGroup(Context context, Sequence.Q3 q3)
        {
            var index = q3.Start;
            var count = q3.Count;
            for (var i = 0; i < count; i++)
            {
                DrawFrameExtended(context, context.Sequence.Q2Items[index + i]);
            }
        }

        private void DrawFrameExtended(Context pParent, Sequence.Q2 q)
        {
            var context = pParent.Clone();
            context.Left = q.Left * context.ScaleX;
            context.Top = q.Top * context.ScaleY;
            context.Right = q.Right * context.ScaleX;
            context.Bottom = q.Bottom * context.ScaleY;

            DrawFrame(context, context.Sequence.Q1Items[q.Q1Index]);
        }

        private void DrawFrame(Context p, Sequence.Q1 q)
        {
            drawing.DrawSurface(p.Surface,
                Rectangle.FromLTRB(q.Left, q.Top, q.Right, q.Bottom),
                RectangleF.FromLTRB(p.PositionX + p.Left, p.PositionY + p.Top,
                    p.PositionX + p.Right, p.PositionY + p.Bottom));
        }

        private static ColorF ConvertColor(int color) => new ColorF(1.0f, 1.0f, 1.0f, 1.0f); // TODO
        private static float Lerp(float m, float x1, float x2) => (x1 * (1.0f - m) + x2 * m);
        private static double Lerp(double m, double x1, double x2) => (x1 * (1.0 - m) + x2 * m);
        private static ColorF Lerp(double m, ColorF x1, ColorF x2) => new ColorF(
            (float)Lerp(m, x1.R, x2.R),
            (float)Lerp(m, x1.G, x2.G),
            (float)Lerp(m, x1.B, x2.B),
            (float)Lerp(m, x1.A, x2.A));
    }
}

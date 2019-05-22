using OpenKh.Kh2;
using System;
using System.Drawing;
using Xe.Drawing;

namespace OpenKh.Tools.LayoutViewer.Renderer
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
            }, sequence.AnimationGroups[animationGroupIndex]);

        private void DrawAnimationGroup(Context contextParent, Sequence.AnimationGroup animationGroup)
        {
            var index = animationGroup.AnimationIndex;
            var count = animationGroup.Count;
            var context = contextParent.Clone();

            //if (animationGroup.Tick1 == 0)
            //    context.CurrentFrameIndex = 0;
            //else if (animationGroup.Tick2 == 0)
            //    context.CurrentFrameIndex = Math.Min(context.CurrentFrameIndex, animationGroup.Tick1);
            //else
            //    context.CurrentFrameIndex = (context.CurrentFrameIndex < animationGroup.Tick1) ? context.CurrentFrameIndex :
            //        (animationGroup.Tick1 + ((context.CurrentFrameIndex - animationGroup.Tick1) % (animationGroup.Tick2 - animationGroup.Tick1)));

            if (animationGroup.Tick2 != 0)
                context.FrameIndex = (context.FrameIndex < animationGroup.Tick1) ? context.FrameIndex :
                (animationGroup.Tick1 + ((context.FrameIndex - animationGroup.Tick1) % (animationGroup.Tick2 - animationGroup.Tick1)));

            for (var i = 0; i < count; i++)
            {
                DrawAnimation(context, sequence.Animations[index + i]);
            }
        }

        private void DrawAnimation(Context contextParent, Sequence.Animation animation)
        {
            // 0000 0001 = (0 = SINC INTERPOLATION, 1 = LINEAR INTERPOLATION)
            // 0000 0008 = (0 = BOUNCING START FROM CENTER, 1 = BOUNCING START FROM X / MOVE FROM Y)
            // 0000 0010 = (0 = ENABLE BOUNCING, 1 = IGNORE BOUNCING)
            // 0000 0020 = (0 = ENABLE ROTATION, 1 = IGNORE ROTATION)
            // 0000 0040 = (0 = ENABLE SCALING, 1 = IGNORE SCALING)
            // 0000 0080 = (0 = ENABLE COLOR FADING, 1 = IGNORE COLOR FADING)
            // 0000 0400 = (0 = ENABLE COLOR MASKING, 1 = IGNORE COLOR MASKING)
            // 0000 4000 = (0 = ENABLE XYB, 1 = IGNORE XYB)

            if (contextParent.FrameIndex < animation.FrameStart || contextParent.FrameIndex > animation.FrameEnd)
                return;

            var context = contextParent.Clone();
            var delta = (float)(context.FrameIndex - animation.FrameStart) / (animation.FrameEnd - animation.FrameStart);
            float t;

            if ((animation.Flags & SincInterpolationFlag) != 0)
                t = delta;
            else
                t = (float)Math.Sin(delta * Math.PI / 2);

            context.PositionX += Lerp(t, animation.Xa0, animation.Xa1);
            context.PositionY += Lerp(t, animation.Ya0, animation.Ya1);
            context.ColorBlendType = animation.ColorBlend;

            if ((animation.Flags & ScalingFlag) == 0)
            {
                var scale = Lerp(t, animation.ScaleStart, animation.ScaleEnd);
                var scaleX = Lerp(t, animation.ScaleXStart, animation.ScaleXEnd);
                var scaleY = Lerp(t, animation.ScaleYStart, animation.ScaleYEnd);
                context.ScaleX = scale * scaleX;
                context.ScaleY = scale * scaleY;
            }
            else
            {
                context.ScaleX = 1.0f;
                context.ScaleY = 1.0f;
            }

            if ((animation.Flags & ColorMaskingFlag) == 0)
            {
                if ((animation.Flags & ColorInterpolationFlag) == 0)
                {
                    context.Color = Lerp(t,
                        ConvertColor(animation.ColorStart),
                        ConvertColor(animation.ColorEnd));
                }
                else
                {
                    context.Color = ConvertColor(animation.ColorStart);
                }
            }
            else
                context.Color = ConvertColor(animation.ColorStart);

            if ((animation.Flags & TraslateFlag) == 0)
            {
                context.PositionX += Lerp(t, animation.Xb0, animation.Xb1);
                context.PositionY += Lerp(t, animation.Yb0, animation.Yb1);
            }

            // CALCULATE TRANSOFRMATIONS AND INTERPOLATIONS
            DrawFrameGroup(context, sequence.FrameGroups[animation.FrameGroupIndex]);
        }

        private void DrawFrameGroup(Context context, Sequence.FrameGroup frameGroup)
        {
            var index = frameGroup.Start;
            var count = frameGroup.Count;
            for (var i = 0; i < count; i++)
            {
                DrawFrameExtended(context, sequence.FramesEx[index + i]);
            }
        }

        private void DrawFrameExtended(Context contextParent, Sequence.FrameEx frameEx)
        {
            var context = contextParent.Clone();
            context.Left = frameEx.Left * context.ScaleX;
            context.Top = frameEx.Top * context.ScaleY;
            context.Right = frameEx.Right * context.ScaleX;
            context.Bottom = frameEx.Bottom * context.ScaleY;

            DrawFrame(context, sequence.Frames[frameEx.FrameIndex]);
        }

        private void DrawFrame(Context context, Sequence.Frame frame)
        {
            drawing.DrawSurface(surface,
                Rectangle.FromLTRB(frame.Left, frame.Top, frame.Right, frame.Bottom),
                RectangleF.FromLTRB(context.PositionX + context.Left, context.PositionY + context.Top,
                    context.PositionX + context.Right, context.PositionY + context.Bottom),
                Multiply(ConvertColor(frame.ColorLeft), context.Color),
                Multiply(ConvertColor(frame.ColorTop), context.Color),
                Multiply(ConvertColor(frame.ColorRight), context.Color),
                Multiply(ConvertColor(frame.ColorBottom), context.Color));
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

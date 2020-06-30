using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace OpenKh.Engine.Renderers
{
    public class SequenceRenderer
    {
        private class Context
        {
            public int GlobalFrameIndex { get; set; }
            public int FrameIndex { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float ScaleX { get; set; }
            public float ScaleY { get; set; }
            public ColorF Color { get; set; }
            public int ColorBlendMode { get; set; }
            public float Left { get; set; }
            public float Top { get; set; }
            public float Right { get; set; }
            public float Bottom { get; set; }

            public Context Clone() => new Context
            {
                GlobalFrameIndex = GlobalFrameIndex,
                FrameIndex = FrameIndex,
                PositionX = PositionX,
                PositionY = PositionY,
                ScaleX = ScaleX,
                ScaleY = ScaleY,
                Color = Color,
                ColorBlendMode = ColorBlendMode,
                Left = Left,
                Top = Top,
                Right = Right,
                Bottom = Bottom
            };
        }

        private const int LinearInterpolationFlag = 0x00000001;
        private const int ScalingFlag = 0x00000040;
        private const int ColorMaskingFlag = 0x00000400;
        private const int ColorInterpolationFlag = 0x00000080;
        private const int TraslateFlag = 0x00004000;

        private readonly Sequence sequence;
        private readonly ISpriteDrawing drawing;
        private readonly ISpriteTexture surface;

        public SequenceRenderer(Sequence sequence, ISpriteDrawing drawing, ISpriteTexture surface)
        {
            this.sequence = sequence;
            this.drawing = drawing;
            this.surface = surface;
            DebugSequenceRenderer = new DefaultDebugSequenceRenderer();
        }

        public IDebugSequenceRenderer DebugSequenceRenderer { get; set; }

        public void Draw(int animationGroupIndex, int frameIndex, float positionX, float positionY) =>
            DrawAnimationGroup(new Context
            {
                GlobalFrameIndex = frameIndex,
                FrameIndex = frameIndex,
                PositionX = positionX,
                PositionY = positionY
            }, sequence.AnimationGroups[animationGroupIndex]);

        private void DrawAnimationGroup(Context contextParent, Sequence.AnimationGroup animationGroup)
        {
            var context = contextParent.Clone();

            if (animationGroup.DoNotLoop == 0)
            {
                var frameEnd = animationGroup.LoopEnd;
                if (frameEnd == 0)
                {
                    frameEnd = animationGroup.Animations.Max(x => x.FrameEnd);
                }

                context.FrameIndex = Loop(animationGroup.LoopStart, frameEnd, context.FrameIndex);
            }

            for (int i = 0; i < animationGroup.Animations.Count; i++)
            {
                DrawAnimation(context, animationGroup.Animations[i], i);
            }
        }

        private void DrawAnimation(Context contextParent, Sequence.Animation animation, int index)
        {
            // 0000 0001 = (0 = CUBIC INTERPOLATION, 1 = LINEAR INTERPOLATION)
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
            var delta = (double)(context.FrameIndex - animation.FrameStart) / (animation.FrameEnd - animation.FrameStart);
            
            float t;

            // loc_23B030
            if ((animation.Flags & LinearInterpolationFlag) != 0)
                t = (float)delta;
            else
                t = (float)(delta * delta * delta);

            context.PositionX += Lerp(t, animation.Xa0, animation.Xa1);
            context.PositionY += Lerp(t, animation.Ya0, animation.Ya1);
            context.ColorBlendMode = animation.ColorBlend;

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

            context.Color *= DebugSequenceRenderer.GetAnimationBlendColor(index);

            // CALCULATE TRANSOFRMATIONS AND INTERPOLATIONS
            DrawFrameGroup(context, sequence.SpriteGroups[animation.SpriteGroupIndex]);
        }

        private void DrawFrameGroup(Context context, List<Sequence.SpritePart> spriteGroup)
        {
            foreach (var spritePart in spriteGroup)
            {
                DrawFrameExtended(context, spritePart);
            }
        }

        private void DrawFrameExtended(Context contextParent, Sequence.SpritePart frameEx)
        {
            var context = contextParent.Clone();
            context.Left = frameEx.Left * context.ScaleX;
            context.Top = frameEx.Top * context.ScaleY;
            context.Right = frameEx.Right * context.ScaleX;
            context.Bottom = frameEx.Bottom * context.ScaleY;

            DrawFrame(context, sequence.Sprites[frameEx.SpriteIndex]);
        }

        private void DrawFrame(Context context, Sequence.Sprite frame)
        {
            var drawContext = new SpriteDrawingContext()
                .SpriteTexture(surface)
                .SourceLTRB(frame.Left, frame.Top, frame.Right, frame.Bottom)
                .Position(context.PositionX + context.Left, context.PositionY + context.Top)
                .DestinationSize(context.Right - context.Left, context.Bottom - context.Top);

            drawContext.Color0 = ConvertColor(frame.ColorLeft);
            drawContext.Color1 = ConvertColor(frame.ColorTop);
            drawContext.Color2 = ConvertColor(frame.ColorRight);
            drawContext.Color3 = ConvertColor(frame.ColorBottom);
            drawContext.ColorMultiply(context.Color);
            drawContext.BlendMode = (BlendMode)context.ColorBlendMode;

            if (frame.UTranslation != 0) // HACK to increase performance
            {
                drawContext.TextureWrapHorizontal(TextureWrapMode.Repeat, Math.Min(frame.Left, frame.Right), Math.Max(frame.Left, frame.Right));
                drawContext.TextureHorizontalShift = frame.UTranslation * context.GlobalFrameIndex;
            }

            if (frame.VTranslation != 0) // HACK to increase performance
            {
                drawContext.TextureWrapVertical(TextureWrapMode.Repeat, Math.Min(frame.Top, frame.Bottom), Math.Max(frame.Top, frame.Bottom));
                drawContext.TextureVerticalShift = frame.VTranslation * context.GlobalFrameIndex;
            }

            drawing.AppendSprite(drawContext);
        }

        private static ColorF ConvertColor(uint color) => new ColorF(
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

        private static int Loop(int min, int max, int val)
        {
            if (val < max)
                return val;
            if (max <= min)
                return min;

            var mod = (val - min) % (max - min);
            if (mod < 0)
                mod += max - min;
            return min + mod;
        }
    }
}

using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Engine.Renderers
{
    public enum TextAnchor
    {
        BottomLeft,
        BottomCenter,
        BottomRight,
        Center,
        TopCenter,
        TopLeft
    }

    public class SequenceRenderer
    {
        public class ChildContext
        {
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public ColorF Color { get; set; }
            public float TextPositionX { get; set; }
            public float TextPositionY { get; set; }
            public float TextScale { get; set; }
            public float UiSize { get; set; }
            public float UiPadding { get; set; }
        }

        private class Context
        {
            public int GlobalFrameIndex { get; set; }
            public int FrameIndex { get; set; }
            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public float PivotX { get; set; }
            public float PivotY { get; set; }
            public float ScaleX { get; set; }
            public float ScaleY { get; set; }
            public float RotationX { get; set; }
            public float RotationY { get; set; }
            public float RotationZ { get; set; }
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
                PivotX = PivotX,
                PivotY = PivotY,
                ScaleX = ScaleX,
                ScaleY = ScaleY,
                RotationX = RotationX,
                RotationY = RotationY,
                RotationZ = RotationZ,
                Color = Color,
                ColorBlendMode = ColorBlendMode,
                Left = Left,
                Top = Top,
                Right = Right,
                Bottom = Bottom
            };
        }

        private readonly ISpriteDrawing drawing;
        private readonly ISpriteTexture surface;

        public Sequence Sequence { get; }
        public ChildContext CurrentChildContext { get; } = new ChildContext();

        public SequenceRenderer(Sequence sequence, ISpriteDrawing drawing, ISpriteTexture surface)
        {
            this.drawing = drawing;
            this.surface = surface;
            Sequence = sequence;
            DebugSequenceRenderer = new DefaultDebugSequenceRenderer();
        }

        public IDebugSequenceRenderer DebugSequenceRenderer { get; set; }

        public bool Draw(int animationGroupIndex, int frameIndex, float positionX, float positionY, float alpha = 1f) =>
            DrawAnimationGroup(new Context
            {
                GlobalFrameIndex = frameIndex,
                FrameIndex = frameIndex,
                PositionX = positionX,
                PositionY = positionY,
                Color = new ColorF(1f, 1f, 1f, alpha)
            }, Sequence.AnimationGroups[animationGroupIndex]);

        public int GetActualFrame(Sequence.AnimationGroup animationGroup, int frameIndex)
        {
            CurrentChildContext.TextPositionX = animationGroup.TextPositionX;
            CurrentChildContext.TextPositionY = animationGroup.TextPositionY;
            CurrentChildContext.TextScale = animationGroup.TextScale;
            CurrentChildContext.UiSize = animationGroup.LightPositionX;
            CurrentChildContext.UiPadding = animationGroup.UiPadding;

            if (animationGroup.DoNotLoop != 0)
                return frameIndex;

            var frameEnd = animationGroup.LoopEnd;
            if (frameEnd == 0 && animationGroup.Animations.Count > 0)
                frameEnd = animationGroup.Animations.Max(x => x.FrameEnd);

            return Loop(animationGroup.LoopStart, frameEnd, frameIndex);
        }

        private bool DrawAnimationGroup(Context contextParent, Sequence.AnimationGroup animationGroup)
        {
            var context = contextParent.Clone();
            context.FrameIndex = GetActualFrame(animationGroup, context.FrameIndex);

            for (int i = 0; i < animationGroup.Animations.Count; i++)
            {
                DrawAnimation(context, animationGroup.Animations[i], i);
            }

            return animationGroup.DoNotLoop == 0 ||
                context.FrameIndex < animationGroup.Animations.Max(x => x.FrameEnd);
        }

        private void DrawAnimation(Context contextParent, Sequence.Animation animation, int index)
        {
            // 0000 0001 = (0 = EASE IN/OUT INTERPOLATION, 1 = LINEAR INTERPOLATION)
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
            if ((animation.Flags & Sequence.LinearInterpolationFlag) != 0)
                t = (float)delta;
            else
                t = (float)((Math.Sin(delta * Math.PI - Math.PI / 2.0) + 1.0) / 2.0);

            context.ColorBlendMode = animation.ColorBlend;

            var translateX = Lerp(t, animation.TranslateXStart, animation.TranslateXEnd);
            var translateY = Lerp(t, animation.TranslateYStart, animation.TranslateYEnd);
            if ((animation.Flags & Sequence.TranslationFlag) == 0)
            {
                context.PositionX += translateX;
                context.PositionY += translateY;
            }
            else
            {
                context.PositionX += animation.TranslateXStart;
                context.PositionY += animation.TranslateYStart;
            }

            if ((animation.Flags & Sequence.ScalingFlag) == 0)
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

            if ((animation.Flags & Sequence.ColorMaskingFlag) == 0)
            {
                if ((animation.Flags & Sequence.ColorInterpolationFlag) == 0)
                {
                    context.Color *= Lerp(t,
                        ConvertColor(animation.ColorStart),
                        ConvertColor(animation.ColorEnd));
                }
                else
                {
                    context.Color *= ConvertColor(animation.ColorStart);
                }
            }
            else
                context.Color *= new ColorF(1, 1, 1, 1);

            if ((animation.Flags & Sequence.RotationFlag) == 0)
            {
                context.RotationX = Lerp(t, animation.RotationXStart, animation.RotationXEnd);
                context.RotationY = Lerp(t, animation.RotationYStart, animation.RotationYEnd);
                context.RotationZ = Lerp(t, animation.RotationZStart, animation.RotationZEnd);
            }

            if ((animation.Flags & Sequence.PivotFlag) == 0)
            {
                context.PivotX += Lerp(t, animation.PivotXStart, animation.PivotXEnd);
                context.PivotY += Lerp(t, animation.PivotYStart, animation.PivotYEnd);
            }

            if ((animation.Flags & Sequence.BouncingFlag) == 0)
            {
                var bounceXValue = (float)Math.Sin(Lerp(delta * animation.BounceXSpeed, 0, Math.PI));
                var bounceYValue = (float)Math.Sin(Lerp(delta * animation.BounceYSpeed, 0, Math.PI));

                context.PositionX += bounceXValue * Lerp(t, animation.BounceXStart, animation.BounceXEnd);
                context.PositionY += bounceYValue * Lerp(t, animation.BounceYStart, animation.BounceYEnd);
            }

            context.Color *= DebugSequenceRenderer.GetAnimationBlendColor(index);

            if ((animation.Flags & Sequence.CanHostChildFlag) != 0)
            {
                CurrentChildContext.PositionX = context.PositionX + context.PivotX;
                CurrentChildContext.PositionY = context.PositionY + context.PivotY;
                CurrentChildContext.Color = context.Color;

                // Horrible hack. Basically if TranslationFlag disallow to us the translation
                // animation, the frame group just uses Translate*Start, but the attached
                // child context still needs to use the animation.
                if ((animation.Flags & Sequence.TranslationFlag) != 0)
                {
                    CurrentChildContext.PositionX += translateX - animation.TranslateXStart;
                    CurrentChildContext.PositionY += translateY - animation.TranslateYStart;
                }
            }

            // CALCULATE TRANSOFRMATIONS AND INTERPOLATIONS
            DrawFrameGroup(context, Sequence.SpriteGroups[animation.SpriteGroupIndex]);
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
            context.Left = frameEx.Left;
            context.Top = frameEx.Top;
            context.Right = frameEx.Right;
            context.Bottom = frameEx.Bottom;

            DrawFrame(context, Sequence.Sprites[frameEx.SpriteIndex]);
        }

        private void DrawFrame(Context context, Sequence.Sprite frame)
        {
            var drawContext = new SpriteDrawingContext()
                .SpriteTexture(surface)
                .SourceLTRB(frame.Left, frame.Top, frame.Right, frame.Bottom)
                .Position(context.Left, context.Top)
                .DestinationSize(context.Right - context.Left, context.Bottom - context.Top)
                .Traslate(context.PivotX, context.PivotY)
                .ScaleSize(context.ScaleX, context.ScaleY)
                .RotateX(-context.RotationX)
                .RotateY(-context.RotationY)
                .RotateZ(-context.RotationZ)
                .Traslate(context.PositionX, context.PositionY);

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

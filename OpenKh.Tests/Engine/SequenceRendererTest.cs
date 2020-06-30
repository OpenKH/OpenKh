using NSubstitute;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Engine
{
    public class SequenceRendererTest
    {
        private const int AnimationFirstFrame = 0;
        private const int AnimationLastFrame = 1000;

        [Theory]
        [InlineData(0, 0, 0, 0, 0)]
        [InlineData(0, 1000, 1, 0, 0)]
        [InlineData(0, 1000, 1, 500, 500)]
        [InlineData(0, 1000, 0, 500, 125)]
        [InlineData(0, 1000, 0, 750, 422)]
        public void TraslateXAnimationTest(int x0, int x1, int flags, int frameIndex, float expected)
        {
            var sequence = MockSequence(new Sequence.Animation
            {
                Flags = flags,
                Xa0 = x0,
                Xa1 = x1,
                FrameStart = AnimationFirstFrame,
                FrameEnd = AnimationLastFrame,
                ScaleStart = 1,
                ScaleEnd = 1,
                ScaleXStart = 1,
                ScaleXEnd = 1,
                ScaleYStart = 1,
                ScaleYEnd = 1,
                ColorStart = 0x80808080,
                ColorEnd = 0x80808080,
            });

            var drawing = MockDrawing();
            var renderer = new SequenceRenderer(sequence, drawing, null);
            renderer.Draw(0, frameIndex, 0, 0);

            AssertDraw(drawing, x =>
            {
                Assert.Equal(expected, x.DestinationX, 0);
            });
        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0)]
        [InlineData(0, 1000, 1, 0, 0)]
        [InlineData(0, 1000, 1, 500, 500)]
        [InlineData(0, 1000, 0, 500, 125)]
        [InlineData(0, 1000, 0, 750, 422)]
        [InlineData(0, 1000, 0x4000, 500, 0)]
        public void TraslateXBAnimationTest(int x0, int x1, int flags, int frameIndex, float expected)
        {
            var sequence = MockSequence(new Sequence.Animation
            {
                Flags = flags,
                Xb0 = x0,
                Xb1 = x1,
                FrameStart = AnimationFirstFrame,
                FrameEnd = AnimationLastFrame,
                ScaleStart = 1,
                ScaleEnd = 1,
                ScaleXStart = 1,
                ScaleXEnd = 1,
                ScaleYStart = 1,
                ScaleYEnd = 1,
                ColorStart = 0x80808080,
                ColorEnd = 0x80808080,
            });

            var drawing = MockDrawing();
            var renderer = new SequenceRenderer(sequence, drawing, null);
            renderer.Draw(0, frameIndex, 0, 0);

            AssertDraw(drawing, x =>
            {
                Assert.Equal(expected, x.DestinationX, 0);
            });
        }

        [Fact]
        public void TranslateUsingXaAndXb()
        {
            var sequence = MockSequence(new Sequence.Animation
            {
                Flags = 0,
                Xa0 = 200,
                Xa1 = 500,
                Xb0 = 150,
                Xb1 = 400,
                FrameStart = AnimationFirstFrame,
                FrameEnd = AnimationLastFrame,
                ScaleStart = 1,
                ScaleEnd = 1,
                ScaleXStart = 1,
                ScaleXEnd = 1,
                ScaleYStart = 1,
                ScaleYEnd = 1,
                ColorStart = 0x80808080,
                ColorEnd = 0x80808080,
            });

            var drawing = MockDrawing();
            var renderer = new SequenceRenderer(sequence, drawing, null);
            renderer.Draw(0, 500, 0, 0);

            AssertDraw(drawing, x =>
            {
                Assert.Equal(419, x.DestinationX, 0);
            });
        }

        [Theory]
        [InlineData(false, 0, 0, AnimationLastFrame - 1, true)]
        [InlineData(false, 0, 0, AnimationLastFrame + 1, false)]
        [InlineData(false, 10, 100, AnimationLastFrame + 1, false)]
        [InlineData(true, 10, 100, AnimationLastFrame + 1, true)]
        [InlineData(true, 10, 1500, AnimationLastFrame + 1, false)]
        [InlineData(true, 500, 1500, 1750, true)]
        [InlineData(true, 500, 1500, 2250, false)]
        [InlineData(true, 500, 1500, 2750, true)]
        [InlineData(true, 500, 1500, 4250, false)]
        [InlineData(true, 500, 1500, 4750, true)]
        public void LoopCorrectly(bool loopEnabled, int loopStart, short loopEnd, short frameIndex, bool doesDrawAnyFrame)
        {
            var sequence = MockSequence(new Sequence.AnimationGroup
            {
                Animations = new List<Sequence.Animation>()
                {
                    new Sequence.Animation
                    {
                        FrameStart = AnimationFirstFrame,
                        FrameEnd = AnimationLastFrame,
                    }
                },
                DoNotLoop = (short)(loopEnabled ? 0 : 1),
                LoopStart = loopStart,
                LoopEnd = loopEnd
            });

            var drawing = MockDrawing();
            var renderer = new SequenceRenderer(sequence, drawing, null);
            renderer.Draw(0, frameIndex, 0, 0);

            if (doesDrawAnyFrame)
                AssertAtLeastOneCall(drawing);
            else
                AssertNoCall(drawing);
        }

        private static ISpriteDrawing MockDrawing() => Substitute.For<ISpriteDrawing>();

        private static void AssertDraw(ISpriteDrawing drawing, Action<SpriteDrawingContext> assertion)
        {
            var call = drawing.ReceivedCalls().FirstOrDefault();
            Assert.NotNull(call);
            assertion(call.GetArguments()[0] as SpriteDrawingContext);
        }

        private static void AssertAtLeastOneCall(ISpriteDrawing drawing)
        {
            if (!drawing.ReceivedCalls().Any())
                throw new Xunit.Sdk.XunitException("Expected at least draw but no draw has been performed.");
        }

        private static void AssertNoCall(ISpriteDrawing drawing)
        {
            if (drawing.ReceivedCalls().Any())
                throw new Xunit.Sdk.XunitException("Expected no draws but at least once has been performed.");
        }

        private static Sequence MockSequence(Sequence.Animation animation) => new Sequence
        {
            AnimationGroups = new List<Sequence.AnimationGroup>()
                {
                    new Sequence.AnimationGroup
                    {
                        Animations = new List<Sequence.Animation>()
                        {
                            animation
                        },
                        DoNotLoop = 1,
                    }
                },
            SpriteGroups = new List<List<Sequence.SpritePart>>()
                {
                    new List<Sequence.SpritePart>()
                    {
                        new Sequence.SpritePart
                        {
                            Left = 0,
                            Top = 0,
                            Right = 512,
                            Bottom = 512,
                        }
                    }
                },
            Sprites = new List<Sequence.Sprite>()
                {
                    new Sequence.Sprite
                    {
                        Left = 0,
                        Top = 0,
                        Right = 512,
                        Bottom = 512,
                        UTranslation = 0,
                        VTranslation = 0,
                        ColorLeft = 0x80808080,
                        ColorTop = 0x80808080,
                        ColorRight = 0x80808080,
                        ColorBottom = 0x80808080,
                    }
                }
        };

        private static Sequence MockSequence(Sequence.AnimationGroup animationGroup) => new Sequence
        {
            AnimationGroups = new List<Sequence.AnimationGroup>()
                {
                    animationGroup,
                },
            SpriteGroups = new List<List<Sequence.SpritePart>>()
                {
                    new List<Sequence.SpritePart>()
                    {
                        new Sequence.SpritePart
                        {
                            Left = 0,
                            Top = 0,
                            Right = 512,
                            Bottom = 512,
                        }
                    }
                },
            Sprites = new List<Sequence.Sprite>()
                {
                    new Sequence.Sprite
                    {
                        Left = 0,
                        Top = 0,
                        Right = 512,
                        Bottom = 512,
                        UTranslation = 0,
                        VTranslation = 0,
                        ColorLeft = 0x80808080,
                        ColorTop = 0x80808080,
                        ColorRight = 0x80808080,
                        ColorBottom = 0x80808080,
                    }
                }
        };
    }
}

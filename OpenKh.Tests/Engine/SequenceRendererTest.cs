using NSubstitute;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Engine
{
    public class SequenceRendererTest
    {
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
                FrameStart = 0,
                FrameEnd = 1000,
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
                FrameStart = 0,
                FrameEnd = 1000,
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
                FrameStart = 0,
                FrameEnd = 1000,
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

        private static ISpriteDrawing MockDrawing() => Substitute.For<ISpriteDrawing>();

        private static void AssertDraw(ISpriteDrawing drawing, Action<SpriteDrawingContext> assertion)
        {
            var call = drawing.ReceivedCalls().FirstOrDefault();
            Assert.NotNull(call);
            assertion(call.GetArguments()[0] as SpriteDrawingContext);
        }

        private static Sequence MockSequence(Sequence.Animation animation)
        {
            return new Sequence
            {
                AnimationGroups = new List<Sequence.AnimationGroup>()
                {
                    new Sequence.AnimationGroup
                    {
                        AnimationIndex = 0,
                        Count = 1,
                    }
                },
                Animations = new List<Sequence.Animation>()
                {
                    animation
                },
                FrameGroups = new List<Sequence.FrameGroup>()
                {
                    new Sequence.FrameGroup
                    {
                        Start = 0,
                        Count = 1
                    }
                },
                FramesEx = new List<Sequence.FrameEx>()
                {
                    new Sequence.FrameEx
                    {
                        Left = 0,
                        Top = 0,
                        Right = 512,
                        Bottom = 512,
                    }
                },
                Frames = new List<Sequence.Frame>()
                {
                    new Sequence.Frame
                    {
                        Unknown00 = 0,
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
}

using Castle.DynamicProxy.Generators;
using NSubstitute;
using OpenKh.Engine;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Game;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Engine
{
    public class AnimatedSequenceFactoryTests
    {
        private class MockMessageRenderer : IMessageRenderer
        {
            public bool HasBeenCalled { get; private set; }
            public double LastPosX { get; private set; }
            public double LastPosY { get; private set; }

            public void Draw(DrawContext context, string message) => 
                throw new NotImplementedException();

            public void Draw(DrawContext context, byte[] data)
            {
                if (context.IgnoreDraw)
                {
                    context.Width = 20;
                    context.Height = 10;
                }
                else
                {
                    HasBeenCalled = true;
                    LastPosX = context.x;
                    LastPosY = context.y;
                }
            }
        }

        public AnimatedSequenceFactoryTests()
        {

        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0)]
        [InlineData(0, 0, 10, 0, 0)]
        [InlineData(20, 0, 10, AnimatedSequenceDesc.DefaultStacking, 0)]
        [InlineData(0, 20, 10, 0, AnimatedSequenceDesc.DefaultStacking)]
        [InlineData(30, 30, 15, AnimatedSequenceDesc.DefaultStacking, AnimatedSequenceDesc.DefaultStacking)]
        [InlineData(30, 40, 15, AnimatedSequenceDesc.DefaultStacking, 20)]
        [InlineData(-40, 30, 15, -20, AnimatedSequenceDesc.DefaultStacking)]
        public void StackChildrenCorrectly(
            int expectedThirdChildPosX,
            int expectedThirdChildPosY,
            int stackValue,
            int stackWidth,
            int stackHeight)
        {
            var sequence = new Sequence
            {
                AnimationGroups = new List<Sequence.AnimationGroup>
                {
                    new Sequence.AnimationGroup
                    {
                        TextPositionX = stackValue,
                        Animations = new List<Sequence.Animation>
                        {
                            new Sequence.Animation
                            {
                                FrameEnd = 1000,
                                SpriteGroupIndex = 0,
                            }
                        }
                    }
                },
                SpriteGroups = new List<List<Sequence.SpritePart>>
                {
                    new List<Sequence.SpritePart>
                    {
                        new Sequence.SpritePart
                        {
                            SpriteIndex = 0,
                            Right = 10,
                            Bottom = 10,
                        }
                    }
                },
                Sprites = new List<Sequence.Sprite>
                {
                    new Sequence.Sprite
                    {
                        
                    }
                }
            };

            var drawing = Extensions.MockDrawing();
            var messageProvider = Substitute.For<IMessageProvider>();
            var messageRenderer = Substitute.For<IMessageRenderer>();
            var messageEncode = Substitute.For<IMessageEncode>();
            var spriteTexture = Substitute.For<ISpriteTexture>();
            var factory = new AnimatedSequenceFactory(
                drawing,
                messageProvider,
                messageRenderer,
                messageEncode,
                sequence,
                spriteTexture);

            var animationDescs = Enumerable.Range(0, 3)
                .Select(x => new AnimatedSequenceDesc
                {
                    StackIndex = x,
                    StackWidth = stackWidth,
                    StackHeight = stackHeight
                });

            var animation = factory.Create(animationDescs);
            animation.Draw(0, 0);

            drawing.AssertAtLeastOneCall();
            drawing.AssertDraw(2, x =>
            {
                Assert.Equal(expectedThirdChildPosX, x.Vec0.X);
                Assert.Equal(expectedThirdChildPosY, x.Vec0.Y);
            });
        }

        [Theory]
        [InlineData(TextAnchor.BottomLeft, 0, 0)]
        [InlineData(TextAnchor.BottomRight, -20, 0)]
        [InlineData(TextAnchor.BottomCenter, -10, 0)]
        [InlineData(TextAnchor.Center, -10, -5)]
        [InlineData(TextAnchor.TopCenter, -10, -10)]
        [InlineData(TextAnchor.TopLeft, 0, -10)]
        public void AnchorTextCorrectly(TextAnchor anchor, int expectedX, int expectedY)
        {
            var sequence = new Sequence
            {
                AnimationGroups = new List<Sequence.AnimationGroup>
                {
                    new Sequence.AnimationGroup
                    {
                        Animations = new List<Sequence.Animation>()
                    }
                },
                SpriteGroups = new List<List<Sequence.SpritePart>>(),
                Sprites = new List<Sequence.Sprite>()
            };

            var drawing = Extensions.MockDrawing();
            var messageProvider = Substitute.For<IMessageProvider>();
            var messageRenderer = new MockMessageRenderer();
            var messageEncode = Substitute.For<IMessageEncode>();
            var spriteTexture = Substitute.For<ISpriteTexture>();
            var factory = new AnimatedSequenceFactory(
                drawing,
                messageProvider,
                messageRenderer,
                messageEncode,
                sequence,
                spriteTexture);

            messageEncode.Encode(Arg.Is<List<MessageCommandModel>>(x => true))
                .Returns(new byte[] { 0x21 });

            var animation = factory.Create(new AnimatedSequenceDesc
            {
                TextAnchor = anchor,
                MessageText = "test"
            });

            animation.Draw(0, 0);

            Assert.True(messageRenderer.HasBeenCalled);
            Assert.Equal(expectedX, messageRenderer.LastPosX);
            Assert.Equal(expectedY, messageRenderer.LastPosY);
            //drawing.AssertDraw(2, x =>
            //{
            //    Assert.Equal(expectedThirdChildPosX, x.Vec0.X);
            //    Assert.Equal(expectedThirdChildPosY, x.Vec0.Y);
            //});
        }
    }
}

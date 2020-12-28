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
            public ColorF LastColor { get; private set; }

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
                    LastColor = context.Color;
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
        }

        [Fact]
        public void StackChildrenHorizontally()
        {
            var sequence = new Sequence
            {
                AnimationGroups = new List<Sequence.AnimationGroup>
                {
                    new Sequence.AnimationGroup
                    {
                        LightPositionX = 100,
                        Animations = new List<Sequence.Animation>
                        {
                            new Sequence.Animation
                            {
                                FrameEnd = 100,
                                Flags =  Sequence.CanHostChildFlag,
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
                            Right = 50,
                            Bottom = 30,
                        }
                    }
                },
                Sprites = new List<Sequence.Sprite>()
                {
                    new Sequence.Sprite()
                }
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

            var animation = factory.Create(new AnimatedSequenceDesc
            {
                Flags = AnimationFlags.ChildStackHorizontally,
                Children = Enumerable.Range(0, 3)
                    .Select(x => new AnimatedSequenceDesc())
                    .ToList()
            });

            animation.Draw(0, 0);

            drawing.AssertAtLeastOneCall();
            drawing.AssertCallCount(4);
            drawing.AssertDraw(3, x =>
            {
                Assert.Equal(200, x.Vec0.X);
                Assert.Equal(0, x.Vec0.Y);
            });
        }

        [Theory]
        [InlineData(AnimationFlags.None, 100, 4, 0, 0)]
        [InlineData(AnimationFlags.StackNextChildHorizontally, 100, 4, 300, 0)]
        [InlineData(AnimationFlags.StackNextChildVertically, 120, 4, 0, 360)]
        public void StackItemsFromChildSize(AnimationFlags flags, int uiPadding,
            int elementCount, int expectedLastPosX, int expectedLastPosY)
        {
            var sequence = new Sequence
            {
                AnimationGroups = new List<Sequence.AnimationGroup>
                {
                    new Sequence.AnimationGroup
                    {
                        UiPadding = 0,
                        Animations = new List<Sequence.Animation>()
                    },
                    new Sequence.AnimationGroup
                    {
                        UiPadding = uiPadding,
                        Animations = new List<Sequence.Animation>
                        {
                            new Sequence.Animation
                            {
                                FrameEnd = 100,
                                Flags =  Sequence.CanHostChildFlag,
                            }
                        }
                    },
                },
                SpriteGroups = new List<List<Sequence.SpritePart>>
                {
                    new List<Sequence.SpritePart>
                    {
                        new Sequence.SpritePart
                        {
                            Right = 50,
                            Bottom = 30,
                        }
                    }
                },
                Sprites = new List<Sequence.Sprite>()
                {
                    new Sequence.Sprite()
                }
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

            var animation = factory.Create(new AnimatedSequenceDesc
            {
                SequenceIndexLoop = 0,
                Flags = flags,
                Children = Enumerable.Range(0, elementCount)
                    .Select(x => new AnimatedSequenceDesc
                    {
                        SequenceIndexLoop = 1,
                    })
                    .ToList()
            });

            animation.Begin();
            animation.Draw(0, 0);

            drawing.AssertCallCount(4);
            drawing.AssertDraw(3, x =>
            {
                Assert.Equal(expectedLastPosX, x.Vec0.X);
                Assert.Equal(expectedLastPosY, x.Vec0.Y);
            });
        }

        [Fact]
        public void DrawTextWithCorrectColor()
        {
            var colorTest = new ColorF(0.5f, 0.75f, 0.25f, 0.125f);
            AssertDrawTextWithCorrectColor(colorTest, colorTest, AnimationFlags.None);
        }

        [Fact]
        public void DrawTextWithDefaultColor()
        {
            var colorTest = new ColorF(0.5f, 0.75f, 0.25f, 0.125f);
            var expectedColor = new ColorF(1f, 1f, 1f, colorTest.A);
            AssertDrawTextWithCorrectColor(expectedColor, colorTest, AnimationFlags.TextIgnoreColor);
        }

        private void AssertDrawTextWithCorrectColor(ColorF expected, ColorF colorValue, AnimationFlags flags)
        {
            const float Divisor = 255f / 128f;
            var sequence = new Sequence
            {
                AnimationGroups = new List<Sequence.AnimationGroup>
                {
                    new Sequence.AnimationGroup
                    {
                        Animations = new List<Sequence.Animation>
                        {
                            new Sequence.Animation
                            {
                                FrameEnd = 100,
                                Flags =  Sequence.CanHostChildFlag,
                                ColorStart = (colorValue / Divisor).ToRgba(),
                                ColorEnd = (colorValue / Divisor).ToRgba(),
                            }
                        }
                    }
                },
                SpriteGroups = new List<List<Sequence.SpritePart>>
                {
                    new List<Sequence.SpritePart>()
                },
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
                Flags = flags,
                MessageText = "test"
            });

            animation.Draw(0, 0);

            expected = ColorF.FromRgba(expected.ToRgba());
            Assert.True(messageRenderer.HasBeenCalled);
            Assert.Equal(expected.R, messageRenderer.LastColor.R, 2);
            Assert.Equal(expected.G, messageRenderer.LastColor.G, 2);
            Assert.Equal(expected.B, messageRenderer.LastColor.B, 2);
            Assert.Equal(expected.A, messageRenderer.LastColor.A, 2);
        }
    }
}

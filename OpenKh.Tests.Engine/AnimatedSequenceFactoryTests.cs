using NSubstitute;
using OpenKh.Engine;
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
    }
}

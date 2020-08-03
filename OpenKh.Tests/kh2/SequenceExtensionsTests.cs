using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.kh2
{
    public class SequenceExtensionsTests
    {
        [Fact]
        public void GetVisibilityRectangleForFrameExTest()
        {
            const int ExpectedX = 50;
            const int ExpectedY = 70;
            const int ExpectedWidth = 100;
            const int ExpectedHeight = 200;

            var rect = new Sequence.SpritePart
            {
                Left = ExpectedX,
                Top = ExpectedY,
                Right = ExpectedX + ExpectedWidth,
                Bottom = ExpectedY + ExpectedHeight
            }.GetVisibilityRectangle();

            Assert.Equal(ExpectedX, rect.X);
            Assert.Equal(ExpectedY, rect.Y);
            Assert.Equal(ExpectedWidth, rect.Width);
            Assert.Equal(ExpectedHeight, rect.Height);
        }

        [Theory]
        [InlineData(0, 0, 0, 0, 0, 0)]
        [InlineData(1, 0, 0, 0, 0, 0)]
        [InlineData(0, 1, 10, 20, 20, 5)]
        [InlineData(1, 1, 20, 20, 5, 5)]
        [InlineData(3, 1, -20, 0, 20, 10)]
        [InlineData(0, 2, 10, 20, 20, 5)]
        [InlineData(2, 1, 10, 10, 20, 40)]
        [InlineData(2, 2, -20, 0, 50, 50)]
        [InlineData(2, 3, -20, 0, 50, 50)]
        public void GetVisibilityRectangleForFrameGroupTest(
            short frameExIndex, short frameExCount,
            int expectedX, int expectedY,
            int expectedWidth, int expectedHeight)
        {
            var sequence = new Sequence()
            {
                SpriteGroups = new List<List<Sequence.SpritePart>>
                {
                    new List<Sequence.SpritePart>
                    {
                        new Sequence.SpritePart() { Left = 10, Top = 20, Right = 30, Bottom = 25 },
                        new Sequence.SpritePart() { Left = 20, Top = 20, Right = 25, Bottom = 25 },
                        new Sequence.SpritePart() { Left = 10, Top = 50, Right = 30, Bottom = 10 },
                        new Sequence.SpritePart() { Left = -20, Top = 0, Right = 0, Bottom = 10 },
                        new Sequence.SpritePart() { Left = 0, Top = 0, Right = 0, Bottom = 100 },
                    }.Skip(frameExIndex).Take(frameExCount).ToList()
                }
            };

            var rect = sequence.GetVisibilityRectangleForFrameGroup(0);

            Assert.Equal(expectedX, rect.X);
            Assert.Equal(expectedY, rect.Y);
            Assert.Equal(expectedWidth, rect.Width);
            Assert.Equal(expectedHeight, rect.Height);
        }
    }
}

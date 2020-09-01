using NSubstitute;
using OpenKh.Engine.Renders;
using System;
using System.Linq;
using Xunit;

namespace OpenKh.Tests.Engine
{
    public static class Extensions
    {
        public static ISpriteDrawing MockDrawing() => Substitute.For<ISpriteDrawing>();

        public static void AssertAtLeastOneCall(this ISpriteDrawing drawing)
        {
            if (!drawing.ReceivedCalls().Any())
                throw new Xunit.Sdk.XunitException("Expected at least draw but no draw has been performed.");
        }

        public static void AssertNoCall(this ISpriteDrawing drawing)
        {
            if (drawing.ReceivedCalls().Any())
                throw new Xunit.Sdk.XunitException("Expected no draws but at least once has been performed.");
        }

        public static void AssertCallCount(this ISpriteDrawing drawing, int count)
        {
            var actual = drawing.ReceivedCalls().Count();
            if (actual != count)
                throw new Xunit.Sdk.XunitException($"Expected {count} draw counts, but got {actual}.");
        }

        public static void AssertDraw(this ISpriteDrawing drawing, Action<SpriteDrawingContext> assertion) =>
            drawing.AssertDraw(0, assertion);

        public static void AssertDraw(this ISpriteDrawing drawing, int callIndex, Action<SpriteDrawingContext> assertion)
        {
            var call = drawing.ReceivedCalls().Skip(callIndex).FirstOrDefault();
            Assert.NotNull(call);
            assertion(call.GetArguments()[0] as SpriteDrawingContext);
        }
    }
}

using kh.kh2;
using Xunit;

namespace kh.tests.kh2
{
    public class MsgSerializerTests
    {
        private const System.Xml.Linq.SaveOptions xmlFormatting = System.Xml.Linq.SaveOptions.DisableFormatting;

        [Fact]
        public void SerializeEntries()
        {
            var entry = new MsgParser.Entry
            {
                Command = MsgParser.Command.PrintText,
                Text = "Hello world!"
            };

            var element = MsgSerializer.SerializeXEntries(1234, new[] { entry });
            Assert.Equal("<message id=\"1234\"><text>Hello world!</text></message>", element.ToString(xmlFormatting));
        }

        [Fact]
        public void SerializeSimpleText()
        {
            var entry = new MsgParser.Entry
            {
                Command = MsgParser.Command.PrintText,
                Text = "Hello world!"
            };

            var element = MsgSerializer.SerializeXEntry(entry);
            Assert.Equal("<text>Hello world!</text>", element.ToString());
        }

        [Theory]
        [InlineData(0, "consumable")]
        [InlineData(1, "tent")]
        [InlineData(2, "material")]
        [InlineData(3, "ability-off")]
        public void SerializeIcon(byte id, string expected)
        {
            var entry = new MsgParser.Entry
            {
                Command = MsgParser.Command.PrintIcon,
                Data = new[] { id }
            };

            var element = MsgSerializer.SerializeXEntry(entry);
            Assert.Equal($"<icon>{expected}</icon>", element.ToString());
        }
    }
}

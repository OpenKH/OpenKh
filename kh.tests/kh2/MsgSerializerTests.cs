using kh.kh2;
using kh.kh2.Messages;
using System.Xml.Linq;
using Xunit;

namespace kh.tests.kh2
{
    public class MsgSerializerTests
    {
        private const SaveOptions xmlFormatting = SaveOptions.DisableFormatting;
        private const int MessageId = 12345;

        [Fact]
        public void SerializeSimpleText()
        {
            var entry = new MessageCommandModel
            {
                Command = MessageCommand.PrintText,
                Text = "Hello world!"
            };

            var element = MsgSerializer.SerializeXEntries(MessageId, new[] { entry });
            var content = new XElement("message",
                new XAttribute("id", MessageId),
                new XElement("text", "Hello world!")
            );
            Assert.Equal(content.ToString(xmlFormatting), element.ToString(xmlFormatting));
        }

        [Theory]
        [InlineData(0, "item-consumable")]
        [InlineData(1, "item-tent")]
        [InlineData(2, "item-key")]
        [InlineData(3, "ability-unequip")]
        public void SerializeIcon(byte id, string content)
        {
            var entry = new MessageCommandModel
            {
                Command = MessageCommand.PrintIcon,
                Data = new[] { id }
            };

            var actual = MsgSerializer.SerializeXEntries(MessageId, new[] { entry });
            var expected = new XElement("message",
                new XAttribute("id", MessageId),
                new XElement("icon", new XAttribute("class", content))
            );
            Assert.Equal(expected.ToString(xmlFormatting), actual.ToString(xmlFormatting));
        }
    }
}

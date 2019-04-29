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
        public void SerializePlainSimpleText()
        {
            var entry = new MessageCommandModel
            {
                Command = MessageCommand.PrintText,
                Text = "Hello world!"
            };

            var element = MsgSerializer.SerializeText(new[] { entry });
            Assert.Equal(entry.Text, element);
        }

        [Fact]
        public void SerializePlainComplexText()
        {
            var entries = new[]
            {
                new MessageCommandModel
                {
                    Command = MessageCommand.PrintText,
                    Text = "Hey "
                },
                new MessageCommandModel
                {
                    Command = MessageCommand.PrintComplex,
                    Text = "VII"
                },
                new MessageCommandModel
                {
                    Command = MessageCommand.PrintText,
                    Text = " complex!"
                },
            };

            var element = MsgSerializer.SerializeText(entries);
            Assert.Equal("Hey {VII} complex!", element);
        }

        [Fact]
        public void SerializeXmlSimpleText()
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
        public void SerializeXmlIcon(byte id, string content)
        {
            var entry = new MessageCommandModel
            {
                Command = MessageCommand.PrintIcon,
                Data = new[] { id }
            };

            var actual = MsgSerializer.SerializeXEntries(MessageId, new[] { entry });
            var expected = new XElement("message",
                new XAttribute("id", MessageId),
                new XElement("icon", new XAttribute("value", content))
            );
            Assert.Equal(expected.ToString(xmlFormatting), actual.ToString(xmlFormatting));
        }
    }
}

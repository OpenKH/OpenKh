using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Kh2.Contextes;
using OpenKh.Kh2.Messages;

namespace OpenKh.Engine.Extensions
{
    public static class Kh2FontContextExtensions
    {
        public static RenderingMessageContext ToKh2EuSystemTextContext(this FontContext fontContext) =>
            new RenderingMessageContext
            {
                Font = fontContext.ImageSystem,
                Font2 = fontContext.ImageSystem2,
                Icon = fontContext.ImageIcon,
                FontSpacing = fontContext.SpacingSystem,
                IconSpacing = fontContext.SpacingIcon,
                Encoder = Encoders.InternationalSystem,
                FontWidth = Constants.FontEuropeanSystemWidth,
                FontHeight = Constants.FontEuropeanSystemHeight,
                TableHeight = Constants.FontTableSystemHeight,
            };

        public static RenderingMessageContext ToKh2EuEventTextContext(this FontContext fontContext) =>
            new RenderingMessageContext
            {
                Font = fontContext.ImageEvent,
                Font2 = fontContext.ImageEvent2,
                Icon = fontContext.ImageIcon,
                FontSpacing = fontContext.SpacingEvent,
                IconSpacing = fontContext.SpacingIcon,
                Encoder = Encoders.InternationalSystem,
                FontWidth = Constants.FontEuropeanEventWidth,
                FontHeight = Constants.FontEuropeanEventHeight,
                TableHeight = Constants.FontTableEventHeight,
            };

        public static RenderingMessageContext ToKh2TRSystemTextContext(this FontContext fontContext) =>
            new RenderingMessageContext
            {
                Font = fontContext.ImageSystem,
                Font2 = fontContext.ImageSystem2,
                Icon = fontContext.ImageIcon,
                FontSpacing = fontContext.SpacingSystem,
                IconSpacing = fontContext.SpacingIcon,
                Encoder = Encoders.TurkishSystem,
                FontWidth = Constants.FontEuropeanSystemWidth,
                FontHeight = Constants.FontEuropeanSystemHeight,
                TableHeight = Constants.FontTableSystemHeight,
            };

        public static RenderingMessageContext ToKh2TREventTextContext(this FontContext fontContext) =>
            new RenderingMessageContext
            {
                Font = fontContext.ImageEvent,
                Font2 = fontContext.ImageEvent2,
                Icon = fontContext.ImageIcon,
                FontSpacing = fontContext.SpacingEvent,
                IconSpacing = fontContext.SpacingIcon,
                Encoder = Encoders.TurkishSystem,
                FontWidth = Constants.FontEuropeanEventWidth,
                FontHeight = Constants.FontEuropeanEventHeight,
                TableHeight = Constants.FontTableEventHeight,
            };

        public static RenderingMessageContext ToKh2JpSystemTextContext(this FontContext fontContext) =>
            new RenderingMessageContext
            {
                Font = fontContext.ImageSystem,
                Font2 = fontContext.ImageSystem2,
                Icon = fontContext.ImageIcon,
                FontSpacing = fontContext.SpacingSystem,
                IconSpacing = fontContext.SpacingIcon,
                Encoder = Encoders.JapaneseSystem,
                FontWidth = Constants.FontJapaneseSystemWidth,
                FontHeight = Constants.FontJapaneseSystemHeight,
                TableHeight = Constants.FontTableSystemHeight,
            };

        public static RenderingMessageContext ToKh2JpEventTextContext(this FontContext fontContext) =>
            new RenderingMessageContext
            {
                Font = fontContext.ImageEvent,
                Font2 = fontContext.ImageEvent2,
                Icon = fontContext.ImageIcon,
                FontSpacing = fontContext.SpacingEvent,
                IconSpacing = fontContext.SpacingIcon,
                Encoder = Encoders.JapaneseEvent,
                FontWidth = Constants.FontJapaneseEventWidth,
                FontHeight = Constants.FontJapaneseEventHeight,
                TableHeight = Constants.FontTableEventHeight,
            };
    }
}

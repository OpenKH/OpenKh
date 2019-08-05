using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using OpenKh.Tools.Common.Models;
using kh2 = OpenKh.Kh2.Contextes;

namespace OpenKh.Tools.Common.Extensions
{
    public static class KingdomTextContextExtensions
    {
        public static KingdomTextContext ToKh2EuSystemTextContext(this kh2.FontContext fontContext) =>
            new KingdomTextContext
            {
                Font = fontContext.ImageSystem,
                Icon = fontContext.ImageIcon,
                FontSpacing = fontContext.SpacingSystem,
                IconSpacing = fontContext.SpacingIcon,
                Encoder = Encoders.InternationalSystem,
                FontWidth = Constants.FontEuropeanSystemWidth,
                FontHeight = Constants.FontEuropeanSystemHeight,
            };

        public static KingdomTextContext ToKh2EuEventTextContext(this kh2.FontContext fontContext) =>
            new KingdomTextContext
            {
                Font = fontContext.ImageEvent,
                Icon = fontContext.ImageIcon,
                FontSpacing = fontContext.SpacingEvent,
                IconSpacing = fontContext.SpacingIcon,
                Encoder = Encoders.InternationalSystem,
                FontWidth = Constants.FontEuropeanEventWidth,
                FontHeight = Constants.FontEuropeanEventHeight,
            };

        public static KingdomTextContext ToKh2JpSystemTextContext(this kh2.FontContext fontContext) =>
            new KingdomTextContext
            {
                Font = fontContext.ImageSystem,
                Icon = fontContext.ImageIcon,
                FontSpacing = fontContext.SpacingSystem,
                IconSpacing = fontContext.SpacingIcon,
                Encoder = Encoders.JapaneseSystem,
                FontWidth = Constants.FontJapaneseSystemWidth,
                FontHeight = Constants.FontJapaneseSystemHeight,
            };

        public static KingdomTextContext ToKh2JpEventTextContext(this kh2.FontContext fontContext) =>
            new KingdomTextContext
            {
                Font = fontContext.ImageEvent,
                Icon = fontContext.ImageIcon,
                FontSpacing = fontContext.SpacingEvent,
                IconSpacing = fontContext.SpacingIcon,
                Encoder = Encoders.JapaneseSystem,
                FontWidth = Constants.FontJapaneseEventWidth,
                FontHeight = Constants.FontJapaneseEventHeight,
            };
    }
}

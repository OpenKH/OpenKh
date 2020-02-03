﻿using OpenKh.Kh2;
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
                Font2 = fontContext.ImageSystem2,
                Icon = fontContext.ImageIcon,
                FontSpacing = fontContext.SpacingSystem,
                IconSpacing = fontContext.SpacingIcon,
                Encoder = Encoders.InternationalSystem,
                FontWidth = Constants.FontEuropeanSystemWidth,
                FontHeight = Constants.FontEuropeanSystemHeight,
                TableHeight = Constants.FontTableSystemHeight,
            };

        public static KingdomTextContext ToKh2EuEventTextContext(this kh2.FontContext fontContext) =>
            new KingdomTextContext
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

        public static KingdomTextContext ToKh2JpSystemTextContext(this kh2.FontContext fontContext) =>
            new KingdomTextContext
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

        public static KingdomTextContext ToKh2JpEventTextContext(this kh2.FontContext fontContext) =>
            new KingdomTextContext
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

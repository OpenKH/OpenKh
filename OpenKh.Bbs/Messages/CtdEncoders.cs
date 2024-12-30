using OpenKh.Bbs.Messages.Internals;

namespace OpenKh.Bbs.Messages
{
    public static class CtdEncoders
    {
        /// <summary>
        /// The unified CTD message encoder and decoder fit for both International and Japanese usage.
        /// </summary>
        public static ICtdMessageEncoder Unified { get; } =
            new UnifiedCtdEncoder();
    }
}

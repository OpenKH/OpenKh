using OpenKh.Bbs.Messages.Internals;

namespace OpenKh.Bbs.Messages
{
    public static class CtdEncoders
    {
        public static ICtdMessageEncoder International { get; } =
            new InternationalCtdEncoder();

        public static ICtdMessageEncoder Japanese { get; } =
            new InternationalCtdEncoder();
    }
}

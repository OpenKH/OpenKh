using kh.kh2.Messages.Internals;

namespace kh.kh2.Messages
{
    public static class Encoders
    {
        public static IMessageEncoder InternationalSystem { get; } =
            new InternationalSystemEncoder();
    }
}

using OpenKh.Kh2.Messages.Internals;

namespace OpenKh.Kh2.Messages
{
    public static class Encoders
    {
        public static IMessageEncoder InternationalSystem { get; } =
            new InternationalSystemEncoder();
    }
}

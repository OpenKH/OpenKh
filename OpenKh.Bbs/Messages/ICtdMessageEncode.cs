namespace OpenKh.Bbs.Messages
{
    public interface ICtdMessageEncode
    {
        byte[] Encode(string text);
    }
}

namespace OpenKh.Bbs.Messages
{
    public interface ICtdMessageEncode
    {
        byte[] FromText(string text);
    }
}

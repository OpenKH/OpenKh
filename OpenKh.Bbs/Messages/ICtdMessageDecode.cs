namespace OpenKh.Bbs.Messages
{
    public interface ICtdMessageDecode
    {
        string ToText(byte[] data);
    }
}

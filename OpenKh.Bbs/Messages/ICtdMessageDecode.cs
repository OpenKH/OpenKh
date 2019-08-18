namespace OpenKh.Bbs.Messages
{
    public interface ICtdMessageDecode
    {
        string Decode(byte[] data);
    }
}

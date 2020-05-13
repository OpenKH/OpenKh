namespace OpenKh.Engine
{
    public interface IMessageProvider
    {
        string GetMessage(ushort id);

        void SetMessage(ushort id, string text);
    }
}

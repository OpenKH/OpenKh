namespace OpenKh.Engine
{
    public interface IMessageProvider
    {
        string GetString(ushort id);

        void SetString(ushort id, string text);
    }
}

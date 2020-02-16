namespace OpenKh.Tools.Kh2SystemEditor.Interfaces
{
    public interface IMessageProvider
    {
        string GetMessage(ushort id);
        void SetMessage(ushort id, string text);
    }
}

namespace OpenKh.Game.Debugging
{
    public interface IDebug
    {
        void Print(string text);
        void Print(ushort messageId);
        void Println(string text);
        void Println(ushort messageId);
    }
}

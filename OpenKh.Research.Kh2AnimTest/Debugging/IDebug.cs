using OpenKh.Research.Kh2AnimTest.States;

namespace OpenKh.Research.Kh2AnimTest.Debugging
{
    public interface IDebug : IStateChange
    {
        void Print(string text);
        void Print(ushort messageId);
        void Println(string text);
        void Println(ushort messageId);
    }
}

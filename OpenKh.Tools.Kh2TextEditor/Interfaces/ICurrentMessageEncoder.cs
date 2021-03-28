using OpenKh.Kh2.Messages;

namespace OpenKh.Tools.Kh2TextEditor.Interfaces
{
    public interface ICurrentMessageEncoder
    {
        IMessageEncoder CurrentMessageEncoder { get; }
    }
}

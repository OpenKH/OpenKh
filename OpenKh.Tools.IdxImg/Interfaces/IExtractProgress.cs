using System.Threading;

namespace OpenKh.Tools.IdxImg.Interfaces
{
    public interface IExtractProgress
    {
        CancellationToken CancellationToken { get; }
        void SetExtractedName(string name);
    }
}

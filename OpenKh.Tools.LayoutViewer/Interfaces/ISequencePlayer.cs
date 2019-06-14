namespace OpenKh.Tools.LayoutViewer.Interfaces
{
    public interface ISequencePlayer
    {
        bool IsSequencePlaying { get; set; }
        int FrameIndex { get; set; }
    }
}

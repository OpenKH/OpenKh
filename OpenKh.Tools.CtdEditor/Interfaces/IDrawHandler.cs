using OpenKh.Bbs;
using OpenKh.Bbs.Messages;
using Xe.Drawing;

namespace OpenKh.Tools.CtdEditor.Interfaces
{
    public interface IDrawHandler
    {
        IDrawing DrawingContext { get; }

        void DrawHandler(
            ICtdMessageEncoder encoder,
            FontsArc.Font fontContext,
            Ctd.Message message,
            Ctd.Layout layout);
    }
}

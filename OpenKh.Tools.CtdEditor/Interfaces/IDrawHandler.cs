using OpenKh.Bbs;
using OpenKh.Bbs.Messages;
using OpenKh.Engine.Renders;

namespace OpenKh.Tools.CtdEditor.Interfaces
{
    public interface IDrawHandler
    {
        ISpriteDrawing DrawingContext { get; }

        void DrawHandler(
            ICtdMessageEncoder encoder,
            FontsArc.Font fontContext,
            Ctd.Message message,
            Ctd.Layout layout);
    }
}

using OpenKh.Bbs;
using OpenKh.Bbs.Messages;
using OpenKh.Engine.Renders;

namespace OpenKh.Tools.CtdEditor.Interfaces
{
    public interface IDrawHandler
    {
        ISpriteDrawing DrawingContext { get; }

        void Create();

        void SetFont(FontsArc.Font font);
        
        void DrawHandler(
            ICtdMessageEncoder encoder,
            Ctd.Message message,
            Ctd.Layout layout);

        void Destroy();
    }
}

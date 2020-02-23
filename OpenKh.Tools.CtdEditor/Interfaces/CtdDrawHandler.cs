using OpenKh.Bbs;
using OpenKh.Bbs.Messages;
using OpenKh.Tools.Common;
using System.Drawing;
using System.Linq;
using Xe.Drawing;

namespace OpenKh.Tools.CtdEditor.Interfaces
{
    public class CtdDrawHandler : IDrawHandler
    {
        public CtdDrawHandler()
        {
            DrawingContext = new DrawingDirect3D();
        }

        public IDrawing DrawingContext { get; }

        public void DrawHandler(ICtdMessageEncoder encoder, FontsArc.Font fontContext, Ctd.Message message)
        {
            DrawingContext.Clear(Color.Black);

            const int BeginX = 0;
            var x = BeginX;
            var y = 0;
            var texture1 = DrawingContext.CreateSurface(fontContext.Image1);
            var texture2 = DrawingContext.CreateSurface(fontContext.Image2);
            foreach (var ch in encoder.ToUcs(message.Data))
            {
                if (ch >= 0x20)
                {
                    var chInfo = fontContext.CharactersInfo.FirstOrDefault(info => info.Id == ch);
                    if (chInfo == null)
                    {
                        if (ch == 0x20)
                            x += fontContext.Info.CharacterWidth / 2;
                        continue;
                    }
                    if (chInfo.Palette >= 2) continue;

                    var texture = chInfo.Palette == 0 ? texture1 : texture2;
                    var source = new Rectangle
                    {
                        X = chInfo.PositionX,
                        Y = chInfo.PositionY,
                        Width = chInfo.Width,
                        Height = fontContext.Info.CharacterHeight
                    };
                    DrawingContext.DrawSurface(texture, source, x, y);

                    x += source.Width;
                }
                else
                {
                    switch (ch)
                    {
                        case 0x0a: // '\n'
                            x = BeginX;
                            y += fontContext.Info.CharacterHeight;
                            break;
                    }
                }
            }
        }
    }
}

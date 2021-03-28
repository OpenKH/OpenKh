using OpenKh.Bbs;
using OpenKh.Bbs.Messages;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renders;
using OpenKh.Tools.Common.Rendering;
using System.Drawing;
using System.Linq;

namespace OpenKh.Tools.CtdEditor.Interfaces
{
    public class CtdDrawHandler : IDrawHandler
    {
        private const int PspScreenWidth = 480;
        private const int PspScreenHeight = 272;

        public CtdDrawHandler()
        {
            DrawingContext = new SpriteDrawingDirect3D();
        }

        public ISpriteDrawing DrawingContext { get; }

        public void DrawHandler(
            ICtdMessageEncoder encoder,
            FontsArc.Font fontContext,
            Ctd.Message message,
            Ctd.Layout layout)
        {
            DrawPspScreen();
            DrawDialog(layout);

            int BeginX = layout.DialogX + layout.TextX;
            int BeginY = layout.DialogY + layout.TextY;
            var x = BeginX;
            var y = BeginY;
            var texture1 = DrawingContext.CreateSpriteTexture(fontContext.Image1);
            var texture2 = DrawingContext.CreateSpriteTexture(fontContext.Image2);
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
                    if (chInfo.Palette >= 2)
                        continue;

                    var texture = chInfo.Palette == 0 ? texture1 : texture2;
                    var source = new Rectangle
                    {
                        X = chInfo.PositionX,
                        Y = chInfo.PositionY,
                        Width = chInfo.Width,
                        Height = fontContext.Info.CharacterHeight
                    };
                    DrawingContext.AppendSprite(new SpriteDrawingContext()
                        .SpriteTexture(texture)
                        .Source(chInfo.PositionX, chInfo.PositionY, chInfo.Width, fontContext.Info.CharacterHeight)
                        .MatchSourceSize()
                        .Position(x, y));

                    x += source.Width + layout.HorizontalSpace;
                }
                else
                {
                    switch (ch)
                    {
                        case 0x0a: // '\n'
                            x = BeginX;
                            y += 16 + layout.VerticalSpace;
                            break;
                    }
                }
            }
        }

        private void DrawPspScreen() =>
            DrawingContext.FillRectangle(0, 0, PspScreenWidth, PspScreenHeight, ColorF.Black);

        private void DrawDialog(Ctd.Layout layout) => DrawingContext.DrawRectangle(
            layout.DialogX - 1,
            layout.DialogY - 1,
            layout.DialogWidth + 1,
            layout.DialogHeight + 1,
            ColorF.FromRgba(Color.Cyan.ToArgb()));
    }
}

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
        private ISpriteDrawing _drawingContext;

        public CtdDrawHandler()
        {
            _drawingContext = new SpriteDrawingDirect3D();
        }

        public ISpriteDrawing DrawingContext => _drawingContext;
        private FontsArc.Font _currentFont;
        private ISpriteTexture _fontTex1;
        private ISpriteTexture _fontTex2;

        public void Create()
        {
            
        }

        public void SetFont(FontsArc.Font font)
        {
            if (_currentFont != null)
            {
                _fontTex1.Dispose();
                _fontTex1 = null;
                _fontTex2.Dispose();
                _fontTex2 = null;
                _currentFont = null;
            }
            _currentFont = font;
            _fontTex1 = _drawingContext.CreateSpriteTexture(font.Image1);
            _fontTex2 = _drawingContext.CreateSpriteTexture(font.Image2);
        }

        public void DrawHandler(
            ICtdMessageEncoder encoder,
            Ctd.Message message,
            Ctd.Layout layout)
        {
            if (_currentFont == null)
                return;

            DrawPspScreen();
            DrawDialog(layout);

            int BeginX = layout.DialogX + layout.TextX;
            int BeginY = layout.DialogY + layout.TextY;
            var x = BeginX;
            var y = BeginY;
            foreach (var ch in encoder.ToUcs(message.Data))
            {
                if (ch >= 0x20)
                {
                    var chInfo = _currentFont.CharactersInfo.FirstOrDefault(info => info.Id == ch);
                    if (chInfo == null)
                    {
                        if (ch == 0x20) // space
                            x += _currentFont.Info.CharacterWidth / 2;
                        continue;
                    }
                    if (chInfo.Palette >= 2)
                        continue;

                    var texture = chInfo.Palette == 0 ? _fontTex1 : _fontTex2;
                    var source = new Rectangle
                    {
                        X = chInfo.PositionX,
                        Y = chInfo.PositionY,
                        Width = chInfo.Width,
                        Height = _currentFont.Info.CharacterHeight
                    };
                    DrawingContext.AppendSprite(new SpriteDrawingContext()
                        .SpriteTexture(texture)
                        .Source(chInfo.PositionX, chInfo.PositionY, chInfo.Width, _currentFont.Info.CharacterHeight)
                        .MatchSourceSize()
                        .Position(x, y)
                        .ColorDefault());

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

        public void Destroy()
        {
            if (_currentFont != null)
                SetFont(null);

            DrawingContext.Dispose();
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

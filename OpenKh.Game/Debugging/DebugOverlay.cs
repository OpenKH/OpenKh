using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.States;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System;
using System.Collections.Generic;

namespace OpenKh.Game.Debugging
{
    // Most of the stuff here is copied from OpenKh.Tools.Common\Controls\KingdomTextArea.cs
    public class DebugOverlay : IState, IDebug
    {
        private IDataContent _dataContent;
        private ArchiveManager _archiveManager;
        private Kernel _kernel;
        private InputManager _inputManager;
        private GraphicsDeviceManager _graphics;
        private bool _overrideExternalDebugFeatures = false;

        private Texture2D _texFontSys1;
        private Texture2D _texFontSys2;
        private Texture2D _texFontEvt1;
        private Texture2D _texFontEvt2;
        private Texture2D _texFontIcon;

        private SpriteBatch _spriteBatch;

        private const int _fontWidth = Constants.FontEuropeanSystemWidth;
        private const int _fontHeight = Constants.FontEuropeanSystemHeight;
        private const int _tableHeight = Constants.FontTableSystemHeight;
        private const int _charTableHeight = Constants.FontTableSystemHeight / _fontHeight * _fontHeight;
        private int _charPerRow;
        private float _textX;
        private float _textY;
        private float _widthMultiplier = 1.0f;
        private float _scale = 1.0f;

        public DebugOverlay()
        {
        }

        public Action<IDebug> OnUpdate { get; set; }
        public Action<IDebug> OnDraw { get; set; }

        public void Initialize(StateInitDesc initDesc)
        {
            _spriteBatch = new SpriteBatch(initDesc.GraphicsDevice.GraphicsDevice);

            _dataContent = initDesc.DataContent;
            _archiveManager = initDesc.ArchiveManager;
            _kernel = initDesc.Kernel;
            _inputManager = initDesc.InputManager;
            _graphics = initDesc.GraphicsDevice;

            _texFontSys1 = _kernel.FontContext.ImageSystem.CreateTexture(_graphics.GraphicsDevice);
            _texFontSys2 = _kernel.FontContext.ImageSystem2.CreateTexture(_graphics.GraphicsDevice);
            _texFontEvt1 = _kernel.FontContext.ImageEvent.CreateTexture(_graphics.GraphicsDevice);
            _texFontEvt2 = _kernel.FontContext.ImageEvent2.CreateTexture(_graphics.GraphicsDevice);
            _texFontIcon = _kernel.FontContext.ImageIcon.CreateTexture(_graphics.GraphicsDevice);

            _charPerRow = _kernel.FontContext.ImageSystem.Size.Width / _fontWidth;
        }

        public void Destroy()
        {
            _spriteBatch?.Dispose();
            _texFontSys1?.Dispose();
            _texFontSys2?.Dispose();
            _texFontEvt1?.Dispose();
            _texFontEvt2?.Dispose();
            _texFontIcon?.Dispose();
        }

        public void Update(DeltaTimes deltaTimes)
        {
            DebugUpdate(this);
            if (!_overrideExternalDebugFeatures)
                OnUpdate?.Invoke(this);
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            // Small hack to avoid weird stuff happening:
            // basically SpriteBatch sets its own stuff in the GraphicsDevice,
            // making 3D models to look weird. Here we are backing up the states
            // before SpriteBatch reset them as its own will, to restore them later.
            var blendState = _graphics.GraphicsDevice.BlendState;
            var depthStencil = _graphics.GraphicsDevice.DepthStencilState;

            _textX = 0;
            _textY = 0;

            DebugDraw(this);
            if (!_overrideExternalDebugFeatures)
                OnDraw?.Invoke(this);

            // small hack: see first comment of the method
            _graphics.GraphicsDevice.DepthStencilState = depthStencil;
            _graphics.GraphicsDevice.BlendState = blendState;
        }

        public void Print(string text)
        {
            var data = Encoders.InternationalSystem.Encode(new List<MessageCommandModel>()
            {
                new MessageCommandModel()
                {
                    Command = MessageCommand.PrintText,
                    Text = text
                }
            });

            Print(data);
        }

        public void Print(ushort messageId) =>
            Print(_kernel.MessageProvider.GetMessage(messageId));

        public void Println(string text)
        {
            Print(text);
            EmitNewLine();
        }

        public void Println(ushort messageId)
        {
            Print(messageId);
            EmitNewLine();
        }

        private void Print(byte[] data)
        {
            var fontSpacing = _kernel.FontContext.SpacingSystem;

            for (int i = 0; i < data.Length; i++)
            {
                byte ch = data[i];
                int spacing;

                if (ch >= 0x20)
                {
                    int chIndex = ch - 0x20;
                    DrawChar(chIndex);
                    spacing = fontSpacing?[chIndex] ?? _fontWidth;
                }
                else if (ch >= 0x19 && ch <= 0x1f)
                {
                    int chIndex = data[++i] + (ch - 0x19) * 0x100 + 0xE0;
                    DrawChar(chIndex);
                    spacing = fontSpacing?[chIndex] ?? _fontHeight;
                }
                else
                {
                    spacing = 0;
                    switch ((MessageCommand)ch)
                    {
                        case MessageCommand.PrintText:
                            spacing = 6;
                            break;
                        case MessageCommand.NewLine:
                            EmitNewLine();
                            break;
                        case MessageCommand.TextWidth:
                            // TODO
                            break;
                        case MessageCommand.TextScale:
                            // TODO
                            break;
                        case MessageCommand.Color:
                            // TODO
                            break;
                    }
                }

                _textX += spacing * _widthMultiplier * _scale;
            }
        }

        private void EmitNewLine()
        {
            _textX = 0;
            _textY += _fontHeight * _scale;
        }

        private void DrawChar(int index)
        {
            DrawChar((index % _charPerRow) * _fontWidth, (index / _charPerRow) * _fontHeight);
        }

        private void DrawChar(int sourceX, int sourceY)
        {
            Texture2D fontTexture = null;

            var tableIndex = sourceY / _charTableHeight;
            sourceY %= _charTableHeight;

            if ((tableIndex & 1) != 0)
                fontTexture = _texFontSys2;
            else
                fontTexture = _texFontSys1;

            if ((tableIndex & 2) != 0)
                sourceY += _tableHeight;

            if (fontTexture == null)
                return;

            DrawImageScale(fontTexture, sourceX, sourceY, _fontWidth, _fontHeight);
        }

        private void DrawImageScale(
            Texture2D surface, int sourceX, int sourceY, int width, int height) =>
            DrawImage(surface, _textX, _textY, sourceX, sourceY, width, height, _widthMultiplier * _scale, _scale, 1.0f, 1.0f, 1.0f, 1.0f);

        protected void DrawImage(Texture2D texture,
            double x, double y, int sourceX, int sourceY,
            int width, int height, double scaleX, double scaleY,
            float r, float g, float b, float a)
        {
            var dstWidth = width * scaleX;
            var dstHeight = height * scaleY;
            var src = new Rectangle(sourceX, sourceY, width, height);
            var dst = new Rectangle((int)x, (int)y, (int)dstWidth, (int)dstHeight);

            _spriteBatch.Begin();
            _spriteBatch.Draw(texture, dst, src, new Color(r, g, b, a));
            _spriteBatch.End();
        }

        public void DebugUpdate(IDebug debug)
        {
        }

        public void DebugDraw(IDebug debug)
        {
        }
    }
}

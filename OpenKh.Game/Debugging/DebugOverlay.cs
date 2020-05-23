using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Renders;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.States;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

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
        private readonly IStateChange _stateChange;
        private bool _overrideExternalDebugFeatures = false;

        private Texture2D _texFontSys1;
        private Texture2D _texFontSys2;
        private Texture2D _texFontEvt1;
        private Texture2D _texFontEvt2;
        private Texture2D _texFontIcon;

        private SpriteBatch _spriteBatch;
        
        
        private IMessageRenderer _messageRenderer;
        private DrawContext _messageDrawContext;



        private const int _fontWidth = Constants.FontEuropeanSystemWidth;
        private const int _fontHeight = Constants.FontEuropeanSystemHeight;

        public DebugOverlay(IStateChange stateChange)
        {
            _stateChange = stateChange;

        }

        public Action<IDebug> OnUpdate { get; set; }
        public Action<IDebug> OnDraw { get; set; }
        public int State { set => _stateChange.State = value; }

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

            var drawing = new MonoDrawing(
                initDesc.GraphicsDevice.GraphicsDevice, initDesc.ContentManager);
            _messageRenderer = new Kh2MessageRenderer(drawing, new RenderingMessageContext
            {
                Font = _kernel.FontContext.ImageSystem,
                Font2 = _kernel.FontContext.ImageSystem2,
                Icon = _kernel.FontContext.ImageIcon,
                FontSpacing = _kernel.FontContext.SpacingSystem,
                IconSpacing = _kernel.FontContext.SpacingIcon,
                Encoder = Encoders.InternationalSystem,

                FontWidth = Constants.FontEuropeanSystemWidth,
                FontHeight = Constants.FontEuropeanSystemHeight,
                TableHeight = Constants.FontTableSystemHeight,
            });
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

            _messageDrawContext = new DrawContext
            {
                IgnoreDraw = false,
                
                x = 0,
                y = 0,
                xStart = 0,
                Width = 0,
                Height = 0,
                WindowWidth = 512,

                Scale = 1,
                WidthMultiplier = 1,
                Color = new Xe.Drawing.ColorF(1.0f, 1.0f, 1.0f, 1.0f)
            };

            DebugDraw(this);
            if (!_overrideExternalDebugFeatures)
                OnDraw?.Invoke(this);

            // small hack: see first comment of the method
            _graphics.GraphicsDevice.BlendState = blendState;
        }

        public void Print(string text)
        {
            Print(Encoders.InternationalSystem.Encode(new List<MessageCommandModel>()
            {
                new MessageCommandModel()
                {
                    Command = MessageCommand.PrintText,
                    Text = text
                }
            }));
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
            _messageDrawContext.WidthMultiplier = 1.2f;
            _messageRenderer.Draw(_messageDrawContext, data);
        }

        private void EmitNewLine()
        {

            Print(Encoders.InternationalSystem.Encode(new List<MessageCommandModel>()
            {
                new MessageCommandModel()
                {
                    Command = MessageCommand.NewLine,
                }
            }));
        }

        public void DebugUpdate(IDebug debug)
        {
        }

        public void DebugDraw(IDebug debug)
        {
        }
    }
}

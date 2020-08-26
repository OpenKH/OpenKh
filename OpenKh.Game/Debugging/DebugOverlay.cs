using Microsoft.Xna.Framework;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Renders;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.States;
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
        private KingdomShader _shader;
        private readonly IStateChange _stateChange;
        private bool _overrideExternalDebugFeatures = false;

        private ISpriteDrawing _drawing;
        private IMessageEncoder _encoder;
        private IMessageRenderer _messageRenderer;
        private DrawContext _messageDrawContext;

        public DebugOverlay(IStateChange stateChange)
        {
            _stateChange = stateChange;

        }

        public Action<IDebug> OnUpdate { get; set; }
        public Action<IDebug> OnDraw { get; set; }
        public int State { set => _stateChange.State = value; }

        public void Initialize(StateInitDesc initDesc)
        {
            _dataContent = initDesc.DataContent;
            _archiveManager = initDesc.ArchiveManager;
            _kernel = initDesc.Kernel;
            _inputManager = initDesc.InputManager;
            _graphics = initDesc.GraphicsDevice;

            _shader = new KingdomShader(initDesc.ContentManager);

            var viewport = initDesc.GraphicsDevice.GraphicsDevice.Viewport;
            _drawing = new MonoSpriteDrawing(_graphics.GraphicsDevice, _shader);
            _drawing.SetProjection(
                viewport.Width,
                viewport.Height,
                Global.ResolutionWidth,
                Global.ResolutionHeight,
                1.0f);

            var messageContext = _kernel.SystemMessageContext;
            _encoder = messageContext.Encoder;
            _messageRenderer = new Kh2MessageRenderer(_drawing, messageContext);
        }

        public void Destroy()
        {
            _shader.Dispose();
            _drawing.Dispose();
            // TODO destroy textures created by Kh2MessageRenderer
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
                Color = new ColorF(1.0f, 1.0f, 1.0f, 1.0f)
            };

            DebugDraw(this);
            if (!_overrideExternalDebugFeatures)
                OnDraw?.Invoke(this);
            _drawing.Flush();

            // small hack: see first comment of the method
            _graphics.GraphicsDevice.BlendState = blendState;
        }

        public void Print(string text)
        {
            Print(_encoder.Encode(new List<MessageCommandModel>()
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
            Print(_encoder.Encode(new List<MessageCommandModel>()
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

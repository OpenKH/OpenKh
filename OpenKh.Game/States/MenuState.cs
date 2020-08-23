using OpenKh.Engine.Extensions;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.Menu;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.States
{
    public class MenuState : IState
    {
        private Kernel _kernel;
        private IDataContent _content;
        private ArchiveManager _archiveManager;
        private InputManager _inputManager;
        private KingdomShader _shader;
        private MonoSpriteDrawing _drawing;
        private Layout _campLayout;
        private LayoutRenderer _layoutRenderer;
        private List<ISpriteTexture> _textures = new List<ISpriteTexture>();
        private Dictionary<ushort, byte[]> _cachedText = new Dictionary<ushort, byte[]>();

        private List<IAnimatedSequence> _mainSeqGroup;
        private IAnimatedSequence _backgroundSeq;

        private AnimatedSequenceFactory _animSeqFactory;
        private Kh2MessageRenderer _messageRenderer;
        private IMenu _subMenu;

        public bool IsMenuOpen { get; private set; }

        public void Initialize(StateInitDesc initDesc)
        {
            _kernel = initDesc.Kernel;
            _content = initDesc.DataContent;
            _archiveManager = initDesc.ArchiveManager;
            _inputManager = initDesc.InputManager;

            var viewport = initDesc.GraphicsDevice.GraphicsDevice.Viewport;
            _shader = new KingdomShader(initDesc.ContentManager);
            _drawing = new MonoSpriteDrawing(initDesc.GraphicsDevice.GraphicsDevice, _shader);
            _drawing.SetProjection(
                viewport.Width,
                viewport.Height,
                Global.ResolutionWidth,
                Global.ResolutionHeight,
                1.0f);
            initDesc.GraphicsDevice.GraphicsDevice.DepthStencilState = new Microsoft.Xna.Framework.Graphics.DepthStencilState
            {
                DepthBufferEnable = false,
                StencilEnable = false,
            };

            var messageContext = _kernel.SystemMessageContext;
            _messageRenderer = new Kh2MessageRenderer(_drawing, messageContext);

            _archiveManager.LoadArchive($"menu/{_kernel.Region}/camp.2ld");
            (_campLayout, _textures) = GetLayoutResources("camp", "camp");
            _layoutRenderer = new LayoutRenderer(_campLayout, _drawing, _textures)
            {
                SelectedSequenceGroupIndex = 0
            };

            _animSeqFactory = new AnimatedSequenceFactory(
                _drawing,
                initDesc.Kernel.MessageProvider,
                _messageRenderer,
                _kernel.SystemMessageContext.Encoder,
                _campLayout.SequenceItems[1],
                _textures.First());

            _mainSeqGroup = CreateMultipleAnimatedSequences(107, 110, 113);
            _backgroundSeq = CreateAnimationSequence(46);

            _subMenu = new MainMenu(_animSeqFactory, _inputManager);
        }

        public void Destroy()
        {
            foreach (var texture in _textures)
                texture.Dispose();
            _shader.Dispose();
            _drawing.Dispose();
        }

        public void OpenMenu()
        {
            _layoutRenderer.FrameIndex = 0;
            _layoutRenderer.SelectedSequenceGroupIndex = 0;

            _backgroundSeq.Begin();
            ForAll(_mainSeqGroup, x => x.Begin());
            _subMenu.Open();

            IsMenuOpen = true;
        }

        public void CloseMenu()
        {
            _layoutRenderer.FrameIndex = 0;
            _layoutRenderer.SelectedSequenceGroupIndex = 2;

            _backgroundSeq.End();
            ForAll(_mainSeqGroup, x => x.End());
            _subMenu.Close();
        }

        public void Update(DeltaTimes deltaTimes)
        {
            var deltaTime = deltaTimes.DeltaTime;

            ProcessInput(_inputManager);
            
            _layoutRenderer.FrameIndex++;
            foreach (var animSequence in _mainSeqGroup)
                animSequence.Update(deltaTime);
            _backgroundSeq.Update(deltaTime);
            _subMenu.Update(deltaTime);
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            switch (_layoutRenderer.SelectedSequenceGroupIndex)
            {
                case 0:
                    if (_layoutRenderer.IsLastFrame)
                    {
                        _layoutRenderer.FrameIndex = 0;
                        _layoutRenderer.SelectedSequenceGroupIndex = 1;
                    }
                    break;
                case 1:
                    break;
                case 2:
                    if (_layoutRenderer.IsLastFrame)
                    {
                        IsMenuOpen = false;
                    }
                    break;
            }
            if (_layoutRenderer.SelectedSequenceGroupIndex == 0 && _layoutRenderer.IsLastFrame)
                _layoutRenderer.SelectedSequenceGroupIndex = 1;

            _layoutRenderer.Draw();
            foreach (var animSequence in _mainSeqGroup)
                animSequence.Draw(0, 0);

            _backgroundSeq.Draw(0, 0);
            _subMenu.Draw();

            _drawing.Flush();
        }

        private void ProcessInput(InputManager inputManager)
        {
            if (inputManager.IsStart)
                CloseMenu();
        }

        private (Layout layout, List<ISpriteTexture> textures) GetLayoutResources(string layoutResourceName, string imagesResourceName)
        {
            var layout = _archiveManager.Get<Layout>(layoutResourceName);
            _textures = _archiveManager.Get<Imgz>(imagesResourceName)
                    ?.Images?.Select(x => _drawing.CreateSpriteTexture(x)).ToList();

            return (layout, _textures);
        }

        private List<IAnimatedSequence> CreateMultipleAnimatedSequences(params int[] anims)
        {
            var sequences = new List<IAnimatedSequence>();
            foreach (var animationIndex in anims)
            {
                sequences.Add(CreateAnimationSequence(animationIndex));
            }

            return sequences;
        }

        private IAnimatedSequence CreateAnimationSequence(int anim) =>
            CreateAnimationSequence(anim, anim + 1, anim + 2);

        private IAnimatedSequence CreateAnimationSequence(int start, int loop, int end)
        {
            var item = _animSeqFactory.Create(new AnimatedSequenceDesc
            {
                SequenceIndexLoop = loop,
                SequenceIndexStart = start,
                SequenceIndexEnd = end
            });
            item.Begin();

            return item;
        }

        public byte[] GetMessage(ushort messageId)
        {
            if (!_cachedText.TryGetValue(messageId, out var data))
                _cachedText[messageId] = data = _kernel.MessageProvider.GetMessage(messageId);

            return data;
        }

        private static void ForAll<T>(IEnumerable<T> list, Action<T> action)
        {
            foreach (var item in list)
                action(item);
        }


        public void DebugDraw(IDebug debug)
        {
        }

        public void DebugUpdate(IDebug debug)
        {
        }
    }
}

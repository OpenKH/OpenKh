using OpenKh.Engine.Extensions;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.States.Title;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.States
{
    public class MenuState : IState
    {
        private const int MaxCharacterCount = 4;
        private const int MenuElementCount = 7;
        private const int MenuOptionSelectedSeq = 132;
        private static readonly ushort[] MenuOptions = new ushort[MenuElementCount]
        {
            0x844b,
            0x844d,
            0x8451,
            0x844e,
            0x844f,
            0x8450,
            0xb617,
        };

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

        private List<AnimatedSequenceRenderer> _mainSeqGroup;
        private List<AnimatedSequenceRenderer> _menuOptionSeqs;
        private AnimatedSequenceRenderer _backgroundSeq;
        private List<AnimatedSequenceRenderer> _characterDescSeqs;
        private AnimatedSequenceRenderer _menuOptionSelectedSeq;
        private AnimatedSequenceRenderer _menuOptionCursorSeq;
        private AnimatedSequenceRenderer _menuOptionLumSeq;

        private int _selectedOption = 0;
        private Kh2MessageRenderer _messageRenderer;

        public int MenuOption
        {
            get => _selectedOption;
            set
            {
                _selectedOption = value;
                if (_selectedOption < 0)
                    _selectedOption += MenuElementCount;
                _selectedOption %= MenuElementCount;

                _menuOptionSelectedSeq.SetMessage(_messageRenderer,
                    GetMessage(MenuOptions[_selectedOption]));
            }
        }

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

            _mainSeqGroup = CreateMultipleAnimatedSequences(107, 110, 113);

            _backgroundSeq = CreateAnimationSequence(46);
            _characterDescSeqs = Enumerable.Range(0, MaxCharacterCount)
                .Select(_ => CreateAnimationSequence(93, 93, 93))
                .ToList();

            _menuOptionSeqs = new List<AnimatedSequenceRenderer>();
            for (var i = 0; i < MenuElementCount; i++)
            {
                var animSequence = CreateAnimationSequence(133);
                animSequence.SetMessage(_messageRenderer, GetMessage(MenuOptions[i]));
                _menuOptionSeqs.Add(animSequence);
            }

            _menuOptionSelectedSeq = CreateAnimationSequence(MenuOptionSelectedSeq);
            _menuOptionCursorSeq = CreateAnimationSequence(25, 25, 25);
            _menuOptionLumSeq = CreateAnimationSequence(27);

            MenuOption = 0;
        }

        public void Destroy()
        {
            foreach (var texture in _textures)
                texture.Dispose();
            _shader.Dispose();
            _drawing.Dispose();
        }

        public void Update(DeltaTimes deltaTimes)
        {
            var deltaTime = deltaTimes.DeltaTime;

            ProcessInput(_inputManager);
            
            _layoutRenderer.FrameIndex++;
            foreach (var animSequence in _mainSeqGroup)
                animSequence.Update(deltaTime);
            foreach (var animSequence in _menuOptionSeqs)
                animSequence.Update(deltaTime);
            _backgroundSeq.Update(deltaTime);
            _menuOptionSelectedSeq.Update(deltaTime);
            _menuOptionCursorSeq.Update(deltaTime);
            _menuOptionLumSeq.Update(deltaTime);
        }

        public void Draw(DeltaTimes deltaTimes)
        {
            if (_layoutRenderer.SelectedSequenceGroupIndex == 0 && _layoutRenderer.IsLastFrame)
                _layoutRenderer.SelectedSequenceGroupIndex = 1;

            _layoutRenderer.Draw();
            foreach (var animSequence in _mainSeqGroup)
                animSequence.Draw(0, 0);

            _backgroundSeq.Draw(0, 0);

            for (int i = 0; i < _menuOptionSeqs.Count; i++)
            {
                const float PosX = 48;
                const float PosY = 82;
                const float Distance = 26;
                var item = _menuOptionSeqs[i];

                if (i == _selectedOption)
                {
                    _menuOptionSelectedSeq.Draw(0, i * Distance);
                    _menuOptionCursorSeq.Draw(PosX, PosY + i * Distance);
                    _menuOptionLumSeq.Draw(PosX + 100, PosY + i * Distance);
                }
                else
                    item.Draw(0, i * Distance);
            }

            for (int i = 0; i < _characterDescSeqs.Count; i++)
            {
                const float PosX = 164;
                const float PosY = 300;
                const float Distance = 96; 
                var item = _characterDescSeqs[i];
                item.Draw(PosX + i * Distance, PosY);
            }

            _drawing.Flush();
        }

        private void ProcessInput(InputManager inputManager)
        {
            if (inputManager.IsMenuUp)
                MenuOption--;
            if (inputManager.IsMenuDown)
                MenuOption++;
        }

        private (Layout layout, List<ISpriteTexture> textures) GetLayoutResources(string layoutResourceName, string imagesResourceName)
        {
            var layout = _archiveManager.Get<Layout>(layoutResourceName);
            _textures = _archiveManager.Get<Imgz>(imagesResourceName)
                    ?.Images?.Select(x => _drawing.CreateSpriteTexture(x)).ToList();

            return (layout, _textures);
        }

        private List<AnimatedSequenceRenderer> CreateMultipleAnimatedSequences(params int[] anims)
        {
            var sequences = new List<AnimatedSequenceRenderer>();
            foreach (var animationIndex in anims)
            {
                var item = new AnimatedSequenceRenderer(
                    new SequenceRenderer(_campLayout.SequenceItems[1], _drawing, _textures[0]),
                    animationIndex);
                item.Begin();

                sequences.Add(item);
            }

            return sequences;
        }

        private AnimatedSequenceRenderer CreateAnimationSequence(int anim)
        {
            var item = new AnimatedSequenceRenderer(
                new SequenceRenderer(_campLayout.SequenceItems[1], _drawing, _textures[0]),
                anim);
            item.Begin();

            return item;
        }

        private AnimatedSequenceRenderer CreateAnimationSequence(int start, int loop, int end)
        {
            var item = new AnimatedSequenceRenderer(
                new SequenceRenderer(_campLayout.SequenceItems[1], _drawing, _textures[0]),
                start, loop, end);
            item.Begin();

            return item;
        }

        public byte[] GetMessage(ushort messageId)
        {
            if (!_cachedText.TryGetValue(messageId, out var data))
                _cachedText[messageId] = data = _kernel.MessageProvider.GetMessage(messageId);

            return data;
        }

        public void DebugDraw(IDebug debug)
        {
        }

        public void DebugUpdate(IDebug debug)
        {
        }
    }
}

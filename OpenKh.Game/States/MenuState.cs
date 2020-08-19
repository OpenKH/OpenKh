using OpenKh.Engine.Extensions;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.States
{
    public class MenuState : IState
    {
        private const int MaxCharacterCount = 4;
        private const int MenuElementCount = 7;
        private const int MenuOptionSelectedSeq = 132;
        private const int CharacterHpBar = 98;
        private const int CharacterMpBar = 99;
        private const int MsgLv = 0x39FC;
        private const int MsgHp = 0x39FD;
        private const int MsgMp = 0x39FE;
        private static readonly ushort[] MenuOptions = new ushort[MenuElementCount]
        {
            0x844b, // Items
            0x844d, // Abilities
            0x8451, // Customize
            0x844e, // Party
            0x844f, // Status
            0x8450, // Journal
            0xb617, // Config
        };
        private static readonly ushort[] CharacterNames = new ushort[MaxCharacterCount]
        {
            0x851f, // Sora
            0x8520, // Donald
            0x8521, // Goofy
            0x852c, // Riku
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

        private List<IAnimatedSequence> _mainSeqGroup;
        private List<IAnimatedSequence> _menuOptionSeqs;
        private IAnimatedSequence _backgroundSeq;
        private IAnimatedSequence _menuOptionSelectedSeq;
        private IAnimatedSequence _menuOptionCursorSeq;
        private IAnimatedSequence _menuOptionLumSeq;
        private IAnimatedSequence _charHpBarSeq;
        private IAnimatedSequence _charMpBarSeq;

        private AnimatedSequenceFactory _animSeqFactory;
        private IAnimatedSequence _characterDescSample;

        private int _selectedOption = 0;
        private Kh2MessageRenderer _messageRenderer;

        public bool IsMenuOpen { get; private set; }

        public int MenuOption
        {
            get => _selectedOption;
            set
            {
                _selectedOption = value;
                if (_selectedOption < 0)
                    _selectedOption += MenuElementCount;
                _selectedOption %= MenuElementCount;

                _menuOptionSelectedSeq.TextAnchor = TextAnchor.Left;
                _menuOptionSelectedSeq.SetMessage(MenuOptions[_selectedOption]);
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

            _animSeqFactory = new AnimatedSequenceFactory(
                _drawing,
                initDesc.Kernel.MessageProvider,
                _messageRenderer,
                _kernel.SystemMessageContext.Encoder,
                _campLayout.SequenceItems[1],
                _textures.First());
            InitializeMenu();

            _mainSeqGroup = CreateMultipleAnimatedSequences(107, 110, 113);

            _backgroundSeq = CreateAnimationSequence(46);

            _menuOptionSeqs = new List<IAnimatedSequence>();
            for (var i = 0; i < MenuElementCount; i++)
            {
                var animSequence = CreateAnimationSequence(133);
                animSequence.TextAnchor = TextAnchor.Left;
                animSequence.SetMessage(MenuOptions[i]);
                _menuOptionSeqs.Add(animSequence);
            }

            _menuOptionSelectedSeq = CreateAnimationSequence(MenuOptionSelectedSeq);
            _menuOptionCursorSeq = CreateAnimationSequence(25, 25, 25);
            _menuOptionLumSeq = CreateAnimationSequence(27);

            _charHpBarSeq = CreateAnimationSequence(CharacterHpBar);
            _charMpBarSeq = CreateAnimationSequence(CharacterMpBar);

            MenuOption = 0;
        }

        private void InitializeMenu()
        {
            var root = new AnimatedSequenceDesc
            {
                SequenceIndexStart = 101,
                SequenceIndexLoop = 102,
                SequenceIndexEnd = 103,
                Children = Enumerable.Range(0, 5)
                    .Select(i => new AnimatedSequenceDesc
                    {
                        SequenceIndexLoop = 93,
                        Children = new List<AnimatedSequenceDesc>()
                        {
                            new AnimatedSequenceDesc
                            {
                                SequenceIndexLoop = 124,
                                MessageText = "Donald",
                                TextAnchor = TextAnchor.Center,
                            },
                            new AnimatedSequenceDesc()
                            {
                                SequenceIndexLoop = 90,
                                Children = new List<AnimatedSequenceDesc>()
                                {
                                    new AnimatedSequenceDesc
                                    {
                                        SequenceIndexLoop = 124,
                                        MessageId = MsgLv,
                                        TextAnchor = TextAnchor.Left,
                                    },
                                    new AnimatedSequenceDesc
                                    {
                                        SequenceIndexLoop = 124,
                                        MessageText = "99",
                                        TextAnchor = TextAnchor.Right,
                                    },
                                    new AnimatedSequenceDesc()
                                    {
                                        StackIndex = 1,
                                        SequenceIndexLoop = 121,
                                        MessageId = MsgHp,
                                        TextAnchor = TextAnchor.Left,
                                    },
                                    new AnimatedSequenceDesc()
                                    {
                                        StackIndex = 1,
                                        SequenceIndexLoop = 121,
                                        MessageText = "60/60",
                                        TextAnchor = TextAnchor.Right,
                                    },
                                    new AnimatedSequenceDesc()
                                    {
                                        StackIndex = 1,
                                        SequenceIndexLoop = CharacterHpBar,
                                        TextAnchor = TextAnchor.Left,
                                    },
                                    new AnimatedSequenceDesc()
                                    {
                                        StackIndex = 2,
                                        SequenceIndexLoop = 118,
                                        MessageId = MsgMp,
                                        TextAnchor = TextAnchor.Left,
                                    },
                                    new AnimatedSequenceDesc()
                                    {
                                        StackIndex = 2,
                                        SequenceIndexLoop = 118,
                                        MessageText = "120/120",
                                        TextAnchor = TextAnchor.Right,
                                    },
                                    new AnimatedSequenceDesc()
                                    {
                                        StackIndex = 2,
                                        SequenceIndexLoop = CharacterMpBar,
                                        TextAnchor = TextAnchor.Left,
                                    },
                                }
                            },
                        }
                    })
                    .ToList()
            };

            _characterDescSample = _animSeqFactory.Create(root);
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
            ForAll(_menuOptionSeqs, x => x.Begin());
            ForAll(_mainSeqGroup, x => x.Begin());
            _menuOptionSelectedSeq.Begin();
            _characterDescSample.Begin();

            IsMenuOpen = true;
        }

        public void CloseMenu()
        {
            _layoutRenderer.FrameIndex = 0;
            _layoutRenderer.SelectedSequenceGroupIndex = 2;

            _backgroundSeq.End();
            ForAll(_menuOptionSeqs, x => x.End());
            ForAll(_mainSeqGroup, x => x.End());
            _menuOptionSelectedSeq.End();
            _characterDescSample.End();
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
            _characterDescSample.Update(deltaTime);
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

            _characterDescSample.Draw(0, 0);

            _drawing.Flush();
        }

        private void ProcessInput(InputManager inputManager)
        {
            if (inputManager.IsMenuUp)
                MenuOption--;
            if (inputManager.IsMenuDown)
                MenuOption++;
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

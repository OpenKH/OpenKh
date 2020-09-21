using OpenKh.Engine.Extensions;
using OpenKh.Engine.MonoGame;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Game.Debugging;
using OpenKh.Game.Infrastructure;
using OpenKh.Game.Menu;
using OpenKh.Kh2;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game.States
{
    public class MenuState : IMenuManager, IState
    {
        private Kernel _kernel;
        private IDataContent _content;
        private ArchiveManager _archiveManager;
        private KingdomShader _shader;
        private MonoSpriteDrawing _drawing;
        private Layout _campLayout;
        private LayoutRenderer _layoutRenderer;
        private List<ISpriteTexture> _textures = new List<ISpriteTexture>();
        private Dictionary<ushort, byte[]> _cachedText = new Dictionary<ushort, byte[]>();

        private IAnimatedSequence _backgroundSeq;
        private IAnimatedSequence _subMenuDescriptionSeq;
        private List<object> _subMenuDescriptionInfo = new List<object>();

        private Kh2MessageRenderer _messageRenderer;
        private IMenu _subMenu;

        public IGameContext GameContext { get; }
        public AnimatedSequenceFactory SequenceFactory { get; private set; }
        public InputManager InputManager { get; private set; }
        public bool IsMenuOpen { get; private set; }

        public MenuState(IGameContext gameContext)
        {
            GameContext = gameContext;
        }

        public void Initialize(StateInitDesc initDesc)
        {
            _kernel = initDesc.Kernel;
            _content = initDesc.DataContent;
            _archiveManager = initDesc.ArchiveManager;
            InputManager = initDesc.InputManager;

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

            SequenceFactory = new AnimatedSequenceFactory(
                _drawing,
                initDesc.Kernel.MessageProvider,
                _messageRenderer,
                _kernel.SystemMessageContext.Encoder,
                _campLayout.SequenceItems[1],
                _textures.First());

            _backgroundSeq = SequenceFactory.Create(new List<AnimatedSequenceDesc>
            {
                new AnimatedSequenceDesc
                {
                    SequenceIndexStart = 107,
                    SequenceIndexLoop = 108,
                    SequenceIndexEnd = 109,
                },
                new AnimatedSequenceDesc
                {
                    SequenceIndexStart = 110,
                    SequenceIndexLoop = 111,
                    SequenceIndexEnd = 112,
                },
                new AnimatedSequenceDesc
                {
                    SequenceIndexStart = 113,
                    SequenceIndexLoop = 114,
                    SequenceIndexEnd = 115,
                }
            });
            _subMenuDescriptionSeq = SequenceFactory.Create(new List<AnimatedSequenceDesc>());
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
            _subMenu = new MainMenu(this);
            _subMenu.Open();

            _subMenuDescriptionInfo.Clear();
            _subMenuDescriptionSeq = SequenceFactory.Create(new List<AnimatedSequenceDesc>());
            _subMenuDescriptionSeq.Begin();

            IsMenuOpen = true;
        }

        public void CloseAllMenu()
        {
            _layoutRenderer.FrameIndex = 0;
            _layoutRenderer.SelectedSequenceGroupIndex = 2;

            _backgroundSeq.End();
            _subMenuDescriptionSeq.End();
            _subMenu.Close();
        }

        public void PushSubMenuDescription(ushort messageId)
        {
            _subMenuDescriptionInfo.Add(messageId);
            SubMenuDescriptionInvalidateByPush();
        }

        public void PushSubMenuDescription(string message)
        {
            _subMenuDescriptionInfo.Add(message);
            SubMenuDescriptionInvalidateByPush();
        }

        private void SubMenuDescriptionInvalidateByPush()
        {
            var count = _subMenuDescriptionInfo.Count;
            _subMenuDescriptionSeq = SequenceFactory.Create(Enumerable.Range(0, count)
                .Select(i => new AnimatedSequenceDesc
                {
                    SequenceIndexStart = i + 1 < count ? -1 : 75,
                    SequenceIndexLoop = 76,
                    SequenceIndexEnd = 77,
                    StackIndex = i,
                    StackWidth = AnimatedSequenceDesc.DefaultStacking,
                    StackHeight = 0,
                    MessageId = _subMenuDescriptionInfo[i] is ushort id ? id : (ushort)0,
                    MessageText = _subMenuDescriptionInfo[i] as string,
                    Flags = AnimationFlags.TextIgnoreColor |
                        AnimationFlags.TextTranslateX |
                        AnimationFlags.ChildStackHorizontally,
                    TextAnchor = TextAnchor.BottomLeft,
                    Children = new List<AnimatedSequenceDesc>
                    {
                        new AnimatedSequenceDesc { },
                        new AnimatedSequenceDesc
                        {
                            SequenceIndexStart = i + 1 < count ? -1 : 27,
                            SequenceIndexLoop = 28,
                            SequenceIndexEnd = 29,
                        }
                    }
                })
                .ToList());
            _subMenuDescriptionSeq.Begin();
        }

        public void PopSubMenuDescription()
        {
            var count = _subMenuDescriptionInfo.Count;
            if (count == 0)
                return;

            _subMenuDescriptionSeq = SequenceFactory.Create(Enumerable.Range(0, count)
                .Select(i => new AnimatedSequenceDesc
                {
                    SequenceIndexLoop = i + 1 < count ? 76 : -1,
                    SequenceIndexEnd = 77,
                    StackIndex = i,
                    StackWidth = AnimatedSequenceDesc.DefaultStacking,
                    StackHeight = 0,
                    MessageId = _subMenuDescriptionInfo[i] is ushort id ? id : (ushort)0,
                    MessageText = _subMenuDescriptionInfo[i] as string,
                    Flags = AnimationFlags.TextIgnoreColor |
                        AnimationFlags.TextTranslateX |
                        AnimationFlags.ChildStackHorizontally,
                    TextAnchor = TextAnchor.BottomLeft,
                    Children = new List<AnimatedSequenceDesc>
                    {
                        new AnimatedSequenceDesc { },
                        new AnimatedSequenceDesc
                        {
                            SequenceIndexLoop = i + 1 < count ? 28 : -1,
                            SequenceIndexEnd = 29,
                        },
                    }
                })
                .ToList());
            _subMenuDescriptionSeq.Begin();

            _subMenuDescriptionInfo.RemoveAt(_subMenuDescriptionInfo.Count - 1);
        }

        public void SetElementDescription(ushort messageId)
        {
        }

        public void Update(DeltaTimes deltaTimes)
        {
            var deltaTime = deltaTimes.DeltaTime;

            ProcessInput(InputManager);
            
            _layoutRenderer.FrameIndex++;
            _backgroundSeq.Update(deltaTime);
            _subMenuDescriptionSeq.Update(deltaTime);
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
            _backgroundSeq.Draw(0, 0);
            _subMenuDescriptionSeq.Draw(0, 0);
            _subMenu.Draw();

            _drawing.Flush();
        }

        private void ProcessInput(InputManager inputManager)
        {
            if (inputManager.IsStart)
                CloseAllMenu();
        }

        private (Layout layout, List<ISpriteTexture> textures) GetLayoutResources(string layoutResourceName, string imagesResourceName)
        {
            var layout = _archiveManager.Get<Layout>(layoutResourceName);
            _textures = _archiveManager.Get<Imgz>(imagesResourceName)
                    ?.Images?.Select(x => _drawing.CreateSpriteTexture(x)).ToList();

            return (layout, _textures);
        }

        public void DebugDraw(IDebug debug)
        {
        }

        public void DebugUpdate(IDebug debug)
        {
        }
    }
}

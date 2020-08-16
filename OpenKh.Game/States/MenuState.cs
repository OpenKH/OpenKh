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

        private Kernel _kernel;
        private IDataContent _content;
        private ArchiveManager _archiveManager;
        private KingdomShader _shader;
        private MonoSpriteDrawing _drawing;
        private Layout _campLayout;
        private LayoutRenderer _layoutRenderer;
        private List<ISpriteTexture> _textures = new List<ISpriteTexture>();

        private List<AnimatedSequenceRenderer> _mainSeqGroup;
        private List<AnimatedSequenceRenderer> _menuOptionSeqs;
        private AnimatedSequenceRenderer _backgroundSeq;
        private List<AnimatedSequenceRenderer> _characterDescSeqs;

        public void Initialize(StateInitDesc initDesc)
        {
            _kernel = initDesc.Kernel;
            _content = initDesc.DataContent;
            _archiveManager = initDesc.ArchiveManager;

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
            _menuOptionSeqs = Enumerable.Range(0, MenuElementCount)
                .Select(_ => CreateAnimationSequence(133))
                .ToList();
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
            
            _layoutRenderer.FrameIndex++;
            foreach (var animSequence in _mainSeqGroup)
                animSequence.Update(deltaTime);
            foreach (var animSequence in _menuOptionSeqs)
                animSequence.Update(deltaTime);
            _backgroundSeq.Update(deltaTime);
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
                const float PosX = 0;
                const float PosY = 0;
                const float Distance = 96;
                var item = _menuOptionSeqs[i];
                item.Draw(PosX, PosY + i * Distance);
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

        public void DebugDraw(IDebug debug)
        {
        }

        public void DebugUpdate(IDebug debug)
        {
        }
    }
}

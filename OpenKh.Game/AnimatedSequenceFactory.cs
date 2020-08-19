using OpenKh.Engine;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game
{
    enum ChildStacking
    {
        None,
        Left,
        Top,
        Right,
        Bottom
    }

    interface IAnimatedSequence
    {
        bool IsEnd { get; }
        TextAnchor TextAnchor { get; set; }
        public ChildStacking ChildStacking { get; set; }

        void Update(double deltaTime);
        void Draw(float x, float y);

        void Begin();
        void Skip();
        void End();

        void SetMessage(ushort id);
        void SetMessage(string text);
    }

    class AnimatedSequenceDesc
    {
        public float X { get; set; }
        public float Y { get; set; }
        public TextAnchor TextAnchor { get; set; }
        public ChildStacking ChildStacking { get; set; }
        public ushort MessageId { get; set; }
        public string MessageText { get; set; }
        public int SequenceIndexLoop { get; set; }
        public int SequenceIndexStart { get; set; } = -1;
        public int SequenceIndexEnd { get; set; } = -1;
        public int StackIndex { get; set; }
        public List<AnimatedSequenceDesc> Children { get; set; }
    }

    class AnimatedSequenceFactory
    {
        private class AnimatedSequence : IAnimatedSequence
        {
            private readonly SequenceRenderer _renderer;
            private readonly IMessageProvider _messageProvider;
            private readonly IMessageRenderer _messageRenderer;
            private readonly IMessageEncode _messageEncode;
            private int _anim;
            private int _frame;
            private bool _isRunning;
            private byte[] _message;

            public int SequenceIndexLoop { get; }
            public int SequenceIndexStart { get; }
            public int SequenceIndexEnd { get; }
            public List<IAnimatedSequence> Children { get; set; }

            public bool IsEnd { get; private set; }

            public float PositionX { get; set; }
            public float PositionY { get; set; }
            public int StackIndex { get; set; }
            public TextAnchor TextAnchor { get; set; }
            public ChildStacking ChildStacking { get; set; }

            public AnimatedSequence(
                ISpriteDrawing drawing,
                IMessageProvider messageProvider,
                IMessageRenderer messageRenderer,
                IMessageEncode messageEncode,
                Sequence sequence,
                ISpriteTexture texture,
                int loop, int start, int end)
            {
                _renderer = new SequenceRenderer(
                    sequence, drawing, texture);
                _messageEncode = messageEncode;
                _messageProvider = messageProvider;
                _messageRenderer = messageRenderer;
                SequenceIndexLoop = loop;
                SequenceIndexStart = start;
                SequenceIndexEnd = end;
            }
            
            public void Update(double deltaTime)
            {
                _frame++;
                foreach (var child in Children)
                    child.Update(deltaTime);
            }

            public void Draw(float x, float y) =>
                Draw(new SequenceRenderer.ChildContext
                {
                    PositionX = x,
                    PositionY = y
                });

            private void Draw(SequenceRenderer.ChildContext context)
            {
                if (!IsEnd && !_renderer.Draw(
                    _anim,
                    _frame,
                    PositionX + context.PositionX,
                    PositionY + context.PositionY))
                {
                    if (_isRunning)
                    {
                        _anim = SequenceIndexLoop;
                        _frame = 0;
                    }
                    else
                        IsEnd = true;
                }

                var childContext = _renderer.CurrentChildContext;
                var originalPosY = childContext.PositionY;
                for (var i = 0; i < Children.Count; i++)
                {
                    var child = Children[i] as AnimatedSequence;
                    childContext.PositionX += childContext.TextPositionX;
                    childContext.PositionY = originalPosY + childContext.UiPadding * child.StackIndex;
                    child.Draw(childContext);
                }

                if (_message != null)
                {
                    const float UiTextScale = 0.75f;
                    float textScale = context.TextScale == 0 ? UiTextScale : (context.TextScale / 16f);

                    var fakeTextDrawContext = new DrawContext
                    {
                        Scale = textScale,
                        IgnoreDraw = true,
                    };
                    _messageRenderer.Draw(fakeTextDrawContext, _message);
                    var width = (float)fakeTextDrawContext.Width;

                    float xPos;
                    switch (TextAnchor)
                    {
                        default:
                        case TextAnchor.Left:
                            xPos = childContext.PositionX;
                            break;
                        case TextAnchor.Center:
                            xPos = childContext.PositionX - width / 2;
                            break;
                        case TextAnchor.Right:
                            xPos = childContext.PositionX + context.UiSize - width;
                            break;
                    }

                    _messageRenderer.Draw(new DrawContext
                    {
                        xStart = childContext.TextPositionX + xPos,
                        x = xPos,
                        y = childContext.PositionY + childContext.TextPositionY,
                        Color = childContext.Color,
                        Scale = textScale,
                        WidthMultiplier = 1.0f,
                    }, _message);
                }
            }

            public void Begin()
            {
                foreach (var child in Children)
                    child.Begin();
                _anim = SequenceIndexStart >= 0 ?
                    SequenceIndexStart :
                    SequenceIndexLoop;
                _isRunning = true;
                IsEnd = false;
                _frame = 0;
            }

            public void End()
            {
                foreach (var child in Children)
                    child.End();

                _isRunning = false;
                if (SequenceIndexEnd >= 0)
                {
                    _anim = SequenceIndexEnd;
                    IsEnd = false;
                    _frame = 0;
                }
                else
                    IsEnd = true;
            }

            public void Skip()
            {
                foreach (var child in Children)
                    child.Skip();
                if (_isRunning)
                {
                    if (_anim == SequenceIndexStart)
                    {
                        _anim = SequenceIndexLoop;
                        _frame = 0;
                    }
                }
                else
                    IsEnd = true;
            }

            public void SetMessage(ushort id) =>
                SetMessage(_messageProvider.GetString(id));

            public void SetMessage(string text)
            {
                var s = MsgSerializer.DeserializeText(text).ToList();
                _message = _messageEncode.Encode(s);
            }
        }

        private readonly ISpriteDrawing _drawing;
        private readonly IMessageProvider _messageProvider;
        private readonly IMessageRenderer _messageRenderer;
        private readonly IMessageEncode _messageEncode;
        private readonly Sequence _sequence;
        private readonly ISpriteTexture _texture;

        public AnimatedSequenceFactory(
            ISpriteDrawing drawing,
            IMessageProvider messageProvider,
            IMessageRenderer messageRenderer,
            IMessageEncode messageEncode,
            Sequence sequence,
            ISpriteTexture texture)
        {
            _drawing = drawing;
            _messageProvider = messageProvider;
            _messageRenderer = messageRenderer;
            _messageEncode = messageEncode;
            _sequence = sequence;
            _texture = texture;
        }

        public IAnimatedSequence Create(AnimatedSequenceDesc desc)
        {
            var animSeq = new AnimatedSequence(
                _drawing,
                _messageProvider,
                _messageRenderer,
                _messageEncode,
                _sequence,
                _texture,
                desc.SequenceIndexLoop,
                desc.SequenceIndexStart,
                desc.SequenceIndexEnd)
            {
                PositionX = desc.X,
                PositionY = desc.Y,
                StackIndex = desc.StackIndex,
                TextAnchor = desc.TextAnchor,
                ChildStacking = desc.ChildStacking,
                Children = desc.Children?
                    .Select(Create)
                    .Cast<IAnimatedSequence>()
                    .ToList() ?? new List<IAnimatedSequence>()
            };
            if (!string.IsNullOrEmpty(desc.MessageText))
                animSeq.SetMessage(desc.MessageText);
            else if (desc.MessageId > 0)
                animSeq.SetMessage(desc.MessageId);

            return animSeq;
        }
    }

    static class AnimatedSequenceFactoryExtensions
    {
        public static IAnimatedSequence FromStaticIndex(
            this AnimatedSequenceFactory factory, int animationIndex) =>
            factory.Create(new AnimatedSequenceDesc
            {
                SequenceIndexLoop = animationIndex
            });

        public static IAnimatedSequence FromAnimatedIndex(
            this AnimatedSequenceFactory factory, int animationIndex) =>
            factory.Create(new AnimatedSequenceDesc
            {
                SequenceIndexStart = animationIndex,
                SequenceIndexLoop = animationIndex + 1,
                SequenceIndexEnd = animationIndex + 2,
            });

        public static IAnimatedSequence FromAnimatedIndex(
            this AnimatedSequenceFactory factory, int start, int loop, int end) =>
            factory.Create(new AnimatedSequenceDesc
            {
                SequenceIndexStart = start,
                SequenceIndexLoop = loop,
                SequenceIndexEnd = end,
            });
    }
}

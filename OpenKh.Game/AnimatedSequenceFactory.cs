using OpenKh.Engine;
using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using OpenKh.Kh2.Messages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenKh.Game
{
    internal enum AnimationFlagsDefinitions
    {
        TextTranslateX,
        NoChildTranslationX,
        ChildStackHorizontally,
        StackNextChildHorizontally,
        StackNextChildVertically,
        TextIgnoreColor,
        TextIgnoreScaling,
    }

    [Flags]
    public enum AnimationFlags
    {
        None,
        TextTranslateX = 1 << AnimationFlagsDefinitions.TextTranslateX,
        NoChildTranslationX = 1 << AnimationFlagsDefinitions.NoChildTranslationX,
        ChildStackHorizontally = 1 << AnimationFlagsDefinitions.ChildStackHorizontally,
        StackNextChildHorizontally = 1 << AnimationFlagsDefinitions.StackNextChildHorizontally,
        StackNextChildVertically = 1 << AnimationFlagsDefinitions.StackNextChildVertically,
        TextIgnoreColor = 1 << AnimationFlagsDefinitions.TextIgnoreColor,
        TextIgnoreScaling = 1 << AnimationFlagsDefinitions.TextIgnoreScaling,
    }

    public interface IAnimatedSequence
    {
        bool IsEnd { get; }
        TextAnchor TextAnchor { get; set; }
        int FrameIndex { get; set; }

        void Update(double deltaTime);
        void Draw(float x, float y);

        void Begin();
        void Skip();
        void End();

        void SetMessage(ushort id);
        void SetMessage(string text);
    }

    public class AnimatedSequenceDesc
    {
        public const int DefaultStacking = int.MinValue;

        public float X { get; set; }
        public float Y { get; set; }
        public AnimationFlags Flags { get; set; }
        public TextAnchor TextAnchor { get; set; }
        public ushort MessageId { get; set; }
        public string MessageText { get; set; }
        public int SequenceIndexLoop { get; set; } = -1;
        public int SequenceIndexStart { get; set; } = -1;
        public int SequenceIndexEnd { get; set; } = -1;
        public int StackIndex { get; set; }
        public int StackWidth { get; set; }
        public int StackHeight { get; set; }
        public List<AnimatedSequenceDesc> Children { get; set; }
    }

    public class AnimatedSequenceFactory
    {
        private class RootAnimatedSequence : IAnimatedSequence
        {
            private int _frameIndex;

            public List<AnimatedSequence> Children { get; set; }

            public bool IsEnd => Children.All(x => x.IsEnd);
            public TextAnchor TextAnchor { get; set; }
            public int FrameIndex
            {
                get => _frameIndex;
                set
                {
                    _frameIndex = value;
                    foreach (var item in Children)
                        item.FrameIndex = value;
                }
            }

            public void Update(double deltaTime)
            {
                _frameIndex++;
                foreach (var item in Children)
                    item.Update(deltaTime);
            }

            public void Draw(float x, float y)
            {
                foreach (var item in Children)
                {
                    item.Draw(x, y);
                }
            }

            public void Begin()
            {
                foreach (var item in Children)
                    item.Begin();
            }

            public void Skip()
            {
                foreach (var item in Children)
                    item.Skip();
            }

            public void End()
            {
                foreach (var item in Children)
                    item.End();
            }

            public void SetMessage(ushort id)
            {
            }

            public void SetMessage(string text)
            {
            }
        }

        private class AnimatedSequence : IAnimatedSequence
        {
            private static ColorF DefaultColor = new ColorF(1f, 1f, 1f, 1f);
            private readonly Sequence _sequence;
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
            public int StackWidth { get; set; }
            public int StackHeight { get; set; }
            public TextAnchor TextAnchor { get; set; }
            public AnimationFlags Flags { get; set; }
            public int FrameIndex
            {
                get => _frame;
                set
                {
                    _frame = value;
                    foreach (var item in Children)
                        item.FrameIndex = value;
                }
            }

            public Sequence.AnimationGroup AnimGroup =>
                _sequence.AnimationGroups[_anim];

            public float UiSize => AnimGroup.UiPadding;

            public AnimatedSequence(
                ISpriteDrawing drawing,
                IMessageProvider messageProvider,
                IMessageRenderer messageRenderer,
                IMessageEncode messageEncode,
                Sequence sequence,
                ISpriteTexture texture,
                int loop, int start, int end)
            {
                _sequence = sequence;
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
                if (!IsEnd)
                    _frame++;

                foreach (var child in Children)
                    child.Update(deltaTime);
            }

            public void Draw(float x, float y) =>
                Draw(new SequenceRenderer.ChildContext
                {
                    PositionX = x,
                    PositionY = y,
                    Color = DefaultColor
                });

            private void Draw(SequenceRenderer.ChildContext context)
            {
                if (_anim == -1)
                    return;

                var anotherPosX = (StackWidth != AnimatedSequenceDesc.DefaultStacking ?
                    StackWidth : AnimGroup.TextPositionX) * StackIndex;
                var anotherPosY = (StackHeight != AnimatedSequenceDesc.DefaultStacking ?
                    StackHeight : AnimGroup.TextPositionX) * StackIndex;

                if (!_renderer.Draw(
                    _anim,
                    _frame,
                    PositionX + anotherPosX + context.PositionX,
                    PositionY + anotherPosY + context.PositionY,
                    context.Color.A))
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

                if (_message != null)
                {
                    const float UiTextScale = 0.7f;
                    float textScale = context.TextScale == 0 || Flags.HasFlag(AnimationFlags.TextIgnoreScaling) ?
                        UiTextScale : (context.TextScale / 22f);

                    var fakeTextDrawContext = new DrawContext
                    {
                        Scale = textScale,
                        IgnoreDraw = true,
                    };
                    _messageRenderer.Draw(fakeTextDrawContext, _message);
                    var width = (float)fakeTextDrawContext.Width;
                    var height = (float)fakeTextDrawContext.Height;

                    float xPos, yPos;
                    switch (TextAnchor)
                    {
                        default:
                        case TextAnchor.BottomLeft:
                            xPos = 0;
                            yPos = 0;
                            break;
                        case TextAnchor.BottomCenter:
                            xPos = -width / 2;
                            yPos = 0;
                            break;
                        case TextAnchor.BottomRight:
                            xPos = context.UiSize - width;
                            yPos = 0;
                            break;
                        case TextAnchor.Center:
                            xPos = -width / 2;
                            yPos = -height / 2;
                            break;
                        case TextAnchor.TopCenter:
                            xPos = -width / 2;
                            yPos = -height;
                            break;
                        case TextAnchor.TopLeft:
                            xPos = 0;
                            yPos = -height;
                            break;
                    }

                    if (!Flags.HasFlag(AnimationFlags.TextTranslateX))
                        xPos += childContext.TextPositionX;

                    var textColor = childContext.Color;
                    if (Flags.HasFlag(AnimationFlags.TextIgnoreColor))
                        textColor = new ColorF(1f, 1f, 1f, textColor.A);

                    const float ShadowDistance = 1.5f;
                    const float ShadowOpacity = 0.75f;
                    _messageRenderer.Draw(new DrawContext
                    {
                        xStart = childContext.PositionX + xPos + ShadowDistance,
                        x = childContext.PositionX + xPos + ShadowDistance,
                        y = childContext.PositionY + childContext.TextPositionY + yPos + ShadowDistance,
                        Color = new ColorF(0, 0, 0, textColor.A * ShadowOpacity),
                        Scale = textScale,
                        WidthMultiplier = 1.0f,
                    }, _message);

                    _messageRenderer.Draw(new DrawContext
                    {
                        xStart = childContext.PositionX + xPos,
                        x = childContext.PositionX + xPos,
                        y = childContext.PositionY + childContext.TextPositionY + yPos,
                        Color = textColor,
                        Scale = textScale,
                        WidthMultiplier = 1.0f,
                    }, _message);
                }

                childContext.PositionX = Flags.HasFlag(AnimationFlags.NoChildTranslationX) ?
                    context.PositionX : childContext.PositionX;

                var originalPosY = childContext.PositionY;
                for (var i = 0; i < Children.Count; i++)
                {
                    var child = Children[i] as AnimatedSequence;
                    childContext.PositionY = originalPosY + childContext.UiPadding * child.StackIndex;

                    child.Draw(childContext);

                    if (Flags.HasFlag(AnimationFlags.ChildStackHorizontally))
                        childContext.PositionX += childContext.UiSize;

                    if (Flags.HasFlag(AnimationFlags.StackNextChildHorizontally))
                        childContext.PositionX += child.UiSize;
                    if (Flags.HasFlag(AnimationFlags.StackNextChildVertically))
                        originalPosY += child.UiSize;
                }
            }

            public void Begin()
            {
                foreach (var child in Children)
                    child.Begin();

                _anim = SequenceIndexStart >= 0 ?
                    SequenceIndexStart :
                    SequenceIndexLoop;

                _frame = 0;
                if (_anim < 0)
                {
                    _anim = SequenceIndexEnd;
                    _isRunning = false;
                    IsEnd = _anim < 0;
                }
                else
                {
                    _isRunning = true;
                    IsEnd = false;
                }
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

        public IAnimatedSequence Create(IEnumerable<AnimatedSequenceDesc> descs) =>
            new RootAnimatedSequence
            {
                Children = descs.Select(x => Create(x) as AnimatedSequence).ToList()
            };

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
                Flags = desc.Flags,
                StackIndex = desc.StackIndex,
                StackWidth = desc.StackWidth,
                StackHeight = desc.StackHeight,
                TextAnchor = desc.TextAnchor,
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

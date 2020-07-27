using OpenKh.Engine.Renderers;
using OpenKh.Engine.Renders;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

namespace OpenKh.Game.States.Title
{
    internal class AnimatedSequenceRenderer
    {
        private readonly SequenceRenderer _renderer;
        private readonly int _animStart;
        private readonly int _animLoop;
        private readonly int _animEnd;
        private int _anim;
        private int _frame;
        private bool _isRunning;
        private IMessageRenderer _messageRenderer;
        private byte[] _message;
        private List<AnimatedSequenceRenderer> _children = new List<AnimatedSequenceRenderer>();
        private int _attachedChildIndex = -1;

        public bool IsEnd { get; set; }
        public float ChildPositionX { get; set; }

        public AnimatedSequenceRenderer(
            SequenceRenderer renderer, int anim) :
            this(renderer, anim, anim + 1, anim + 2)
        { }

        public AnimatedSequenceRenderer(SequenceRenderer renderer,
            int animStart, int animLoop, int animEnd)
        {
            _renderer = renderer;
            _animStart = animStart;
            _animLoop = animLoop;
            _animEnd = animEnd;
            IsEnd = true;

        }

        public void SetMessage(IMessageRenderer messageRenderer, byte[] message)
        {
            _messageRenderer = messageRenderer;
            _message = message;
        }

        public void Update(double deltaTime)
        {
            _frame++;
            foreach (var child in _children)
                child.Update(deltaTime);
        }

        public void Draw(float x, float y)
        {
            if (!IsEnd && !_renderer.Draw(_messageRenderer, _message, _anim, _frame, x, y))
            {
                if (_isRunning)
                {
                    _anim = _animLoop;
                    _frame = 0;
                }
                else
                    IsEnd = true;
            }

            var curAnimGroup = _renderer.Sequence.AnimationGroups.Skip(_anim).FirstOrDefault();
            for (var i = 0; i < _children.Count; i++)
            {
                var child = _children[i];
                var childX = ChildPositionX + x;
                if (i == _attachedChildIndex)
                    childX += curAnimGroup?.LightPositionX ?? 0;
                child.Draw(childX, y);
            }
        }

        public void Begin()
        {
            foreach (var child in _children)
                child.Begin();
            _anim = _animStart;
            _isRunning = true;
            IsEnd = false;
            _frame = 0;
        }

        public void Skip()
        {
            foreach (var child in _children)
                child.Skip();
            if (_isRunning)
            {
                if (_anim == _animStart)
                {
                    _anim = _animLoop;
                    _frame = 0;
                }
            }
            else
                IsEnd = true;
        }

        public void End()
        {
            foreach (var child in _children)
                child.End();
            _anim = _animEnd;
            _isRunning = false;
            IsEnd = false;
            _frame = 0;
        }

        public void AddChild(AnimatedSequenceRenderer asr, bool attach = false)
        {
            if (attach)
                _attachedChildIndex = _children.Count;
            _children.Add(asr);
        }
    }
}

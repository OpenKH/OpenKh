using OpenKh.Engine.Renderers;

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

        public bool IsEnd { get; set; }

        public AnimatedSequenceRenderer(SequenceRenderer renderer, int anim) :
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

        public void Update(double deltaTime)
        {
            _frame++;
        }

        public void Draw(float x, float y)
        {
            if (IsEnd)
                return;

            if (!_renderer.Draw(_anim, _frame, x, y))
            {
                if (_isRunning)
                {
                    _anim = _animLoop;
                    _frame = 0;
                }
                else
                    IsEnd = true;
            }
        }

        public void Begin()
        {
            _anim = _animStart;
            _isRunning = true;
            IsEnd = false;
            _frame = 0;
        }

        public void Skip()
        {
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
            _anim = _animEnd;
            _isRunning = false;
            IsEnd = false;
            _frame = 0;
        }
    }
}

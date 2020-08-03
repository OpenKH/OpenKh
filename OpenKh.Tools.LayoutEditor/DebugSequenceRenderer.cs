using OpenKh.Engine.Renders;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;

namespace OpenKh.Tools.LayoutEditor
{
    public class DebugSequenceRenderer : IDebugSequenceRenderer
    {
        [Flags]
        private enum State
        {
            Default,
            Hidden,
        }

        private static readonly ColorF DefaultColor = ColorF.White;
        private static readonly ColorF HideColor = new ColorF(0, 0, 0, 0);
        private static readonly ColorF[] StateColors = new ColorF[]
        {
            DefaultColor,
            HideColor,
        };

        private Sequence.AnimationGroup _animationGroup;
        private State[] _animationStates = new State[0];
        
        public int FocusOnAnimation { get; set; } = -1;

        public Sequence.AnimationGroup AnimationGroup
        {
            get => _animationGroup;
            set
            {
                _animationGroup = value;
                _animationStates = new State[_animationGroup.Animations.Count];
                FocusOnAnimation = -1;
            }
        }

        public bool IsAnimationVisible(int index) => !HasFlag(index, State.Hidden);

        public void ShowAnimation(int index, bool show)
        {
            if (show)
                RemoveFlag(index, State.Hidden);
            else
                AddFlag(index, State.Hidden);
        }

        public ColorF GetAnimationBlendColor(int index)
        {
            if (index < 0 || index >= _animationStates.Length)
                return DefaultColor;
            if (FocusOnAnimation < 0)
                return StateColors[(int)_animationStates[index]];

            return FocusOnAnimation == index ? DefaultColor : HideColor;
        }

        private bool HasFlag(int index, State flag)
        {
            if (index < 0 || index >= _animationStates.Length)
                return false;

            return _animationStates[index].HasFlag(flag);
        }

        private void AddFlag(int index, State flag)
        {
            if (index < 0 || index >= _animationStates.Length)
                return;

            _animationStates[index] |= flag;
        }

        private void RemoveFlag(int index, State flag)
        {
            if (index < 0 || index >= _animationStates.Length)
                return;

            _animationStates[index] &= ~flag;
        }
    }
}

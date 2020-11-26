using OpenKh.Common;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;

namespace OpenKh.Engine.Motion
{
    public class Kh2MotionEngine : IMotionEngine
    {
        private readonly List<Bar.Entry> _animEntries;
        private int _animationIndex;
        private Kh2.Motion _motion;

        public Kh2MotionEngine(List<Bar.Entry> entries)
        {
            _animEntries = entries;
            _animationIndex = -1;
        }

        public int AnimationCount => _animEntries.Count;

        public int CurrentAnimationIndex
        {
            get => _animationIndex;
            set
            {
                _animationIndex = value;
                var entry = _animEntries[_animationIndex];
                var subEntries = Bar.Read(entry.Stream.SetPosition(0));
                var animationDataEntry = subEntries.FirstOrDefault(x => x.Type == Bar.EntryType.Motion);
                if (animationDataEntry != null)
                    _motion = Kh2.Motion.Read(animationDataEntry.Stream.SetPosition(0));
                else
                    Console.Error.WriteLine($"MSET animation {CurrentAnimationIndex} ({CurrentAnimationShortName}) does not contain any {Bar.EntryType.Motion}");

                var animationBinaryEntry = subEntries.FirstOrDefault(x => x.Type == Bar.EntryType.MotionTriggers);
                if (animationDataEntry != null)
                {
                    // We do not have any ANB parser
                }
                else
                    Console.Error.WriteLine($"MSET animation {CurrentAnimationIndex} ({CurrentAnimationShortName}) does not contain any {Bar.EntryType.MotionTriggers}");
            }
        }

        public string CurrentAnimationShortName =>
            _animEntries[_animationIndex].Name;

        public Kh2.Motion CurrentMotion => _motion;

        public void ApplyMotion(IModelMotion model, float time)
        {   
            if (CurrentMotion.IsRaw)
                ApplyRawMotion(model, CurrentMotion.Raw, time);
            else
                ApplyInterpolatedMotion(model, CurrentMotion.Interpolated, time);
        }

        private void ApplyRawMotion(IModelMotion model, Kh2.Motion.RawMotion motion, float time)
        {
            var absoluteFrame = (int)Math.Floor(motion.FramePerSecond * time);
            var actualFrame = absoluteFrame % motion.Matrices.Count;
            model.ApplyMotion(motion.Matrices[actualFrame]);
        }

        public static void ApplyInterpolatedMotion(IModelMotion model, Kh2.Motion.InterpolatedMotion motion, float time)
        {
            model.ApplyMotion(model.InitialPose);
        }
    }
}

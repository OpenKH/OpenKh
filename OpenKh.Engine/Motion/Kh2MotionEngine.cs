using OpenKh.Common;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;

namespace OpenKh.Engine.Motion
{
    public class Kh2MotionEngine : IMotionEngine
    {
        private readonly Bar _binarc;
        private int _animationIndex;
        private int _slotIndex;
        private Kh2.Motion _motion;

        public Kh2MotionEngine()
        {
            _binarc = null;
            _animationIndex = -1;
        }

        public Kh2MotionEngine(Bar binarc)
        {
            _binarc = binarc;
            _animationIndex = -1;
        }

        public int AnimationCount => _binarc?.Count ?? 0;

        public int CurrentAnimationIndex
        {
            get => _animationIndex;
            set
            {
                if (_animationIndex == value)
                    return;

                _animationIndex = value;
                if (_binarc == null)
                {
                    Console.Error.WriteLine($"Does not have a MSET.");
                    return;
                }

                _slotIndex = _binarc.Motionset == Bar.MotionsetType.Default ? _animationIndex :
                    MotionSet.GetMotionSetIndex(_binarc, (MotionSet.MotionName)_animationIndex, false, false);
                if (_slotIndex >= 0 && _binarc[_slotIndex].Stream.Length > 0)
                {
                    var subEntries = Bar.Read(_binarc[_slotIndex].Stream.SetPosition(0));
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
                else
                {
                    Console.Error.WriteLine($"MSET animation {_animationIndex} ({CurrentAnimationShortName}) not found. Falling back to T-pose.");
                    _slotIndex = -1;
                    _motion = null;
                }
            }
        }

        public int CurrentSlotIndex => _slotIndex;

        public string CurrentAnimationShortName =>
            _animationIndex >= 0 ? ((MotionSet.MotionName)_animationIndex).ToString() : "dummy";

        public Kh2.Motion CurrentMotion => _motion;

        public void ApplyMotion(IModelMotion model, float time)
        {
            if (CurrentMotion == null)
                model.ApplyMotion(model.InitialPose);
            else if (CurrentMotion.IsRaw)
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
            var absoluteFrame = (float)Math.Floor(60.0f * time);
            var actualFrame = (int)Loop(motion.FrameCount * 2, motion.FrameEnd * 2, absoluteFrame);

            var boneList = model.Bones;
            var matrices = new Matrix4x4[boneList.Count];

            var totalBoneCount = Math.Max(motion.BoneCount, model.Bones.Count) + motion.IKHelpers.Count;
            var sourceTranslations = new Vector3[totalBoneCount];
            var sourceRotations = new Quaternion[totalBoneCount];
            var absTranslationList = new Vector3[totalBoneCount];
            var absRotationList = new Quaternion[totalBoneCount];

            for (var i = 0; i < model.Bones.Count; i++)
            {
                var bone = model.Bones[i];
                sourceRotations[i].X = bone.RotationX;
                sourceRotations[i].Y = bone.RotationY;
                sourceRotations[i].Z = bone.RotationZ;
                sourceTranslations[i].X = bone.TranslationX;
                sourceTranslations[i].Y = bone.TranslationY;
                sourceTranslations[i].Z = bone.TranslationZ;
            }

            foreach (var pose in motion.StaticPose)
            {
                switch (pose.Channel)
                {
                    case 3:
                        sourceRotations[pose.BoneIndex].X = pose.Value;
                        break;
                    case 4:
                        sourceRotations[pose.BoneIndex].Y = pose.Value;
                        break;
                    case 5:
                        sourceRotations[pose.BoneIndex].Z = pose.Value;
                        break;
                    case 6:
                        sourceTranslations[pose.BoneIndex].X = pose.Value;
                        break;
                    case 7:
                        sourceTranslations[pose.BoneIndex].Y = pose.Value;
                        break;
                    case 8:
                        sourceTranslations[pose.BoneIndex].Z = pose.Value;
                        break;
                }
            }

            foreach (var animation in motion.ModelBoneAnimation)
            {
                // Check if it would be better to use a linear or binary search
                if (true || animation.TimelineCount < 4)
                {
                    for (var index = animation.TimelineCount - 1; index >= 0; index--)
                    {

                        if (actualFrame >= motion.Timeline[animation.TimelineStartIndex + index].KeyFrame)
                        {
                            PerformInterpolation(motion.Timeline, animation, sourceTranslations, sourceRotations, actualFrame, index);
                            break;
                        }
                    }
                }
                else
                {
                    // If there are two keyframes, just interpolate between 0 and 1
                    if (animation.TimelineCount < 3)
                    {
                        PerformInterpolation(motion.Timeline, animation, sourceTranslations, sourceRotations, actualFrame, 0);
                        continue;
                    }

                    var left = 0;
                    var right = animation.TimelineCount - 1;
                    while (true)
                    {
                        var mid = (left + right) / 2;
                        var keyFrame = motion.Timeline[animation.TimelineStartIndex + mid].KeyFrame;
                        if (actualFrame >= keyFrame)
                        {
                            if (actualFrame <= keyFrame)
                            {
                                PerformInterpolation(motion.Timeline, animation, sourceTranslations, sourceRotations, actualFrame, mid);
                                break;
                            }

                            left = mid;
                        }
                        else
                            right = mid;

                        if (right - left <= 1)
                        {
                            PerformInterpolation(motion.Timeline, animation, sourceTranslations, sourceRotations, actualFrame, right - 1);
                            break;
                        }
                    }
                }
            }

            for (int x = 0; x < matrices.Length; x++)
            {
                Quaternion absRotation;
                Vector3 absTranslation;
                var oneBone = boneList[x];
                var parent = oneBone.Parent;
                if (parent < 0)
                {
                    absRotation = Quaternion.Identity;
                    absTranslation = Vector3.Zero;
                }
                else
                {
                    absRotation = absRotationList[parent];
                    absTranslation = absTranslationList[parent];
                }

                var localTranslation = Vector3.Transform(sourceTranslations[x], Matrix4x4.CreateFromQuaternion(absRotation));
                absTranslationList[x] = absTranslation + localTranslation;

                var localRotation = Quaternion.Identity;
                localRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitZ, sourceRotations[x].Z);
                localRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitY, sourceRotations[x].Y);
                localRotation *= Quaternion.CreateFromAxisAngle(Vector3.UnitX, sourceRotations[x].X);
                absRotationList[x] = absRotation * localRotation;
            }

            for (int x = 0; x < matrices.Length; x++)
            {
                var absMatrix = Matrix4x4.Identity;
                absMatrix *= Matrix4x4.CreateFromQuaternion(absRotationList[x]);
                absMatrix *= Matrix4x4.CreateTranslation(absTranslationList[x]);
                matrices[x] = absMatrix;
            }

            model.ApplyMotion(matrices);
        }

        private static void PerformInterpolation(
            IList<Kh2.Motion.TimelineTable> timelines,
            Kh2.Motion.BoneAnimationTable animation,
            Vector3[] sourceTranslations,
            Quaternion[] sourceRotations,
            int actualFrame,
            int currentIndex)
        {
            var left = timelines[animation.TimelineStartIndex + currentIndex];
            var right = currentIndex < animation.TimelineCount - 1
                ? timelines[animation.TimelineStartIndex + currentIndex + 1]
                : timelines[animation.TimelineStartIndex];

            var timeDiff = right.KeyFrame - left.KeyFrame;
            var n = (actualFrame - left.KeyFrame) / timeDiff;
            float value;
            switch (left.Interpolation)
            {
                case Kh2.Motion.Interpolation.Nearest:
                    value = left.Value;
                    break;
                case Kh2.Motion.Interpolation.Linear:
                    value = MathEx.Lerp(left.Value, right.Value, n);
                    break;
                case Kh2.Motion.Interpolation.Hermite:
                case Kh2.Motion.Interpolation.Hermite3: // Unknown why (and where) it is used
                case Kh2.Motion.Interpolation.Hermite4: // Unknown why (and where) it is used
                    value = MathEx.CubicHermite(
                        n, left.Value, right.Value,
                        left.TangentEaseIn, left.TangentEaseOut);
                    break;
                default:
                    value = 0;
                    break;
            }

            switch (animation.Channel)
            {
                case 3:
                    sourceRotations[animation.JointIndex].X = value;
                    break;
                case 4:
                    sourceRotations[animation.JointIndex].Y = value;
                    break;
                case 5:
                    sourceRotations[animation.JointIndex].Z = value;
                    break;
                case 6:
                    sourceTranslations[animation.JointIndex].X = value;
                    break;
                case 7:
                    sourceTranslations[animation.JointIndex].Y = value;
                    break;
                case 8:
                    sourceTranslations[animation.JointIndex].Z = value;
                    break;
            }
        }

        public void UseCustomMotion(Kh2.Motion motion) => _motion = motion;

        private static float Loop(float min, float max, float val)
        {
            if (val < max)
                return val;
            if (max <= min)
                return min;

            var mod = (val - min) % (max - min);
            if (mod < 0)
                mod += max - min;
            return min + mod;
        }
    }
}

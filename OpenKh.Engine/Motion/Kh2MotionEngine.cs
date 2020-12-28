using OpenKh.Common;
using OpenKh.Kh2;
using System;
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

        public Kh2MotionEngine(Bar binarc)
        {
            _binarc = binarc;
            _animationIndex = -1;
        }

        public int AnimationCount => _binarc.Count;

        public int CurrentAnimationIndex
        {
            get => _animationIndex;
            set
            {
                _animationIndex = value;
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
            var absoluteFrame = (int)Math.Floor(30.0f * time);
            var actualFrame = absoluteFrame % motion.FrameEnd * 2;

            var boneList = model.Bones;
            var matrices = new Matrix4x4[boneList.Count];

            var totalBoneCount = motion.BoneCount + motion.IKHelpers.Count;
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
                for (var i = animation.TimelineCount - 1; i >= 0; i--)
                {
                    var timeline = motion.Timeline[animation.TimelineStartIndex + i];

                    if (actualFrame >= timeline.KeyFrame)
                    {
                        Kh2.Motion.TimelineTable nextTimeline;
                        if (i < animation.TimelineCount - 1)
                            nextTimeline = motion.Timeline[animation.TimelineStartIndex + i + 1];
                        else
                            nextTimeline = motion.Timeline[animation.TimelineStartIndex];

                        var timeDiff = nextTimeline.KeyFrame - timeline.KeyFrame;
                        var n = (actualFrame - timeline.KeyFrame) / timeDiff;
                        float value;
                        switch (timeline.Interpolation)
                        {
                            case Kh2.Motion.Interpolation.Nearest:
                                value = timeline.Value;
                                break;
                            case Kh2.Motion.Interpolation.Linear:
                                value = Lerp(timeline.Value, nextTimeline.Value, n);
                                break;
                            case Kh2.Motion.Interpolation.Hermite:
                                value = CubicHermite(n, timeline.Value, nextTimeline.Value,
                                    timeline.TangentEaseIn, timeline.TangentEaseOut);
                                break;
                            case Kh2.Motion.Interpolation.Zero:
                                value = 0; // EVIL!!1!
                                break;
                            default:
                                value = timeline.Value;
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
                        break;
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

        private static float Lerp(float firstFloat, float secondFloat, float by)
        {
            return firstFloat * (1 - by) + secondFloat * by;
        }

        private static float CubicHermite(float t, float p0, float p1, float m0, float m1)
        {
            var t2 = t * t;
            var t3 = t2 * t;
            return (2 * t3 - 3 * t2 + 1) * p0 + (t3 - 2 * t2 + t) * m0 + (-2 * t3 + 3 * t2) * p1 + (t3 - t2) * m1;
        }
    }
}

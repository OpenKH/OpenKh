using OpenKh.Engine;
using OpenKh.Game.Field;
using OpenKh.Game.Infrastructure;
using System.Collections.Generic;
using System.Numerics;
using System.Text;
using static OpenKh.Kh2.Ard.Event;

namespace OpenKh.Game.Events
{
    public class EventPlayer
    {
        private const int FramesPerSecond = 30;
        private const float TimeMul = 1.0f / FramesPerSecond;
        private readonly IField _field;
        private readonly IList<IEventEntry> _eventEntries;
        private readonly IList<SetCameraData> _cameras = new List<SetCameraData>();
        private readonly Dictionary<int, string> _actors = new Dictionary<int, string>();

        private double _secondsPrev;
        private double _seconds;
        private int _cameraId;
        private int _eventDuration;

        public bool IsEnd { get; private set; }

        public EventPlayer(IField field, IList<IEventEntry> eventEntries)
        {
            _field = field;
            _eventEntries = eventEntries;
        }

        public void Initialize()
        {
            IsEnd = false;
            foreach (var entry in _eventEntries)
            {
                switch (entry)
                {
                    case SetEndFrame item:
                        _eventDuration = item.EndFrame;
                        break;
                    case ReadAssets assets:
                        foreach (var assetEntry in assets.Set)
                        {
                            switch (assetEntry)
                            {
                                case ReadActor item:
                                    _actors[item.ActorId] = item.Name;
                                    _field.AddActor(item.ActorId, item.ObjectId);
                                    _field.SetActorVisibility(item.ActorId, false);
                                    break;
                            }
                        }

                        // Double for-loop to ensure to load actors first, then
                        // animations to prevent crashes.
                        foreach (var assetEntry in assets.Set)
                        {
                            switch (assetEntry)
                            {
                                case ReadMotion item:
                                    _field.SetActorAnimation(
                                        item.ActorId,
                                        GetAnmPath(item.ActorId, item.Name));
                                    _field.SetActorVisibility(item.ActorId, item.UnknownIndex == 0);
                                    break;
                            }
                        }
                        break;
                    case EventStart item:
                        _field.FadeFromBlack(item.FadeIn * TimeMul);
                        break;
                    case SetCameraData item:
                        _cameras.Add(item);
                        break;
                }
            }

            _cameraId = 0;
        }

        public void Update(double deltaTime)
        {
            _seconds += deltaTime;

            var nFrame = (int)(_seconds * FramesPerSecond);
            var nPrevFrame = (int)(_secondsPrev * FramesPerSecond);
            var cameraFrameTime = _seconds;

            if (nFrame >= _eventDuration)
            {
                IsEnd = true;
                return;
            }

            foreach (var entry in _eventEntries)
            {
                switch (entry)
                {
                    case SeqActorPosition item:
                        if (item.Frame > nPrevFrame && item.Frame <= nFrame)
                        {
                            _field.SetActorPosition(
                                item.ActorId,
                                item.PositionX,
                                item.PositionY,
                                -item.PositionZ,
                                item.RotationY);
                        }
                        break;
                    case SeqPlayAnimation item:
                        if (item.FrameStart > nPrevFrame && item.FrameStart <= nFrame)
                        {
                            _field.SetActorAnimation(
                                item.ActorId, GetAnmPath(item.ActorId, item.Path));
                            _field.SetActorVisibility(item.ActorId, true);
                        }
                        break;
                    case SeqActorLeave item:
                        if (item.Frame > nPrevFrame && item.Frame <= nFrame)
                            _field.SetActorVisibility(item.ActorId, false);
                        break;
                    case SeqCamera item:
                        if (nFrame >= item.FrameStart && nFrame < item.FrameEnd)
                        {
                            _cameraId = item.CameraId;
                            var frameLength = item.FrameEnd - item.FrameStart;
                            cameraFrameTime = (_seconds * FramesPerSecond - item.FrameStart) / 30f;
                        }
                        break;
                    case SeqFade item:
                        if (item.FrameIndex > nPrevFrame && item.FrameIndex <= nFrame)
                        {
                            switch (item.Type)
                            {
                                case SeqFade.FadeType.FromBlack:
                                case SeqFade.FadeType.FromBlackVariant:
                                    _field.FadeFromBlack(item.Duration * TimeMul);
                                    break;
                                case SeqFade.FadeType.FromWhite:
                                case SeqFade.FadeType.FromWhiteVariant:
                                    _field.FadeFromWhite(item.Duration * TimeMul);
                                    break;
                                case SeqFade.FadeType.ToBlack:
                                case SeqFade.FadeType.ToBlackVariant:
                                    _field.FadeToBlack(item.Duration * TimeMul);
                                    break;
                                case SeqFade.FadeType.ToWhite:
                                case SeqFade.FadeType.ToWhiteVariant:
                                    _field.FadeToWhite(item.Duration * TimeMul);
                                    break;
                            }
                        }
                        break;
                    case SeqSubtitle item:
                        if (nFrame >= item.FrameStart && nPrevFrame < item.FrameStart)
                        {
                            if (item.HideFlag == 0)
                                _field.ShowSubtitle(item.Index, (ushort)item.MessageId);
                            else if (item.HideFlag != 0)
                                _field.HideSubtitle(item.Index);
                        }
                        break;
                }
            }

            if (_cameraId < _cameras.Count)
            {
                var curCamera = _cameras[_cameraId];
                _field.SetCamera(
                    new Vector3(
                        (float)GetCameraValue(cameraFrameTime, curCamera.PositionX, null),
                        (float)GetCameraValue(cameraFrameTime, curCamera.PositionY, null),
                        (float)GetCameraValue(cameraFrameTime, curCamera.PositionZ, null)),
                    new Vector3(
                        (float)GetCameraValue(cameraFrameTime, curCamera.LookAtX, null),
                        (float)GetCameraValue(cameraFrameTime, curCamera.LookAtY, null),
                        (float)GetCameraValue(cameraFrameTime, curCamera.LookAtZ, null)),
                    (float)GetCameraValue(cameraFrameTime, curCamera.FieldOfView, null),
                    (float)GetCameraValue(cameraFrameTime, curCamera.Roll, null));
            }

            _secondsPrev = _seconds;
        }

        private string GetAnmPath(int actorId, string name)
        {
            var sb = new StringBuilder();
            var split = name.Split("anm_")[1].Split("/");

            sb.Append(split[0]);
            sb.Append("/");
            sb.Append(_actors[actorId]);
            sb.Append("/");
            sb.Append(split[1]);
            return sb.ToString();
        }

        private static double GetCameraValue(
            double time,
            IList<SetCameraData.CameraKeys> keyFrames,
            SetCameraData.CameraKeys prevKey)
        {
            if (keyFrames.Count == 0)
                return 0.0;

            if (keyFrames.Count == 1)
                return keyFrames[0].Value;

            const int First = 0;
            var Last = keyFrames.Count - 1;

            var m = time + 1.0 / 60.0;
            var currentFrameIndex = (int)(m * 512.0);
            if (currentFrameIndex > keyFrames[First].KeyFrame - 0x10000000)
            {
                if (currentFrameIndex < keyFrames[Last].KeyFrame)
                {
                    // Do a binary search through all the key frames
                    var left = First;
                    var right = Last;
                    if (right <= 1)
                        return InterpolateCamera(right - 1, m, keyFrames, prevKey);

                    while (true)
                    {
                        var mid = (left + right) / 2;
                        if (currentFrameIndex >= keyFrames[mid].KeyFrame)
                        {
                            if (currentFrameIndex <= keyFrames[mid].KeyFrame)
                                return keyFrames[mid].Value;
                            left = mid;
                        }
                        else
                            right = mid;

                        if (right - left <= 1)
                            return InterpolateCamera(right - 1, m, keyFrames, prevKey);
                    }
                }

                double tangent;
                var keyFrameDistance = keyFrames[Last].KeyFrame - keyFrames[Last - 1].KeyFrame;
                if (keyFrames[Last].Interpolation != Kh2.Motion.Interpolation.Linear || keyFrameDistance == 0)
                    tangent = keyFrames[Last].TangentEaseOut;
                else
                    tangent = (keyFrames[Last].Value - keyFrames[Last - 1].Value) / keyFrameDistance;
                return ((currentFrameIndex - (keyFrames[Last].KeyFrame + 0x10000000) + 0x10000000) * tangent) + keyFrames[Last].Value;
            }
            else
            {
                double tangent;
                var keyFrameDistance = keyFrames[First + 1].KeyFrame - keyFrames[First].KeyFrame;
                if (keyFrames[First].Interpolation != Kh2.Motion.Interpolation.Linear || keyFrameDistance == 0)
                    tangent = keyFrames[First].TangentEaseIn;
                else
                    tangent = (keyFrames[First + 1].Value - keyFrames[First].Value) / keyFrameDistance;
                return -(((keyFrames[First].KeyFrame - currentFrameIndex - 0x10000000) * tangent) - keyFrames[First].Value);
            }
        }

        private static double InterpolateCamera(
            int keyFrameIndex,
            double time,
            IList<SetCameraData.CameraKeys> keyFrames,
            SetCameraData.CameraKeys prevKey)
        {
            const double N = 1.0 / 512.0;
            var curKeyFrame = keyFrames[keyFrameIndex];
            var nextKeyFrame = keyFrames[keyFrameIndex + 1];
            if (prevKey != null)
                prevKey.TangentEaseOut = curKeyFrame.TangentEaseOut;

            var t = time - curKeyFrame.KeyFrame * N;
            var tx = (nextKeyFrame.KeyFrame - curKeyFrame.KeyFrame) * N;
            switch (curKeyFrame.Interpolation)
            {
                case Kh2.Motion.Interpolation.Nearest:
                    return curKeyFrame.Value;
                case Kh2.Motion.Interpolation.Linear:
                    return curKeyFrame.Value + ((nextKeyFrame.Value - curKeyFrame.Value) * t / tx);
                case Kh2.Motion.Interpolation.Hermite:
                case (Kh2.Motion.Interpolation)3:
                case (Kh2.Motion.Interpolation)4:
                    var itx = 1.0 / tx;
                    // Perform a cubic hermite interpolation
                    var p0 = curKeyFrame.Value;
                    var p1 = nextKeyFrame.Value;
                    var m0 = curKeyFrame.TangentEaseOut;
                    var m1 = nextKeyFrame.TangentEaseIn;
                    var t2 = t * t * itx;
                    var t3 = t * t * t * itx * itx;
                    return p0 * (2 * t3 * itx - 3 * t2 * itx + 1) +
                        m0 * (t3 - 2 * t2 + t) +
                        p1 * (-2 * t3 * itx + 3 * t2 * itx) +
                        m1 * (t3 - t2);
                default:
                    return 0;
            }
        }
    }
}

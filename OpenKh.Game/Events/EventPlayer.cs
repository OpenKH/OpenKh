using OpenKh.Engine;
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
        private readonly IList<SeqCamera> _cameraTimeline = new List<SeqCamera>();
        private readonly IList<SeqPlayAnimation> _runAnimations = new List<SeqPlayAnimation>();
        private readonly IList<SeqFade> _fades = new List<SeqFade>();
        private readonly IList<SeqSubtitle> _subtitles = new List<SeqSubtitle>();
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
                    case SeqCamera item:
                        _cameraTimeline.Add(item);
                        break;
                    case SeqActorPosition item:
                        _field.SetActorPosition(
                            item.ActorId,
                            item.PositionX,
                            item.PositionY,
                            -item.PositionZ,
                            item.RotationY - 45f);
                        break;
                    case SeqPlayAnimation item:
                        _runAnimations.Add(item);
                        break;
                    case SeqFade item:
                        _fades.Add(item);
                        break;
                    case SeqSubtitle item:
                        _subtitles.Add(item);
                        break;
                }
            }

            _cameraId = 0;
        }

        public void Update(double deltaTime)
        {
            var visibleEntries = new Dictionary<int, bool>();
            _seconds += deltaTime;

            var nSeconds = (int)(_seconds * FramesPerSecond);
            var nPrevSeconds = (int)(_secondsPrev * FramesPerSecond);

            if (nSeconds >= _eventDuration)
            {
                IsEnd = true;
                return;
            }

            var cameraInterpolationM = 0f;
            foreach (var item in _runAnimations)
            {
                if (item.FrameStart > nPrevSeconds && item.FrameStart <= nSeconds)
                    _field.SetActorAnimation(
                        item.ActorId, GetAnmPath(item.ActorId, item.Path));

                if (nSeconds >= item.FrameStart && nSeconds < item.FrameEnd)
                    visibleEntries[item.ActorId] = true;
                else if (!visibleEntries.ContainsKey(item.ActorId))
                    visibleEntries[item.ActorId] = false;
            }
            foreach (var item in _cameraTimeline)
            {
                if (nSeconds > item.FrameStart && nSeconds < item.FrameEnd)
                {
                    _cameraId = item.CameraId;
                    var frameLength = item.FrameEnd - item.FrameStart;
                    cameraInterpolationM = (float)((_seconds * FramesPerSecond - item.FrameStart) / frameLength);
                }
            }
            foreach (var item in _fades)
            {
                if (item.FrameIndex > nPrevSeconds && item.FrameIndex <= nSeconds)
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
            }
            foreach (var item in _subtitles)
            {
                if (nSeconds >= item.FrameStart && nPrevSeconds < item.FrameStart)
                {
                    if (item.HideFlag == 0)
                        _field.ShowSubtitle(item.Index, (ushort)item.MessageId);
                    else if (item.HideFlag != 0)
                        _field.HideSubtitle(item.Index);
                }
            }

            if (_cameraId < _cameras.Count)
            {
                var curCamera = _cameras[_cameraId];
                _field.SetCamera(
                    new Vector3(
                        GetCameraValue(curCamera.PositionX, cameraInterpolationM),
                        GetCameraValue(curCamera.PositionY, cameraInterpolationM),
                        GetCameraValue(curCamera.PositionZ, cameraInterpolationM)),
                    new Vector3(
                        GetCameraValue(curCamera.LookAtX, cameraInterpolationM),
                        GetCameraValue(curCamera.LookAtY, cameraInterpolationM),
                        GetCameraValue(curCamera.LookAtZ, cameraInterpolationM)),
                    GetCameraValue(curCamera.FieldOfView, cameraInterpolationM),
                    GetCameraValue(curCamera.Roll, cameraInterpolationM));
            }

            foreach (var item in visibleEntries)
                _field.SetActorVisibility(item.Key, item.Value);

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

        private static float GetCameraValue(IList<SetCameraData.CameraValue> values, float m)
        {
            if (values.Count == 0)
                return 0f;
            if (values.Count == 1)
                return values[0].Value;

            var valSrc = values[0];
            var valDst = values[1];
            return MathEx.Lerp(valSrc.Value, valDst.Value, m);
        }
    }
}

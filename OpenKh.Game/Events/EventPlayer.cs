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
        private readonly IList<EntryCamera> _cameras = new List<EntryCamera>();
        private readonly IList<EntryCameraTimeline> _cameraTimeline = new List<EntryCameraTimeline>();
        private readonly IList<EntryRunAnimation> _runAnimations = new List<EntryRunAnimation>();
        private readonly IList<EntryFade> _fades = new List<EntryFade>();
        private readonly IList<EntrySubtitle> _subtitles = new List<EntrySubtitle>();
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
                    case EntryUnk08 item:
                        _eventDuration = item.FrameDuration;
                        break;
                    case EntryLoadAssets assets:
                        foreach (var assetEntry in assets.Loads)
                        {
                            switch (assetEntry)
                            {
                                case LoadObject item:
                                    _actors[item.ActorId] = item.Name;
                                    _field.AddActor(item.ActorId, item.ObjectId);
                                    break;
                            }
                        }

                        // Double for-loop to ensure to load actors first, then
                        // animations to prevent crashes.
                        foreach (var assetEntry in assets.Loads)
                        {
                            switch (assetEntry)
                            {
                                case LoadAnimation item:
                                    _field.SetActorAnimation(
                                        item.ActorId,
                                        GetAnmPath(item.ActorId, item.Name));
                                    break;
                            }
                        }
                        break;
                    case EntryFadeIn item:
                        _field.FadeFromBlack(item.FadeIn * TimeMul);
                        break;
                    case EntryCamera item:
                        _cameras.Add(item);
                        break;
                    case EntryCameraTimeline item:
                        _cameraTimeline.Add(item);
                        break;
                    case EntryActorPosition item:
                        _field.SetActorPosition(
                            item.ActorId,
                            item.PositionX,
                            item.PositionY,
                            -item.PositionZ,
                            item.Rotation - 45f);
                        break;
                    case EntryRunAnimation item:
                        _runAnimations.Add(item);
                        break;
                    case EntryFade item:
                        _fades.Add(item);
                        break;
                    case EntrySubtitle item:
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
                        case EntryFade.FadeType.FromBlack:
                        case EntryFade.FadeType.FromBlackVariant:
                            _field.FadeFromBlack(item.Duration * TimeMul);
                            break;
                        case EntryFade.FadeType.FromWhite:
                        case EntryFade.FadeType.FromWhiteVariant:
                            _field.FadeFromWhite(item.Duration * TimeMul);
                            break;
                        case EntryFade.FadeType.ToBlack:
                        case EntryFade.FadeType.ToBlackVariant:
                            _field.FadeToBlack(item.Duration * TimeMul);
                            break;
                        case EntryFade.FadeType.ToWhite:
                        case EntryFade.FadeType.ToWhiteVariant:
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

        private static float GetCameraValue(IList<EntryCamera.CameraValue> values, float m)
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

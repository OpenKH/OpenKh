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
        private readonly Dictionary<int, string> _actors = new Dictionary<int, string>();

        private double _secondsPrev;
        private double _seconds;
        private int _cameraId;

        public EventPlayer(IField field, IList<IEventEntry> eventEntries)
        {
            _field = field;
            _eventEntries = eventEntries;
        }

        public void Initialize()
        {
            foreach (var entry in _eventEntries)
            {
                switch (entry)
                {
                    case EntryLoadAssets assets:
                        foreach (var assetEntry in assets.Loads)
                        {
                            switch (assetEntry)
                            {
                                case LoadObject item:
                                    _actors[item.ActorId] = item.Name;
                                    _field.AddActor(item.ActorId, item.ObjectId);
                                    break;
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
                            item.PositionZ,
                            item.Rotation);
                        break;
                    case EntryRunAnimation item:
                        _runAnimations.Add(item);
                        break;
                    case EntryFade item:
                        _fades.Add(item);
                        break;
                }
            }

            _cameraId = 0;
        }

        public void Update(double deltaTime)
        {
            _seconds += deltaTime;

            var nSeconds = (int)(_seconds * FramesPerSecond);
            var nPrevSeconds = (int)(_secondsPrev * FramesPerSecond);
            foreach (var item in _runAnimations)
            {
                if (item.FrameStart > nPrevSeconds && item.FrameStart <= nSeconds)
                    _field.SetActorAnimation(
                        item.ActorId, GetAnmPath(item.ActorId, item.Path));
            }
            foreach (var item in _cameraTimeline)
            {
                if (item.FrameStart > nPrevSeconds && item.FrameEnd <= nSeconds)
                    _cameraId = item.CameraId;
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

            var curCamera = _cameras[_cameraId];
            _field.SetCamera(
                new Vector3(
                    curCamera.PositionX[0].Value,
                    -curCamera.PositionZ[0].Value,
                    curCamera.PositionY[0].Value),
                new Vector3(
                    curCamera.Channel3[0].Value,
                    curCamera.Channel4[0].Value,
                    curCamera.Channel5[0].Value),
                curCamera.Channel6[0].Value,
                curCamera.Channel7[0].Value);

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
    }
}

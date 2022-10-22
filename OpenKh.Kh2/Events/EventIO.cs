using OpenKh.Common;
using OpenKh.Kh2.Events.EventModels;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.Events
{
    public class EventIO
    {
        private static readonly Encoding _enc = Encoding.GetEncoding("latin1");

        public static IEnumerable<IEventObject> Read(Stream stream)
        {
            while (true)
            {
                var chunkSize = stream.ReadUInt16();
                if (chunkSize == 0)
                {
                    break;
                }

                var type = stream.ReadUInt16();

                yield return ReadOne(stream, stream.Position + chunkSize - 4, type);
            }
        }

        public static IEventObject ReadOne(EventRoot source)
        {
            switch (source.type)
            {
                case "Head":
                    return ReadEventObject<Head>(source.with);
                case "Chara":
                    return ReadEventObject<Chara>(source.with);
                case "Bg":
                    return ReadEventObject<Bg>(source.with);
                case "CameraData":
                    return ReadEventObject<CameraData>(source.with);
                case "CameraSeq":
                    return ReadEventObject<CameraSeq>(source.with);
                case "EffectData":
                    return ReadEventObject<EffectData>(source.with);
                case "EventEndFrame":
                    return ReadEventObject<EventEndFrame>(source.with);
                case "EffectSeq":
                    return ReadEventObject<EffectSeq>(source.with);
                case "AttachSeq":
                    return ReadEventObject<AttachSeq>(source.with);
                case "KageSeq":
                    return ReadEventObject<KageSeq>(source.with);
                case "BgcolSeq":
                    return ReadEventObject<BgcolSeq>(source.with);
                case "PartSeq":
                    return ReadEventObject<PartSeq>(source.with);
                case "AlphaSeq":
                    return ReadEventObject<AlphaSeq>(source.with);
                case "SystemStart":
                    return ReadEventObject<SystemStart>(source.with);
                case "EventJump":
                    return ReadEventObject<EventJump>(source.with);
                case "SeqFade":
                    return ReadEventObject<SeqFade>(source.with);
                case "CameraDataEnc":
                    return ReadEventObject<CameraDataEnc>(source.with);
                case "SeqMes":
                    return ReadEventObject<SeqMes>(source.with);
                case "BgGrupe":
                    return ReadEventObject<BgGrupe>(source.with);
                case "SeqBlur":
                    return ReadEventObject<SeqBlur>(source.with);
                case "SeqFocus":
                    return ReadEventObject<SeqFocus>(source.with);
                case "SeqTexanim":
                    return ReadEventObject<SeqTexanim>(source.with);
                case "SeqMem":
                    return ReadEventObject<SeqMem>(source.with);
                case "SeqCrossfade":
                    return ReadEventObject<SeqCrossfade>(source.with);
                case "IkSeq":
                    return ReadEventObject<IkSeq>(source.with);
                case "SplineDataEnc":
                    return ReadEventObject<SplineDataEnc>(source.with);
                case "SplinePoint":
                    return ReadEventObject<SplinePoint>(source.with);
                case "SplineSeq":
                    return ReadEventObject<SplineSeq>(source.with);
                case "SeqSystemGameSpeed":
                    return ReadEventObject<SeqSystemGameSpeed>(source.with);
                case "TexFade":
                    return ReadEventObject<TexFade>(source.with);
                case "WideMask":
                    return ReadEventObject<WideMask>(source.with);
                case "Audio":
                    return ReadEventObject<Audio>(source.with);
                case "ReadCtrlMotionTbl":
                    return ReadEventObject<ReadCtrlMotionTbl>(source.with);
                case "ReadCtrlAudioTbl":
                    return ReadEventObject<ReadCtrlAudioTbl>(source.with);
                case "Shake":
                    return ReadEventObject<Shake>(source.with);
                case "Scale":
                    return ReadEventObject<Scale>(source.with);
                case "Turn":
                    return ReadEventObject<Turn>(source.with);
                case "SeData":
                    return ReadEventObject<SeData>(source.with);
                case "SeSeq":
                    return ReadEventObject<SeSeq>(source.with);
                case "SeqBlendMotion":
                    return ReadEventObject<SeqBlendMotion>(source.with);
                case "SeqWaitMessage":
                    return ReadEventObject<SeqWaitMessage>(source.with);
                case "SeqBgm":
                    return ReadEventObject<SeqBgm>(source.with);
                case "BgmData":
                    return ReadEventObject<BgmData>(source.with);
                case "SendBgm":
                    return ReadEventObject<SendBgm>(source.with);
                case "SeqObjcamera":
                    return ReadEventObject<SeqObjcamera>(source.with);
                case "MusicalHeader":
                    return ReadEventObject<MusicalHeader>(source.with);
                case "MusicalTarget":
                    return ReadEventObject<MusicalTarget>(source.with);
                case "MusicalScene":
                    return ReadEventObject<MusicalScene>(source.with);
                case "VibData":
                    return ReadEventObject<VibData>(source.with);
                case "Lookat":
                    return ReadEventObject<Lookat>(source.with);
                case "ShadowAlpha":
                    return ReadEventObject<ShadowAlpha>(source.with);
                case "ReadCtrlCharaTbl":
                    return ReadEventObject<ReadCtrlCharaTbl>(source.with);
                case "ReadCtrlEffectTbl":
                    return ReadEventObject<ReadCtrlEffectTbl>(source.with);
                case "Mirror":
                    return ReadEventObject<Mirror>(source.with);
                case "SeqTreasure":
                    return ReadEventObject<SeqTreasure>(source.with);
                case "SeqMissionEffect":
                    return ReadEventObject<SeqMissionEffect>(source.with);
                case "SeqLayout":
                    return ReadEventObject<SeqLayout>(source.with);
                case "EffectDelete":
                    return ReadEventObject<EffectDelete>(source.with);
                case "CacheClear":
                    return ReadEventObject<CacheClear>(source.with);
                case "SeqObjPause":
                    return ReadEventObject<SeqObjPause>(source.with);
                case "SeqBgse":
                    return ReadEventObject<SeqBgse>(source.with);
                case "SeqGlow":
                    return ReadEventObject<SeqGlow>(source.with);
                case "SeqMovie":
                    return ReadEventObject<SeqMovie>(source.with);
                case "SeqSavePoint":
                    return ReadEventObject<SeqSavePoint>(source.with);
                case "SeqCameraCollision":
                    return ReadEventObject<SeqCameraCollision>(source.with);
                case "SeqPosMove":
                    return ReadEventObject<SeqPosMove>(source.with);
                case "BlackFog":
                    return ReadEventObject<BlackFog>(source.with);
                case "Fog":
                    return ReadEventObject<Fog>(source.with);
                case "PlayerOffsetCamera":
                    return ReadEventObject<PlayerOffsetCamera>(source.with);
                case "SkyOff":
                    return ReadEventObject<SkyOff>(source.with);
                case "HideFobj":
                    return ReadEventObject<HideFobj>(source.with);
                case "Light":
                    return ReadEventObject<Light>(source.with);
                case "SeqMob":
                    return ReadEventObject<SeqMob>(source.with);
                case "Countdown":
                    return ReadEventObject<Countdown>(source.with);
                case "Tag":
                    return ReadEventObject<Tag>(source.with);
                case "WallClip":
                    return ReadEventObject<WallClip>(source.with);
                case "VoiceAllFadeout":
                    return ReadEventObject<VoiceAllFadeout>(source.with);
            }
            return null;
        }

        private static IEventObject ReadEventObject<T>(object with) where T : IEventObject, new()
        {
            if (with is IDictionary<object, object> dict)
            {
                var instance = new T();
                var type = typeof(T);
                foreach (var entry in dict)
                {
                    type.GetProperty(entry.Key + "")?.SetValue(instance, entry.Value);
                }
                return instance;
            }
            throw new NotSupportedException($"Cannot read {with}");
        }

        private static IEventObject ReadOne(Stream stream, long nextPosition, ushort type)
        {
            if (type == Head.Type)
            {
                var it = BinaryMapping.ReadObject<Head>(stream);
                var rest = Convert.ToInt32(nextPosition - stream.Position);
                it.name = stream.ReadString(rest, _enc);
                return it;
            }

            if (type == Chara.Type)
            {
                var it = BinaryMapping.ReadObject<Chara>(stream);
                var rest = Convert.ToInt32(nextPosition - stream.Position);
                it.name = stream.ReadString(rest, _enc);
                return it;
            }

            if (type == SeqPosi.Type)
            {
                var it = BinaryMapping.ReadObject<SeqPosi>(stream);
                return it;
            }


            if (type == Bg.Type)
            {
                var it = BinaryMapping.ReadObject<Bg>(stream);
                var rest = Convert.ToInt32(nextPosition - stream.Position);
                it.world = stream.ReadString(rest, _enc);
                return it;
            }

            if (type == CameraData.Type)
            {
                var it = BinaryMapping.ReadObject<CameraData>(stream);
                return it;
            }

            if (type == CameraSeq.Type)
            {
                var it = BinaryMapping.ReadObject<CameraSeq>(stream);
                return it;
            }

            if (type == EffectData.Type)
            {
                var it = BinaryMapping.ReadObject<EffectData>(stream);
                var rest = Convert.ToInt32(nextPosition - stream.Position);
                it.name = stream.ReadString(rest, _enc);
                return it;
            }

            if (type == EventEndFrame.Type)
            {
                var it = BinaryMapping.ReadObject<EventEndFrame>(stream);
                return it;
            }

            if (type == EffectSeq.Type)
            {
                var it = BinaryMapping.ReadObject<EffectSeq>(stream);
                return it;
            }

            if (type == AttachSeq.Type)
            {
                var it = BinaryMapping.ReadObject<AttachSeq>(stream);
                return it;
            }

            if (type == KageSeq.Type)
            {
                var it = BinaryMapping.ReadObject<KageSeq>(stream);
                return it;
            }

            if (type == BgcolSeq.Type)
            {
                var it = BinaryMapping.ReadObject<BgcolSeq>(stream);
                return it;
            }

            if (type == PartSeq.Type)
            {
                var it = BinaryMapping.ReadObject<PartSeq>(stream);
                return it;
            }

            if (type == AlphaSeq.Type)
            {
                var it = BinaryMapping.ReadObject<AlphaSeq>(stream);
                return it;
            }

            if (type == SystemPrestart.Type)
            {
                var it = BinaryMapping.ReadObject<SystemPrestart>(stream);
                return it;
            }

            if (type == SystemStart.Type)
            {
                var it = BinaryMapping.ReadObject<SystemStart>(stream);
                return it;
            }

            if (type == EventJump.Type)
            {
                var it = BinaryMapping.ReadObject<EventJump>(stream);
                var rest = Convert.ToInt32(nextPosition - stream.Position);
                it.world = stream.ReadString(rest, _enc);
                return it;
            }

            if (type == SeqFade.Type)
            {
                var it = BinaryMapping.ReadObject<SeqFade>(stream);
                return it;
            }

            if (type == CameraDataEnc.Type)
            {
                var it = BinaryMapping.ReadObject<CameraDataEnc>(stream);
                it.works = Enumerable.Range(0, it.cmetx_cnt + it.cmety_cnt + it.cmetz_cnt + it.cmietx_cnt + it.cmiety_cnt + it.cmietz_cnt + it.cmroll_cnt + it.cmfov_cnt)
                    .Select(_ => BinaryMapping.ReadObject<CameraDataEncWk>(stream))
                    .ToArray();
                return it;
            }

            if (type == SeqMes.Type)
            {
                var it = BinaryMapping.ReadObject<SeqMes>(stream);
                return it;
            }

            if (type == BgGrupe.Type)
            {
                var it = BinaryMapping.ReadObject<BgGrupe>(stream);
                return it;
            }

            if (type == SeqBlur.Type)
            {
                var it = BinaryMapping.ReadObject<SeqBlur>(stream);
                return it;
            }

            if (type == SeqFocus.Type)
            {
                var it = BinaryMapping.ReadObject<SeqFocus>(stream);
                return it;
            }

            if (type == SeqTexanim.Type)
            {
                var it = BinaryMapping.ReadObject<SeqTexanim>(stream);
                return it;
            }

            if (type == SeqMem.Type)
            {
                var it = BinaryMapping.ReadObject<SeqMem>(stream);
                return it;
            }

            if (type == SeqCrossfade.Type)
            {
                var it = BinaryMapping.ReadObject<SeqCrossfade>(stream);
                return it;
            }

            if (type == IkSeq.Type)
            {
                var it = BinaryMapping.ReadObject<IkSeq>(stream);
                return it;
            }

            if (type == SplineDataEnc.Type)
            {
                var it = BinaryMapping.ReadObject<SplineDataEnc>(stream);
                it.works = Enumerable.Range(0, it.trans_cnt)
                    .Select(_ => BinaryMapping.ReadObject<CameraDataEncWk>(stream))
                    .ToArray();
                return it;
            }

            if (type == SplinePoint.Type)
            {
                var it = BinaryMapping.ReadObject<SplinePoint>(stream);
                it.points = Enumerable.Range(0, it.cnt)
                    .Select(_ => BinaryMapping.ReadObject<SplinePointData>(stream))
                    .ToArray();
                return it;
            }

            if (type == SplineSeq.Type)
            {
                var it = BinaryMapping.ReadObject<SplineSeq>(stream);
                return it;
            }

            if (type == SeqSystemGameSpeed.Type)
            {
                var it = BinaryMapping.ReadObject<SeqSystemGameSpeed>(stream);
                return it;
            }

            if (type == TexFade.Type)
            {
                var it = BinaryMapping.ReadObject<TexFade>(stream);
                return it;
            }

            if (type == WideMask.Type)
            {
                var it = BinaryMapping.ReadObject<WideMask>(stream);
                return it;
            }

            if (type == Audio.Type)
            {
                var it = BinaryMapping.ReadObject<Audio>(stream);
                return it;
            }

            if (type == ReadCtrl.Type)
            {
                var it = BinaryMapping.ReadObject<ReadCtrl>(stream);
                it.ctrls_object = Enumerable.Range(0, it.cnt)
                    .Select(
                        index =>
                        {
                            var subType = stream.ReadUInt16();
                            var subSize = stream.ReadUInt16();
                            var subNext = stream.Position + subSize - 4;
                            return ReadOne(stream, subNext, subType);
                        }
                    )
                    .ToArray();
                return it;
            }

            if (type == ReadCtrlMotionTbl.Type)
            {
                var it = BinaryMapping.ReadObject<ReadCtrlMotionTbl>(stream);
                var rest = Convert.ToInt32(nextPosition - stream.Position);
                it.motion_name = stream.ReadString(rest, _enc);
                return it;
            }

            if (type == ReadCtrlAudioTbl.Type)
            {
                var it = BinaryMapping.ReadObject<ReadCtrlAudioTbl>(stream);
                var rest = Convert.ToInt32(nextPosition - stream.Position);
                it.voicenumber = stream.ReadString(rest, _enc);
                return it;
            }

            if (type == Shake.Type)
            {
                var it = BinaryMapping.ReadObject<Shake>(stream);
                return it;
            }

            if (type == Scale.Type)
            {
                var it = BinaryMapping.ReadObject<Scale>(stream);
                return it;
            }

            if (type == Turn.Type)
            {
                var it = BinaryMapping.ReadObject<Turn>(stream);
                return it;
            }

            if (type == SeData.Type)
            {
                var it = BinaryMapping.ReadObject<SeData>(stream);
                return it;
            }

            if (type == SeSeq.Type)
            {
                var it = BinaryMapping.ReadObject<SeSeq>(stream);
                return it;
            }

            if (type == SeqBlendMotion.Type)
            {
                var it = BinaryMapping.ReadObject<SeqBlendMotion>(stream);
                var rest = Convert.ToInt32(nextPosition - stream.Position);
                it.name = stream.ReadString(rest, _enc);
                return it;
            }

            if (type == SeqWaitMessage.Type)
            {
                var it = BinaryMapping.ReadObject<SeqWaitMessage>(stream);
                return it;
            }

            if (type == SeqBgm.Type)
            {
                var it = BinaryMapping.ReadObject<SeqBgm>(stream);
                return it;
            }

            if (type == BgmData.Type)
            {
                var it = BinaryMapping.ReadObject<BgmData>(stream);
                return it;
            }

            if (type == SendBgm.Type)
            {
                var it = BinaryMapping.ReadObject<SendBgm>(stream);
                return it;
            }

            if (type == SeqObjcamera.Type)
            {
                var it = BinaryMapping.ReadObject<SeqObjcamera>(stream);
                return it;
            }

            if (type == MusicalHeader.Type)
            {
                var it = BinaryMapping.ReadObject<MusicalHeader>(stream);
                return it;
            }

            if (type == MusicalTarget.Type)
            {
                var it = BinaryMapping.ReadObject<MusicalTarget>(stream);
                return it;
            }

            if (type == MusicalScene.Type)
            {
                var it = BinaryMapping.ReadObject<MusicalScene>(stream);
                return it;
            }

            if (type == VibData.Type)
            {
                var it = BinaryMapping.ReadObject<VibData>(stream);
                return it;
            }

            if (type == Lookat.Type)
            {
                var it = BinaryMapping.ReadObject<Lookat>(stream);
                return it;
            }

            if (type == ShadowAlpha.Type)
            {
                var it = BinaryMapping.ReadObject<ShadowAlpha>(stream);
                return it;
            }

            if (type == ReadCtrlCharaTbl.Type)
            {
                var it = BinaryMapping.ReadObject<ReadCtrlCharaTbl>(stream);
                var rest = Convert.ToInt32(nextPosition - stream.Position);
                it.name = stream.ReadString(rest, _enc);
                return it;
            }

            if (type == ReadCtrlEffectTbl.Type)
            {
                var it = BinaryMapping.ReadObject<ReadCtrlEffectTbl>(stream);
                var rest = Convert.ToInt32(nextPosition - stream.Position);
                it.name = stream.ReadString(rest, _enc);
                return it;
            }

            if (type == Mirror.Type)
            {
                var it = BinaryMapping.ReadObject<Mirror>(stream);
                return it;
            }

            if (type == SeqTreasure.Type)
            {
                var it = BinaryMapping.ReadObject<SeqTreasure>(stream);
                return it;
            }

            if (type == SeqMissionEffect.Type)
            {
                var it = BinaryMapping.ReadObject<SeqMissionEffect>(stream);
                return it;
            }

            if (type == SeqLayout.Type)
            {
                var it = BinaryMapping.ReadObject<SeqLayout>(stream);
                var rest = Convert.ToInt32(nextPosition - stream.Position);
                it.name = stream.ReadString(rest, _enc);
                return it;
            }

            if (type == EffectDelete.Type)
            {
                var it = BinaryMapping.ReadObject<EffectDelete>(stream);
                return it;
            }

            if (type == CacheClear.Type)
            {
                var it = BinaryMapping.ReadObject<CacheClear>(stream);
                return it;
            }

            if (type == SeqObjPause.Type)
            {
                var it = BinaryMapping.ReadObject<SeqObjPause>(stream);
                return it;
            }

            if (type == SeqBgse.Type)
            {
                var it = BinaryMapping.ReadObject<SeqBgse>(stream);
                return it;
            }

            if (type == SeqGlow.Type)
            {
                var it = BinaryMapping.ReadObject<SeqGlow>(stream);
                return it;
            }

            if (type == SeqMovie.Type)
            {
                var it = BinaryMapping.ReadObject<SeqMovie>(stream);
                var rest = Convert.ToInt32(nextPosition - stream.Position);
                it.name = stream.ReadString(rest, _enc);
                return it;
            }

            if (type == SeqSavePoint.Type)
            {
                var it = BinaryMapping.ReadObject<SeqSavePoint>(stream);
                return it;
            }

            if (type == SeqCameraCollision.Type)
            {
                var it = BinaryMapping.ReadObject<SeqCameraCollision>(stream);
                return it;
            }

            if (type == SeqPosMove.Type)
            {
                var it = BinaryMapping.ReadObject<SeqPosMove>(stream);
                return it;
            }

            if (type == BlackFog.Type)
            {
                var it = BinaryMapping.ReadObject<BlackFog>(stream);
                return it;
            }

            if (type == Fog.Type)
            {
                var it = BinaryMapping.ReadObject<Fog>(stream);
                return it;
            }

            if (type == PlayerOffsetCamera.Type)
            {
                var it = BinaryMapping.ReadObject<PlayerOffsetCamera>(stream);
                return it;
            }

            if (type == SkyOff.Type)
            {
                var it = BinaryMapping.ReadObject<SkyOff>(stream);
                return it;
            }

            if (type == HideFobj.Type)
            {
                var it = BinaryMapping.ReadObject<HideFobj>(stream);
                return it;
            }

            if (type == Light.Type)
            {
                var it = BinaryMapping.ReadObject<Light>(stream);
                return it;
            }

            if (type == SeqMob.Type)
            {
                var it = BinaryMapping.ReadObject<SeqMob>(stream);
                return it;
            }

            if (type == Countdown.Type)
            {
                var it = BinaryMapping.ReadObject<Countdown>(stream);
                return it;
            }

            if (type == Tag.Type)
            {
                var it = BinaryMapping.ReadObject<Tag>(stream);
                return it;
            }

            if (type == WallClip.Type)
            {
                var it = BinaryMapping.ReadObject<WallClip>(stream);
                return it;
            }

            if (type == VoiceAllFadeout.Type)
            {
                var it = BinaryMapping.ReadObject<VoiceAllFadeout>(stream);
                return it;
            }

            throw new NotSupportedException($"Unknown event chunk type {type}");
        }
    }
}

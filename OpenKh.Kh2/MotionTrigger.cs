using System.Collections.Generic;
using System.IO;

namespace OpenKh.Kh2
{
    public class MotionTrigger
    {
        public List<RangeTrigger> RangeTriggerList { get; set; }
        public List<FrameTrigger> FrameTriggerList { get; set; }

        public class RangeTrigger
        {
            public short StartFrame { get; set; }
            public short EndFrame { get; set; }
            public byte Trigger { get; set; }
            public byte ParamSize { get; set; }
            public short Param1 { get; set; }
            public short Param2 { get; set; }
            public short Param3 { get; set; }
            public short Param4 { get; set; }


            public RangeTrigger()
            {
            }

            public enum RangeEnum
            {
                u0 = 0,
                u1 = 1,
                u2 = 2,
                u3 = 3,
                u4 = 4,
                u5 = 5,
                u6 = 6,
                u7 = 7,
                u8 = 8,
                u9 = 9,
                u10 = 10,
                u11 = 11,
                u12 = 12,
                u13 = 13,
                u14 = 14,
                u15 = 15,
                u16 = 16,
                u17 = 17,
                u18 = 18,
                u19 = 19,
                u20 = 20,
                u21 = 21,
                u22 = 22,
                u23 = 23,
                u24 = 24,
                u25 = 25,
                u26 = 26,
                u27 = 27,
                u28 = 28,
                u29 = 29,
                u30 = 30,
                u31 = 31,
                u32 = 32,
                u33 = 33,
                u34 = 34,
                u35 = 35,
                u36 = 36,
                u37 = 37,
                u38 = 38,
                u39 = 39,
                u40 = 40,
                u41 = 41,
                u42 = 42,
                u43 = 43,
                u44 = 44,
                u45 = 45,
                u46 = 46,
                u47 = 47,
                u48 = 48,
                u49 = 49,
                u50 = 50,
                u51 = 51,
                u52 = 52,
                u53 = 53
            }
        }

        public class FrameTrigger
        {
            public short Frame { get; set; }
            public byte Trigger { get; set; }
            public byte ParamSize { get; set; }
            /*public List<short> Param { get; set; }

            public short? Param1
            {
                get { return (ParamSize > 0) ? Param[0] : null; }
                set { if (ParamSize > 0) Param[0] = (short)value; }
            }
            public short? Param2
            {
                get { return (ParamSize > 1) ? Param[1] : null; }
                set { if (ParamSize > 1) Param[1] = (short)value; }
            }
            public short? Param3
            {
                get { return (ParamSize > 2) ? Param[2] : null; }
                set { if (ParamSize > 2) Param[2] = (short)value; }
            }
            public short? Param4
            {
                get { return (ParamSize > 3) ? Param[3] : null; }
                set { if (ParamSize > 3) Param[3] = (short)value; }
            }*/
            public short Param1 { get; set; }
            public short Param2 { get; set; }
            public short Param3 { get; set; }
            public short Param4 { get; set; }

            public FrameTrigger()
            {
            }

            public enum FrameEnum
            {
                u0 = 0,
                u1 = 1,
                u2 = 2,
                u3 = 3,
                u4 = 4,
                u5 = 5,
                u6 = 6,
                u7 = 7,
                u8 = 8,
                u9 = 9,
                u10 = 10,
                u11 = 11,
                u12 = 12,
                u13 = 13,
                u14 = 14,
                u15 = 15,
                u16 = 16,
                u17 = 17,
                u18 = 18,
                u19 = 19,
                u20 = 20,
                u21 = 21,
                u22 = 22,
                u23 = 23,
                u24 = 24,
                u25 = 25,
                u26 = 26,
                u27 = 27,
                u28 = 28,
                u29 = 29,
                u30 = 30,
                u31 = 31,
                u32 = 32,
                u33 = 33,
                u34 = 34,
                u35 = 35
            }
        }

        public enum RangeTriggerType
        {
            Dummy = 0,
            Binary = 1
        }
        public enum FrameTriggerType
        {
            Dummy = 0,
            Binary = 1
        }

        public MotionTrigger()
        {
            RangeTriggerList = new List<RangeTrigger>();
            FrameTriggerList = new List<FrameTrigger>();
        }

        public MotionTrigger(Stream stream)
        {
            if (!stream.CanRead || !stream.CanSeek)
                throw new InvalidDataException($"Read or seek must be supported.");

            BinaryReader reader = new BinaryReader(stream);

            byte rangeTriggerCount = reader.ReadByte();
            byte frameTriggerCount = reader.ReadByte();
            short frameTriggerOffset = reader.ReadInt16();

            RangeTriggerList = new List<RangeTrigger>();
            for (int i = 0; i < rangeTriggerCount; i++)
            {
                RangeTrigger rangeTrigger = new RangeTrigger();
                rangeTrigger.StartFrame = reader.ReadInt16();
                rangeTrigger.EndFrame = reader.ReadInt16();
                rangeTrigger.Trigger = reader.ReadByte();
                rangeTrigger.ParamSize = reader.ReadByte();
                /*rangeTrigger.Param = new List<short>();
                for (int j = 0; j < rangeTrigger.ParamSize; j++)
                {
                    rangeTrigger.Param.Add(reader.ReadInt16());
                }*/
                if(rangeTrigger.ParamSize > 0)
                    rangeTrigger.Param1 = reader.ReadInt16();
                if (rangeTrigger.ParamSize > 1)
                    rangeTrigger.Param2 = reader.ReadInt16();
                if (rangeTrigger.ParamSize > 2)
                    rangeTrigger.Param3 = reader.ReadInt16();
                if (rangeTrigger.ParamSize > 3)
                    rangeTrigger.Param4 = reader.ReadInt16();

                RangeTriggerList.Add(rangeTrigger);
            }

            FrameTriggerList = new List<FrameTrigger>();
            for (int i = 0; i < frameTriggerCount; i++)
            {
                FrameTrigger frameTrigger = new FrameTrigger();
                frameTrigger.Frame = reader.ReadInt16();
                frameTrigger.Trigger = reader.ReadByte();
                frameTrigger.ParamSize = reader.ReadByte();
                /*frameTrigger.Param = new List<short>();
                for (int j = 0; j < frameTrigger.ParamSize; j++)
                {
                    frameTrigger.Param.Add(reader.ReadInt16());
                }*/

                if (frameTrigger.ParamSize > 0)
                    frameTrigger.Param1 = reader.ReadInt16();
                if (frameTrigger.ParamSize > 1)
                    frameTrigger.Param2 = reader.ReadInt16();
                if (frameTrigger.ParamSize > 2)
                    frameTrigger.Param3 = reader.ReadInt16();
                if (frameTrigger.ParamSize > 3)
                    frameTrigger.Param4 = reader.ReadInt16();

                FrameTriggerList.Add(frameTrigger);
            }
        }

        public Stream toStream()
        {
            Stream stream = new MemoryStream();
            BinaryWriter writer = new BinaryWriter(stream);

            short offset = 4;

            foreach (RangeTrigger rangeTrigger in RangeTriggerList)
            {
                offset += 6;
                offset += (short)(2 * rangeTrigger.ParamSize);
            }

            writer.Write((byte)RangeTriggerList.Count);
            writer.Write((byte)FrameTriggerList.Count);
            writer.Write((short)offset);

            foreach (RangeTrigger rangeTrigger in RangeTriggerList)
            {
                writer.Write(rangeTrigger.StartFrame);
                writer.Write(rangeTrigger.EndFrame);
                writer.Write(rangeTrigger.Trigger);
                writer.Write(rangeTrigger.ParamSize);
                /*foreach (short par in rangeTrigger.Param)
                {
                    writer.Write(par);
                }*/
                if (rangeTrigger.ParamSize > 0)
                    writer.Write(rangeTrigger.Param1);
                if (rangeTrigger.ParamSize > 1)
                    writer.Write(rangeTrigger.Param2);
                if (rangeTrigger.ParamSize > 2)
                    writer.Write(rangeTrigger.Param3);
                if (rangeTrigger.ParamSize > 3)
                    writer.Write(rangeTrigger.Param4);
            }

            foreach (FrameTrigger frameTrigger in FrameTriggerList)
            {
                writer.Write(frameTrigger.Frame);
                writer.Write(frameTrigger.Trigger);
                writer.Write(frameTrigger.ParamSize);
                /*foreach (short par in frameTrigger.Param)
                {
                    writer.Write(par);
                }*/
                if (frameTrigger.ParamSize > 0)
                    writer.Write(frameTrigger.Param1);
                if (frameTrigger.ParamSize > 1)
                    writer.Write(frameTrigger.Param2);
                if (frameTrigger.ParamSize > 2)
                    writer.Write(frameTrigger.Param3);
                if (frameTrigger.ParamSize > 3)
                    writer.Write(frameTrigger.Param4);
            }

            stream.Position = 0;
            return stream;
        }

        /* 
         * Notes on what each trigger does
        
        FRAME TRIGGERS
        0	OBJ::change_action >>> (Triggers jump state(value 2))
        1	TriggerEffect(PAX) >>> (Plays a PAX effect from a.fm)
        2	OBJ::footstep >>> (Play footstep sound[Id of the sound])
        3	OBJ::change_action(DUSKJUMP)
        4	OBJ::texanm_start
        5	OBJ::texanm_stop
        6	ITEM_EFFECT::Cast	>>> (Use Item)
        7	LIMIT::AnmatrCallback
        8	MOTION::play_se >>> (Play a sound from a.fm.Eg: Sora's chain when walking [Always 0? Other than 0 doesn't work])
        9	VariousTrigger | 1
        10	VariousTrigger | 2
        11	VariousTrigger | 4
        12	VariousTrigger | 8
        13	ZEXT48((this_00->super_PARTY).super_BTLOBJ.super_STDOBJ.super_OBJ.pVTable) >>> (Immune to party attacks' knockback?)
        14	(Same as 13)	>>> (when using item, getting hit on air) (srk: play a VAG lop voice from VSB file) (Eg: Sora's "OK" when using items)
        15	BTLOBJ::turn_to_target
        16	(this_00->super_PARTY).super_BTLOBJ.super_STDOBJ.super_OBJ.DisableCommandTime	>> (When hit on air) (srk: keyblade pop)
        17	MAGIC::shot		>>> (Magic cast)
        18	(RETURN)
        19	OBJ::footprint >>> (Apply footstep effects)
        20	(RETURN)
        21	BTLOBJ::turn_to_lockon
        22	PARTY::show_weapon
        23	FADE::start
        24	FADE::start
        25	ZEXT48((this_00->super_PARTY).super_BTLOBJ.super_STDOBJ.super_OBJ.pVTable)
        26	OBJ::set_parts_color
        27	OBJ::reset_parts_color
        28	ACT::request(revenge) >>> (Revenge check)
        29	PARTY::appear_weapon
        30	LIMIT::PlayVoice
        31	VIBRATION::Start	>>> (Trigger vibration)
        32	(RETURN)
        33	(RETURN)
        34	PLAYER::ability_airslide_sub	>>> (Check for airslide instead of dodge roll)
        35	MOTION::start	>>> (on dodge roll)

        RANGE TRIGGERS
        0	BITFLAG::set 2 (To a MOTION) >>> [grounded] (Set to grounded >>> goes to idle)
        1	BITFLAG::set 3 (To a MOTION) >>> (to fall)[jump + fall] (Set to aerial  >>> locks animation? >>> on attacks can immediately continue combo)
        2	BITFLAG::set 4 (To a MOTION) >>> (interpolate to idle) [land + to idle + item + taking ground damage + recovery from damage] (Set to landing >>> )
        3	BITFLAG::set 5 (To a MOTION) >>> (State no gravity)
        4	COLLISION_FLAG::enable	>>> (srk: reaction command) (shows on LW's cannon...)
        5	COLLISION_FLAG::disable
        6	RECOM_FLAG::enable
        7	RECOM_FLAG::disable
        8	printf "using NO_DAMAGE_REACTION attribute"
        9	BITFLAG::set 9 (To a MOTION) >>> (Lingering Will land)
        10	STDOBJ::motion_attack_on(Attack[attack_param > int ?])
        11	State change >>> (Allows combo attack)
        12	State change
        13	State change
        14	State change >>> (Allows battle RCs)
        15	State change
        16	>>> (Lingering will dash) (srk: mobility enhancement)
        17	>>> (srk: target id: AI combo 1)
        18	>>> (srk: model state: AI combo 2)
        19	>>> (Lingering Will idle, getting hit, ground and air)(srk: disable force i-frames)
        20	ReactionCommand
        21	State change
        22	BTLOBJ::turn_to_target_range	>>> (Turn to target(Item, heal, thunder) [turn speed])
        23	OBJ::texanm_set		>>> (Triggers a textura animation(param 1 animation, param 2 final animation))
        24	super_OBJ.unlabelled240	>>> (srk: disables gravity but keeps kinetics)
        25	super_OBJ.AnmatrCommand
        26	super_OBJ.AnmatrCommand
        27	State change(Hitbox off - Can't be hit; i-frames)
        28	BTLOBJ::turn_to_lockon_range  >>> (Turn to lock [turn speed])
        29	State change    >>> (State: can't fall off edges (easily))
        30	State change    >>> (Freeze animation? Immovable?)
        31	State change
        32	State change
        33	STDOBJ::motion_attack_on	>>> (execute attack (enemy))
        34	State change
        35	State change   >>> (Fire 35_56 > allows attack > makes fire count as combo)
        36	Pattern enable  >>> (Lingering will dash & bow; tp to player?)
        37	Pattern disable
        38	State change
        39	State change
        40	State change
        41	State change
        42	State change    >>> (Used when landing, using items, changing party; can't move? > NOPE)
        43	State change
        44	State change    >>> (Explosion finisher, finishing leap, LW bow; not blocking; not unmovable > not movable by collision wihtout damage?)
        45	State change
        46	State change
        47	State change
        48	State change
        49	State change    >>> (on dodge-roll)
        50	State change   >>> (Allows combo finisher next)
        51	STDOBJ::play_singleton_se
        52	State change
        53	State change
        */
    }
}

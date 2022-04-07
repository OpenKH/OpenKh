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
            public List<short> Param { get; set; }

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
            }
        }

        public class FrameTrigger
        {
            public short Frame { get; set; }
            public byte Trigger { get; set; }
            public byte ParamSize { get; set; }
            public List<short> Param { get; set; }

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
                rangeTrigger.Param = new List<short>();
                for (int j = 0; j < rangeTrigger.ParamSize; j++)
                {
                    rangeTrigger.Param.Add(reader.ReadInt16());
                }
                RangeTriggerList.Add(rangeTrigger);
            }

            FrameTriggerList = new List<FrameTrigger>();
            for (int i = 0; i < frameTriggerCount; i++)
            {
                FrameTrigger frameTrigger = new FrameTrigger();
                frameTrigger.Frame = reader.ReadInt16();
                frameTrigger.Trigger = reader.ReadByte();
                frameTrigger.ParamSize = reader.ReadByte();
                frameTrigger.Param = new List<short>();
                for (int j = 0; j < frameTrigger.ParamSize; j++)
                {
                    frameTrigger.Param.Add(reader.ReadInt16());
                }
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
                offset += (short) (2 * rangeTrigger.ParamSize);
            }

            writer.Write( (byte) RangeTriggerList.Count );
            writer.Write( (byte) FrameTriggerList.Count );
            writer.Write( (short) offset);

            foreach (RangeTrigger rangeTrigger in RangeTriggerList)
            {
                writer.Write(rangeTrigger.StartFrame);
                writer.Write(rangeTrigger.EndFrame);
                writer.Write(rangeTrigger.Trigger);
                writer.Write(rangeTrigger.ParamSize);
                foreach(short par in rangeTrigger.Param)
                {
                    writer.Write(par);
                }
            }

            foreach (FrameTrigger frameTrigger in FrameTriggerList)
            {
                writer.Write(frameTrigger.Frame);
                writer.Write(frameTrigger.Trigger);
                writer.Write(frameTrigger.ParamSize);
                foreach (short par in frameTrigger.Param)
                {
                    writer.Write(par);
                }
            }

            stream.Position = 0;
            return stream;
        }
    }
}

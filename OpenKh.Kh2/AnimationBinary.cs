using System.Collections.Generic;
using System.IO;

namespace OpenKh.Kh2
{
    public class AnimationBinary
    {
        public Motion MotionFile;
        public MotionTrigger MotionTriggerFile;

        // BAR data
        public int MotionIndex { get; set; }
        public string MotionName { get; set; }
        public int TriggerIndex { get; set; }
        public string TriggerName { get; set; }

        public AnimationBinary(Stream stream)
        {
            Bar barFile = Bar.Read(stream);

            if(!isValidAnimationBinary(barFile))
                throw new InvalidDataException($"Invalid animation binary.");

            foreach (Bar.Entry barEntry in barFile)
            {
                switch (barEntry.Type)
                {
                    case Bar.EntryType.Motion:
                        MotionFile = Motion.Read(barEntry.Stream);
                        MotionIndex = barEntry.Index;
                        MotionName = barEntry.Name;
                        break;
                    case Bar.EntryType.MotionTriggers:
                        if (barEntry.Stream.Length == 0)
                            break;

                        MotionTriggerFile = new MotionTrigger(barEntry.Stream);
                        TriggerIndex = barEntry.Index;
                        TriggerName = barEntry.Name;
                        break;
                    default:
                        break;
                }
            }
        }

        public static bool isValidAnimationBinary(Bar barFile)
        {
            if (barFile.Count != 2)
                return false;

            bool hasMotion = false;
            bool hasTrigger = false;
            foreach (Bar.Entry barEntry in barFile)
            {
                switch (barEntry.Type)
                {
                    case Bar.EntryType.Motion:
                        hasMotion = true;
                        break;
                    case Bar.EntryType.MotionTriggers:
                        hasTrigger = true;
                        break;
                    default:
                        break;
                }
            }
            if (!hasMotion || !hasTrigger)
                return false;

            return true;
        }

        public Stream toStream()
        {
            Stream stream = new MemoryStream();

            Bar.Entry MotionEntry = new Bar.Entry();
            MotionEntry.Type = Bar.EntryType.Motion;
            MotionEntry.Index = MotionIndex;
            MotionEntry.Name = MotionName;
            MotionEntry.Stream = new MemoryStream();
            Motion.Write(MotionEntry.Stream, MotionFile);

            Bar.Entry TriggerEntry = new Bar.Entry();
            TriggerEntry.Type = Bar.EntryType.MotionTriggers;
            TriggerEntry.Index = TriggerIndex;
            TriggerEntry.Name = TriggerName;
            TriggerEntry.Stream = (MotionTriggerFile != null) ? MotionTriggerFile.toStream() : new MemoryStream();

            Bar.Write(stream, new List<Bar.Entry> { MotionEntry, TriggerEntry });

            stream.Position = 0;
            return stream;
        }
    }
}

using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public class Fmab
    {
        //public int Index { get; set; }
        [Data] public float HighJumpHeight { get; set; }
        [Data] public float AirDodgeHeight { get; set; }
        [Data] public float AirDodgeSpeed { get; set; }
        [Data] public float AirSlideTime { get; set; }
        [Data] public float AirSlideSpeed { get; set; }
        [Data] public float AirSlideBrake { get; set; }
        [Data] public float AirSlideStopBrake { get; set; }
        [Data] public float AirSlideInvulnerableFrames { get; set; }
        [Data] public float GlideSpeed { get; set; }
        [Data] public float GlideFallRatio { get; set; }
        [Data] public float GlideFallHeight { get; set; }
        [Data] public float GlideFallMax { get; set; }
        [Data] public float GlideAcceleration { get; set; }
        [Data] public float GlideStartHeight { get; set; }
        [Data] public float GlideEndHeight { get; set; }
        [Data] public float GlideTurnSpeed { get; set; }
        [Data] public float DodgeRollInvulnerableFrames { get; set; }

        public class FmabEntries
        {
            [Data] public Dictionary<int, Fmab> Entries { get; set; }
        }

        public static List<Fmab> Read(Stream stream) => BaseTableOffsets<Fmab>.Read(stream);

        public static void Write(Stream stream, IEnumerable<Fmab> entries) => BaseTableOffsets<Fmab>.Write(stream, entries);
    }
}

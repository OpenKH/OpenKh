using OpenKh.Common;
using System;
using System.Collections.Generic;
using System.IO;
using Xe.BinaryMapper;
namespace OpenKh.Kh2
//Can properly read/write and update. Can insert new entries between.
{
    public class Soundinfo
    {
        //[Data] public int Count { get; set; }

        [Data] public short Reverb {  get; set; }
        [Data] public short Rate { get; set; }
        [Data] public short EnvironmentWAV { get; set; }
        [Data] public short EnvironmentSEB { get; set; }
        [Data] public short EnvironmentNUMBER { get; set; }
        [Data] public short EnvironmentSPOT { get; set; }
        [Data] public short FootstepWAV { get; set; }
        [Data] public short FootstepSORA{ get; set; }
        [Data] public short FootstepDONALD { get; set; }
        [Data] public short FootstepGOOFY { get; set; }
        [Data] public short FootstepWORLDFRIEND { get; set; }
        [Data] public short FootstepOTHER { get; set; }


        public class SoundinfoPatch
        {
            [Data] public int Index { get; set; }
            [Data] public short Reverb { get; set; }
            [Data] public short Rate { get; set; }
            [Data] public short EnvironmentWAV { get; set; }
            [Data] public short EnvironmentSEB { get; set; }
            [Data] public short EnvironmentNUMBER { get; set; }
            [Data] public short EnvironmentSPOT { get; set; }
            [Data] public short FootstepWAV { get; set; }
            [Data] public short FootstepSORA { get; set; }
            [Data] public short FootstepDONALD { get; set; }
            [Data] public short FootstepGOOFY { get; set; }
            [Data] public short FootstepWORLDFRIEND { get; set; }
            [Data] public short FootstepOTHER { get; set; }
        }

        public static List<Soundinfo> Read(Stream stream) => BaseTableCountOnly<Soundinfo>.Read(stream);
        public static void Write(Stream stream, IEnumerable<Soundinfo> entries) =>
            BaseTableCountOnly<Soundinfo>.Write(stream, entries);

    }
}

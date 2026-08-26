using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public class Went
    {
        public List<uint> Offsets { get; set; }
        public List<WentSet> Sets { get; set; }

        public class WentSet
        {
            public uint OriginalOffset; //WENT set offset
            public uint SetSize;
            public List<uint> WeaponIds;
        }

        private static uint ReadUInt32(Stream stream)
        {
            var buffer = new byte[4];
            if (stream.Read(buffer, 0, 4) != 4)
                throw new EndOfStreamException();

            return BitConverter.ToUInt32(buffer, 0);
        }

        private static void WriteUInt32(Stream stream, uint value)
        {
            var buffer = BitConverter.GetBytes(value);
            stream.Write(buffer, 0, 4);
        }

        //Right now, we don't support adding brand new Went Sets. Went Indices correspond to "NeoMovesets" used to assign objects to a Went set, so theres a lot of repeats.
        public static Went Read(Stream stream)
        {
            var went = new Went();
            went.Offsets = new List<uint>();
            went.Sets = new List<WentSet>();

            long fileStart = stream.Position;


            //Read header & ONLY track unique offsets, due to WENT repeating offsets within the header.
            for (int i = 0; i < 70; i++)
                went.Offsets.Add(ReadUInt32(stream));
            var visited = new HashSet<uint>();

            foreach (var offset in went.Offsets)
            {
                if (offset == 0 || visited.Contains(offset))
                    continue;

                visited.Add(offset);

                long realOffset = fileStart + (offset * 4);
                stream.Position = realOffset;

                var set = new WentSet();
                set.OriginalOffset = offset;
                set.SetSize = ReadUInt32(stream);

                int entryCount = (int)set.SetSize - 1;

                set.WeaponIds = new List<uint>();
                for (int i = 0; i < entryCount; i++)
                    set.WeaponIds.Add(ReadUInt32(stream));

                went.Sets.Add(set);
            }

            return went;
        }

        public void Write(Stream stream)
        {
            long fileStart = stream.Position;

            //For now, hardcode WENT to always be 70 sets long
            for (int i = 0; i < 70; i++)
                WriteUInt32(stream, 0);

            long setsStart = stream.Position;

            //Sort unique offsets, and only update offsets in the header when we add in new entries to a WENT set.
            var uniqueOffsets = Offsets
                .Where(x => x != 0)
                .Distinct()
                .OrderBy(x => x)
                .ToList();

            var offsetMap = new Dictionary<uint, uint>();

            foreach (var oldOffset in uniqueOffsets)
            {
                var set = Sets.Find(x => x.OriginalOffset == oldOffset);

                long currentPos = stream.Position;
                uint newOffset = (uint)((currentPos - fileStart) / 4);

                offsetMap[oldOffset] = newOffset;

                set.SetSize = (uint)(set.WeaponIds.Count + 1);

                WriteUInt32(stream, set.SetSize);

                foreach (var id in set.WeaponIds)
                    WriteUInt32(stream, id);
            }

            long endPos = stream.Position;

            stream.Position = fileStart;

            foreach (var oldOffset in Offsets)
            {
                if (oldOffset == 0)
                {
                    WriteUInt32(stream, 0);
                }
                else
                {
                    WriteUInt32(stream, offsetMap[oldOffset]);
                }
            }

            stream.Position = endPos;
        }
    }
}

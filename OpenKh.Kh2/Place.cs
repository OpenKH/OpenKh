using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class Places
    {
        [Data] public ushort MessageId { get; set; }
        [Data] public ushort Padding { get; set; }

        //Two bytes after MessageId don't seem to matter.

        public class PlacePatch
        {
            [Data] public int Index { get; set; }
            [Data] public ushort MessageId { get; set; }
            [Data] public ushort Padding { get; set; }
        }
        public static List<Places> Read(Stream stream)
        {
            long count = stream.Length / 4; // Each entry is 4 bytes
            var placesList = new List<Places>((int)count);

            for (long i = 0; i < count; i++)
            {
                placesList.Add(new Places
                {
                    MessageId = stream.ReadUInt16(),
                    Padding = stream.ReadUInt16()
                });
            }

            return placesList;
        }

        public static void Write(Stream stream, List<Places> placesList)
        {
            foreach (var place in placesList)
            {
                stream.Write(place.MessageId);
                stream.Write(place.Padding);
            }
        }
    }
}

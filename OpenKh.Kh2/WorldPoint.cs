using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2
{
    public class WorldPoint
    {
        [Data] public byte World { get; set; }
        [Data] public byte Area { get; set; }
        [Data] public byte Entrance { get; set; }
        [Data] public byte Padding { get; set; }

        public static List<WorldPoint> Read(Stream stream)
        {
            var estimatedItemCount = (int)(stream.Length - stream.Position) / 4;
            return Enumerable.Range(0, estimatedItemCount)
                .Select(_ => BinaryMapping.ReadObject<WorldPoint>(stream))
                .ToList();
        }

        public static void Write(Stream stream, IEnumerable<WorldPoint> worldPoints)
        {
            foreach (var item in worldPoints)
                BinaryMapping.WriteObject(stream, item);
        }
    }
}

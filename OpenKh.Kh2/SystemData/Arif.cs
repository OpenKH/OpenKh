using OpenKh.Common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.BinaryMapper;

namespace OpenKh.Kh2.SystemData
{
    public class BgmSet
    {
        [Data] public ushort BgmField { get; set; }
        [Data] public ushort BgmBattle { get; set; }
    }

    /// <summary>
    /// AreaInfo
    /// </summary>
    public class Arif
    {
        [Data] public uint Flags { get; set; }
        [Data] public int Reverb { get; set; }
        [Data] public int SoundEffectBank1 { get; set; }
        [Data] public int SoundEffectBank2 { get; set; }
        [Data(Count = 8)] public BgmSet[] Bgms { get; set; }
        [Data] public ushort Voice { get; set; }
        [Data] public ushort NavigationMapItem { get; set; }
        [Data] public byte Command { get; set; }
        [Data(Count = 11)] public byte[] Reserved { get; set; }

        public static List<List<Arif>> Read(Stream stream)
        {
            var basePosition = stream.Position;
            var version = stream.ReadInt32();
            var worldCount = stream.ReadInt32();

            List<(ushort areaCount, ushort offset)> worldsInfo =
                Enumerable.Range(0, worldCount)
                .Select(x => (stream.ReadUInt16(), stream.ReadUInt16()))
                .ToList();

            return Enumerable.Range(0, worldCount)
                .Select(index =>
                {
                    var worldInfo = worldsInfo[index];
                    stream.Position = basePosition + worldInfo.offset;
                    return Enumerable.Range(0, worldInfo.areaCount)
                        .Select(_ => BinaryMapping.ReadObject<Arif>(stream))
                        .ToList();
                })
                .ToList();
        }

        public static void Write(Stream stream, ICollection<List<Arif>> areainfos)
        {
            const int SupportedVersion = 1;

            stream.Write(SupportedVersion);
            stream.Write(areainfos.Count);

            var offset = 8 + areainfos.Count * 4;
            foreach (var worldInfo in areainfos)
            {
                stream.Write((ushort)worldInfo.Count);
                stream.Write((ushort)offset);
                offset += worldInfo.Count * 0x40;
            }

            foreach (var worldInfo in areainfos)
                foreach (var areaInfo in worldInfo)
                    BinaryMapping.WriteObject(stream, areaInfo);
        }
    }
}

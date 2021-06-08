using System.IO;

namespace OpenKh.Audio
{
    public interface IScdExtraData
    {
        // If Data is read at the start and then stream is getting rewound,
        // either save position or pass a substream to the Read function
        byte[] Data { get; set; }
        void Read(Stream stream, int length);
    }
}

using Xe.BinaryMapper;
using static OpenKh.Kh2.Models.ModelCommon;

namespace OpenKh.Kh2.Models
{
    internal class ModelBackground
    {
        [Data] public ModelHeader ModelHeader { get; set; }
        [Data] public ushort GroupCount { get; set; }
        [Data] public ushort ShadowGroupCount { get; set; }
        [Data] public ushort TextureCount { get; set; }
        [Data] public ushort OctreeGroupCount { get; set; }
        [Data] public uint GroupTableOffset { get; set; }
        [Data] public uint PartTableOffset { get; set; }
        //public List<BackgroundGroup> Groups { get; set; }

        private class BackgroundGroupHeader
        {
            [Data] public uint DmaTagOffset { get; set; }
            [Data] public ushort TextureIndex { get; set; }
            [Data] public ushort Flags { get; set; }
            [Data] public uint Priority { get; set; }
            [Data] public uint Attributes { get; set; }
        }

        /* BackgroundGroup flags bitfield
         * reserved [2]
         * alt [12]
         * hasVB
         * specular
         */

        /* BackgroundGroup attributes bitfield
         * polygonCount [16]
         * phase
         * priority [5]
         * multi
         * alpha
         * alphaSub
         * alphaAdd
         * uvScroll [5] // UV scroll index
         * shadowOff
         */
    }
}

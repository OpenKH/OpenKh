using OpenKh.Command.MapGen.Interfaces;
using OpenKh.Command.MapGen.Models;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.MapGen.Utils
{
    public class DoctDummyBuilder
    {
        private readonly Doct doct = new Doct();
        private readonly List<SingleFace[]> vifPacketRenderingGroup = new List<SingleFace[]>();

        public DoctBuilt GetBuilt() => new DoctBuilt
        {
            Doct = doct,
            VifPacketRenderingGroup = vifPacketRenderingGroup,
        };

        public DoctDummyBuilder(IEnumerable<SingleFace> faces)
        {
            doct.Entry1List.Add(
                new Doct.Entry1
                {
                    BoundingBox = new Kh2.Utils.BoundingBox(
                        new Vector3(-32767, -32767, -32767),
                        new Vector3(32767, 32767, 32767)
                    ),
                    Entry2Index = 0,
                    Entry2LastIndex = 1,
                }
            );

            doct.Entry2List.Add(
                new Doct.Entry2
                {
                    BoundingBox = new Kh2.Utils.BoundingBox(
                        new Vector3(-32767, -32767, -32767),
                        new Vector3(32767, 32767, 32767)
                    ),
                    Flags = 0,
                }
            );

            vifPacketRenderingGroup.Add(faces.ToArray());
        }
    }
}

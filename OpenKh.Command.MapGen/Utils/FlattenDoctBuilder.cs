using OpenKh.Command.MapGen.Interfaces;
using OpenKh.Command.MapGen.Models;
using OpenKh.Kh2;
using OpenKh.Kh2.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.MapGen.Utils
{
    public class FlattenDoctBuilder
    {
        private readonly Doct doct = new Doct();
        private readonly List<SingleFace[]> vifPacketRenderingGroup = new List<SingleFace[]>();

        public DoctBuilt GetBuilt() => new DoctBuilt
        {
            Doct = doct,
            VifPacketRenderingGroup = vifPacketRenderingGroup,
        };

        public FlattenDoctBuilder(
            ISpatialNodeCutter cutter,
            Func<MaterialDef, int> getAttributeFrom
        )
        {
            var final = FlattenSpatialNode.From(cutter);

            BoundingBox GetBoundingBoxOf(IEnumerable<SingleFace> faces)
            {
                return BoundingBox.FromManyPoints(
                    faces
                        .SelectMany(face => face.positionList)
                        .Select(point => new Vector3(point.X, -point.Y, -point.Z)) // why -Y and -Z ?
                );
            }

            var entry1 = new Doct.Entry1
            {
                BoundingBox = GetBoundingBoxOf(
                    final
                        .SelectMany(
                            face => face.Faces
                        )
                ),
            };
            doct.Entry1List.Add(entry1);

            var entry2FirstIdx = doct.Entry2List.Count;

            foreach (var secondary in final)
            {
                foreach (var group in secondary.Faces.GroupBy(face => getAttributeFrom(face.matDef)))
                {
                    var entry2 = new Doct.Entry2
                    {
                        BoundingBox = GetBoundingBoxOf(group),
                        Flags = (uint)group.Key,
                    };
                    doct.Entry2List.Add(entry2);

                    vifPacketRenderingGroup.Add(
                        group.ToArray()
                    );
                }
            }

            var entry2LastIdx = doct.Entry2List.Count;

            entry1.Entry2Index = Convert.ToUInt16(entry2FirstIdx);
            entry1.Entry2LastIndex = Convert.ToUInt16(entry2LastIdx);
        }
    }
}

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
    public class FlattenDoctBuilderAlt
    {
        private readonly Doct doct = new Doct();
        private readonly List<SingleFace[]> vifPacketRenderingGroup = new List<SingleFace[]>();

        public DoctBuilt GetBuilt() => new DoctBuilt
        {
            Doct = doct,
            VifPacketRenderingGroup = vifPacketRenderingGroup,
        };

        public FlattenDoctBuilderAlt(
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
                // New: Group by explicit group number if defined, otherwise by attribute
                var groupedFaces = secondary.Faces
                    .GroupBy(face =>
                    {
                        var group = face.matDef?.group ?? -1;
                        return group >= 0 ? $"g{group}" : $"a{getAttributeFrom(face.matDef)}";
                    });

                foreach (var group in groupedFaces)
                {
                    var faces = group.ToArray();
                    if (faces.Length == 0)
                        continue;

                    // Extract bounding box
                    var bbox = GetBoundingBoxOf(faces);

                    // - if group starts with "g", use it as special group to be able to be hidden with MapVisibility.
                    // - else fallback to attribute flags
                    var key = group.Key;
                    uint flags;

                    if (key.StartsWith("g"))
                    {
                        var groupId = int.Parse(key.Substring(1));
                        var floorLevel = faces[0].matDef.floorLevel;
                        //What's known as "flags" is really a set of 4 bytes known as Visible, Group, floorLevel, & padding. Bitshift to accomodate the improper DOCT class.
                        flags = 0x00000000 | ((uint)groupId << 8) | ((uint)floorLevel << 16); //Shift group to +0x01 & +0x02 position, since its stored as a 4-byte value and not 4, independent values.
                    }
                    else
                    {
                        var attr = int.Parse(key.Substring(1));
                        var floorLevel = faces[0].matDef.floorLevel;
                        flags = (uint)attr | ((uint)floorLevel << 16);
                    }

                    // Add Entry2
                    var entry2 = new Doct.Entry2
                    {
                        BoundingBox = bbox,
                        Flags = flags,
                    };

                    doct.Entry2List.Add(entry2);
                    vifPacketRenderingGroup.Add(faces);
                }
            }
            var entry2LastIdx = doct.Entry2List.Count;

            entry1.Entry2Index = Convert.ToUInt16(entry2FirstIdx);
            entry1.Entry2LastIndex = Convert.ToUInt16(entry2LastIdx);
        }
    }
}

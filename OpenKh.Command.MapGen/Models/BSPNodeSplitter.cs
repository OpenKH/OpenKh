using OpenKh.Command.MapGen.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenKh.Command.MapGen.Models
{
    public class BSPNodeSplitter : ISpatialNodeCutter
    {
        public IEnumerable<SingleFace> Faces { get; }

        public class Option
        {
            public int PartitionSize { get; set; } = 30;
        }

        private readonly Option option;

        public IEnumerable<ISpatialNodeCutter> Cut()
        {
            var faces = Faces.ToArray();

            if (faces.Length > option.PartitionSize)
            {
                BSPNodeSplitter Make(IEnumerable<SingleFace> faces) =>
                    new BSPNodeSplitter(faces, option);

                (IEnumerable<T> a, IEnumerable<T> b) HalfHalf<T>(IEnumerable<T> collection)
                {
                    var array = collection.ToArray();
                    var halfPos = array.Length / 2;
                    return (array.Take(halfPos), array.Skip(halfPos));
                }

                (IEnumerable<T> y, IEnumerable<T> n) CutByCondition<T>(IEnumerable<T> collection, Func<T, bool> condition)
                {
                    var y = new List<T>();
                    var n = new List<T>();
                    foreach (var item in collection)
                    {
                        (condition(item) ? y : n).Add(item);
                    }
                    return (y, n);
                }

                BSPNodeSplitter[] CutNodeByConditionOrHalfHelper(Func<SingleFace, bool> condition)
                {
                    var (a, b) = CutByCondition(faces, condition);

                    if (!a.Any())
                    {
                        (a, b) = HalfHalf(b);
                    }
                    else if (!b.Any())
                    {
                        (a, b) = HalfHalf(a);
                    }

                    return new BSPNodeSplitter[] { Make(a), Make(b), };
                }

                var range = new Range(faces);
                var max = Math.Max(
                    range.zDistinct,
                    Math.Max(range.xDistinct, range.yDistinct)
                );
                if (max == range.zDistinct)
                {
                    // z-cut
                    return CutNodeByConditionOrHalfHelper(it => it.referencePosition.Z >= range.zCenter);
                }
                else if (max == range.yDistinct)
                {
                    // y-cut
                    return CutNodeByConditionOrHalfHelper(it => it.referencePosition.Y >= range.yCenter);
                }
                else
                {
                    // x-cut
                    return CutNodeByConditionOrHalfHelper(it => it.referencePosition.X >= range.xCenter);
                }
            }
            return new BSPNodeSplitter[] { this };
        }

        private class Range
        {
            public float xCenter;
            public float yCenter;
            public float zCenter;

            public int xDistinct;
            public int yDistinct;
            public int zDistinct;

            public Range(SingleFace[] faces)
            {
                var xArray = new float[faces.Length];
                var yArray = new float[faces.Length];
                var zArray = new float[faces.Length];

                var num = faces.Length;

                var centerIdx = num / 2;

                for (int idx = 0; idx < num; idx++)
                {
                    var pos = faces[idx].referencePosition;
                    xArray[idx] = pos.X;
                    yArray[idx] = pos.Y;
                    zArray[idx] = pos.Z;
                }

                Array.Sort(xArray);
                Array.Sort(yArray);
                Array.Sort(zArray);

                xCenter = xArray[centerIdx];
                yCenter = yArray[centerIdx];
                zCenter = zArray[centerIdx];

                xDistinct = xArray.Select(v => (int)v).Distinct().Count();
                yDistinct = yArray.Select(v => (int)v).Distinct().Count();
                zDistinct = zArray.Select(v => (int)v).Distinct().Count();
            }
        }

        public BSPNodeSplitter(IEnumerable<SingleFace> faces, Option option)
        {
            Faces = faces;
            this.option = option;
        }
    }
}

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

                static (IEnumerable<T> a, IEnumerable<T> b) HalfHalf<T>(IEnumerable<T> collection)
                {
                    var array = collection.ToArray();
                    var halfPos = array.Length / 2;
                    return (array.Take(halfPos), array.Skip(halfPos));
                }

                static (IEnumerable<T> y, IEnumerable<T> n) CutByCondition<T>(IEnumerable<T> collection, Func<T, bool> condition)
                {
                    var y = new List<T>();
                    var n = new List<T>();
                    foreach (var item in collection)
                    {
                        (condition(item) ? y : n).Add(item);
                    }
                    return (y, n);
                }

                BSPNodeSplitter[] CutNodeByConditionOrHalfHelper1Ax(
                    Func<SingleFace, bool> condition
                )
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

                BSPNodeSplitter[] CutNodeByConditionOrHalfHelper2Axis(
                    Func<SingleFace, bool> condition1, 
                    Func<SingleFace, bool> condition2
                )
                {
                    var (x, y) = CutByCondition(faces, condition1);
                    var (a, b) = CutByCondition(x, condition2);
                    var (c, d) = CutByCondition(y, condition2);

                    if (!a.Any())
                    {
                        (a, b) = HalfHalf(b);
                    }
                    else if (!b.Any())
                    {
                        (a, b) = HalfHalf(a);
                    }

                    if (!c.Any())
                    {
                        (c, d) = HalfHalf(d);
                    }
                    else if (!d.Any())
                    {
                        (c, d) = HalfHalf(c);
                    }

                    return new BSPNodeSplitter[] { Make(a), Make(b), Make(c), Make(d), };
                }

                BSPNodeSplitter[] CutNodeByConditionOrHalfHelper3Axis(
                    Func<SingleFace, bool> condition1, 
                    Func<SingleFace, bool> condition2,
                    Func<SingleFace, bool> condition3
                )
                {
                    var (x, y) = CutByCondition(faces, condition1);
                    var (s, t) = CutByCondition(x, condition2);
                    var (u, v) = CutByCondition(y, condition2);
                    var (a, b) = CutByCondition(s, condition3);
                    var (c, d) = CutByCondition(t, condition3);
                    var (e, f) = CutByCondition(u, condition3);
                    var (g, h) = CutByCondition(v, condition3);

                    if (!a.Any())
                    {
                        (a, b) = HalfHalf(b);
                    }
                    else if (!b.Any())
                    {
                        (a, b) = HalfHalf(a);
                    }

                    if (!c.Any())
                    {
                        (c, d) = HalfHalf(d);
                    }
                    else if (!d.Any())
                    {
                        (c, d) = HalfHalf(c);
                    }

                    if (!e.Any())
                    {
                        (e, f) = HalfHalf(f);
                    }
                    else if (!f.Any())
                    {
                        (e, f) = HalfHalf(e);
                    }

                    if (!g.Any())
                    {
                        (g, h) = HalfHalf(h);
                    }
                    else if (!h.Any())
                    {
                        (g, h) = HalfHalf(g);
                    }

                    return new BSPNodeSplitter[] { Make(a), Make(b), Make(c), Make(d), Make(e), Make(f), Make(g), Make(h), };
                }

                var gist = new Gist(faces);
                var max = Math.Max(
                    gist.zDistinct,
                    Math.Max(gist.xDistinct, gist.yDistinct)
                );
                var threshold = max / 3;

                var conditions = new List<Func<SingleFace, bool>>();

                if (threshold <= gist.zDistinct)
                {
                    conditions.Add((it => gist.zCenter <= it.referencePosition.Z));
                }
                if (threshold <= gist.yDistinct)
                {
                    conditions.Add((it => gist.yCenter <= it.referencePosition.Y));
                }
                if (threshold <= gist.xDistinct)
                {
                    conditions.Add((it => gist.xCenter <= it.referencePosition.X));
                }

                if (conditions.Count == 1)
                {
                    return CutNodeByConditionOrHalfHelper1Ax(conditions[0]);
                }
                else if (conditions.Count == 2)
                {
                    return CutNodeByConditionOrHalfHelper2Axis(conditions[0], conditions[1]);
                }
                else if (conditions.Count == 3)
                {
                    return CutNodeByConditionOrHalfHelper3Axis(conditions[0], conditions[1], conditions[2]);
                }
                else
                {
                    throw new NotSupportedException();
                }
            }
            return new BSPNodeSplitter[] { this };
        }

        private class Gist
        {
            public float xCenter;
            public float yCenter;
            public float zCenter;

            public int xDistinct;
            public int yDistinct;
            public int zDistinct;

            public Gist(SingleFace[] faces)
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

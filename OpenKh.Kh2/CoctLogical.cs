using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Numerics;
using OpenKh.Kh2.Utils;

namespace OpenKh.Kh2
{
    /// <summary>
    /// A high standard builder of coct
    /// </summary>
    public class CoctLogical
    {
        public CoctLogical(Coct coct)
        {
            Collision1 = coct.Collision1
                .Select(
                    collision1 =>
                    {
                        var newCollision1 = Map1(collision1);

                        newCollision1.Meshes = Enumerable.Range(
                            collision1.Collision2Start,
                            collision1.Collision2End - collision1.Collision2Start
                        )
                            .Select(
                                collision2Index =>
                                {
                                    var collision2 = coct.Collision2[collision2Index];

                                    var newCollision2 = Map2(collision2);

                                    newCollision2.Items = Enumerable.Range(
                                        collision2.Collision3Start,
                                        collision2.Collision3End - collision2.Collision3Start
                                    )
                                        .Select(
                                            collision3Index =>
                                            {
                                                var collision3 = coct.Collision3[collision3Index];

                                                var newCollision3 = Map3(collision3);

                                                return newCollision3;
                                            }
                                        )
                                        .ToList();

                                    return newCollision2;
                                }
                            )
                            .ToList();

                        return newCollision1;
                    }
                )
                .ToList();

            CollisionVertices = coct.CollisionVertices
                .Select(collision4 => Map4(collision4))
                .ToList();

            Collision5 = coct.Collision5
                .Select(collision5 => Map5(collision5))
                .ToList();

            Collision6 = coct.Collision6
                .Select(collision6 => Map6(collision6))
                .ToList();

            Collision7 = coct.Collision7
                .Select(collision7 => Map7(collision7))
                .ToList();
        }

        private static CoctVector4 Map4(Vector4 source)
        {
            return new CoctVector4
            {
                X = source.X,
                Y = source.Y,
                Z = source.Z,
                W = source.W,
            };
        }

        private static Co7 Map7(Coct.Co7 source)
        {
            return new Co7
            {
                Unknown = source.Unknown,
            };
        }

        private static Co6 Map6(BoundingBoxInt16 source)
        {
            return new Co6
            {
                MinX = source.Minimum.X,
                MinY = source.Minimum.Y,
                MinZ = source.Minimum.Z,
                MaxX = source.Maximum.X,
                MaxY = source.Maximum.Y,
                MaxZ = source.Maximum.Z,
            };
        }

        private static Co5 Map5(Plane source) =>
            new Co5
            {
                X = source.Normal.X,
                Y = source.Normal.Y,
                Z = source.Normal.Z,
                D = source.D,
            };

        private static Co3 Map3(Coct.Co3 source)
        {
            return new Co3
            {
                v00 = source.v00,
                Vertex1 = source.Vertex1,
                Vertex2 = source.Vertex2,
                Vertex3 = source.Vertex3,
                Vertex4 = source.Vertex4,
                Co5Index = source.Co5Index,
                Co6Index = source.Co6Index,
                Co7Index = source.Co7Index,
            };
        }

        private static CollisionMesh Map2(Coct.CollisionMesh source)
        {
            return new CollisionMesh
            {
                MinX = source.BoundingBox.Minimum.X,
                MinY = source.BoundingBox.Minimum.Y,
                MinZ = source.BoundingBox.Minimum.Z,
                MaxX = source.BoundingBox.Maximum.X,
                MaxY = source.BoundingBox.Maximum.Y,
                MaxZ = source.BoundingBox.Maximum.Z,
                v10 = source.v10,
                v12 = source.v12,
            };
        }

        private static Co1 Map1(Coct.Co1 source)
        {
            return new Co1
            {
                Child1 = source.Child1,
                Child2 = source.Child2,
                Child3 = source.Child3,
                Child4 = source.Child4,
                Child5 = source.Child5,
                Child6 = source.Child6,
                Child7 = source.Child7,
                Child8 = source.Child8,
                MinX = source.BoundingBox.Minimum.X,
                MinY = source.BoundingBox.Minimum.Y,
                MinZ = source.BoundingBox.Minimum.Z,
                MaxX = source.BoundingBox.Maximum.X,
                MaxY = source.BoundingBox.Maximum.Y,
                MaxZ = source.BoundingBox.Maximum.Z,
            };
        }

        public List<Co1> Collision1 { get; }
        public List<CoctVector4> CollisionVertices { get; }
        public List<Co5> Collision5 { get; }
        public List<Co6> Collision6 { get; }
        public List<Co7> Collision7 { get; }

        public class Co1
        {
            public short Child1 { get; set; }
            public short Child2 { get; set; }
            public short Child3 { get; set; }
            public short Child4 { get; set; }
            public short Child5 { get; set; }
            public short Child6 { get; set; }
            public short Child7 { get; set; }
            public short Child8 { get; set; }
            public short MinX { get; set; }
            public short MinY { get; set; }
            public short MinZ { get; set; }
            public short MaxX { get; set; }
            public short MaxY { get; set; }
            public short MaxZ { get; set; }

            public List<CollisionMesh> Meshes { get; set; }
        }

        public class CollisionMesh
        {
            public short MinX { get; set; }
            public short MinY { get; set; }
            public short MinZ { get; set; }
            public short MaxX { get; set; }
            public short MaxY { get; set; }
            public short MaxZ { get; set; }
            public short v10 { get; set; }
            public short v12 { get; set; }

            public List<Co3> Items { get; set; }
        }

        public class Co3
        {
            public short v00 { get; set; }
            public short Vertex1 { get; set; }
            public short Vertex2 { get; set; }
            public short Vertex3 { get; set; }
            public short Vertex4 { get; set; }
            public short Co5Index { get; set; }
            public short Co6Index { get; set; }
            public short Co7Index { get; set; }
        }

        public class CoctVector4
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public float W { get; set; }

            public override string ToString() =>
                $"V({X}, {Y}, {Z}, {W})";
        }

        /// <summary>
        /// Plane (x,y,z,d)
        /// </summary>
        public class Co5
        {
            public float X { get; set; }
            public float Y { get; set; }
            public float Z { get; set; }
            public float D { get; set; }

            public override string ToString() =>
                $"N({X}, {Y}, {Z}, {D})";
        }


        public class Co6
        {
            public short MinX { get; set; }
            public short MinY { get; set; }
            public short MinZ { get; set; }
            public short MaxX { get; set; }
            public short MaxY { get; set; }
            public short MaxZ { get; set; }
        }

        public class Co7
        {
            public int Unknown { get; set; }
        }

        public Coct CreateCoct()
        {
            var coct = new Coct();

            coct.Collision1.AddRange(
                Collision1
                    .Select(
                        collision1 =>
                        {
                            var newCollision1 = Ummap1(collision1);

                            newCollision1.Collision2Start = Convert.ToUInt16(
                                coct.Collision2.Count
                            );

                            coct.Collision2.AddRange(
                                collision1.Meshes
                                    .Select(
                                        collision2 =>
                                        {
                                            var newCollision2 = Unmap2(collision2);

                                            newCollision2.Collision3Start = Convert.ToUInt16(
                                                coct.Collision3.Count
                                            );

                                            coct.Collision3.AddRange(
                                                collision2.Items
                                                    .Select(
                                                        collision3 =>
                                                        {
                                                            var newCollision3 = Unmap3(collision3);

                                                            return newCollision3;
                                                        }
                                                    )
                                            );

                                            newCollision2.Collision3End = Convert.ToUInt16(
                                                coct.Collision3.Count
                                            );

                                            return newCollision2;
                                        }
                                    )
                            );

                            newCollision1.Collision2End = Convert.ToUInt16(
                                coct.Collision2.Count
                            );

                            return newCollision1;
                        }
                    )
            );

            coct.CollisionVertices.AddRange(
                CollisionVertices
                    .Select(Unmap4)
            );

            coct.Collision5.AddRange(
                Collision5
                    .Select(Unmap5)
            );

            coct.Collision6.AddRange(
                Collision6
                    .Select(Unmap6)
            );

            coct.Collision7.AddRange(
                Collision7
                    .Select(Unmap7)
            );

            return coct;
        }

        private static Coct.Co7 Unmap7(Co7 source)
        {
            return new Coct.Co7
            {
                Unknown = source.Unknown,
            };
        }

        private static BoundingBoxInt16 Unmap6(Co6 source)
        {
            return new BoundingBoxInt16(
                new Vector3Int16(source.MinX, source.MinY, source.MinZ),
                new Vector3Int16(source.MaxX, source.MaxY, source.MaxZ)
            );
        }

        private static Plane Unmap5(Co5 source) =>
            new Plane(source.X, source.Y, source.Z, source.D);

        private static Vector4 Unmap4(CoctVector4 source) =>
            new Vector4(source.X, source.Y, source.Z, source.W);

        private static Coct.Co3 Unmap3(Co3 source)
        {
            return new Coct.Co3
            {
                v00 = source.v00,
                Vertex1 = source.Vertex1,
                Vertex2 = source.Vertex2,
                Vertex3 = source.Vertex3,
                Vertex4 = source.Vertex4,
                Co5Index = source.Co5Index,
                Co6Index = source.Co6Index,
                Co7Index = source.Co7Index,
            };
        }

        private Coct.CollisionMesh Unmap2(CollisionMesh source)
        {
            return new Coct.CollisionMesh
            {
                BoundingBox = new BoundingBoxInt16(
                    new Vector3Int16(source.MinX, source.MinY, source.MinZ),
                    new Vector3Int16(source.MaxX, source.MaxY, source.MaxZ)
                ),
                v10 = source.v10,
                v12 = source.v12,
            };
        }

        private static Coct.Co1 Ummap1(Co1 collision1)
        {
            return new Coct.Co1
            {
                Child1 = collision1.Child1,
                Child2 = collision1.Child2,
                Child3 = collision1.Child3,
                Child4 = collision1.Child4,
                Child5 = collision1.Child5,
                Child6 = collision1.Child6,
                Child7 = collision1.Child7,
                Child8 = collision1.Child8,
                BoundingBox = new BoundingBoxInt16(
                    new Vector3Int16(collision1.MinX, collision1.MinY, collision1.MinZ),
                    new Vector3Int16(collision1.MaxX, collision1.MaxY, collision1.MaxZ)
                ),
            };
        }
    }
}

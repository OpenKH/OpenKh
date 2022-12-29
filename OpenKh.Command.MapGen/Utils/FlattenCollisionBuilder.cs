using OpenKh.Command.MapGen.Interfaces;
using OpenKh.Command.MapGen.Models;
using OpenKh.Kh2;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using static OpenKh.Kh2.Coct;

namespace OpenKh.Command.MapGen.Utils
{
    public class FlattenCollisionBuilder : ICollisionBuilder
    {
        private readonly Coct coct = new Coct();
        private bool isValid = false;

        public CollisionBuilt GetBuilt() => new CollisionBuilt
        {
            Coct = coct,
            IsValid = isValid,
        };

        public FlattenCollisionBuilder(
            ISpatialNodeCutter cutter,
            Func<MaterialDef, int> getAttributeFrom
        )
        {
            var final = FlattenSpatialNode.From(cutter);

            var helper = new Coct.BuildHelper(coct);

            var collisionNode = new CollisionNode
            {
                Meshes = new List<CollisionMesh>(),
            };

            if (final.Any())
            {
                isValid = true;

                foreach (var faces in final)
                {
                    var collisionMesh = new CollisionMesh
                    {
                        Collisions = new List<Collision>()
                    };

                    foreach (var face in faces.Faces)
                    {
                        var quad = face.positionList.Length == 4;

                        var v1 = face.positionList[0];
                        var v2 = face.positionList[1];
                        var v3 = face.positionList[2];
                        var v4 = quad ? face.positionList[3] : Vector3.Zero;

                        collisionMesh.Collisions.Add(coct.Complete(
                            new Collision
                            {
                                Vertex1 = helper.AllocateVertex(v1.X, -v1.Y, -v1.Z), // why -Y and -Z ?
                                Vertex2 = helper.AllocateVertex(v2.X, -v2.Y, -v2.Z),
                                Vertex3 = helper.AllocateVertex(v3.X, -v3.Y, -v3.Z),
                                Vertex4 = Convert.ToInt16(quad ? helper.AllocateVertex(v4.X, -v4.Y, -v4.Z) : -1),
                                Attributes = new Attributes() { Flags = getAttributeFrom(face.matDef) },
                                Ground = face.matDef.ground,
                                FloorLevel = face.matDef.floorLevel,
                            },
                            inflate: 1
                        ));
                    }

                    if (collisionMesh.Collisions.Any())
                    {
                        coct.Complete(collisionMesh);

                        collisionNode.Meshes.Add(collisionMesh);
                    }
                }
            }

            coct.Nodes.Add(collisionNode);
            helper.CompleteBBox(collisionNode);
        }
    }
}

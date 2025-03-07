using Assimp;
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

//Alternate version of FlattenCollisionBuilder. 
//Allows specifying faces as their own Group Numbers so that ARDs can hide/show specific collision.
//Hackily implemented for now as an alternative option, because it can increase collision filesizes by ~1.5x due to each face becoming its own "mesh."
namespace OpenKh.Command.MapGen.Utils
{
    public class FlattenCollisionBuilderAlt : ICollisionBuilder
    {
        private readonly Coct coct = new Coct();
        private bool isValid = false;

        public CollisionBuilt GetBuilt() => new CollisionBuilt
        {
            Coct = coct,
            IsValid = isValid,
        };

        public FlattenCollisionBuilderAlt(
            ISpatialNodeCutter cutter,
            Func<MaterialDef, int> getAttributeFrom
        )
        {
            var faces = cutter.Faces;
            var helper = new Coct.BuildHelper(coct);

            var collisionNode = new CollisionNode
            {
                Meshes = new List<CollisionMesh>(),
            };

            if (faces.Any())
            {
                isValid = true;

                foreach (var face in faces)
                {
                    var matDef = face.matDef; // Access MaterialDef from the face

                    if (matDef == null)
                        continue; // Skip faces with no material definition

                    var group = matDef.group;

                    var collisionMesh = new CollisionMesh
                    {
                        Collisions = new List<Collision>(),
                        Group = group
                    };

                    if (face.positionList.Length >= 3)
                    {
                        var v1 = face.positionList[0];
                        var v2 = face.positionList[1];
                        var v3 = face.positionList[2];
                        var isQuad = face.positionList.Length == 4;

                        var collision = coct.Complete(
                            new Collision
                            {
                                Vertex1 = helper.AllocateVertex(v1.X, -v1.Y, -v1.Z),
                                Vertex2 = helper.AllocateVertex(v2.X, -v2.Y, -v2.Z),
                                Vertex3 = helper.AllocateVertex(v3.X, -v3.Y, -v3.Z),
                                Vertex4 = Convert.ToInt16(isQuad ? helper.AllocateVertex(face.positionList[3].X, -face.positionList[3].Y, -face.positionList[3].Z) : -1),
                                Attributes = new Attributes() { Flags = getAttributeFrom(matDef) },
                                Ground = matDef.ground,
                                FloorLevel = matDef.floorLevel,
                            },
                            inflate: 1
                        );

                        // Check for invalid plane (3 points are on the same line)
                        if (!float.IsNaN(collision.Plane.D))
                        {
                            collisionMesh.Collisions.Add(collision);
                        }
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

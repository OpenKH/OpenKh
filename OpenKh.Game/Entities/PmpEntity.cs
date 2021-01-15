using OpenKh.Bbs;
using System.IO;
using System.Collections.Generic;
using OpenKh.Engine;
using System.Numerics;

namespace OpenKh.Game.Entities
{
    public class PmpEntity : IEntity
    {
        public PmpEntity(int pmoIndex, Vector3 pmoPosition, Vector3 pmoRotation, Vector3 pmoScale)
        {
            Index = pmoIndex;
            Position = new Vector3(pmoPosition.X, pmoPosition.Y, pmoPosition.Z);
            Rotation = pmoRotation;
            Scaling = pmoScale;
        }

        public int Index { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scaling { get; set; }
    }
}

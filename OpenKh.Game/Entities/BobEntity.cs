using Microsoft.Xna.Framework;
using OpenKh.Kh2.Models;

namespace OpenKh.Game.Entities
{
    public class BobEntity : IEntity
    {
        public BobEntity(BobDescriptor bobDesc)
        {
            BobIndex = bobDesc.BobIndex;
            Position = new Vector3(bobDesc.PositionX, -bobDesc.PositionY, -bobDesc.PositionZ);
            Rotation = new Vector3(bobDesc.RotationX, bobDesc.RotationY, bobDesc.RotationZ);
            Scaling = new Vector3(bobDesc.ScalingX, bobDesc.ScalingY, bobDesc.ScalingZ);
        }

        public int BobIndex { get; set; }
        public Vector3 Position { get; set; }
        public Vector3 Rotation { get; set; }
        public Vector3 Scaling { get; set; }
    }
}

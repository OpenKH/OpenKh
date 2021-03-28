using OpenKh.Engine.Input;
using OpenKh.Game.Infrastructure;
using OpenKh.Kh2;
using System;
using System.Numerics;

namespace OpenKh.Game.Entities
{
    public class PlayerManager
    {
        public static void ProcessPlayer(IInput input, ObjectEntity entity, float rotation, double deltaTime)
        {
            const float Speed = 500f;
            var move = new Vector3(input.AxisLeft.X, 0, -input.AxisLeft.Y);

            if (move != Vector3.Zero)
            {
                var finalMove = Vector3.Transform(move * Speed, Matrix4x4.CreateRotationY(-rotation + (float)Math.PI));
                var angle = Math.Atan2(finalMove.X, finalMove.Z);
                entity.Position += new Vector3((float)(finalMove.X * deltaTime), 0, (float)(finalMove.Z * deltaTime));
                entity.Rotation = new Vector3(0, (float)angle, 0);

                var actualSpeed = move.Length();
                if (actualSpeed < 0.5)
                    entity.Motion.CurrentAnimationIndex = (int)MotionSet.MotionName.WALK;
                else
                    entity.Motion.CurrentAnimationIndex = (int)MotionSet.MotionName.RUN;
            }
            else
            {
                entity.Motion.CurrentAnimationIndex = (int)MotionSet.MotionName.IDLE;
            }
        }
    }
}

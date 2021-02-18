using OpenKh.Game.Infrastructure;
using OpenKh.Kh2;
using System;
using System.Numerics;

namespace OpenKh.Game.Entities
{
    public class PlayerManager
    {
        public static void ProcessPlayer(InputManager input, ObjectEntity entity, float rotation, double deltaTime)
        {
            const float Speed = 500f;
            bool isPressed = false;

            var move = Vector3.Zero;
            if (input.A)
            {
                move.X = -Speed;
                isPressed = true;
            }
            else if (input.D)
            {
                move.X = +Speed;
                isPressed = true;
            }

            if (input.W)
            {
                move.Z = -Speed;
                isPressed = true;
            }
            else if (input.S)
            {
                move.Z = +Speed;
                isPressed = true;
            }

            if (isPressed)
            {
                move = Vector3.Transform(move, Matrix4x4.CreateRotationY(-rotation + (float)Math.PI));
                var angle = Math.Atan2(move.X, move.Z);
                entity.Position += new Vector3((float)(move.X * deltaTime), 0, (float)(move.Z * deltaTime));
                entity.Rotation = new Vector3(0, (float)angle, 0);
                entity.Motion.CurrentAnimationIndex = (int)MotionSet.MotionName.RUN;
            }
            else
            {
                entity.Motion.CurrentAnimationIndex = (int)MotionSet.MotionName.IDLE;
            }
        }
    }
}

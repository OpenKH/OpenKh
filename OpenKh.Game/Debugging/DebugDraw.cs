using System;
using System.Collections.Generic;
using System.Text;
using System.Numerics;
using xna = Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenKh.Game.Debugging
{
    class DebugDraw
    {
        public static void DrawLine(GraphicsDevice graphics, Vector3 start, Vector3 end, Vector3 color)
        {
            DrawLine(graphics, new xna.Vector3(start.X, start.Y, start.Z), new xna.Vector3(end.X, end.Y, end.Z), new xna.Color(color.X, color.Y, color.Z));
        }

        public static void DrawLine(GraphicsDevice graphics, Vector3 start, Vector3 end, xna.Color color)
        {
            DrawLine(graphics, new xna.Vector3(start.X, start.Y, start.Z), new xna.Vector3(end.X, end.Y, end.Z), color);
        }

        public static void DrawLine(GraphicsDevice graphics, xna.Vector3 start, xna.Vector3 end, xna.Color color)
        {
            var prevState = graphics.DepthStencilState;
            graphics.DepthStencilState = DepthStencilState.None;
            graphics.DrawUserPrimitives(
                PrimitiveType.LineList,
                new VertexPositionColor[]
                {
                    new VertexPositionColor
                    {
                        Position = start,
                        Color = color
                    },
                    new VertexPositionColor
                    {
                        Position = end,
                        Color = color
                    }
                },
                0, 1
            );
            graphics.DepthStencilState = prevState;
        }
    }
}

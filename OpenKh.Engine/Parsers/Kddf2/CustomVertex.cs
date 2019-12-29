using OpenKh.Engine.Maths;
using System;
using System.Runtime.InteropServices;

namespace OpenKh.Engine.Parsers.Kddf2
{
    [Flags]
    public enum VertexFormat
    {
        Position = 1,
        Normal = 2,
        Diffuse = 4,
        Texture = 8,
    }

    public class CustomVertex
    {
        [StructLayout(LayoutKind.Sequential)]
        public struct PositionColoredTextured
        {
            public float X, Y, Z;
            public int Color;
            public float Tu, Tv;

            public PositionColoredTextured(Vector3 v, int clr, float tu, float tv)
            {
                X = v.X;
                Y = v.Y;
                Z = v.Z;
                Color = clr;
                Tu = tu;
                Tv = tv;
            }

            public PositionColoredTextured(float x, float y, float z, int clr, float tu, float tv)
            {
                X = x;
                Y = y;
                Z = z;
                Color = clr;
                Tu = tu;
                Tv = tv;
            }

            public Vector3 Position { get { return new Vector3(X, Y, Z); } }

            public static VertexFormat Format { get { return VertexFormat.Position | VertexFormat.Diffuse | VertexFormat.Texture; } }
            public static int Size { get { return Marshal.SizeOf(typeof(PositionColoredTextured)); } }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PositionColored
        {
            public float X, Y, Z;
            public int Color;

            public PositionColored(Vector3 v, int clr)
            {
                X = v.X;
                Y = v.Y;
                Z = v.Z;
                Color = clr;
            }

            public PositionColored(float x, float y, float z, int clr)
            {
                X = x;
                Y = y;
                Z = z;
                Color = clr;
            }

            public Vector3 Position { get { return new Vector3(X, Y, Z); } }

            public static VertexFormat Format { get { return VertexFormat.Position | VertexFormat.Diffuse; } }
            public static int Size { get { return Marshal.SizeOf(typeof(PositionColored)); } }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct Position
        {
            public float X, Y, Z;

            public Position(Vector3 v)
            {
                X = v.X;
                Y = v.Y;
                Z = v.Z;
            }

            public Position(float x, float y, float z)
            {
                X = x;
                Y = y;
                Z = z;
            }

            public static VertexFormat Format { get { return VertexFormat.Position; } }
            public static int Size { get { return Marshal.SizeOf(typeof(Position)); } }
        }

        [StructLayout(LayoutKind.Sequential)]
        public struct PositionNormalColored
        {
            public float X, Y, Z;
            public float Nx, Ny, Nz;
            public int Color;

            public PositionNormalColored(Vector3 v, Vector3 n, int c)
            {
                X = v.X;
                Y = v.Y;
                Z = v.Z;
                Nx = n.X;
                Ny = n.Y;
                Nz = n.Z;
                Color = c;
            }

            public PositionNormalColored(float x, float y, float z, float nx, float ny, float nz, int c)
            {
                X = x;
                Y = y;
                Z = z;
                Nx = nx;
                Ny = ny;
                Nz = nz;
                Color = c;
            }

            public static VertexFormat Format { get { return VertexFormat.Position | VertexFormat.Normal | VertexFormat.Diffuse; } }
            public static int Size { get { return Marshal.SizeOf(typeof(PositionNormalColored)); } }
        }
    }
}

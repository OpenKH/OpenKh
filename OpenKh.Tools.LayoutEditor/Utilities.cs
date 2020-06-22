using System.Numerics;

namespace OpenKh.Tools.LayoutEditor
{
    public static class Utilities
    {
        public static Vector4 ConvertColor(uint color) => new Vector4(
            ((color >> 0) & 0xFF) / 128.0f,
            ((color >> 8) & 0xFF) / 128.0f,
            ((color >> 16) & 0xFF) / 128.0f,
            ((color >> 24) & 0xFF) / 128.0f);

        public static uint ConvertColor(Vector4 color) =>
            ((uint)(color.X * 128f) << 0) |
            ((uint)(color.Y * 128f) << 8) |
            ((uint)(color.Z * 128f) << 16) |
            ((uint)(color.W * 128f) << 24);
    }
}

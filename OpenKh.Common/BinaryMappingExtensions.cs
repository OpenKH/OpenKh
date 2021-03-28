using System.Numerics;
using Xe.BinaryMapper;

namespace OpenKh.Common
{
    public static class BinaryMapperExtensions
    {
        public static MappingConfiguration ForTypeVector2(this MappingConfiguration cfg) =>
            cfg.ForType<Vector2>(x => x.Reader.BaseStream.ReadVector2(),
                x => x.Writer.BaseStream.Write((Vector2)x.Item));

        public static MappingConfiguration ForTypeVector3(this MappingConfiguration cfg) =>
            cfg.ForType<Vector3>(x => x.Reader.BaseStream.ReadVector3(),
                x => x.Writer.BaseStream.Write((Vector3)x.Item));

        public static MappingConfiguration ForTypeVector4(this MappingConfiguration cfg) =>
            cfg.ForType<Vector4>(x => x.Reader.BaseStream.ReadVector4(),
                x => x.Writer.BaseStream.Write((Vector4)x.Item));

        public static MappingConfiguration ForTypeMatrix4x4(this MappingConfiguration cfg) =>
            cfg.ForType<Matrix4x4>(x => x.Reader.BaseStream.ReadMatrix4x4(),
                x => x.Writer.BaseStream.Write((Matrix4x4)x.Item));
    }
}

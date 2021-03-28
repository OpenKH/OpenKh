using System.Numerics;

namespace OpenKh.Engine
{
    public interface IEntity
    {
        Vector3 Position { get; }
        Vector3 Rotation { get; }
        Vector3 Scaling { get; }
    }

    public static class EntityExtensions
    {
        public static Matrix4x4 GetMatrix(this IEntity entity) =>
            Matrix4x4.CreateRotationX(entity.Rotation.X) *
            Matrix4x4.CreateRotationY(entity.Rotation.Y) *
            Matrix4x4.CreateRotationZ(entity.Rotation.Z) *
            Matrix4x4.CreateScale(entity.Scaling.X, entity.Scaling.Y, entity.Scaling.Z) *
            Matrix4x4.CreateTranslation(entity.Position.X, entity.Position.Y, entity.Position.Z);
    }
}

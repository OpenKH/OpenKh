using Microsoft.Xna.Framework;

namespace OpenKh.Game.Entities
{
    public interface IEntity
    {
        Vector3 Position { get; }
        Vector3 Rotation { get; }
        Vector3 Scaling { get; }
    }

    public static class EntityExtensions
    {
        public static Matrix GetMatrix(this IEntity entity) =>
            Matrix.CreateRotationX(entity.Rotation.X) *
            Matrix.CreateRotationY(entity.Rotation.Y) *
            Matrix.CreateRotationZ(entity.Rotation.Z) *
            Matrix.CreateScale(entity.Scaling.X, entity.Scaling.Y, entity.Scaling.Z) *
            Matrix.CreateTranslation(entity.Position.X, entity.Position.Y, entity.Position.Z);
    }
}

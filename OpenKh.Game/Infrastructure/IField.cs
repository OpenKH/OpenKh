using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using System;

namespace OpenKh.Game.Infrastructure
{
    public interface IField
    {
        void Update(double deltaTime);

        void ForEveryModel(Action<IEntity, IMonoGameModel> action);
        void AddActor(int objectId);
        void RemoveAllActors();
    }
}

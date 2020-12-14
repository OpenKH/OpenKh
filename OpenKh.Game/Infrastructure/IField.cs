using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using System;
using System.Numerics;

namespace OpenKh.Game.Infrastructure
{
    public interface IField
    {
        void Update(double deltaTime);
        void Draw();

        void ForEveryModel(Action<IEntity, IMonoGameModel> action);
        void AddActor(int actorId, int objectId);
        void SetActorPosition(int actorId, float x, float y, float z, float rotation);
        void SetActorAnimation(int actorId, string path);
        void SetActorVisibility(int actorId, bool visibility);
        void RemoveAllActors();

        void SetCamera(Vector3 position, Vector3 lookAt, float fieldOfView, float roll);

        void FadeToBlack(float seconds);
        void FadeToWhite(float seconds);
        void FadeFromBlack(float seconds);
        void FadeFromWhite(float seconds);

        void ShowSubtitle(int subtitleId, ushort messageId);
        void HideSubtitle(int subtitleId);
    }
}

using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine;
using OpenKh.Engine.MonoGame;
using System.Collections.Generic;
using System.Numerics;

namespace OpenKh.Game.Field
{
    public interface IField
    {
        List<string> Events { get; }

        void Update(double deltaTime);
        void Draw();

        void PlayEvent(string eventName);

        void Render(Camera camera, KingdomShader shader, EffectPass pass, bool passRenderOpaque);
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

using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using OpenKh.Engine.Renders;
using System;
using System.Numerics;
using System.Runtime.CompilerServices;
using xna = Microsoft.Xna.Framework;

namespace OpenKh.Engine.MonoGame
{
    public class KingdomShader : IDisposable
    {
        private static Matrix4x4 MatrixIdentity = Matrix4x4.Identity;
        public static Vector2 DefaultTextureRegion = new Vector2(0, 1);

        private readonly EffectParameter _modelViewParameter;
        private readonly EffectParameter _worldViewParameter;
        private readonly EffectParameter _projectionViewParameter;
        private readonly EffectParameter _parameterTextureRegionU;
        private readonly EffectParameter _parameterTextureRegionV;
        private readonly EffectParameter _parameterTextureWrapModeU;
        private readonly EffectParameter _parameterTextureWrapModeV;
        private readonly EffectParameter _parameterTexture0;
        private readonly EffectParameter _parameterUseAlphaMask;

        public KingdomShader(ContentManager contentManager)
        {
            Effect = contentManager.Load<Effect>("KingdomShader");
            _modelViewParameter = Effect.Parameters["ModelView"];
            _worldViewParameter = Effect.Parameters["WorldView"];
            _projectionViewParameter = Effect.Parameters["ProjectionView"];
            _parameterTextureRegionU = Effect.Parameters["TextureRegionU"];
            _parameterTextureRegionV = Effect.Parameters["TextureRegionV"];
            _parameterTextureWrapModeU = Effect.Parameters["TextureWrapModeU"];
            _parameterTextureWrapModeV = Effect.Parameters["TextureWrapModeV"];
            _parameterTexture0 = Effect.Parameters["Texture0"];
            _parameterUseAlphaMask = Effect.Parameters["UseAlphaMask"];

            SetModelViewIdentity();
            SetWorldViewIdentity();
            SetProjectionViewIdentity();
            SetTextureRegionUDefault();
            SetTextureRegionVDefault();
            TextureWrapModeU = TextureWrapMode.Clamp;
            TextureWrapModeV = TextureWrapMode.Clamp;
        }

        public Effect Effect { get; }

        public void SetModelViewIdentity() =>
            _modelViewParameter.SetValue(Unsafe.As<Matrix4x4, xna.Matrix>(ref MatrixIdentity));

        public void SetModelView(Matrix4x4 matrix) =>
            _modelViewParameter.SetValue(Unsafe.As<Matrix4x4, xna.Matrix>(ref matrix));

        public void SetModelView(ref Matrix4x4 matrix) =>
            _modelViewParameter.SetValue(Unsafe.As<Matrix4x4, xna.Matrix>(ref matrix));

        public void SetWorldViewIdentity() =>
            _worldViewParameter.SetValue(Unsafe.As<Matrix4x4, xna.Matrix>(ref MatrixIdentity));

        public void SetWorldView(Matrix4x4 matrix) =>
            _worldViewParameter.SetValue(Unsafe.As<Matrix4x4, xna.Matrix>(ref matrix));

        public void SetWorldView(ref Matrix4x4 matrix) =>
            _worldViewParameter.SetValue(Unsafe.As<Matrix4x4, xna.Matrix>(ref matrix));

        public void SetProjectionViewIdentity() =>
            _projectionViewParameter.SetValue(Unsafe.As<Matrix4x4, xna.Matrix>(ref MatrixIdentity));

        public void SetProjectionView(Matrix4x4 matrix) =>
            _projectionViewParameter.SetValue(Unsafe.As<Matrix4x4, xna.Matrix>(ref matrix));

        public void SetProjectionView(ref Matrix4x4 matrix) =>
            _projectionViewParameter.SetValue(Unsafe.As<Matrix4x4, xna.Matrix>(ref matrix));

        public Texture2D Texture0
        {
            get => _parameterTexture0.GetValueTexture2D();
            set => _parameterTexture0.SetValue(value);
        }

        public void SetTextureRegionU(Vector2 vector) =>
            _parameterTextureRegionU.SetValue(Unsafe.As<Vector2, xna.Vector2>(ref vector));
        public void SetTextureRegionU(ref Vector2 vector) =>
            _parameterTextureRegionU.SetValue(Unsafe.As<Vector2, xna.Vector2>(ref vector));
        public void SetTextureRegionUDefault() =>
            _parameterTextureRegionU.SetValue(Unsafe.As<Vector2, xna.Vector2>(ref DefaultTextureRegion));

        public void SetTextureRegionV(Vector2 vector) =>
            _parameterTextureRegionV.SetValue(Unsafe.As<Vector2, xna.Vector2>(ref vector));
        public void SetTextureRegionV(ref Vector2 vector) =>
            _parameterTextureRegionV.SetValue(Unsafe.As<Vector2, xna.Vector2>(ref vector));
        public void SetTextureRegionVDefault() =>
            _parameterTextureRegionV.SetValue(Unsafe.As<Vector2, xna.Vector2>(ref DefaultTextureRegion));

        public TextureWrapMode TextureWrapModeU
        {
            get => (TextureWrapMode)_parameterTextureWrapModeU.GetValueInt32();
            set => _parameterTextureWrapModeU.SetValue((int)value);
        }

        public TextureWrapMode TextureWrapModeV
        {
            get => (TextureWrapMode)_parameterTextureWrapModeV.GetValueInt32();
            set => _parameterTextureWrapModeV.SetValue((int)value);
        }

        public bool UseAlphaMask
        {
            get => _parameterUseAlphaMask.GetValueBoolean();
            set => _parameterUseAlphaMask.SetValue(value);
        }

        public void Pass(Action<EffectPass> action)
        {
            foreach (var pass in Effect.CurrentTechnique.Passes)
                action(pass);
        }

        public void Dispose()
        {
            Effect?.Dispose();
        }
    }
}

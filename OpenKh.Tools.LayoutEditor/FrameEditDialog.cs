using ImGuiNET;
using OpenKh.Engine.Renders;
using OpenKh.Tools.LayoutEditor.Models;
using System;
using System.Numerics;

namespace OpenKh.Tools.LayoutEditor
{
    class FrameEditDialog
    {
        private readonly SpriteModel _spriteModel;
        private readonly ISpriteTexture _atlasTexture;
        private readonly IntPtr _textureId;

        public FrameEditDialog(SpriteModel spriteModel, ISpriteTexture atlasTexture, IntPtr atlasTextureId)
        {
            _spriteModel = spriteModel;
            _atlasTexture = atlasTexture;
            _textureId = atlasTextureId;
        }

        public void Run()
        {
            ImGui.Image(_textureId, new Vector2(_atlasTexture.Width, _atlasTexture.Height));

            var source = new int[]
            {
                _spriteModel.Sprite.Left, _spriteModel.Sprite.Top,
                _spriteModel.Sprite.Right, _spriteModel.Sprite.Bottom
            };
            if (ImGui.DragInt4("Source", ref source[0]))
            {
                _spriteModel.Sprite.Left = source[0];
                _spriteModel.Sprite.Top = source[1];
                _spriteModel.Sprite.Right = source[2];
                _spriteModel.Sprite.Bottom = source[3];
                _spriteModel.SizeChanged();
            }

            var colorTopLeft = Utilities.ConvertColor(_spriteModel.Sprite.ColorLeft);
            if (ImGui.ColorEdit4("Top left", ref colorTopLeft))
                _spriteModel.Sprite.ColorLeft = Utilities.ConvertColor(colorTopLeft);

            var colorTopRight = Utilities.ConvertColor(_spriteModel.Sprite.ColorTop);
            if (ImGui.ColorEdit4("Top right", ref colorTopRight))
                _spriteModel.Sprite.ColorTop = Utilities.ConvertColor(colorTopRight);

            var colorBottomLeft = Utilities.ConvertColor(_spriteModel.Sprite.ColorRight);
            if (ImGui.ColorEdit4("Bottom left", ref colorBottomLeft))
                _spriteModel.Sprite.ColorRight = Utilities.ConvertColor(colorBottomLeft);

            var colorBottomRight = Utilities.ConvertColor(_spriteModel.Sprite.ColorBottom);
            if (ImGui.ColorEdit4("Bottom right", ref colorBottomRight))
                _spriteModel.Sprite.ColorBottom = Utilities.ConvertColor(colorBottomRight);

            var uvAnim = new Vector2(_spriteModel.Sprite.UTranslation, _spriteModel.Sprite.VTranslation);
            if (ImGui.DragFloat2("UV animation", ref uvAnim, 0.0001f))
            {
                _spriteModel.Sprite.UTranslation = uvAnim.X;
                _spriteModel.Sprite.VTranslation = uvAnim.Y;
            }

            var unknown00 = _spriteModel.Sprite.Unknown00;
            if (ImGui.DragInt("Unknown00", ref unknown00))
                _spriteModel.Sprite.Unknown00 = unknown00;

            _spriteModel.Draw(0, 0);
            ImGui.Image(_spriteModel.TextureId, SuggestSpriteSize());

            if (ImGui.Button("Close"))
                ImGui.CloseCurrentPopup();
        }

        private Vector2 SuggestSpriteSize()
        {
            const int MaxZoomLevel = 8;
            var zoomLevelX = _atlasTexture.Width / _spriteModel.Width;
            var zoomLevelY = _atlasTexture.Height / _spriteModel.Height;
            var zoomLevel = Math.Max(1, Math.Min(MaxZoomLevel, Math.Min(zoomLevelX, zoomLevelY)));

            return new Vector2(_spriteModel.Width * zoomLevel, _spriteModel.Height * zoomLevel);
        }
    }
}

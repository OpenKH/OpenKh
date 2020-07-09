using ImGuiNET;
using OpenKh.Engine.Extensions;
using OpenKh.Engine.Renders;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using OpenKh.Tools.LayoutEditor.Interfaces;
using OpenKh.Tools.LayoutEditor.Models;
using System;
using System.Numerics;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;
using System.Linq;
using System.Collections.Generic;

namespace OpenKh.Tools.LayoutEditor.Dialogs
{
    class SpriteGroupEditDialog : IDisposable
    {
        private readonly List<SpriteGroupModel> _spriteGroupModels;
        private readonly ISpriteDrawing _spriteDrawing;
        private readonly ISpriteTexture _atlasTexture;
        private readonly ITextureBinder _textureBinder;
        private readonly IEditorSettings _settings;
        private bool _isPivotVisible = true;

        private int _selectedSpriteGroupModel;
        private SpriteGroupModel SpriteGroupModel => _spriteGroupModels[_selectedSpriteGroupModel];

        public SpriteGroupEditDialog(
            List<SpriteGroupModel> spriteGroupModels,
            int selectedSpriteGroupModel,
            ISpriteDrawing spriteDrawing,
            ISpriteTexture atlasTexture,
            ITextureBinder textureBinder,
            IEditorSettings settings)
        {
            _spriteGroupModels = spriteGroupModels;
            _selectedSpriteGroupModel = selectedSpriteGroupModel;
            _spriteDrawing = spriteDrawing;
            _atlasTexture = atlasTexture;
            _textureBinder = textureBinder;
            _settings = settings;
        }

        public void Dispose()
        {
        }

        public void Run()
        {
            const float PreviewWidthMul = 1f;
            const float PreviewWidthMax = 512f;
            const float EditorWidthMul = 0.5f;
            const float EditorWidthMax = 256f;
            const float TotalWidthMul = PreviewWidthMul + EditorWidthMul;

            var windowSize = ImGui.GetIO().DisplaySize.X;
            var previewWidth = Math.Min(windowSize / TotalWidthMul * PreviewWidthMul, PreviewWidthMax);
            var editorWidth = Math.Min(windowSize / TotalWidthMul * EditorWidthMul, EditorWidthMax);

            ForChild(nameof(SpriteGroupPreview), previewWidth, 512, true, SpriteGroupPreview);
            ImGui.SameLine();
            ForChild(nameof(SptiteGroupEditor), editorWidth, 512, false, SptiteGroupEditor);
        }

        private void SpriteGroupPreview()
        {
            SpriteGroupModel.Draw(0, 0, PreDraw, PostDraw);
            ImGui.Image(SpriteGroupModel.TextureId, SuggestSpriteSize());
        }

        private void PreDraw(ISpriteDrawing drawing)
        {
            DrawCenter(drawing, 1.0f);
        }

        private void PostDraw(ISpriteDrawing drawing)
        {
            DrawCenter(drawing, 0.333f);

            //var backgroundColorInverse = new ColorF(
            //    1f - _settings.EditorBackground.R,
            //    1f - _settings.EditorBackground.G,
            //    1f,
            //    1f);
            //drawing.FillRectangle(50, 0, 100, 100, backgroundColorInverse);
            //drawing.Flush();
        }

        private void SptiteGroupEditor()
        {
            if (ImGui.InputInt("Selected", ref _selectedSpriteGroupModel))
                _selectedSpriteGroupModel = Math.Min(Math.Max(_selectedSpriteGroupModel, 0), _spriteGroupModels.Count - 1);

            ImGui.Checkbox("Show pivot", ref _isPivotVisible);

            var origin = GetOrigin(SpriteGroupModel);
            var originValues = new int[] { origin.X, origin.Y };
            if (ImGui.DragInt2("Pivot", ref originValues[0]))
            {
                var diffX = originValues[0] - origin.X;
                var diffY = originValues[1] - origin.Y;
                foreach (var item in SpriteGroupModel.SpriteGroup)
                {
                    item.Left -= diffX;
                    item.Right -= diffX;
                    item.Top -= diffY;
                    item.Bottom -= diffY;
                }
            }

            var size = SpriteGroupModel.SpriteGroup.GetVisibilityRectangleForFrameGroup();
            ImGui.Text($"Width: {size.Width}, Heigth: {size.Height}");

            for (var i = 0; i < SpriteGroupModel.SpriteGroup.Count; i++)
            {
                if (ImGui.CollapsingHeader($"Sprite part {i + 1}"))
                {
                    SpritePartEdit(SpriteGroupModel.SpriteGroup[i], i);
                }
            }
        }

        private void SpritePartEdit(Sequence.SpritePart spritePart, int index)
        {
            var position = new int[]
            {
                spritePart.Left,
                spritePart.Top
            };
            var size = new int[]
            {
                spritePart.Right - spritePart.Left,
                spritePart.Bottom - spritePart.Top
            };

            if (ImGui.DragInt2($"Position##{index}", ref position[0]))
            {
                spritePart.Left = position[0];
                spritePart.Top = position[1];
                spritePart.Right = position[0] + size[0];
                spritePart.Bottom = position[1] + size[1];
                SpriteGroupModel.SizeChanged();
            }
            if (ImGui.DragInt2($"Size##{index}", ref size[0]))
            {
                spritePart.Right = position[0] + size[0];
                spritePart.Bottom = position[1] + size[1];
                SpriteGroupModel.SizeChanged();
            }
        }

        private Vector2 SuggestSpriteSize()
        {
            const int MaxZoomLevel = 8;
            var zoomLevelX = 512f / SpriteGroupModel.Width;
            var zoomLevelY = 512f / SpriteGroupModel.Height;
            var zoomLevel = Math.Max(1, Math.Min(MaxZoomLevel, Math.Min(zoomLevelX, zoomLevelY)));

            return new Vector2(SpriteGroupModel.Width * zoomLevel, SpriteGroupModel.Height * zoomLevel);
        }

        private void DrawCenter(ISpriteDrawing drawing, float alpha)
        {
            if (!_isPivotVisible)
                return;

            const float Infinite = 65535f;
            var origin = GetOrigin(SpriteGroupModel);
            var backgroundColorInverse = new ColorF(
                1f - _settings.EditorBackground.R,
                1f - _settings.EditorBackground.G,
                1f - _settings.EditorBackground.B,
                alpha);

            drawing.FillRectangle(origin.X, 0, 1, Infinite, backgroundColorInverse);
            drawing.FillRectangle(0, origin.Y, Infinite, 1, backgroundColorInverse);
            drawing.Flush();
        }

        private static (int X, int Y) GetOrigin(SpriteGroupModel spriteGroupModel) =>
            (-spriteGroupModel.SpriteGroup.Min(x => Math.Min(x.Left, x.Right)),
             -spriteGroupModel.SpriteGroup.Min(x => Math.Min(x.Top, x.Bottom)));
    }
}

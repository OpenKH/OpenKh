using ImGuiNET;
using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class BigOneSelectorPopupUsecase
    {
        private readonly LayoutOnMultiColumnsUsecase _layoutOnMultiColumnsUsecase;

        public BigOneSelectorPopupUsecase(
            LayoutOnMultiColumnsUsecase layoutOnMultiColumnsUsecase
        )
        {
            _layoutOnMultiColumnsUsecase = layoutOnMultiColumnsUsecase;
        }

        public Action<IReadOnlyList<ItemType>, Action<ItemType, int>> Popup<ItemType>(string name, float columnWidth)
        {
            return (list, onRender) =>
            {
                var visible = true;

                if (ImGui.BeginPopupModal(name, ref visible,
                    ImGuiWindowFlags.Popup | ImGuiWindowFlags.Modal))
                {
                    var layout = _layoutOnMultiColumnsUsecase.Layout(
                        ImGui.GetWindowSize(),
                        columnWidth,
                        ImGui.GetTextLineHeightWithSpacing(),
                        list.Count()
                    );

                    ImGui.Columns(layout.NumColumns);

                    foreach (var cell in layout.Cells)
                    {
                        var index = cell.Index;

                        onRender(list[index], index);

                        ImGui.NextColumn();
                    }

                    ImGui.EndPopup();
                }
            };
        }
    }
}

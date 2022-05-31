using ImGuiNET;
using OpenKh.Tools.BbsMapStudio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.BbsMapStudio.Windows
{
    class MeshGroupWindow
    {
        public static bool Run(List<MeshGroupModel> meshGroups) => ForHeader("Mesh groups", () =>
        {
            for (var i = 0; i < meshGroups.Count; i++)
            {
                MeshGroupModel meshGroup = meshGroups[i];
                ForTreeNode(meshGroup.Name, () => MeshGroup(meshGroup, i));
            }
        });

        private static void MeshGroup(MeshGroupModel meshGroup, int index)
        {
            if (ImGui.SmallButton("Apply changes"))
                meshGroup.Invalidate();

            
        }
    }
}

using ImGuiNET;
using OpenKh.Tools.Kh2MapStudio.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MapStudio.Windows
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

            ForEdit("Unk04", () => meshGroup.Map.unk04, x => meshGroup.Map.unk04 = x);
            ForEdit("Unk08", () => meshGroup.Map.unk08, x => meshGroup.Map.unk08 = x);
            ForEdit("VA4", () => meshGroup.Map.va4, x => meshGroup.Map.va4 = x);

            for (var i = 0; i < meshGroup.Map.vifPacketRenderingGroup.Count; i++)
            {
                ForTreeNode($"Mesh Rendering Group {i}##{index}", () =>
                {
                    var group = meshGroup.Map.vifPacketRenderingGroup[i];
                    for (var j = 0; j < group.Length; j++)
                    {
                        var meshIndex = group[j];
                        ForTreeNode($"Index {j}, Mesh {meshIndex}##{index}", () =>
                        {
                            var vifPacket = meshGroup.Map.VifPackets[meshIndex];
                            ForEdit("Texture", () => vifPacket.TextureId, x =>
                            {
                                vifPacket.TextureId = Math.Min(Math.Max(x, 0), meshGroup.Texture.Count - 1);
                            });
                            ForEdit("Unk08", () => vifPacket.Unk08, x => vifPacket.Unk08 = x);
                            ForEdit("Unk0c", () => vifPacket.Unk0c, x => vifPacket.Unk0c = x);
                            ForEdit("Alpha flag", () => vifPacket.IsTransparentFlag, x => vifPacket.IsTransparentFlag = x);
                            ImGui.Text("DMA per VIF dump:");
                            ImGui.Text(string.Join(",", vifPacket.DmaPerVif.Select(x => $"{x}")));
                        });
                    }
                });
            }

            for (var i = 0; i < meshGroup.Map.VifPackets.Count; i++)
            {
            }
        }
    }
}

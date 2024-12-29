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

            ForEdit("Is sky box",
                () => meshGroup.Map.BgType == Kh2.ModelBackground.BackgroundType.Skybox,
                x => meshGroup.Map.BgType = x ? Kh2.ModelBackground.BackgroundType.Skybox : Kh2.ModelBackground.BackgroundType.Field);
            ForEdit("Attribute", () => meshGroup.Map.Attribute, x => meshGroup.Map.Attribute = x);
            ForEdit("Shadow count", () => meshGroup.Map.ShadowCount, x => { });
            ForEdit("Texture count", () => meshGroup.Map.TextureCount, x => { });
            ForEdit("Octaltree count", () => meshGroup.Map.OctalTreeCount, x => { });

            for (var i = 0; i < meshGroup.Map.vifPacketRenderingGroup.Count; i++)
            {
                ForTreeNode($"Mesh Rendering Group {i}##{index}", () =>
                {
                    // if (ImGui.SmallButton("Apply changes")) //Adds the button ONLY for the entire group.
                    // meshGroup.Invalidate();
                    var group = meshGroup.Map.vifPacketRenderingGroup[i];
                    for (var j = 0; j < group.Length; j++)
                    {
                        //if (ImGui.SmallButton("Apply changes")) //Adds the button for EACH group.
                        //    meshGroup.Invalidate();
                        var meshIndex = group[j];
                        ForTreeNode($"Index {j}, Mesh {meshIndex}##{index}", () =>
                        {
                            if (ImGui.SmallButton("Apply changes")) //Adds the button for EACH group.
                                meshGroup.Invalidate();
                            var vifPacket = meshGroup.Map.Chunks[meshIndex];
                            ForEdit("Texture",
                                () => vifPacket.TextureId,
                                x => vifPacket.TextureId = (short)Math.Min(Math.Max(x, 0), meshGroup.Texture.Count - 1));
                            ForEdit("Is transparent",
                                () => vifPacket.TransparencyFlag > 0,
                                x => vifPacket.TransparencyFlag = (short)(x ? 1 : 0));
                            ForEdit("Is specular", () => vifPacket.IsSpecular, x => vifPacket.IsSpecular = x);
                            ForEdit("Has vertex buffer", () => vifPacket.HasVertexBuffer, x => vifPacket.HasVertexBuffer = x);
                            ForEdit("Alpha blend", () => vifPacket.IsAlpha, x => vifPacket.IsAlpha = x);
                            ForEdit("Alpha subtract", () => vifPacket.IsAlphaSubtract, x => vifPacket.IsAlphaSubtract = x);
                            ForEdit("Alpha add", () => vifPacket.IsAlphaAdd, x => vifPacket.IsAlphaAdd = x);
                            ForEdit("Hide shadow", () => vifPacket.IsShadowOff, x => vifPacket.IsShadowOff = x);
                            ForEdit("Is phase", () => vifPacket.IsPhase, x => vifPacket.IsPhase = x);
                            ForEdit("Is multi", () => vifPacket.IsMulti, x => vifPacket.IsMulti = x);
                            ForEdit("Priority", () => vifPacket.Priority, x => vifPacket.Priority = x);
                            ForEdit("Draw priority", () => vifPacket.DrawPriority, x => vifPacket.DrawPriority = x);
                            ForEdit("Alpha flag", () => vifPacket.TransparencyFlag, x => vifPacket.TransparencyFlag = x);
                            ForEdit("UV Scroll Index", () => vifPacket.UVScrollIndex, x => vifPacket.UVScrollIndex = x);
                            ImGui.Text("DMA per VIF dump:");
                            ImGui.Text(string.Join(",", vifPacket.DmaPerVif.Select(x => $"{x}")));
                        });
                    }
                });
            }

            for (var i = 0; i < meshGroup.Map.Chunks.Count; i++)
            {
            }
        }
    }
}

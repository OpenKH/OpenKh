using ImGuiNET;
using OpenKh.Kh2;
using System.Windows.Controls;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MapStudio.Windows
{
    static class CollisionWindow
    {
        public static void Run(Coct coct) => ForWindow("Collision", () =>
        {
            Node(coct, 0);
        });

        private static void Node(Coct coct, int index)
        {
            if (index == -1) return;
            ForTreeNode($"Node {index}", () =>
            {
                var node = coct.CollisionMeshGroupList[index];
                ImGui.Text($"Box {node.BoundingBox}");
                Node(coct, node.Child1);
                Node(coct, node.Child2);
                Node(coct, node.Child3);
                Node(coct, node.Child4);
                Node(coct, node.Child5);
                Node(coct, node.Child6);
                Node(coct, node.Child7);
                Node(coct, node.Child8);

                for (int i = node.CollisionMeshStart; i < node.CollisionMeshEnd; i++)
                {
                    ForTreeNode($"Mesh {i}", () =>
                    {
                        var mesh = coct.CollisionMeshList[i];
                        ImGui.Text($"Box {mesh.BoundingBox}");
                        ImGui.Text($"Unk10 {mesh.v10}");
                        ImGui.Text($"Unk12 {mesh.v12}");

                        for (int j = mesh.CollisionStart; j < mesh.CollisionEnd; j++)
                        {
                            ForTreeNode($"Collision {j}", () =>
                            {
                                var collision = coct.CollisionList[j];
                                ImGui.Text($"Unk00 {collision.v00}");
                                ImGui.Text($"Plane {collision.PlaneIndex}");
                                ImGui.Text($"Bound Box {collision.BoundingBoxIndex}");
                                ImGui.Text($"Flags {collision.SurfaceFlagsIndex}");

                                var plane = coct.PlaneList[collision.PlaneIndex];
                                var bbox = coct.BoundingBoxList[collision.BoundingBoxIndex];
                                var flags = coct.SurfaceFlagsList[collision.SurfaceFlagsIndex];
                            });
                        }
                    });
                }
            });
        }
    }
}

using ImGuiNET;
using OpenKh.Kh2;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MapStudio.Windows
{
    static class CollisionWindow
    {
        public static void Run(Coct coct) => ForHeader("Collision", () =>
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

                foreach (var mesh in node.Meshes)
                {
                    ForTreeNode($"Mesh {mesh.GetHashCode()}", () =>
                    {
                        ImGui.Text($"Box {mesh.BoundingBox}");
                        ImGui.Text($"Unk10 {mesh.v10}");
                        ImGui.Text($"Unk12 {mesh.v12}");

                        foreach (var collision in mesh.Collisions)
                        {
                            ForTreeNode($"Collision {collision.GetHashCode()}", () =>
                            {
                                ImGui.Text($"Unk00 {collision.v00}");
                                ImGui.Text($"Plane {collision.Plane}");
                                ImGui.Text($"Bound Box {collision.BoundingBox}");
                                ImGui.Text($"Flags {collision.SurfaceFlags:X08}");
                            });
                        }
                    });
                }
            });
        }
    }
}

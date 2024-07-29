using ImGuiNET;
using OpenKh.Engine.MonoGame;
using OpenKh.Kh2;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using OpenKh.Kh2.Utils;
using System.Numerics;

//Partially implemented. Allows you to view the data, 
namespace OpenKh.Tools.Kh2MapStudio.Windows
{
    class CollisionWindow
    {
        public static void Run(Coct coct) => ForHeader("Collision", () =>
        {
            Node(coct, 0);
        });

        private static void Node(Coct coct, int index)
        {
            if (index == -1)
                return;
            ForTreeNode($"Node {index}", () =>
            {
                var node = coct.Nodes[index];
                //ImGui.Text($"Box {node.BoundingBox}");
                BoundingBoxInt16 boundingBoxCopy = node.BoundingBox;
                ImGuiCollHelper.EditBoundingBox("Bounding Box (Node)", ref boundingBoxCopy);
                node.BoundingBox = boundingBoxCopy;  // Set the modified bounding box back
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
                        ForEdit("Visibility", () => mesh.Visibility, x => mesh.Visibility = x);
                        ForEdit("Group", () => mesh.Group, x => mesh.Group = x);
                        BoundingBoxInt16 boundingBoxCopy = mesh.BoundingBox;
                        ImGuiCollHelper.EditBoundingBox("Bounding Box (Mesh)", ref boundingBoxCopy);
                        mesh.BoundingBox = boundingBoxCopy;  // Set the modified bounding box back
                        //ImGui.Text($"Box {mesh.BoundingBox}");
                        //ImGui.Text($"Visibility {mesh.Visibility}");
                        //ImGui.Text($"Group {mesh.Group}");
                        foreach (var collision in mesh.Collisions)
                        {
                            ForTreeNode($"Collision {collision.GetHashCode()}", () =>
                            {
                                ForEdit("Ground", () => collision.Ground, x => collision.Ground = x);
                                ForEdit("Floor Level", () => collision.FloorLevel, x => collision.FloorLevel = x);
                                //ImGui.Text($"Plane {collision.Plane}");
                                //ImGui.Text($"Bound Box {collision.BoundingBox}");
                                ForEdit("Flags", () => collision.Attributes.Flags, x => collision.Attributes.Flags = x);
                                BoundingBoxInt16 boundingBoxCopy = collision.BoundingBox;
                                ImGuiCollHelper.EditBoundingBox("Bounding Box (Collision)", ref boundingBoxCopy);
                                collision.BoundingBox = boundingBoxCopy;  // Set the modified bounding box back
                                                                          //ForEdit("UV Scroll Index", () => vifPacket.UVScrollIndex, x => vifPacket.UVScrollIndex = x);
                                Plane plane = collision.Plane;
                                ImGuiCollHelper.EditPlane(ref plane);
                                collision.Plane = plane; // Update the collision plane if it was changed
                                //ImGui.Text($"Flags {collision.Attributes:X08}");
                            });
                        }
                    });
                }
            });
        }
        //Helper for Bounding Box
        public static class ImGuiCollHelper
        {
            public static void EditBoundingBox(string label, ref BoundingBoxInt16 boundingBox)
            {
                if (ImGui.TreeNode(label))
                {
                    Vector3Int16 min = boundingBox.Minimum;
                    Vector3Int16 max = boundingBox.Maximum;

                    EditVector3Int16("Minimum", ref min);
                    EditVector3Int16("Maximum", ref max);

                    boundingBox = new BoundingBoxInt16(min, max);

                    ImGui.TreePop();
                }
            }

            private static void EditVector3Int16(string label, ref Vector3Int16 vector)
            {
                int x = vector.X;
                int y = vector.Y;
                int z = vector.Z;

                if (ImGui.DragInt(label + " X", ref x))
                    vector.X = (short)x;
                if (ImGui.DragInt(label + " Y", ref y))
                    vector.Y = (short)y;
                if (ImGui.DragInt(label + " Z", ref z))
                    vector.Z = (short)z;
            }
        
        public static void EditPlane(ref Plane plane)
            {
                Vector3 normal = plane.Normal;
                float d = plane.D;
                if (ImGui.TreeNode("Plane"))
                {
                    //ImGui.Text("Plane:");
                    ImGui.DragFloat("X", ref normal.X);
                    ImGui.DragFloat("Y", ref normal.Y);
                    ImGui.DragFloat("Z", ref normal.Z);
                    ImGui.DragFloat("D", ref d);
                    plane = new Plane(normal, d);
                    ImGui.TreePop();
                }
            }
        }
    }
}

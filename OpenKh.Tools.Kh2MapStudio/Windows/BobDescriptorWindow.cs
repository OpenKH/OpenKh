using ImGuiNET;
using Microsoft.Xna.Framework;
using OpenKh.Kh2.Models;
using System;
using System.Collections.Generic;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;
using static OpenKh.Tools.Kh2MapStudio.ImGuiExHelpers;

namespace OpenKh.Tools.Kh2MapStudio.Windows
{
    public class BobDescriptorWindow
    {
        public static bool Run(List<BobDescriptor> bobDescs, int bobCount) => ForHeader("BOB descriptors", () =>
        {
            var bobToRemove = -1;
            for (int i = 0; i < bobDescs.Count; i++)
            {
                var desc = bobDescs[i];
                if (ImGui.CollapsingHeader($"BOB descriptor##{i}"))
                {
                    if (ImGui.BeginCombo("BOB model", bobCount > 0 ?
                        $"BOB #{desc.BobIndex}" : "No bob exists in this map"))
                    {
                        for (var j = 0; j < bobCount; j++)
                        {
                            if (ImGui.Selectable($"BOB #{j}", j == desc.BobIndex))
                                desc.BobIndex = j;
                        }
                        ImGui.EndCombo();
                    }

                    ForEdit3("Position",
                        () => new Vector3(desc.PositionX, desc.PositionY, desc.PositionZ),
                        x =>
                        {
                            desc.PositionX = x.X;
                            desc.PositionY = x.Y;
                            desc.PositionZ = x.Z;
                        }, 10f);
                    ForEdit3("Rotation",
                        () => new Vector3(
                            (float)(desc.RotationX * 180.0 / Math.PI),
                            (float)(desc.RotationY * 180.0 / Math.PI),
                            (float)(desc.RotationZ * 180.0 / Math.PI)),
                        x =>
                        {
                            desc.RotationX = (float)(x.X * Math.PI / 180.0);
                            desc.RotationY = (float)(x.Y * Math.PI / 180.0);
                            desc.RotationZ = (float)(x.Z * Math.PI / 180.0);
                        });
                    ForEdit3("Scaling",
                        () => new Vector3(desc.ScalingX, desc.ScalingY, desc.ScalingZ),
                        x =>
                        {
                            desc.ScalingX = x.X;
                            desc.ScalingY = x.Y;
                            desc.ScalingZ = x.Z;
                        }, 0.01f);

                    ForEdit("Unk28", () => desc.Unknown28, x => desc.Unknown28 = x);
                    ForEdit("Unk2c", () => desc.Unknown2c, x => desc.Unknown2c = x);
                    ForEdit("Unk30", () => desc.Unknown30, x => desc.Unknown30 = x);
                    ForEdit("Unk34", () => desc.Unknown34, x => desc.Unknown34 = x);
                    ForEdit("Unk38", () => desc.Unknown38, x => desc.Unknown38 = x);
                    ForEdit("Unk3c", () => desc.Unknown3c, x => desc.Unknown3c = x);
                    ForEdit("Unk40", () => desc.Unknown40, x => desc.Unknown40 = x);
                    ForEdit("Unk44", () => desc.Unknown44, x => desc.Unknown44 = x);
                    ForEdit("Unk48", () => desc.Unknown48, x => desc.Unknown48 = x);
                    ForEdit("Unk4c", () => desc.Unknown4c, x => desc.Unknown4c = x);
                    ForEdit("Unk50", () => desc.Unknown50, x => desc.Unknown50 = x);
                    ForEdit("Unk54", () => desc.Unknown54, x => desc.Unknown54 = x);
                    ForEdit("Unk58", () => desc.Unknown58, x => desc.Unknown58 = x);
                    ForEdit("Unk5c", () => desc.Unknown5c, x => desc.Unknown5c = x);
                    ForEdit("Unk60", () => desc.Unknown60, x => desc.Unknown60 = x);
                    ForEdit("Unk64", () => desc.Unknown64, x => desc.Unknown64 = x);

                    if (ImGui.SmallButton("Remove this BOB descriptor"))
                        bobToRemove = i;
                }
            }

            if (bobToRemove >= 0)
                bobDescs.RemoveAt(bobToRemove);

            if (ImGui.SmallButton("Add a new BOB descriptor"))
                bobDescs.Add(new BobDescriptor());
        });
    }
}

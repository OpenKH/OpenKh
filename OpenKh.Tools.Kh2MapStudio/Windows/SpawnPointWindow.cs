using ImGuiNET;
using OpenKh.Tools.Kh2MapStudio.Interfaces;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MapStudio.Windows
{
    static class SpawnPointWindow
    {
        public static void Run(ISpawnPointController ctrl) => ForWindow("Spawn point editor", () =>
        {
            if (ImGui.BeginCombo("Spawn point", ctrl.SelectSpawnPoint))
            {
                foreach (var spawnPoint in ctrl.SpawnPoints)
                {
                    if (ImGui.Selectable(spawnPoint.Name, spawnPoint.Name == ctrl.SelectSpawnPoint))
                    {
                        ctrl.SelectSpawnPoint = spawnPoint.Name;
                    }
                }

                ImGui.EndCombo();
            }
        });
    }
}

using OpenKh.Tools.Kh2MapStudio.Interfaces;
using static OpenKh.Tools.Common.CustomImGui.ImGuiEx;

namespace OpenKh.Tools.Kh2MapStudio.Windows
{
    static class LayerControllerWindow
    {
        public static void Run(ILayerController layerController) => ForWindow("Layer control", () =>
        {
            if (layerController.ShowMap.HasValue)
                ForEdit("Show MAP", () => layerController.ShowMap.Value, x => layerController.ShowMap = x);
            if (layerController.ShowSk0.HasValue)
                ForEdit("Show SK0", () => layerController.ShowSk0.Value, x => layerController.ShowSk0 = x);
            if (layerController.ShowSk1.HasValue)
                ForEdit("Show SK1", () => layerController.ShowSk1.Value, x => layerController.ShowSk1 = x);
            if (layerController.ShowBobs.HasValue)
                ForEdit("Show BOBs", () => layerController.ShowBobs.Value, x => layerController.ShowBobs = x);
            if (layerController.ShowMapCollision.HasValue)
                ForEdit("Show map collisions", () => layerController.ShowMapCollision.Value, x => layerController.ShowMapCollision = x);
        });
    }
}

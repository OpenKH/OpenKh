namespace OpenKh.Tools.BbsMapStudio.Interfaces
{
    interface ILayerController
    {
        bool? ShowMap { get; set; }
        bool? ShowSk0 { get; set; }
        bool? ShowMapCollision { get; set; }
        bool? ShowCameraCollision { get; set; }
        bool? ShowLightCollision { get; set; }
    }
}

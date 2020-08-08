namespace OpenKh.Tools.Kh2MapStudio.Interfaces
{
    interface ILayerController
    {
        bool? ShowMap { get; set; }
        bool? ShowSk0 { get; set; }
        bool? ShowSk1 { get; set; }
        bool? ShowBobs { get; set; }
        bool? ShowMapCollision { get; set; }
        bool? ShowCameraCollision { get; set; }
        bool? ShowLightCollision { get; set; }
    }
}

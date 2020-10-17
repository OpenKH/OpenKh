namespace OpenKh.Imaging
{
    public interface IImageRead : IImage
    {
        /// <summary>
        /// Get pixel data
        /// </summary>
        /// <remarks>
        /// PixelFormat == Rgba8888
        /// - 4 bytes [B, G, R, A] per pixel.
        /// </remarks>
        byte[] GetData();

        byte[] GetClut();
    }
}

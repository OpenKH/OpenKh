namespace OpenKh.Imaging
{
    public interface IImageRead : IImage
    {
        /// <summary>
        /// Get pixel data
        /// </summary>
        /// <remarks>
        /// Always byte aligned.
        /// </remarks>
        byte[] GetData();

        /// <summary>
        /// Get color lookup table
        /// </summary>
        /// <remarks>
        /// - 4 bytes [R, G, B, A] per entry.
        /// </remarks>
        byte[] GetClut();
    }
}

namespace OpenKh.Imaging
{
    public interface IImageRead : IImage
    {
        /// <summary>
        /// Get pixels
        /// </summary>
        /// <returns>
        /// Return bitmap pixels in these orders.
        /// - `Indexed4`: The first pixel is high byte. The second pixel is low byte.
        /// - `Indexed8`: No conversion, one byte is one pixel.
        /// - `Rgba8888`: `BB GG RR AA` (same as Format32bppArgb)
        /// </returns>
        byte[] GetData();

        /// <summary>
        /// Get color look at table in this order: `RR GG BB AA`, for `Indexed4` or `Indexed8` images.
        /// `AA` uses normal alpha range (0 to 255).
        /// </summary>
        byte[] GetClut();
    }
}

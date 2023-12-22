namespace OpenKh.Imaging
{
    public enum PixelFormat
    {
        Undefined,
        /// <summary>
        /// 4 bpp (16 colors palette) images
        /// </summary>
        Indexed4,
        /// <summary>
        /// 8 bpp (256 colors palette) image
        /// </summary>
        Indexed8,
        /// <summary>
        /// 2 bytes per pixel. 16 bits format is ARRRRRGGGGGBBBBB. A is most significant bit. Same as `PixelFormat.Format16bppArgb1555`
        /// </summary>
        /// <remarks>
        /// Do not use Rgba1555. Not tested well.
        /// </remarks>
        Rgba1555,
        /// <summary>
        /// 3 bytes `BB GG RR` per pixel. Same as `PixelFormat.Format24bppRgb`
        /// </summary>
        Rgb888,
        /// <summary>
        /// 4 bytes `BB GG RR xx` per pixel. xx component is unused. Same as `PixelFormat.Format32bppRgb`
        /// </summary>
        Rgbx8888,
        /// <summary>
        /// 4 bytes `BB GG RR AA` per pixel. Same as `PixelFormat.Format32bppPArgb`
        /// </summary>
        Rgba8888
    }
}

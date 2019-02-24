using System.Drawing;

namespace kh.kh2
{
	public enum ImageFormat
	{
		Undefined,
		Indexed4,
		Indexed8,
		Rgba1555,
		Rgb888,
		Rgbx8888,
		Rgba8888
	}

    public interface IImage
	{
		Size Size { get; }

		byte[] GetBitmap();
	}
}

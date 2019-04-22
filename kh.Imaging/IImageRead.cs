namespace kh.Imaging
{
    public interface IImageRead : IImage
    {
        byte[] GetData();

        byte[] GetClut();
    }
}

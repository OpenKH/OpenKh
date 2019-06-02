namespace OpenKh.Imaging
{
    public interface IImageRead : IImage
    {
        byte[] GetData();

        byte[] GetClut();
    }
}

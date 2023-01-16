using OpenKh.Imaging;
using OpenKh.Tools.ImageViewer.Services;
using OpenKh.Tools.ImageViewer.ViewModels;
using System.Drawing.Imaging;
using System.IO;
using Xunit;
using static OpenKh.Tools.ImageViewer.Services.ImageFormatService;

namespace OpenKh.Tools.ImageViewer.Tests
{
    public class ImageFormatServiceTests
    {
        private static string ResolveTestResourceFilePath(string relativePath) => Path.Combine(
            Environment.CurrentDirectory, "..", "..", "..", "res", relativePath
        );

        [Theory]
        [InlineData("helloworld.imd", "helloworld-saveas-png.png")]
        public void ConversionTest(string fileLoadFrom, string fileSaveTo)
        {
            using (var loadFrom = File.OpenRead(ResolveTestResourceFilePath(fileLoadFrom)))
            using (var saveTo = File.Create(ResolveTestResourceFilePath(fileSaveTo)))
            {
                var imageFormatService = new ImageFormatService();

                IImageRead singleImage;

                {
                    var imageFormat = imageFormatService.GetFormatByContent(loadFrom);

                    if (imageFormat.IsContainer)
                    {
                        var imageContainer = imageFormat.As<IImageMultiple>().Read(loadFrom);
                        singleImage = imageContainer.Images.First();
                    }
                    else
                    {
                        singleImage = imageFormat.As<IImageSingle>().Read(loadFrom);
                    }

                }

                {
                    var imageFormat = imageFormatService.GetFormatByFileName(fileSaveTo)
                        ?? throw new NotSupportedException();

                    imageFormat.As<IImageSingle>().Write(
                       saveTo,
                       singleImage
                   );
                }
            }
        }
    }
}

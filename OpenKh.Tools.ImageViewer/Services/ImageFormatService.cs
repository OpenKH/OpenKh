using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using OpenKh.Bbs;
using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Extensions;

namespace OpenKh.Tools.ImageViewer.Services
{  
    public partial class ImageFormatService : IImageFormatService
    {
        private static readonly IImageFormat[] imageFormat;

        static ImageFormatService()
        {
            imageFormat = new IImageFormat[]
            {
                GetImageFormat("PNG", "png", true, Png.IsValid, Png.Read, (stream, image) => Png.Write(stream, image)),
                GetImageFormat("BMP", "bmp", true, Bmp.IsValid, Bmp.Read, (stream, image) => Bmp.Write(stream, image)),
                GetImageFormat("TIFF", "tiff", true, Tiff.IsValid, Tiff.Read, (stream, image) => Tiff.Write(stream, image)),
                GetImageFormat("IMGD", "imd", true, Imgd.IsValid, Imgd.Read, (stream, image) => image.AsImgd().Write(stream)),

                GetImageFormat("IMGZ", "imz", true, Imgz.IsValid, s => Imgz.Read(s), (stream, images) =>
                    Imgz.Write(stream, images.Select(x => x.AsImgd()))),

                GetImageFormat("Font ARC", "arc", false, FontsArc.IsValid, s => FontsArc.Read(s).Images, (stream, images) =>
                    throw new NotImplementedException()),

                GetImageFormat("TIM2", "tm2", false, Tm2.IsValid, s => Tm2.Read(s), (stream, images) =>
                    throw new NotImplementedException()),

                GetImageFormat("KH2TIM", "tex", true, _ => true,
                    s => ModelTexture.Read(s).Images.Cast<IImageRead>(),
                    (stream, images) => throw new NotImplementedException()),
            };
        }

        public IEnumerable<IImageFormat> Formats => imageFormat;

        public IImageFormat GetFormatByContent(Stream stream) =>
            imageFormat.FirstOrDefault(x => x.IsValid(stream.SetPosition(0)));

        public IImageFormat GetFormatByFileName(string fileName)
        {
            var extension = Path.GetExtension(fileName);
            var dotIndex = extension.IndexOf('.');
            if (dotIndex >= 0)
                extension = extension.Substring(dotIndex + 1);

            return imageFormat.FirstOrDefault(x => string.Compare(x.Extension, extension, System.StringComparison.OrdinalIgnoreCase) == 0);
        }

        private static IImageFormat GetImageFormat(
            string name,
            string extension,
            bool isCreationSupported,
            Func<Stream, bool> isValid,
            Func<Stream, IImageRead> read,
            Action<Stream, IImageRead> write)
        {
            return new SingleImageFormat(name, extension, isCreationSupported, isValid, read, write);
        }

        private static IImageFormat GetImageFormat(
            string name,
            string extension,
            bool isCreationSupported,
            Func<Stream, bool> isValid,
            Func<Stream, IEnumerable<IImageRead>> read,
            Action<Stream, IEnumerable<IImageRead>> write)
        {
            return new MultipleImageFormat(name, extension, isCreationSupported, isValid,
                stream => new ImageContainer(read(stream)),
                (stream, container) => write(stream, container.Images));
        }
    }
}

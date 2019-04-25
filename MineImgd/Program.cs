using kh.kh2;
using kh.Imaging;
using System.IO;
using System.Linq;
using System.Collections.Generic;
using System;

namespace MineImgd
{
    public class Program
    {
        static void Main(string[] args)
        {
            foreach (var item in Directory
                .GetFiles(".", "*", SearchOption.AllDirectories)
                .Select(x =>
                {
                    var stream = File.OpenRead(x);
                    {
                        var imgds = GetImgd(stream);
                        return new
                        {
                            FileName = x,
                            ImageStreams = imgds
                        };
                    }
                })
                .Where(x => x.ImageStreams.Count > 0)
                .Select(x => x.ImageStreams.Select(imageStream => new
                {
                    FileName = GetExportPath(x.FileName),
                    ImageStream = imageStream
                }))
                .SelectMany(x => x))
            {
                SaveImageRaw(item.ImageStream, item.FileName);
            }
        }

        private static void SaveImageRaw(Stream stream, string fileName)
        {
            using (var outStream = File.Create($"{fileName}.imd"))
            {
                stream.Position = 0;
                stream.CopyTo(outStream);
            }
        }

        private static void SaveImagePng(Stream stream, string fileName)
        {
            new Imgd(stream).SaveImage($"{fileName}.png");
        }

        private static string GetExportPath(string path)
        {
            var fileName = path.Substring(2);
            fileName = fileName.Replace('\\', '-');
            fileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
            fileName = Path.Combine("export-imgd", fileName);
            return $"{fileName}_{Guid.NewGuid()}";
        }

        private static List<Stream> GetImgd(Stream stream)
        {
            if (TryGetImgdFromBar(stream, out List<Stream> imgdStreams))
                return imgdStreams;
            if (TryGetImgdFromImgz(stream, out imgdStreams))
                return imgdStreams;
            if (TryGetImgdFromImgd(stream, out imgdStreams))
                return imgdStreams;
            return new List<Stream>();
        }

        private static bool TryGetImgdFromBar(Stream stream, out List<Stream> streams)
        {
            if (!Bar.IsValid(stream))
            {
                streams = null;
                return false;
            }

            streams = Bar.Open(stream)
                .Select(x =>
                {
                    if (TryGetImgdFromBar(x.Stream, out List<Stream> imgdStreams))
                        return imgdStreams;
                    if (TryGetImgdFromImgz(x.Stream, out imgdStreams))
                        return imgdStreams;
                    if (TryGetImgdFromImgd(x.Stream, out imgdStreams))
                        return imgdStreams;
                    return new List<Stream>();
                })
                .SelectMany(x => x)
                .ToList();
            return true;
        }

        private static bool TryGetImgdFromImgz(Stream stream, out List<Stream> imgds)
        {
            if (!Imgz.IsValid(stream))
            {
                imgds = null;
                return false;
            }

            imgds = Imgz.OpenAsStream(stream).ToList();
            return true;
        }

        private static bool TryGetImgdFromImgd(Stream stream, out List<Stream> outStream)
        {
            if (!Imgd.IsValid(stream))
            {
                outStream = null;
                return false;
            }

            outStream = new List<Stream>{ stream };
            return true;
        }
    }
}

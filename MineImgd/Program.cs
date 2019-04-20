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
            Directory
                .GetFiles(".", "*", SearchOption.AllDirectories)
                .Select(x =>
                {
                    using (var stream = File.OpenRead(x))
                    {
                        var imgds = GetImgd(stream);
                        return new
                        {
                            FileName = x,
                            Images = imgds
                        };
                    }
                })
                .Where(x => x.Images.Count > 0)
                .Select(x => x.Images.Select(image => new
                {
                    FileName = GetExportPath(x.FileName),
                    Image = image
                }))
                .SelectMany(x => x)
                .ToList()
                .AsParallel()
                .ForAll(x =>
                {
                    x.Image.SaveImage(x.FileName);
                });
        }

        private static string GetExportPath(string path)
        {
            var fileName = path.Substring(2);
            fileName = fileName.Replace('\\', '-');
            fileName = Path.Combine(Path.GetDirectoryName(fileName), Path.GetFileNameWithoutExtension(fileName));
            fileName = Path.Combine("export-imgd", fileName);
            return $"{fileName}_{Guid.NewGuid()}.png";
        }

        private static List<Imgd> GetImgd(Stream stream)
        {
            if (TryGetImgdFromBar(stream, out List<Imgd> imgs))
                return imgs;
            if (TryGetImgdFromImgz(stream, out imgs))
                return imgs;
            if (TryGetImgdFromImgd(stream, out imgs))
                return imgs;
            return new List<Imgd>();
        }

        private static bool TryGetImgdFromBar(Stream stream, out List<Imgd> imgds)
        {
            if (!Bar.IsValid(stream))
            {
                imgds = null;
                return false;
            }

            imgds = Bar.Open(stream)
                .Select(x =>
                {
                    if (TryGetImgdFromBar(x.Stream, out List<Imgd> imgs))
                        return imgs;
                    if (TryGetImgdFromImgz(x.Stream, out imgs))
                        return imgs;
                    if (TryGetImgdFromImgd(x.Stream, out imgs))
                        return imgs;
                    return new List<Imgd>();
                })
                .SelectMany(x => x)
                .ToList();
            return true;
        }

        private static bool TryGetImgdFromImgz(Stream stream, out List<Imgd> imgds)
        {
            if (!Imgz.IsValid(stream))
            {
                imgds = null;
                return false;
            }

            imgds = Imgz.Open(stream).ToList();
            return true;
        }

        private static bool TryGetImgdFromImgd(Stream stream, out List<Imgd> imgds)
        {
            if (!Imgd.IsValid(stream))
            {
                imgds = null;
                return false;
            }

            imgds = new List<Imgd> { new Imgd(stream) };
            return true;
        }


    }
}

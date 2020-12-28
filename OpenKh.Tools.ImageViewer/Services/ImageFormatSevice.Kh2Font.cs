using OpenKh.Common;
using OpenKh.Imaging;
using OpenKh.Kh2;
using OpenKh.Kh2.Contextes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace OpenKh.Tools.ImageViewer.Services
{
    public partial class ImageFormatService
    {
        private static bool IsKh2Font(Stream inputStream)
        {
            if (!Bar.IsValid(inputStream.SetPosition(0)))
                return false;

            return Bar.Read(inputStream.SetPosition(0))
                .Any(x => x.Type == Bar.EntryType.RawBitmap);
        }

        private static IEnumerable<IImageRead> ReadKh2Font(Stream inputStream)
        {
            var fontContext = new FontContext();
            fontContext.Read(Bar.Read(inputStream.SetPosition(0))
                .Where(x => x.Type == Bar.EntryType.RawBitmap));

            return new[]
            {
                fontContext.ImageSystem,
                fontContext.ImageSystem2,
                fontContext.ImageEvent,
                fontContext.ImageEvent2,
                fontContext.ImageIcon
            }.Where(x => x != null);
        }

        private static void WriteKh2Font(Stream outputStream, IEnumerable<IImageRead> images)
        {
            throw new NotImplementedException();
        }
    }
}

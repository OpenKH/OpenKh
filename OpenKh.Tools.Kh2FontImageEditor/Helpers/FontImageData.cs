using OpenKh.Imaging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2FontImageEditor.Helpers
{
    public record FontImageData(
        IImageRead? ImageSystem,
        IImageRead? ImageSystem2,
        IImageRead? ImageEvent,
        IImageRead? ImageEvent2,
        IImageRead? ImageIcon)
    {
    }
}

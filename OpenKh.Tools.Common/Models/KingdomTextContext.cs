using OpenKh.Imaging;
using OpenKh.Kh2.Messages;

namespace OpenKh.Tools.Common.Models
{
    public class KingdomTextContext
    {
        public IImageRead Font { get; set; }
        public IImageRead Icon { get; set; }
        public byte[] FontSpacing { get; set; }
        public byte[] IconSpacing { get; set; }
        public IMessageEncoder Encoder { get; set; }

        public int FontWidth { get; set; }
        public int FontHeight { get; set; }
    }
}

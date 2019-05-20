using OpenKh.Imaging;
using OpenKh.Kh2.Messages;

namespace OpenKh.Tools.Common.Models
{
    public class KingdomTextContext
    {
        public IImageRead Font { get; set; }
        public IMessageEncode Encode { get; set; }
    }
}

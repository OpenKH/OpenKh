using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.Txa
{
    internal class Group
    {
        public string DestTex { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public string DefaultAnim { get; set; }
        public Dictionary<string, Anim> Anims { get; set; } = new Dictionary<string, Anim>();
    }

    internal class Anim
    {
        public int LoopFrame { get; set; }
        public List<Frame> Frames { get; set; } = new List<Frame>();
    }

    internal class Frame
    {
        public int FrameLo { get; set; }
        public int FrameHi { get; set; }
        public string Source { get; set; }
    }
}

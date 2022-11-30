using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.JsonAnimSource
{
    public class BFCurve
    {
        /// <summary>
        /// `location.0`
        /// </summary>
        public string ChannelRef { get; set; }

        public BKeyFrame[] KeyFrames { get; set; }
    }
}

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.JsonAnimSource
{
    public class BKeyFrame
    {
        public float Time { get; set; }
        public float Value { get; set; }
        /// <summary>
        /// `BEZIER`
        /// </summary>
        public string Interpolation { get; set; }
        public BHandleValue HandleLeft { get; set; }
        public BHandleValue HandleRight { get; set; }

    }
}

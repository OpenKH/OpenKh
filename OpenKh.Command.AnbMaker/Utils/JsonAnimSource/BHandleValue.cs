using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.JsonAnimSource
{
    public class BHandleValue
    {
        /// <summary>
        /// `AUTO_CLAMPED`
        /// </summary>
        public string Type { get; set; }
        public float Frame { get; set; }
        public float Value { get; set; }
    }
}

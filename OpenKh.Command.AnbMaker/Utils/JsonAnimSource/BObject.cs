using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Utils.JsonAnimSource
{
    public class BObject
    {
        /// <summary>
        /// `ARMATURE`
        /// </summary>
        public string Type { get; set; }

        public string Name { get; set; }

        public float Fps { get; set; }

        public BAnimationAction AnimationAction { get; set; }

        public BBone[] Bones { get; set; }
    }
}

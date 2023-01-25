using System;
using System.Collections.Generic;
using System.Text;

namespace OpenKh.Command.MapGen.Models
{
    public class TextureOptions
    {
        /// <summary>
        /// Horizontal texture addressing mode
        /// </summary>
        /// <example>["Repeat", "Clamp", "RegionClamp", "RegionRepeat"]</example>
        public string addressU { get; set; }

        /// <summary>
        /// Vertical texture addressing mode
        /// </summary>
        /// <example>["Repeat", "Clamp", "RegionClamp", "RegionRepeat"]</example>
        public string addressV { get; set; }
    }
}

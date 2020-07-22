using OpenKh.Command.MapGen.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenKh.Command.MapGen.Models
{
    public class MapGenConfig
    {
        /// <summary>
        /// Input 3D model file
        /// </summary>
        public string inputFile { get; set; }

        /// <summary>
        /// Output `.map` file
        /// </summary>
        public string outputFile { get; set; }

        public float scale { get; set; } = 1;

        public string[] imageDirs { get; set; }

        /// <summary>
        /// options for imgtool if needed
        /// </summary>
        public string imgtoolOptions { get; set; }

        /// <summary>
        /// The material set for matching 3D model's material name
        /// </summary>
        public List<MaterialDef> materials { get; set; } = new List<MaterialDef>();

        public List<AddFile> addFiles { get; set; } = new List<AddFile>();

        public class AddFile
        {
            public string name { get; set; }
            public int type { get; set; }
            public string fromFile { get; set; }
            public int index { get; set; }
        }

        public BarConfig bar { get; set; }

        public class BarConfig
        {
            public BarEntryConfig model { get; set; }
            public BarEntryConfig texture { get; set; }
            public BarEntryConfig coct { get; set; }
            public BarEntryConfig doct { get; set; }
        }

        public class BarEntryConfig
        {
            public string name { get; set; }
        }

        public MaterialDef FindMaterial(string name)
        {
            return materials
                .FirstOrDefault(one => SimplePatternUtil.CreateFrom(one.name).IsMatch(name));
        }
    }
}

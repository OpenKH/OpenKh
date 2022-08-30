using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;
using Xe.Graphics;

namespace OpenKh.Command.MapGen.Models
{
    /// <summary>
    /// Single face mode by triangle strip
    /// </summary>
    public class SingleFace
    {
        public MaterialDef matDef;

        /// <summary>
        /// A single 3D coordinate represents a center, corner or such.
        /// This is used to geometry splitter.
        /// </summary>
        public Vector3 referencePosition;

        public override string ToString() => referencePosition.ToString();

        public Vector3[] positionList;
        public Vector2[] uvList;
        public Color[] colorList;
    }
}

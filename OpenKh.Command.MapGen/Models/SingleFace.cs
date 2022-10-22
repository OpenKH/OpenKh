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
    /// Single face representation by polygon shape.
    /// </summary>
    /// <remarks>
    /// - Usually this is a triangle (3 coords) or triangle fan (4 coords).
    ///   There may be more (5 or greater), but it is not supported yet.
    /// 
    /// - `Assimp.PostProcessSteps.Triangulate` option will resolve all kinds of polygon shapes.
    ///   But generated UV coords seem to be broken.
    /// </remarks>
    public class SingleFace
    {
        public MaterialDef matDef;

        /// <summary>
        /// A single 3D coordinate represents a center position.
        /// </summary>
        /// <remarks>
        /// This is used to geometry splitter.
        /// </remarks>
        public Vector3 referencePosition;

        public override string ToString() => referencePosition.ToString();

        public Vector3[] positionList;
        public Vector2[] uvList;
        public Vector3[] normalList;
        public Color[] colorList;
    }
}

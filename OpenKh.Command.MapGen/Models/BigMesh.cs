using OpenKh.Command.MapGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using Xe.Graphics;
using static OpenKh.Command.MapGen.Models.MapGenConfig;

namespace OpenKh.Command.MapGen.Models
{
    /// <summary>
    /// 
    /// </summary>
    /// <remarks>
    /// tripletIndicesList.Count == uvList.Count == vertexColorList.Count
    /// </remarks>
    public class BigMesh
    {
        public List<Vector3> vertexList = new List<Vector3>();
        public List<TriangleStrip> triangleStripList = new List<TriangleStrip>();

        /// <summary>
        /// -1 if nodraw flag set
        /// </summary>
        public int textureIndex = -1;

        public MaterialDef matDef;

        public class TriangleStrip
        {
            public List<int> vertexIndices = new List<int>();
            public List<Vector2> uvList = new List<Vector2>();
            public List<Color> vertexColorList = new List<Color>();
        }
    }
}

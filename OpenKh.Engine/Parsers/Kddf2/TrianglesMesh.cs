using System.Collections.Generic;

namespace OpenKh.Engine.Parsers.Kddf2
{
    public class TrianglesMesh
    {
        public List<CustomVertex.PositionColoredTextured> Vertices { get; } =
            new List<CustomVertex.PositionColoredTextured>();
    }
}
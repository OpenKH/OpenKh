using OpenKh.Engine.Maths;
using System.Collections.Generic;

namespace OpenKh.Engine.Parsers.Kddf2
{
    class FfMesh
    {
        public List<Vector3> alpos = new List<Vector3>();
        public List<Vector2> alst = new List<Vector2>();
        public List<Ff3> al3 = new List<Ff3>();
        public List<MJ1[]> almtxuse = new List<MJ1[]>();
    }
}

using OpenKh.Command.MapGen.Interfaces;
using System.Collections.Generic;

namespace OpenKh.Command.MapGen.Models
{
    internal class SingleNodeEmitter : ISpatialNodeCutter
    {
        public SingleNodeEmitter(IEnumerable<SingleFace> faces)
        {
            Faces = faces;
        }

        public IEnumerable<SingleFace> Faces { get; }

        public IEnumerable<ISpatialNodeCutter> Cut()
        {
            return new SingleNodeEmitter[] { this };
        }
    }
}

using OpenKh.Command.MapGen.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

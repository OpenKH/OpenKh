using OpenKh.Command.MapGen.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.MapGen.Interfaces
{
    public interface ISpatialMeshCutter
    {
        /// <summary>
        /// Split this node into sub nodes.
        /// 
        /// This must not be empty.
        /// The end node continues to supply single node `this`.
        /// 
        /// Read and process `Meshes` property, only when this collection count is `1`.
        /// </summary>
        IEnumerable<ISpatialMeshCutter> Cut();

        /// <summary>
        /// Array of meshes belonging to this and descendant nodes.
        /// 
        /// This is always available.
        /// This also includes descendants meshes, recursively.
        /// </summary>
        IEnumerable<CenterPointedMesh> Meshes { get; }
    }
}

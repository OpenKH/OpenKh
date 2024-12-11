using OpenKh.Command.MapGen.Models;
using System.Collections.Generic;

namespace OpenKh.Command.MapGen.Interfaces
{
    public interface ISpatialNodeCutter
    {
        /// <summary>
        /// Split this node into sub nodes.
        /// 
        /// This must not be empty.
        /// The end node continues to supply single node `this`.
        /// 
        /// Read and process `Faces` property, only when this collection count is `1`.
        /// </summary>
        IEnumerable<ISpatialNodeCutter> Cut();

        /// <summary>
        /// Array of faces belonging to this and descendant nodes.
        /// 
        /// This is always available.
        /// This also includes descendants meshes, recursively.
        /// </summary>
        IEnumerable<SingleFace> Faces { get; }
    }
}

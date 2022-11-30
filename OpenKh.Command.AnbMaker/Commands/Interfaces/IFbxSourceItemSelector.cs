using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Commands.Interfaces
{
    internal interface IFbxSourceItemSelector
    {
        string RootName { get; }
        string MeshName { get; }
        string AnimationName { get; }
    }
}

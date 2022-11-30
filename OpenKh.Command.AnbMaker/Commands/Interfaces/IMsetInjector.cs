using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.AnbMaker.Commands.Interfaces
{
    internal interface IMsetInjector
    {
        string MsetFile { get; }
        int MsetIndex { get; }
    }
}

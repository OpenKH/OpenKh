using OpenKh.Command.MapGen.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Command.MapGen.Interfaces
{
    public interface ICollisionBuilder
    {
        CollisionBuilt GetBuilt();
    }
}

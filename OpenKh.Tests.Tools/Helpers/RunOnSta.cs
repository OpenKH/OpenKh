using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tests.Tools.Helpers
{
    internal static class RunOnSta
    {
        internal static void Run(Action action)
        {
            var thread = new Thread(_ => action());
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
    }
}

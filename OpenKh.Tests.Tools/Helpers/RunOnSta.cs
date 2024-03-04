using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace OpenKh.Tests.Tools.Helpers
{
    internal static class RunOnSta
    {
        private static Lazy<SynchronizationContext> _lazy = new Lazy<SynchronizationContext>(
            () =>
            {
                var source = new TaskCompletionSource<SynchronizationContext>();

                var thread = new Thread(
                    _ =>
                    {
                        // Make SynchronizationContext available
                        new Control().Dispose();
                        source.SetResult(SynchronizationContext.Current ?? throw new NullReferenceException());
                        Application.Run();
                    }
                );
                thread.SetApartmentState(ApartmentState.STA);
                thread.Start();

                return source.Task.Result;
            }
        );

        internal static void Run(Action action)
        {
            var synchronizationContext = _lazy.Value;

            synchronizationContext.Send(_ => action(), null);
        }
    }
}

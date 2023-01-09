using OpenKh.Tools.ModsManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Media.Animation;

namespace OpenKh.Tools.ModsManager.Services
{
    public class ProgressWindowService
    {
        public record ProgressMonitor(
            Action<string> SetTitle,
            Action<float> SetProgress,
            CancellationToken Cancellation
        );

        public async Task ShowAsync(Func<ProgressMonitor, Task> yourService)
        {
            await ShowAsync(
                async monitor =>
                {
                    await yourService(monitor);
                    return true;
                }
            );
        }

        public async Task<T> ShowAsync<T>(
            Func<ProgressMonitor, Task<T>> yourService
        )
        {
            var cancel = new CancellationTokenSource();
            var vm = new WorkInProgressWindow.TViewModel(
                DialogTitle: "OpenKh",
                OperationName: "Work in progress",
                ModName: "",
                ProgressUnknown: true,
                ProgressValue: 0f,
                ProgressText: "Please wait",
                Cancel: () => cancel.Cancel()
            );
            var window = new WorkInProgressWindow() { ViewModel = vm, };
            window.Show();
            var monitor = new ProgressMonitor(
                SetTitle: title =>
                {
                    vm = vm with { ProgressText = title, };
                    window.ViewModel = vm;
                },
                SetProgress: rate =>
                {
                    vm = vm with { ProgressValue = rate, ProgressUnknown = false, };
                    window.ViewModel = vm;
                },
                Cancellation: cancel.Token
            );
            try
            {
                var result = await yourService(monitor);
                return result;
            }
            finally
            {
                window.Close();
            }
        }

        public class Agent : IDisposable
        {
            public void Dispose()
            {
                throw new NotImplementedException();
            }
        }
    }
}

using OpenKh.Tools.ModsManager.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Services
{
    public class ProgressWindowService
    {
        public record ProgressMonitor(
            Action<string> SetTitle,
            Action<float> SetProgress
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
            var window = new InstallModProgressWindow()
            {
                OperationName = "In progress",
                ModName = "OpenKh",
                ProgressText = "In progress",
            };
            window.Show();
            var monitor = new ProgressMonitor(
                SetTitle: title => window.OperationName = title,
                SetProgress: rate => window.ProgressValue = rate
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

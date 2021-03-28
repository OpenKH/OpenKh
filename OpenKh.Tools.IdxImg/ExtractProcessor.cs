using OpenKh.Tools.IdxImg.Interfaces;
using OpenKh.Tools.IdxImg.Views;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;

namespace OpenKh.Tools.IdxImg
{
    public static class ExtractProcessor
    {
        public static Task ShowProgress(Action<IExtractProgress> action)
        {
            using var cts = new CancellationTokenSource();
            var window = Application.Current.Dispatcher.Invoke(() =>
                new ExtractProgressWindow(cts)
                {
                    Owner = Application.Current.MainWindow
                });

            var task = Task.Run(() => action(window), window.CancellationToken);
            Application.Current.Dispatcher.Invoke(() =>
                window.ShowDialog());
            cts.Cancel();
            if (!task.IsCompleted)
                task.Wait();

            return task;
        }
    }
}

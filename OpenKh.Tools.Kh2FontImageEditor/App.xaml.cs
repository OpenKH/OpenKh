using Microsoft.Extensions.DependencyInjection;
using OpenKh.Tools.Kh2FontImageEditor.DependencyInjection;
using OpenKh.Tools.Kh2FontImageEditor.Views;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace OpenKh.Tools.Kh2FontImageEditor
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        private ServiceProvider? _container;

        protected override void OnExit(ExitEventArgs e)
        {
            _container?.Dispose();

            base.OnExit(e);
        }

        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            _container = new ServiceCollection()
                .UseKh2FontImageEditor()
                .BuildServiceProvider();
            ShutdownMode = ShutdownMode.OnMainWindowClose;
            MainWindow = _container.GetRequiredService<MainWindow>();
            MainWindow.Show();
        }
    }
}

using Microsoft.Extensions.DependencyInjection;
using OpenKh.Tools.Common.Wpf;
using OpenKh.Tools.CtdEditor.Interfaces;
using OpenKh.Tools.CtdEditor.Services;
using OpenKh.Tools.CtdEditor.ViewModels;
using OpenKh.Tools.CtdEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.CtdEditor.Helpers
{
    public static class AddExtension
    {
        public static void AddCtdEditor(this IServiceCollection services)
        {
            services.AddTransient<MainWindow>();
            services.AddTransient<MainViewModel>();

            services.AddTransient<LayoutWindow>();
            services.AddTransient<Func<LayoutEditorViewModel, LayoutWindow>>(
                serviceProvider =>
                    (LayoutEditorViewModel vm) =>
                    {
                        var window = serviceProvider.GetRequiredService<LayoutWindow>();
                        window.DataContext = vm;
                        return window;
                    }
            );
            services.AddTransient<LayoutEditorViewModel>();

            services.AddTransient<FontWindow>();
            services.AddTransient<Func<FontEditorViewModel, FontWindow>>(
                serviceProvider =>
                    (FontEditorViewModel vm) =>
                    {
                        var window = serviceProvider.GetRequiredService<FontWindow>();
                        window.DataContext = vm;
                        return window;
                    }
            );
            services.AddTransient<FontEditorViewModel>();

            services.AddSingleton(
                new AppInfoService(
                    ApplicationName: Utilities.GetApplicationName()
                )
            );
            services.AddSingleton<CtdDrawHandler>();
        }
    }
}

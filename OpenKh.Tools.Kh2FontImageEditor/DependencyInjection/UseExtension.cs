using Microsoft.Extensions.DependencyInjection;
using OpenKh.Tools.Kh2FontImageEditor.Usecases;
using OpenKh.Tools.Kh2FontImageEditor.ViewModels;
using OpenKh.Tools.Kh2FontImageEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2FontImageEditor.DependencyInjection
{
    internal static class UseExtension
    {
        public static IServiceCollection UseKh2FontImageEditor(this IServiceCollection services)
        {
            services.AddSingleton<MainWindow>();
            services.AddSingleton<MainWindowVM>();
            services.AddSingleton<ShowErrorMessageUsecase>();
            services.AddSingleton<ExitAppUsecase>();
            services.AddSingleton<Func<MainWindow>>(sp => () => sp.GetRequiredService<MainWindow>());
            services.AddSingleton<ReplacePaletteAlphaUsecase>();

            return services;
        }
    }
}

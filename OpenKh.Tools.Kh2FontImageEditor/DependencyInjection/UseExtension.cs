using Microsoft.Extensions.DependencyInjection;
using OpenKh.Tools.Kh2FontImageEditor.Usecases;
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
            services.AddSingleton<ConvertFontImageUsecase>();
            services.AddSingleton<CombineBarUsecase>();
            services.AddSingleton<ConvertFontDataUsecase>();
            services.AddSingleton<CopyArrayUsecase>();
            services.AddSingleton<ApplySpacingToImageReadUsecase>();
            services.AddSingleton<CreateGlyphCellsUsecase>();

            services.AddTransient<SpacingWindow>();
            services.AddSingleton<Func<SpacingWindow>>(sp => () => sp.GetRequiredService<SpacingWindow>());
            services.AddTransient<SpacingWindowVM>();

            return services;
        }
    }
}

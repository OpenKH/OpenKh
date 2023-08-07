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
        public static void UseKh2FontImageEditor(this ServiceCollection container)
        {
            container.AddSingleton<MainWindow>();
            container.AddSingleton<MainWindowVM>();
            container.AddSingleton<ShowErrorMessageUsecase>();
        }
    }
}

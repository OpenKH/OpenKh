using Avalonia.Controls;
using OpenKh.Tools.Common.Avalonia.Internal;

namespace OpenKh.Tools.ModsManager.Services
{
    // Avalonia counterpart of the WPF GetActiveWindowService; the shared
    // ViewModels only rely on the returned window exposing Close().
    public class GetActiveWindowService
    {
        public Window GetActiveWindow() => WindowLocator.GetActiveWindow();
    }
}

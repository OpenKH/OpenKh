using kh.tools.common;

namespace kh.tools.layout.ViewModels
{
    public class MainViewModel
    {
        public string Title { get; }

        public RendererViewModel Renderer { get; }

        public MainViewModel()
        {
            Title = Utilities.GetApplicationName();
            Renderer = new RendererViewModel();
        }
    }
}

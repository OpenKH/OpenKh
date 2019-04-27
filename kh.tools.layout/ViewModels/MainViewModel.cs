namespace kh.tools.layout.ViewModels
{
    public class MainViewModel
    {
        public RendererViewModel Renderer { get; }

        public MainViewModel()
        {
            Renderer = new RendererViewModel();
        }
    }
}

using kh.kh2;
using kh.tools.common;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using Xe.Drawing;

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

        private void OpenFile(string fileName)
        {
            using (var stream = File.OpenRead(fileName))
            {
                if (!Bar.IsValid(stream))
                    throw new InvalidDataException("Not a bar file");

                OpenBarContent(Bar.Open(stream));
            }
        }

        private void OpenBarContent(List<Bar.Entry> entries)
        {
            var layout = entries.Where(x => x.Type == Bar.EntryType.Layout).Select(x => Layout.Read(x.Stream)).First();
            var images = entries.Where(x => x.Type == Bar.EntryType.Imgz).Select(x => Imgz.Open(x.Stream)).First();
            OpenLayout(layout, images);
        }

        private void OpenLayout(Layout layout, IEnumerable<Imgd> images)
        {
            Renderer.SetLayout(layout, images);
        }
    }
}

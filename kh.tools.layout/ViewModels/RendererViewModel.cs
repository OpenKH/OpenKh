using kh.tools.common;
using System.Drawing;
using Xe.Drawing;
using Xe.Tools.Wpf.Commands;

namespace kh.tools.layout.ViewModels
{
    public class RendererViewModel
    {
        public IDrawing Drawing { get; }
        public RelayCommand DrawCreateCommand { get; }
        public RelayCommand DrawDestroyCommand { get; }
        public RelayCommand DrawBeginCommand { get; }
        public RelayCommand DrawEndCommand { get; }

        public RendererViewModel()
        {
            Drawing = new DrawingDirect3D();
            DrawCreateCommand = new RelayCommand<IDrawing>(x =>
            {

            });
            DrawDestroyCommand = new RelayCommand<IDrawing>(x =>
            {

            });
            DrawBeginCommand = new RelayCommand<IDrawing>(x =>
            {
                x.Clear(Color.Magenta);
            });
            DrawEndCommand = new RelayCommand<IDrawing>(x =>
            {

            });
        }
    }
}

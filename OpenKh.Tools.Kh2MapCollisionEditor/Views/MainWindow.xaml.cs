using Microsoft.Xna.Framework.Graphics;
using OpenKh.Tools.Kh2MapCollisionEditor.Services;
using OpenKh.Tools.Kh2MapCollisionEditor.ViewModels;
using System;
using System.Windows;
using System.Windows.Interop;

namespace OpenKh.Tools.Kh2MapCollisionEditor.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private GraphicsDevice _graphicsDevice;
        private MonoDrawing _monoDrawing;

        public MainWindow()
        {
            InitializeComponent();

        }

        protected override void OnInitialized(EventArgs e)
        {
            base.OnInitialized(e);
            CreateGraphicsContext();
        }

        private void CreateGraphicsContext()
        {
            var handle = new WindowInteropHelper(this).Handle;
            _graphicsDevice = new GraphicsDevice(GraphicsAdapter.DefaultAdapter, GraphicsProfile.HiDef, new PresentationParameters
            {
                BackBufferWidth = Math.Max(800, 1),
                BackBufferHeight = Math.Max(480, 1),
                BackBufferFormat = SurfaceFormat.Color,
                DepthStencilFormat = DepthFormat.Depth24,
                DeviceWindowHandle = handle,
                PresentationInterval = PresentInterval.Immediate,
                IsFullScreen = false
            });
            _monoDrawing = new MonoDrawing(_graphicsDevice);
            DataContext = new MainViewModel(_graphicsDevice, _monoDrawing);
        }
    }
}

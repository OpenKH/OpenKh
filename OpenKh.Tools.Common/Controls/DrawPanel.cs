using System;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Xe.Drawing;
using static OpenKh.Tools.Common.DependencyPropertyUtils;

namespace kh.tools.common.Controls
{
    public class DrawPanel : FrameworkElement
    {
        [DllImport("kernel32.dll", EntryPoint = nameof(CopyMemory), SetLastError = false)]
        private static extern void CopyMemory(IntPtr dest, IntPtr src, int count);

        public static readonly DependencyProperty DrawingProperty =
            GetDependencyProperty<DrawPanel, IDrawing>(nameof(Drawing),(o, x) => o.SetDrawing(x));

        public static readonly DependencyProperty DrawCreateCommandProperty =
            GetDependencyProperty<DrawPanel, ICommand>(nameof(DrawCreate), (o, x) => o.drawCreateCommand = x);

        public static readonly DependencyProperty DrawDestroyCommandProperty =
            GetDependencyProperty<DrawPanel, ICommand>(nameof(DrawDestroy), (o, x) => o.drawDestroyCommand = x);

        public static readonly DependencyProperty DrawBeginCommandProperty =
            GetDependencyProperty<DrawPanel, ICommand>(nameof(DrawBegin), (o, x) => o.drawBeginCommand = x);

        public static readonly DependencyProperty DrawEndCommandProperty =
            GetDependencyProperty<DrawPanel, ICommand>(nameof(DrawEnd), (o, x) => o.drawEndCommand = x);

        public static readonly DependencyProperty FramesPerSecondProperty =
            GetDependencyProperty<DrawPanel, double>(nameof(FramesPerSecond), 30.0f, (o, x) => o.SetFramesPerSecond(x), x => x >= 0.0f);

        private IDrawing drawing;
        private ICommand drawCreateCommand;
        private ICommand drawDestroyCommand;
        private ICommand drawBeginCommand;
        private ICommand drawEndCommand;

        private VisualCollection _children;
        private DrawingVisual _visual;
        private WriteableBitmap _writeableBitmap;
        private System.Timers.Timer _timer = new System.Timers.Timer();
        private System.Diagnostics.Stopwatch _stopwatch = new System.Diagnostics.Stopwatch();
        private System.Diagnostics.Stopwatch _stopwatchDeltaTime = new System.Diagnostics.Stopwatch();

        public IDrawing Drawing
        {
            get => (IDrawing)GetValue(DrawingProperty);
            set => SetValue(DrawingProperty, value);
        }

        /// <summary>
        /// Called when an IDrawing has been set.
        /// </summary>
        public ICommand DrawCreate
        {
            get => (ICommand)GetValue(DrawCreateCommandProperty);
            set => SetValue(DrawCreateCommandProperty, value);
        }

        /// <summary>
        /// Called before disposing an IDrawing.
        /// </summary>
        public ICommand DrawDestroy
        {
            get => (ICommand)GetValue(DrawDestroyCommandProperty);
            set => SetValue(DrawDestroyCommandProperty, value);
        }

        /// <summary>
        /// Called when the frame needs to be rendered.
        /// </summary>
        public ICommand DrawBegin
        {
            get => (ICommand)GetValue(DrawBeginCommandProperty);
            set => SetValue(DrawBeginCommandProperty, value);
        }

        /// <summary>
        /// Called once the frame has been rendered.
        /// </summary>
        public ICommand DrawEnd
        {
            get => (ICommand)GetValue(DrawEndCommandProperty);
            set => SetValue(DrawEndCommandProperty, value);
        }

        /// <summary>
        /// Get or set how frames per second needs to be drawn.
        /// A value of 0 stops the execution.
        /// Values below 0 are not valid.
        /// </summary>
        public double FramesPerSecond
        {
            get => (double)GetValue(FramesPerSecondProperty);
            set => SetValue(FramesPerSecondProperty, value);
        }

        public double LastDrawTime { get; private set; }
        public double LastDrawAndPresentTime { get; private set; }
        public double DeltaTime { get; private set; }

        public DrawPanel()
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                _visual = new DrawingVisual();
                _children = new VisualCollection(this)
                {
                    _visual
                };

                _timer.Elapsed += (sender, args) =>
                {
                    if (drawing == null)
                        return;
                    Application.Current?.Dispatcher.Invoke(new Action(() =>
                    {
                        DoRender();
                    }));
                };

                SetFramesPerSecond(FramesPerSecond);
            }
            else
            {
                _children = new VisualCollection(this);
            }
        }

        /// <summary>
        /// Rendering on demand.
        /// </summary>
        public void DoRender()
        {
            DeltaTime = _stopwatchDeltaTime.Elapsed.TotalMilliseconds / 1000.0;
            _stopwatchDeltaTime.Restart();

            _stopwatch.Restart();

            if (Drawing == null)
                return;

            OnDrawBegin();
            LastDrawTime = _stopwatch.Elapsed.TotalMilliseconds;
            Present();
            LastDrawAndPresentTime = _stopwatch.Elapsed.TotalMilliseconds;
            OnDrawEnd();
        }

        public async Task DoRenderAsync() => await DoRenderTask();

        public Task DoRenderTask() =>
            Task.Run(() =>
            {
                DoRender();
            });

        // Provide a required override for the VisualChildrenCount property.
        protected override int VisualChildrenCount => _children.Count;

        // Provide a required override for the GetVisualChild method.
        protected override Visual GetVisualChild(int index)
        {
            if (index < 0 || index >= _children.Count)
                throw new ArgumentOutOfRangeException();

            return _children[index];
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            if (!DesignerProperties.GetIsInDesignMode(this))
            {
                var size = sizeInfo.NewSize;
                ResizeRenderingEngine((int)size.Width, (int)size.Height);
                base.OnRenderSizeChanged(sizeInfo);
            }
        }

        protected virtual void OnDrawCreate() => drawCreateCommand.Invoke(drawing);
        protected virtual void OnDrawDestroy() => drawDestroyCommand.Invoke(drawing);
        protected virtual void OnDrawBegin() => drawBeginCommand.Invoke(drawing);
        protected virtual void OnDrawEnd() => drawEndCommand.Invoke(drawing);

        private void SetDrawing(IDrawing drawing)
        {
            if (this.drawing == drawing)
                return;

            if (this.drawing != null)
            {
                OnDrawDestroy();
                this.drawing.Dispose();
            }

            this.drawing = drawing;
            ResizeRenderingEngine((int)Math.Round(Math.Max(1, ActualWidth)), (int)Math.Round(Math.Max(1, ActualHeight)));
            OnDrawCreate();
        }

        private void SetFramesPerSecond(double framesPerSec)
        {
            if (framesPerSec < 0)
                throw new ArgumentException($"{nameof(FramesPerSecond)} value is set to {framesPerSec}, but it cannot be below than 0.");

            if (framesPerSec > 0)
            {
                _timer.Enabled = true;
                _timer.Interval = 1000.0 / framesPerSec;
            }
            else
            {
                _timer.Enabled = false;
            }
        }

        private void Present()
        {
            Present(drawing?.Surface);
        }

        private void Present(ISurface surface)
        {
            if (surface != null && surface.Width > 0 && surface.Height > 0)
            {
                BlitSutface(surface);
            }

            using (var dc = _visual.RenderOpen())
            {
                Present(dc, _writeableBitmap);
            }
        }

        private void BlitSutface(ISurface surface)
        {
            using (var map = surface.Map())
            {
                if (_writeableBitmap == null ||
                    surface.Width != _writeableBitmap.Width ||
                    surface.Height != _writeableBitmap.Height ||
                    map.Stride / 4 != _writeableBitmap.Width)
                {
                    _writeableBitmap = new WriteableBitmap(map.Stride / 4, surface.Height, 96.0, 96.0, PixelFormats.Bgra32, null);
                }

                _writeableBitmap.Lock();
                CopyMemory(_writeableBitmap.BackBuffer, map.Data, map.Length);
                _writeableBitmap.AddDirtyRect(new Int32Rect()
                {
                    X = 0,
                    Y = 0,
                    Width = surface.Width,
                    Height = surface.Height
                });
                _writeableBitmap.Unlock();
            }
        }

        private void Present(DrawingContext dc, ImageSource image)
        {
            if (image == null)
                return;

            dc.DrawImage(image, new Rect()
            {
                X = 0,
                Y = 0,
                Width = image.Width,
                Height = image.Height
            });
        }

        private void ResizeRenderingEngine(int width, int height)
        {
            if (drawing == null)
                return;

            width = Math.Min(Math.Max(1, width), 65536);
            height = Math.Min(Math.Max(1, height), 65536);

            drawing.Surface?.Dispose();
            drawing.Surface = drawing.CreateSurface(
                width, height, Xe.Drawing.PixelFormat.Format32bppArgb, SurfaceType.InputOutput);
            DoRender();
        }
    }
}

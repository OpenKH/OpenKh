using OpenKh.Tools.Kh2ObjectEditor.Services;
using System.Diagnostics;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;

namespace OpenKh.Tools.Kh2ObjectEditor.Views
{
    public partial class Viewport_Control : UserControl
    {
        private bool _isMouseOverDockPanel = false;

        public Viewport_Control()
        {
            InitializeComponent();
            DataContext = ViewerService.Instance;
            ViewerService.Instance.HookViewport(HelixViewport);
        }

        private void Button_PreviousFrame(object sender, System.Windows.RoutedEventArgs e)
        {
            PreviousFrame();
        }

        private void Button_NextFrame(object sender, System.Windows.RoutedEventArgs e)
        {
            NextFrame();
        }

        private void Slider_DragStarted(object sender, DragStartedEventArgs e)
        {
            if (MsetService.Instance.LoadedMotion == null) {
                return;
            }
            ViewerService.Instance.AnimationRunning = false;
        }
        private void Slider_DragCompleted(object sender, DragCompletedEventArgs e)
        {
            if (MsetService.Instance.LoadedMotion == null) {
                return;
            }
            ViewerService.Instance.LoadFrame();
        }

        private void DockPanel_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isMouseOverDockPanel = true;
        }

        private void DockPanel_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
        {
            _isMouseOverDockPanel = false;
        }

        private void Viewport_KeyDown(object sender, KeyEventArgs e)
        {
            // Prevent Viewport to use these keys
            if (e.Key == Key.Left ||
                e.Key == Key.Right ||
                e.Key == Key.Up ||
                e.Key == Key.Down ||
                e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (!_isMouseOverDockPanel) {
                return;
            }

            if (e.Key == Key.Space)
            {
                ViewerService.Instance.AnimationRunning = !ViewerService.Instance.AnimationRunning;
            }
            else if(e.Key == Key.Left)
            {
                PreviousFrame();
            }
            else if (e.Key == Key.Right)
            {
                NextFrame();
            }
        }

        private void PreviousFrame(int frameStep = -1)
        {
            if (MsetService.Instance.LoadedMotion == null)
            {
                return;
            }
            ViewerService.Instance.AnimationRunning = false;
            ViewerService.Instance.FrameIncrease(frameStep);
            ViewerService.Instance.LoadFrame();
        }

        private void NextFrame(int frameStep = 1)
        {
            if (MsetService.Instance.LoadedMotion == null)
            {
                return;
            }
            ViewerService.Instance.AnimationRunning = false;
            ViewerService.Instance.FrameIncrease(frameStep);
            ViewerService.Instance.LoadFrame();
        }

        private void Button_Play(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewerService.Instance.AnimationRunning = true;
        }

        private void Button_Pause(object sender, System.Windows.RoutedEventArgs e)
        {
            ViewerService.Instance.AnimationRunning = false;
        }

        private void Button_Stop(object sender, System.Windows.RoutedEventArgs e)
        {
            if (MdlxService.Instance.ModelFile == null)
            {
                return;
            }
            ViewerService.Instance.AnimationRunning = false;
            ViewerService.Instance.Render();
        }
    }
}

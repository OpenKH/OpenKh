using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using static kh.tools.common.DependencyPropertyUtils;

namespace OpenKh.Tools.LayoutViewer.Controls
{
    /// <summary>
    /// Interaction logic for Timeline.xaml
    /// </summary>
    public partial class Timeline : UserControl
    {
        public static readonly DependencyProperty ValueProperty =
            GetDependencyProperty<Timeline, double>(nameof(Value), (o, x) => o.SetValue(x));

        public static readonly DependencyProperty MaxValueProperty =
            GetDependencyProperty<Timeline, double>(nameof(MaxValue), (o, x) => o.SetMaxValue(x));

        public double Value
        {
            get => (double)GetValue(ValueProperty);
            set => SetValue(ValueProperty, value);
        }

        public double MaxValue
        {
            get => (double)GetValue(MaxValueProperty);
            set => SetValue(MaxValueProperty, value);
        }

        public Timeline()
        {
            InitializeComponent();
        }

        private void SetValue(double x)
        {
            UpdateCursorPosition();
        }

        private void SetMaxValue(double x)
        {
            UpdateCursorPosition();
        }

        private void UpdateCursorPosition()
        {
            Canvas.SetLeft(cursor, Math.Min(Value, MaxValue) / MaxValue * ActualWidth);
        }

        protected override void OnRenderSizeChanged(SizeChangedInfo sizeInfo)
        {
            base.OnRenderSizeChanged(sizeInfo);
            UpdateCursorPosition();
        }

        private void UserControl_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            var mousePosition = e.GetPosition(sender as FrameworkElement);
            Value = mousePosition.X / ActualWidth * MaxValue;
        }
    }
}

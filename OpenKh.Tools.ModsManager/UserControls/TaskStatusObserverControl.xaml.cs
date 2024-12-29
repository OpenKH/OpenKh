using System;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace OpenKh.Tools.ModsManager.UserControls
{
    /// <summary>
    /// Interaction logic for TaskStatusObserverControl.xaml
    /// </summary>
    public partial class TaskStatusObserverControl : UserControl
    {
        public static readonly DependencyProperty TaskProperty = DependencyProperty.Register(
            nameof(Task),
            typeof(Task),
            typeof(TaskStatusObserverControl),
            new PropertyMetadata(null, TaskPropertyChangedCallback)
        );

        private static void TaskPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TaskStatusObserverControl)d).TaskPropertyChangedCallback((Task)e.NewValue);
        }

        private void TaskPropertyChangedCallback(Task value)
        {
            if (value == null)
            {
                _label.Text = "(Task result here)";
            }
            else
            {
                async void AwaitAsync(Task task)
                {
                    try
                    {
                        _label.Text = "(Awaiting task result)";

                        await task;

                        if (ReferenceEquals(task, Task))
                        {
                            _label.Text = $"Done on {DateTime.Now}";
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ReferenceEquals(task, Task))
                        {
                            _label.Text = $"Error: {ex.Message}";
                            _label.ToolTip = ex + "";
                        }
                    }
                }

                AwaitAsync(value);
            }
        }

        public Task Task
        {
            get => (Task)GetValue(TaskProperty);
            set
            {
                SetValue(TaskProperty, value);
            }
        }

        public TaskStatusObserverControl()
        {
            InitializeComponent();
        }
    }
}

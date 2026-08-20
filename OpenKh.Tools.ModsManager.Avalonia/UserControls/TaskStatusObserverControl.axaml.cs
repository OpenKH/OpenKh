using Avalonia;
using Avalonia.Controls;
using System;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.UserControls
{
    public partial class TaskStatusObserverControl : UserControl
    {
        public static readonly StyledProperty<Task> TaskProperty =
            AvaloniaProperty.Register<TaskStatusObserverControl, Task>(nameof(Task));

        public Task Task
        {
            get => GetValue(TaskProperty);
            set => SetValue(TaskProperty, value);
        }

        public TaskStatusObserverControl()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == TaskProperty)
                OnTaskChanged(change.GetNewValue<Task>());
        }

        private void OnTaskChanged(Task value)
        {
            if (value == null)
            {
                _label.Text = "(Task result here)";
                return;
            }

            async void AwaitAsync(Task task)
            {
                try
                {
                    _label.Text = "(Awaiting task result)";

                    await task;

                    if (ReferenceEquals(task, Task))
                        _label.Text = $"Done on {DateTime.Now}";
                }
                catch (Exception ex)
                {
                    if (ReferenceEquals(task, Task))
                    {
                        _label.Text = $"Error: {ex.Message}";
                        ToolTip.SetTip(_label, ex + "");
                    }
                }
            }

            AwaitAsync(value);
        }
    }
}

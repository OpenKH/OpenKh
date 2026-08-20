using Avalonia;
using Avalonia.Controls;
using System;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.UserControls
{
    // Avalonia counterpart of the WPF control; the four themed status icons
    // are replaced with text glyphs so no icon resources are needed.
    public partial class TaskStatusByIconControl : UserControl
    {
        public static readonly StyledProperty<Task> TaskProperty =
            AvaloniaProperty.Register<TaskStatusByIconControl, Task>(nameof(Task));

        public Task Task
        {
            get => GetValue(TaskProperty);
            set => SetValue(TaskProperty, value);
        }

        public TaskStatusByIconControl()
        {
            InitializeComponent();
            ShowState("·");
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);
            if (change.Property == TaskProperty)
                OnTaskChanged(change.GetNewValue<Task>());
        }

        private void ShowState(string glyph, string toolTip = "")
        {
            _icon.Text = glyph;
            ToolTip.SetTip(_icon, toolTip);
        }

        private void OnTaskChanged(Task value)
        {
            if (value == null)
            {
                ShowState("·");
                return;
            }

            async void AwaitAsync(Task task)
            {
                try
                {
                    ShowState("⏳");

                    await task;

                    if (ReferenceEquals(task, Task))
                        ShowState("✔");
                }
                catch (Exception ex)
                {
                    if (ReferenceEquals(task, Task))
                        ShowState("❌", ex + "");
                }
            }

            AwaitAsync(value);
        }
    }
}

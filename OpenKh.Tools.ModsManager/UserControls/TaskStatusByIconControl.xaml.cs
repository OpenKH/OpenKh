using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace OpenKh.Tools.ModsManager.UserControls
{
    /// <summary>
    /// Interaction logic for TaskStatusByIconControl.xaml
    /// </summary>
    public partial class TaskStatusByIconControl : UserControl
    {
        #region ReadyIcon Property
        public ImageSource ReadyIcon
        {
            get => (ImageSource)GetValue(ReadyIconProperty);
            set => SetValue(ReadyIconProperty, value);
        }

        public static readonly DependencyProperty ReadyIconProperty = DependencyProperty.Register(
            nameof(ReadyIcon),
            typeof(ImageSource),
            typeof(TaskStatusByIconControl)
        );
        #endregion

        #region InProgressIcon Property
        public ImageSource InProgressIcon
        {
            get => (ImageSource)GetValue(InProgressIconProperty);
            set => SetValue(InProgressIconProperty, value);
        }

        public static readonly DependencyProperty InProgressIconProperty = DependencyProperty.Register(
            nameof(InProgressIcon),
            typeof(ImageSource),
            typeof(TaskStatusByIconControl)
        );
        #endregion

        #region SuccessfulIcon Property
        public ImageSource SuccessfulIcon
        {
            get => (ImageSource)GetValue(SuccessfulIconProperty);
            set => SetValue(SuccessfulIconProperty, value);
        }

        public static readonly DependencyProperty SuccessfulIconProperty = DependencyProperty.Register(
            nameof(SuccessfulIcon),
            typeof(ImageSource),
            typeof(TaskStatusByIconControl)
        );
        #endregion

        #region FailureIcon Property
        public ImageSource FailureIcon
        {
            get => (ImageSource)GetValue(FailureIconProperty);
            set => SetValue(FailureIconProperty, value);
        }

        public static readonly DependencyProperty FailureIconProperty = DependencyProperty.Register(
            nameof(FailureIcon),
            typeof(ImageSource),
            typeof(TaskStatusByIconControl)
        );
        #endregion

        #region Task Property
        public Task Task
        {
            get => (Task)GetValue(TaskProperty);
            set => SetValue(TaskProperty, value);
        }

        public static readonly DependencyProperty TaskProperty = DependencyProperty.Register(
            nameof(Task),
            typeof(Task),
            typeof(TaskStatusByIconControl),
            new PropertyMetadata(null, TaskPropertyChangedCallback)
        );

        private static void TaskPropertyChangedCallback(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ((TaskStatusByIconControl)d).TaskPropertyChangedCallback((Task)e.NewValue);
        }

        private void TaskPropertyChangedCallback(Task value)
        {
            _icon.ToolTip = "";

            if (value == null)
            {
                _icon.Source = ReadyIcon;
            }
            else
            {
                async void AwaitAsync(Task task)
                {
                    try
                    {
                        _icon.Source = InProgressIcon;

                        await task;

                        if (ReferenceEquals(task, Task))
                        {
                            _icon.Source = SuccessfulIcon;
                        }
                    }
                    catch (Exception ex)
                    {
                        if (ReferenceEquals(task, Task))
                        {
                            _icon.Source = FailureIcon;
                            _icon.ToolTip = ex + "";
                        }
                    }
                }

                AwaitAsync(value);
            }
        }
        #endregion

        public TaskStatusByIconControl()
        {
            InitializeComponent();
        }
    }
}

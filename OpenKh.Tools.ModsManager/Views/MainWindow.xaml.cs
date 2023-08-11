using OpenKh.Tools.ModsManager.Services;
using OpenKh.Tools.ModsManager.ViewModels;
using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace OpenKh.Tools.ModsManager.Views
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            DataContext = new MainViewModel();

            if(Directory.EnumerateFiles(ConfigurationService.PresetPath).Any())
            {
                foreach (string file in Directory.GetFiles(ConfigurationService.PresetPath))
                {
                    MenuItem item = new MenuItem();
                    item.Header = file.Replace(ConfigurationService.PresetPath, "").Replace(".txt", "").Trim("\\".ToCharArray());
                    item.SetBinding(Button.CommandProperty, new Binding("LoadPreset"));
                    item.CommandParameter = file.Replace(ConfigurationService.PresetPath, "").Replace(".txt", "").Trim("\\".ToCharArray());
                    PresetMenu.Items.Add(item);
                }
            }
            else
            {
                MenuItem item = new MenuItem();
                item.Header = "No Presets Found";
                item.Focusable = false;
                item.IsHitTestVisible = false;
                PresetMenu.Items.Add(item);
            }
        }     

        protected override void OnClosed(EventArgs e)
        {
            (DataContext as MainViewModel)?.CloseAllWindows();
            base.OnClosed(e);
        }
    }    
}

using OpenKh.Tools.LevelUpEditor.ViewModels;
using System.IO;
using System.Windows;

namespace OpenKh.Tools.LevelUpEditor
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class LvupView : Window
    {
        public LvupView()
        {
            InitializeComponent();
            DataContext = new LvupViewModel();
        }

        public LvupView(object[] args) : this()
        {
            if (args.Length > 0)
            {
                if (args[0] is Stream stream)
                    Initialize(stream);
            }
        }

        private void Initialize(Stream stream)
        {
            DataContext = new LvupViewModel(stream);
        }
    }
}

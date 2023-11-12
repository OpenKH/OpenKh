using OpenKh.Tools.Kh2FontImageEditor.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2FontImageEditor.Usecases
{
    public class ExitAppUsecase
    {
        private readonly Func<MainWindow> _mainWindow;

        public ExitAppUsecase(
            Func<MainWindow> mainWindow)
        {
            _mainWindow = mainWindow;
        }

        public void ExitApp() => _mainWindow().Close();
    }
}

using OpenKh.Tools.Common.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.Kh2FontImageEditor.Usecases
{
    public class ShowErrorMessageUsecase
    {
        public void Show(Exception ex)
        {
            new MessageDialog($"There is a critical error:\n\n{ex}")
                .ShowDialog();
        }
    }
}

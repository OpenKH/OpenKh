using OpenKh.Tools.Kh2MsetMotionEditor.Helpers;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace OpenKh.Tools.Kh2MsetMotionEditor.Usecases
{
    public class AskOpenFileNowUsecase
    {
        private readonly ErrorMessages _errorMessages;

        public AskOpenFileNowUsecase(ErrorMessages errorMessages)
        {
            _errorMessages = errorMessages;
        }

        public void AskAndOpen(string file)
        {
            if (File.Exists(file))
            {
                if (MessageBox.Show($"Open this file now?\n\n{file}", "Question", MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        Process.Start(
                            new ProcessStartInfo(file)
                            {
                                UseShellExecute = true,
                            }
                        );
                    }
                    catch (Exception ex)
                    {
                        _errorMessages.Add(new Exception($"The file \"{file}\" couldn't be opened due to error", ex));
                    }
                }
            }
        }
    }
}

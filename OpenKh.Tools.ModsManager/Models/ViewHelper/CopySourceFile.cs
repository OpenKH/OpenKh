using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenKh.Tools.ModsManager.Models.ViewHelper
{
    public record CopySourceFile(string Display, string ActionName, bool DestinationFileExists, Func<Task> AsyncAction) : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;

        #region DoAction
        private bool _doAction = false;
        public bool DoAction
        {
            get => _doAction;
            set
            {
                _doAction = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(DoAction)));
            }
        }
        #endregion
    }
}
